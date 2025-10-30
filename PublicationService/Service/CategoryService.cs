using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Category;

namespace NovelService.Service
{
    public class CategoryService: ICategory
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(NovelDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                category_id = category.category_id,
                category_name = category.category_name
            };
        }


        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto)
        {
            bool nameExists = await _context.Categories.AnyAsync(c => c.category_name == dto.category_name);
            if (nameExists)
            {
                throw new InvalidOperationException("Name exist.");
            }

            var newCategory = new Category
            {
                category_name = dto.category_name
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return MapToDto(newCategory);
        }



        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.category_id == id);

            if (category == null)
            {
                return null;
            }

            return MapToDto(category);
        }

        public async Task<bool> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return false;
            }

            if (category.category_name != dto.category_name)
            {
                bool nameExists = await _context.Categories.AnyAsync(c => c.category_name == dto.category_name && c.category_id != id);
                if (nameExists)
                {
                    throw new InvalidOperationException("Category name exists.");
                }
            }

            category.category_name = dto.category_name;

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency exception updating category {CategoryId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            bool isInUse = await _context.Novel_Series.AnyAsync(s => s.category_id == id);
            if (isInUse)
            {
                throw new InvalidOperationException("In in user in series");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
