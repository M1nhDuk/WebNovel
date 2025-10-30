using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Tag;

namespace NovelService.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagsController: ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }


        [HttpGet("{id:int}")]
        [AllowAnonymous] 
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ Admin
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] TagCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdTag = await _tagService.CreateTagAsync(dto);
                return CreatedAtAction(nameof(GetTag), new { id = createdTag.tagId }, createdTag);
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to create tag.");
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> UpdateTag(int id, [FromBody] TagUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _tagService.UpdateTagAsync(id, dto);
                if (!success)
                {
                    return NotFound(new { message = "Tag not found." });
                }
                return NoContent(); 
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to update tag {TagId}.", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                var success = await _tagService.DeleteTagAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Tag not found." });
                }
                return NoContent(); // 204 No Content
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to delete tag {TagId}.", id);
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
