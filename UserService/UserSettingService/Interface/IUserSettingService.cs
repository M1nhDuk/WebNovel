using Shareds.DTOs.UserService;

namespace UserService.Services.Interfaces
{
    public interface IUserSettingService
    {
        
        Task<UserSettingDto> GetUserSettingsAsync(Guid userId);

     
        Task<UserSettingDto> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingDto dto);
    }
}