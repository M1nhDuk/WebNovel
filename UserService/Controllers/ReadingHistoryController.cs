using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService.ReadingHistory;
using System.Security.Claims;
using UserService.UserSettingService.Interface;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user/reading-history")]
    [Authorize]
    public class ReadingHistoryController : ControllerBase
    {
        private readonly IReadingHistoryService _readingHistoryService;
        private readonly ILogger<ReadingHistoryController> _logger;

        public ReadingHistoryController(IReadingHistoryService readingHistoryService, ILogger<ReadingHistoryController> logger)
        {
            _readingHistoryService = readingHistoryService;
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


        [HttpGet("series/{seriesId}/chapters")]
        public async Task<IActionResult> GetReadChapters(int seriesId)
        {
            var userId = GetUserIdFromToken();

            if (userId == Guid.Empty) return Unauthorized();

            var chapterIds = await _readingHistoryService.GetReadChapterIdsAsync(userId, seriesId);

            return Ok(chapterIds);
        }

        [HttpPost] 
        public async Task<IActionResult> AddOrUpdateHistory([FromBody] AddReadingHistoryDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _readingHistoryService.AddOrUpdateHistoryAsync(userId, dto);
                return Ok(new { message = "Reading history updated." });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ReadingHistoryDto>>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var history = await _readingHistoryService.GetHistoryAsync(userId, page, pageSize);
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reading history for User {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveHistory([FromBody] RemoveReadingHistoryDto dto)
        {
            if (dto == null || dto.HistoryIds == null || !dto.HistoryIds.Any()) // Kiểm tra HistoryIds
            {
                return BadRequest(new { message = "Provide list of HistoryIds to remove." });
            }

            try
            {
                var userId = GetUserIdFromToken();
                var deletedCount = await _readingHistoryService.RemoveHistoryAsync(userId, dto.HistoryIds); 

                if (deletedCount == 0)
                {
                   
                    return Ok(new { message = "Cannot find any history to remove.", deletedCount });
                }

                return Ok(new
                {
                    message = $"Remove {deletedCount} from reading history.",
                    deletedCount
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lịch sử đọc cho User {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
