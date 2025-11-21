using InteractionService.Models;
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
        public DbSet<UserBookmark> UserBookmarks { get; set; }

        public DbSet<ReadingHistory> ReadingHistories { get; set; }

        public DbSet<UserReadChapter> UserReadChapter { get; set; }

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


            modelBuilder.Entity<UserBookmark>(entity =>
            {
                entity.HasKey(b => b.BookmarkId);
              
                entity.HasIndex(b => new { b.UserId, b.ChapterId }).IsUnique();       
              
                entity.HasIndex(b => new { b.UserId, b.CreatedAt });
                entity.HasIndex(b => new { b.UserId, b.SeriesId, b.ChapterId });
            });


            modelBuilder.Entity<ReadingHistory>(entity =>
            {
                entity.HasKey(rh => rh.HistoryId);

                entity.HasIndex(rh => new { rh.UserId, rh.LastAccessedAt });

                entity.HasIndex(e => new { e.UserId, e.SeriesId })
                      .IsUnique()
                      .HasDatabaseName("IX_ReadingHistory_User_Series_Unique");
            });

            modelBuilder.Entity<UserReadChapter>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.UserId, e.SeriesId, e.ChapterId })
                      .IsUnique()
                      .HasDatabaseName("IX_UserReadChapter_User_Series_Chapter_Unique");
            });
        }
    }
}
