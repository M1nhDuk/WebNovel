using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext: DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
            
        }

        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<UserFavorite> UserFavorite { get; set; }
        public DbSet<Notification> Notification { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserSetting>()
                .HasKey(us => us.UserId);

            modelBuilder.Entity<UserFavorite>()
                .HasIndex(f => new { f.UserId, f.seriesId })
                .IsUnique();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId);
        }
    }
}
