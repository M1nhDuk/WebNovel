using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using UserService.UserSettingService;
using UserService.UserSettingService.Interface;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user/bookmarks")]
    [Authorize]
    public class BookmarksController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly ILogger<BookmarksController> _logger;


        public BookmarksController(IBookmarkService bookmarkService, ILogger<BookmarksController> logger)
        {
            _bookmarkService = bookmarkService;
            _logger = logger;
        }

        private Guid GetUserIdFromToken()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }



        [HttpPost("toggle")] 
        public async Task<ActionResult<BookmarkToggleResultDto>> ToggleBookmark([FromBody] ToggleBookmarkDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _bookmarkService.ToggleBookmarkAsync(userId, dto);
             
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex) 
            {
                _logger.LogWarning("ToggleBookmark failed validation: {ErrorMessage}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling bookmark for User {UserId}, Chapter {ChapterId}", GetUserIdFromToken(), dto.ChapterId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("chapter/{chapterId:int}")] 
        public async Task<IActionResult> RemoveBookmarkForChapter(int chapterId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var success = await _bookmarkService.RemoveBookmarkForChapterAsync(userId, chapterId);
                if (!success)
                {
                    return NotFound(new { message = "No bookmark found for this chapter to remove." });
                }
                return NoContent(); 
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark for Chapter {ChapterId} for User {UserId}", chapterId, GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<BookmarkDto>>> GetMyBookmarks()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var bookmarks = await _bookmarkService.GetGroupedBookmarksByUserAsync(userId);
                return Ok(bookmarks); 
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmarks for User {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

    
        [HttpGet("{bookmarkId:guid}", Name = "GetBookmarkById")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetBookmarkById(Guid bookmarkId)
        {
            return NotFound();
        }
    }
}
