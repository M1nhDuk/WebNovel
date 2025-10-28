using InteractionService.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractionService.Data
{
    public class InteracDbContext: DbContext
    {
        public InteracDbContext(DbContextOptions<InteracDbContext> options) : base(options)
        {

        }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comment>(entity =>
            {
     
                entity.HasIndex(c => c.SeriesId);
                entity.HasIndex(c => c.ChapterId);
                entity.HasIndex(c => c.ParentCommentId);

                entity.HasOne(c => c.ParentComment)
                      .WithMany(p => p.Replies)
                      .HasForeignKey(c => c.ParentCommentId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

           
        }
    }
}
