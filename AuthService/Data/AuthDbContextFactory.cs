using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AuthService.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Tạo builder để đọc cấu hình từ appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Lấy chuỗi kết nối từ file cấu hình
        var connectionString = configuration.GetConnectionString("MySqlConnection");

        // Tạo options cho DbContext
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AuthDbContext(optionsBuilder.Options);
    }
}