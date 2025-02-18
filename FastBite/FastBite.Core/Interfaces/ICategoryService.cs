using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDTO>> GetAllCategoriesAsync();
    Task<CategoryDTO> GetCategoryByIdAsync(Guid id);
    Task<CategoryDTO> CreateCategoryAsync(CategoryDTO categoryDto);
    Task<CategoryDTO> UpdateCategoryAsync(Guid id, CategoryDTO categoryDto);
    Task DeleteCategoryAsync(Guid id);
} 