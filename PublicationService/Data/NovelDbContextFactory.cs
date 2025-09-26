using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NovelService.Data
{
    public class NovelDbContextFactory : IDesignTimeDbContextFactory<NovelDbContext>
    {
        public NovelDbContext CreateDbContext(string[] args)
        {
            // Tạo builder để đọc appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Lấy path hiện tại
                .AddJsonFile("appsettings.json") // Đọc file cấu hình
                .Build();

            // Lấy connection string từ appsettings.json
            var connectionString = configuration.GetConnectionString("MySqlConnection");

            // Tạo options cho DbContext
            var optionsBuilder = new DbContextOptionsBuilder<NovelDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new NovelDbContext(optionsBuilder.Options);
        }
    }
}
