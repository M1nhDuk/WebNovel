
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService;
using UserService.Models;

namespace UserService.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
        Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<int> RemoveNotificationsAsync(Guid userId, List<Guid> notificationIds);

        Task<UnreadSummaryDto> GetUnreadSummaryAsync(Guid userId);

        Task MarkAllByTypeAsReadAsync(Guid userId, NotificationType type);
    }
}