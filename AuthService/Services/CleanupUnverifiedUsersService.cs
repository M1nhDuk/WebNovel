using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class CleanupUnverifiedUsersService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupUnverifiedUsersService> _logger;
        public CleanupUnverifiedUsersService(IServiceProvider serviceProvider, ILogger<CleanupUnverifiedUsersService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    _logger.LogInformation("Clean up service is running");

                    //Chờ 24h cho lần chạy tiếp theo
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                    // Tạo một scope mới để lấy DbContext
                    using (var scop = _serviceProvider.CreateScope())
                    {
                        var dbContext = scop.ServiceProvider.GetRequiredService<AuthDbContext>();

                        // Đặt ra ngưỡng thời gian, ví dụ: xóa các tài khoản chưa xác thực cũ hơn 24 giờ
                        var timeThreshold = DateTime.UtcNow.AddHours(-24);

                        //tìm user quá hạn để đăng kí(chưa xác minh)
                        var usersToDelete = await dbContext.Users
                            .Where(u => !u.IsEmailConfirmed && u.Created_At < timeThreshold)
                            .ToListAsync(stoppingToken);

                        if (usersToDelete.Any())
                        {
                            dbContext.Users.RemoveRange(usersToDelete);
                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Successfully deleted {usersToDelete.Count} unverified users.");
                        }
                        else
                        {
                            _logger.LogInformation("No unverified users to delete.");
                        }
                    }
                } catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the Cleanup Service.");
                }
            }
        }

    }
}
