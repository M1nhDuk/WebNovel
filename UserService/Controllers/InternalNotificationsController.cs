using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.UserService;
using UserService.UserSettingService.Interface; // Namespace chuẩn
using System;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Services.Interfaces; 

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

        // 1. API GỬI HÀNG LOẠT (Cho PublicationService: Update/Delete Series)
        [HttpPost("series-general")]
        public async Task<IActionResult> NotifySeriesGeneral([FromBody] SeriesGeneralNotificationDto dto)
        {

            if (!Enum.TryParse<NotificationType>(dto.Type, true, out var notifType))
            {
                return BadRequest($"Invalid Notification Type: {dto.Type}");
            }

            await _notificationService.NotifySeriesFollowersAsync(
                dto.SeriesId,
                dto.Message,
                notifType,
                dto.LinkUrl
            );

            return Ok(new { message = "Notification have been sent." });
        }


        //API gửi đơn lẻ cho user
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] CreateNotificationDto dto)
        {
            await _notificationService.CreateNotificationAsync(dto);
            return Ok(new { message = "Email have sent to user ." });
        }
    }
}