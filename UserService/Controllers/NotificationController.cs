using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using UserService.Services.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var notifications = await _notificationService.GetNotificationsAsync(userId, page, pageSize);
                return Ok(notifications);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(count);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok("All notifications marked as read.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsReadAsync(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var success = await _notificationService.MarkAsReadAsync(userId, id);
                if (!success)
                {
                    return NotFound("Notification not found or already read.");
                }
                return Ok(success);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
