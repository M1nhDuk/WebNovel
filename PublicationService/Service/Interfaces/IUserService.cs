using Shareds.DTOs.UserService;

namespace NovelService.Service.Interfaces
{
    public interface IUserService
    {
        Task NotifySeriesGeneralAsync(SeriesGeneralNotificationDto dto);

        Task IncrementUnreadCountAsync(int seriesId);
    }
}