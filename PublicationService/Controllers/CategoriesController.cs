using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelService.Service;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Category;

namespace NovelService.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController: ControllerBase
    {
        private readonly ICategory _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategory categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }



        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Not found category" });
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.category_id }, createdCategory);
            }
            catch (InvalidOperationException ex) 
            {

                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro");
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(id, dto);
                if (!success)
                {
                    return NotFound(new { message = "Not found category." });
                }
                return NoContent();
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to update category {CategoryId}.", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}.", id);
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }



        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Not found category" });
                }
                return NoContent(); 
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to delete category {CategoryId}.", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro");
            }
        }
    }
}
