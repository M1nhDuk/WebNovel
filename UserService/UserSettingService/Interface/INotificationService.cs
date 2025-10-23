// In UserService/Services/Interface/INotificationService.cs
using Shareds.DTOs.UserService;

namespace UserService.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}