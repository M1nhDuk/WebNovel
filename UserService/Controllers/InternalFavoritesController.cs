using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/internal/favorites")]
    public class InternalFavoritesController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly ILogger<InternalFavoritesController> _logger;

        public InternalFavoritesController(UserDbContext context, ILogger<InternalFavoritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{seriesId}/followers")]
        public async Task<ActionResult<List<Guid>>> GetFollower(int seriesId)
        {
            try
            {
                var follower = await _context.UserFavorite
                    .Where(f => f.seriesId == seriesId)
                    .Select(f => f.UserId)
                    .Distinct()
                    .ToListAsync();
                return Ok(follower);
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Erro when retrive SeriesId: {SeriesId}", seriesId);
                return StatusCode(500, "Erro");
            }
        }

        [HttpPost("increment-unread/{seriesId}")]
        public async Task<IActionResult> IncrementUnreadCount(int seriesId)
        {
            try
            {
                await _context.UserFavorite
                    .Where(f => f.seriesId == seriesId)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        f => f.UnreadCount,    
                        f => f.UnreadCount + 1  
                    ));

                _logger.LogInformation("Incremented UnreadCount for SeriesId: {SeriesId}", seriesId);
                return Ok(new { message = "Notification counts updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing UnreadCount for SeriesId: {SeriesId}", seriesId);
                return StatusCode(500, "Error updating notifications");
            }
        }
    }
}
