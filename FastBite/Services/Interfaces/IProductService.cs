using FastBite.Data.DTOS;
using FastBite.Data.Models;

namespace FastBite.Services.Interfaces;

public interface IProductService {
    public Task<List<ProductDTO>> GetAllProductsAsync();
    public Task<ProductDTO> AddNewProductAsync(ProductDTO product, CancellationToken cancellationToken);
    public Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken);
    public Task<PostResponse> DeleteProductAsync(string productName);
    public Task<PostResponse> UpdateProductAsync(string productName, ProductDTO updatedProductDto);
}