using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FastBite.Data.Configs;
using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Data.Models;
using FastBite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Services.Classes;

public class ProductService : IProductService
{
    private readonly FastBiteContext _context;
    public Mapper mapper;
    private readonly IConfiguration _config;
     private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public ProductService(FastBiteContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        mapper = MappingConfiguration.InitializeConfig();
        _blobServiceClient = new BlobServiceClient(_config["BlobConnection:ConnectionString"]);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_config["BlobConnection:ContainerName"]);
    }

    public async Task<List<ProductDTO>> GetAllProductsAsync()
    {
        var res = await _context.Products.Include(p => p.Category)
        .Include(p => p.Translations)
        .ToListAsync(); 
        return mapper.Map<List<ProductDTO>>(res);
    }

    public async Task<ProductDTO> AddNewProductAsync(ProductDTO productDto, CancellationToken cancellationToken)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(productDto.ImageUrl); 

        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == productDto.CategoryName);
            if (category == null)
            {
                throw new Exception("Category not found.");
            }

            var product = new Product
            {
                Price = productDto.Price,
                CategoryId = category.Id,
                ImageUrl = productDto.ImageUrl,
                Translations = new List<ProductTranslation>()
            };

            foreach (var translationDto in productDto.Translations)
            {
                var translation = new ProductTranslation
                {
                    LanguageCode = translationDto.LanguageCode,
                    Name = translationDto.Name,
                    Description = translationDto.Description,
                    Product = product 
                };
                product.Translations.Add(translation);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return mapper.Map<ProductDTO>(product);
        }
        catch (OperationCanceledException)
        {
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            throw new Exception("Operation was cancelled");
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded");
            }

            BlobClient blobClient = _containerClient.GetBlobClient(file.FileName);

            using var stream = file.OpenReadStream();

            var ops = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType ?? "application/octet-stream"
                }
            };

            var uploadTask = blobClient.UploadAsync(stream, ops, cancellationToken);

            if (await Task.WhenAny(uploadTask, Task.Delay(TimeSpan.FromSeconds(15), cancellationToken)) == uploadTask)
            {
                await uploadTask; 
                return blobClient.Uri.ToString(); // Возвращаем только URL
            }
            else
            {   
                throw new TimeoutException("Request timed out");
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error uploading file: {e.Message}");
        }
    }

    public async Task<PostResponse> DeleteProductAsync(string productName)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Translations
                    .Any(t => t.LanguageCode == "en" && t.Name == productName));

            if (product == null)
            {
                return new PostResponse("Product Not Found", 404);
            }

            _context.ProductTranslations.RemoveRange(product.Translations);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return new PostResponse("Product Deleted", 200);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Error deleting product: {ex.InnerException?.Message ?? ex.Message}");
            return new PostResponse("Error deleting product. Please check related data constraints.", 500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return new PostResponse("An unexpected error occurred.", 500);
        }
    }

    public async Task<PostResponse> UpdateProductAsync(string productName, ProductDTO updatedProductDto)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Translations
                    .Any(t => t.LanguageCode == "en" && t.Name == productName));

            if (product == null)
            {
                return new PostResponse("Product not found", 404);
            }

            product.Price = updatedProductDto.Price;

            var englishTranslation = product.Translations
                .FirstOrDefault(t => t.LanguageCode == "en");

            if (englishTranslation != null)
            {
                englishTranslation.Name = updatedProductDto.Translations
                    .First(t => t.LanguageCode == "en").Name;
            }

            await _context.SaveChangesAsync();
            return new PostResponse("Product updated successfully", 200);
        }
        catch (Exception ex)
        {
            return new PostResponse($"Error updating product: {ex.Message}", 500);
        }
    }
}