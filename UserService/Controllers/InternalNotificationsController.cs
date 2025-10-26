using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.UserService;
using UserService.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/internal/notifications")]
    public class InternalNotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<InternalNotificationsController> _logger;

        public InternalNotificationsController(INotificationService notificationService, ILogger<InternalNotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var result = await _notificationService.CreateNotificationAsync(dto);
                return CreatedAtAction(nameof(CreateNotification), new { id = result.NotificationId }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Không thể tạo thông báo do loại không hợp lệ: {Type}", dto.Type);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thông báo nội bộ cho User {UserId}", dto.UserId);
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }
    }
}
