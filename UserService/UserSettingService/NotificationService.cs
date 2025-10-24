using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.UserService;
using UserService.Data;
using UserService.Models;
using UserService.Services.Interfaces;

namespace UserService.UserSettingService
{
    public class NotificationService : INotificationService
    {
        public readonly UserDbContext _context;
        public readonly ILogger<NotificationService> _logger;

        public NotificationService(UserDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            if (!Enum.TryParse<NotificationType>(dto.Type, true, out var notificationType))
            {
                throw new ArgumentException("Invalid NotificationType", nameof(dto.Type));
            }

            var notification = new Notification
            {
                UserId = dto.UserId,
                Type = notificationType,
                Message = dto.Message,
                LinkUrl = dto.LinkUrl,
                CreatedDate = DateTime.UtcNow,
                IsRead = false,
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created notification {NotificationId} for User {UserId}", notification.NotificationsId, notification.UserId);

            return MapToDto(notification);
        }

        //get all
        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var notification = await _context.Notification.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => MapToDto(n))
                .ToListAsync();

            return notification;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var notification = await _context.Notification.CountAsync(n => n.UserId == userId && !n.IsRead);

            return notification;
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notification.FirstOrDefaultAsync(n => n.NotificationsId == notificationId && n.UserId == userId);

            if (notification == null || notification.IsRead)
            {
                return false;
            }

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var unReadNotification = await _context.Notification.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();

            if (!unReadNotification.Any())
            {
                return false;
            }

            foreach (var notification in unReadNotification)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked all notifications as read for User {UserId}", userId);
            return true;
        }

        public async Task<int> RemoveNotificationsAsync(Guid userId, List<Guid> notificationIds)
        {

            if (notificationIds == null || !notificationIds.Any())
            {
                return 0; 
            }

            // Tìm tất cả thông báo của user này
            // VÀ có NotificationId nằm trong danh sách được cung cấp
            var notificationsToDelete = await _context.Notification
                .Where(n => n.UserId == userId && notificationIds.Contains(n.NotificationsId))
                .ToListAsync();

            if (!notificationsToDelete.Any())
            {
                _logger.LogWarning("User {UserId} not found any notify to clear", userId);
                return 0; 
            }

            
            _context.Notification.RemoveRange(notificationsToDelete);

          
            var deletedCount = await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} delete notify {Count} successfully.", userId, deletedCount);

            return deletedCount; 
        }

        private NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto
            {
                NotificationId = n.NotificationsId,
                Type = n.Type.ToString(),
                Message = n.Message,
                LinkUrl = n.LinkUrl,
                CreatedAt = n.CreatedDate,
                IsRead = n.IsRead
            };
        }

    }
}
