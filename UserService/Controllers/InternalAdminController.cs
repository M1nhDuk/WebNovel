using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/internal/admin")]
    [Authorize(Roles = "Admin")]
    public class InternalAdminController: ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly ILogger<InternalAdminController> _logger;

        public InternalAdminController(UserDbContext context, ILogger<InternalAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpDelete("users/{id:guid}")]
        public async Task <IActionResult> DeleteUserData (Guid id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            {
                try
                {
                    await _context.UserSettings
                        .Where(s => s.UserId == id)
                        .ExecuteDeleteAsync(); 

                    await _context.UserFavorite
                        .Where(f => f.UserId == id)
                        .ExecuteDeleteAsync();

                    await _context.UserBookmarks
                        .Where(b => b.UserId == id)
                        .ExecuteDeleteAsync();

                    await _context.Notification
                        .Where(n => n.UserId == id)
                        .ExecuteDeleteAsync();

                    await _context.ReadingHistories
                        .Where(r => r.UserId == id)
                        .ExecuteDeleteAsync();

                    await _context.UserReadChapter
                        .Where(r => r.UserId == id)
                        .ExecuteDeleteAsync();

                    await transaction.CommitAsync();

                    return NoContent();
                } catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to delete user data for {UserId}", id);
                    return StatusCode(500, "Lỗi máy chủ nội bộ khi xóa dữ liệu người dùng.");
                }
            }
        }
    }
}
