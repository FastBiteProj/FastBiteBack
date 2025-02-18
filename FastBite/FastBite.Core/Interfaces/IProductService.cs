using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Http;

namespace FastBite.Core.Interfaces;

public interface IProductService
{
    Task<List<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO> AddNewProductAsync(ProductDTO product, CancellationToken cancellationToken);
    Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken);
    Task<PostResponse> DeleteProductAsync(string productName);
    Task<PostResponse> UpdateProductAsync(string productName, ProductDTO updatedProductDto);
}