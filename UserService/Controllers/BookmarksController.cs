using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.NovelSeries;
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
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BookmarkDto>>> GetMyBookmarks(
             [FromQuery] int page = 1,
             [FromQuery] int pageSize = 10) 
        {
            try
            {
                var userId = GetUserIdFromToken();
                var bookmarks = await _bookmarkService.GetGroupedBookmarksByUserAsync(userId, page, pageSize);
                return Ok(bookmarks); 
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("chapter/{chapterId:int}")] 
        public async Task<ActionResult<BookmarkDto>> GetBookmarkForChapter(int chapterId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var bookmark = await _bookmarkService.GetBookmarkForChapterAsync(userId, chapterId);

                if (bookmark == null)
                {
                    return NotFound(); 
                }

                return Ok(bookmark);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmark for Chapter {ChapterId}, User {UserId}", chapterId, GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("{bookmarkId:guid}", Name = "GetBookmarkById")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetBookmarkById(Guid bookmarkId)
        {
            return NotFound();
        }




        [HttpDelete("{bookmarkId:guid}")] 
        public async Task<IActionResult> RemoveBookmarkById(Guid bookmarkId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var success = await _bookmarkService.RemoveBookmarkByIdAsync(userId, bookmarkId);
                if (!success)
                {
                    return NotFound(new { message = "Bookmark not found or you are not authorized to delete it." });
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
