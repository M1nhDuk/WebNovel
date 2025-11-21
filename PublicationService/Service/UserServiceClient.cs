using NovelService.Service.Interfaces;
using Shareds.DTOs.UserService;
using System.Net.Http.Json;

namespace NovelService.Service
{
    public class UserServiceClient : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task NotifySeriesGeneralAsync(SeriesGeneralNotificationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/internal/notifications/series-general", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send general notification. Status: {Status}. Error: {Error}", response.StatusCode, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to UserService for general notification.");
            }
        }

        public async Task IncrementUnreadCountAsync(int seriesId)
        {
            try
            {             
                var response = await _httpClient.PostAsync($"/api/internal/favorites/increment-unread/{seriesId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to increment unread count for Series {seriesId}. Status: {response.StatusCode}, Error: {error}");
                }
                else
                {
                    _logger.LogInformation($"Successfully incremented unread count for Series {seriesId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling UserService to increment unread count for Series {seriesId}");
                
            }
        }
    }
}