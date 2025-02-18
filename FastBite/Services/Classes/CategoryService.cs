using AutoMapper;
using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Data.Models;
using FastBite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FastBite.Data.Configs;

namespace FastBite.Services.Classes;

public class CategoryService : ICategoryService
{
    private readonly FastBiteContext _context;
    private readonly IMapper _mapper;

    public CategoryService(FastBiteContext context)
    {
        _context = context;
        _mapper = MappingConfiguration.InitializeConfig();
    }

    public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
        return _mapper.Map<List<CategoryDTO>>(categories);
    }

    public async Task<CategoryDTO> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        return _mapper.Map<CategoryDTO>(category);
    }

    public async Task<CategoryDTO> CreateCategoryAsync(CategoryDTO categoryDto)
    {
        var category = _mapper.Map<Category>(categoryDto);
        
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDTO>(category);
    }

    public async Task<CategoryDTO> UpdateCategoryAsync(Guid id, CategoryDTO categoryDto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        category.Name = categoryDto.Name;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDTO>(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
} 