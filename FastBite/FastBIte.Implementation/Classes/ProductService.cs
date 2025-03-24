using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FastBite.Implementation.Configs;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;
using FastBite.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace FastBite.Implementation.Classes;

public class ProductService : IProductService
{
    private readonly FastBiteContext _context;
    public IMapper mapper;
    private readonly IConfiguration _config;
     private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;


    public ProductService(FastBiteContext context, IConfiguration config, IConnectionMultiplexer redis)
    {
        _context = context;
        _config = config;
        mapper = MappingConfiguration.InitializeConfig();
        _blobServiceClient = new BlobServiceClient(_config["BlobConnection:ConnectionString"]);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_config["BlobConnection:ContainerName"]);
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task<List<ProductDTO>> GetAllProductsAsync()
    {
        var res = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Translations)
            .Include(p => p.ProductTags)
            .ToListAsync(); 
        return mapper.Map<List<ProductDTO>>(res);
    }

    public async Task AddProductToCartAsync(string userId, Guid productId)
    {
        var cartKey = $"cart:{userId}";
        var expirationKey = $"{cartKey}:expiration"; 
        var product = await _context.Products.FindAsync(productId);

        if (product == null)
        {
            throw new Exception("Product not found.");
        }

        await _db.ListLeftPushAsync(cartKey, productId.ToString());

        var expirationTime = DateTime.UtcNow.AddMinutes(15);
        await _db.KeyExpireAsync(cartKey, TimeSpan.FromMinutes(15));
        await _db.StringSetAsync(expirationKey, expirationTime.ToString("o"), TimeSpan.FromMinutes(30)); 
    }

    public async Task<List<ProductDTO>> GetUserCartAsync(string userId)
    {
        var cartKey = $"cart:{userId}";
        var productIds = await _db.ListRangeAsync(cartKey);
    
        var products = new List<ProductDTO>();
    
        foreach (var productId in productIds)
        {
            var product = await _context.Products.Include(p => p.Category)
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == Guid.Parse(productId));
            if (product != null)
            {
                products.Add(mapper.Map<ProductDTO>(product));
            }
        }
    
        return products;
    }
    
    public async Task RemoveProductFromCartAsync(string userId, Guid productId)
    { 
        string cartKey = $"cart:{userId}";
    
        var result = await _db.ListRemoveAsync(cartKey, productId.ToString());
    
        if (result == 0)
        {
            throw new Exception("Product not found in the cart.");
        }
    
        Console.WriteLine($"Product {productId} removed from cart.");
    }
    public async Task ClearCartAsync(string userId)
    {
        string cartKey = $"cart:{userId}";
    
        await _db.KeyDeleteAsync(cartKey);
    
        Console.WriteLine("Cart has been cleared.");
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
                Translations = new List<ProductTranslation>(),
                ProductTags = new List<ProductTag>()
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

            if (productDto.ProductTags != null)
            {
                foreach (var tagDto in productDto.ProductTags)
                {
                    var tag = await _context.ProductTags
                        .FirstOrDefaultAsync(t => t.Name == tagDto.Name, cancellationToken);
                    
                    if (tag == null)
                    {
                        tag = new ProductTag
                        {
                            Id = Guid.NewGuid(),
                            Name = tagDto.Name
                        };
                        _context.ProductTags.Add(tag);
                    }
                    
                    product.ProductTags.Add(tag);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Translations)
                .Include(p => p.ProductTags)
                .FirstAsync(p => p.Id == product.Id, cancellationToken);

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
                return blobClient.Uri.ToString();
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

    public async Task<List<ProductTagDTO>> GetAllProductTagsAsync()
    {
        var tags = await _context.ProductTags
            .OrderBy(t => t.Name) 
            .ToListAsync();
        
        return tags.Select(t => new ProductTagDTO(t.Name)).ToList();
    }
}