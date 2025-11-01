using InteractionService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InteractionService.Controllers
{
    [ApiController]
    [Route("api/internal/admin")]
    [Authorize(Roles = "Admin")]
    public class InternalAdminController: ControllerBase
    {
        private readonly InteracDbContext _context;
        private readonly ILogger<InternalAdminController> _logger;

        public InternalAdminController(InteracDbContext context, ILogger<InternalAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpDelete("users/{id:guid}")]
        public async Task<IActionResult> DeleteUserComment(Guid id)
        {
            try
            {
                await _context.Comments
                        .Where(c => c.UserId == id)
                        .ExecuteDeleteAsync();

                return NoContent();
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user user comment");
                return StatusCode(500, "Lỗi máy chủ nội bộ khi xóa dữ liệu người dùng.");
            }
        }
    }
}
