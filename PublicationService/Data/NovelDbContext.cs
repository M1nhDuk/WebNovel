using Microsoft.EntityFrameworkCore;
using NovelService.Models;
using static System.Net.Mime.MediaTypeNames;
using System;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NovelService.Data
{
    public class NovelDbContext: DbContext
    {
        public NovelDbContext(DbContextOptions<NovelDbContext> options) : base(options)
        {

        }
        public DbSet<NovelSeries> Novel_Series { get; set; }
        public DbSet<Novel> Novels { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NovelTag> Novel_Tags { get; set; }
        public DbSet<NovelStatus> Novel_Statuses { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ClassicSeries> ClassicSeries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Set defautl value


            // Novel - Chapter : 1 -> many
            modelBuilder.Entity<Novel>()
                .HasMany(n => n.Chapters)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.novelID)
                .OnDelete(DeleteBehavior.Cascade);

            //NovelSeries - Novel: 1 -> many
            modelBuilder.Entity<NovelSeries>()
                .HasMany(n => n.Novel)
                .WithOne(c => c.NovelSeries)
                .HasForeignKey(c => c.series_Id)
                .OnDelete(DeleteBehavior.Cascade);

            //NovelSeries - Chapter: 1 -> many
            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.TS)
                .WithMany(c => c.Chapters)
                .HasForeignKey(c => c.series_Id)
                .OnDelete(DeleteBehavior.Cascade);


            // NovelSeries - Category : many -> one
            modelBuilder.Entity<NovelSeries>()
                .HasOne(n => n.category)
                .WithMany(c => c.NovelSeries)
                .HasForeignKey(c => c.category_id)
                .OnDelete(DeleteBehavior.Restrict);

            // NovelSeries - NovelStatus : many -> one
            modelBuilder.Entity<NovelSeries>()
                .HasOne(n => n.status)
                .WithMany(c => c.NovelSeries)
                .HasForeignKey(n => n.status_id)
                .OnDelete(DeleteBehavior.Restrict);

            // NovelTag (join table) cấu hình
            modelBuilder.Entity<NovelTag>()
                .HasKey(c => c.novelTagId);
            
            modelBuilder.Entity<NovelTag>()
                .HasOne(nt => nt.NovelSeries)
                .WithMany(n => n.NovelTags)
                .HasForeignKey(nt => nt.series_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NovelTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NovelTags)
                .HasForeignKey(nt => nt.tagID)
                .OnDelete(DeleteBehavior.Restrict);

            // Ngăn duplicate (1 novel - 1 tag chỉ 1 lần)
            modelBuilder.Entity<NovelTag>()
                .HasIndex(nt => new { nt.series_Id, nt.tagID })
                .IsUnique();

            modelBuilder.Entity<NovelSeries>()
                .Property(n => n.uploader_id)
                .IsRequired();



            // Map base series
            modelBuilder.Entity<NovelSeries>().ToTable("novel_series");
            modelBuilder.Entity<NovelSeries>().Property(s => s.type).HasConversion<string>();

            // Map derived ClassicSeries -> separate table (TPT)
            modelBuilder.Entity<ClassicSeries>().ToTable("classic_series");

            // Indexes for uniqueness per parent
            modelBuilder.Entity<Chapter>()
                .HasIndex(c => new { c.novelID, c.chapter_number })
                .HasDatabaseName("IX_Chapter_Novel_ChapterNumber")
                .IsUnique();

            modelBuilder.Entity<Chapter>()
                .HasIndex(c => new { c.series_Id, c.chapter_number })
                .HasDatabaseName("IX_Chapter_Series_ChapterNumber")
                .IsUnique();


            //Ràng buộc

            //NovelService
            modelBuilder.Entity<NovelSeries>(entity =>
            {
                entity.HasKey(c => c.series_Id);

                entity.Property(c => c.series_Id);
                
                
                entity.Property(c => c.series_title)
                .IsRequired()
                .HasMaxLength(250);

                entity.Property<string>(c => c.description)
               .IsRequired();

                entity.Property(c => c.updated_at)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.created_at)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.cover_images)
                    .HasDefaultValue("/images/covers/default_cover.jpg");

            });

            //Novel
            modelBuilder.Entity<Novel>(entity =>
            {
                entity.HasKey(c => c.novel_Id);
                

                entity.Property(c => c.novel_Id)
                .ValueGeneratedOnAdd();

                entity.Property(c => c.title)
                .IsRequired()
                .HasMaxLength(250);

                entity.HasIndex(c => new { c.series_Id, c.novel_number })
                   .IsUnique()
                   .HasDatabaseName("IX_NovelSeries_Novel_ChapterNumber");

                entity.Property(c => c.updated_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.cover_images)
                    .HasDefaultValue("/images/covers/default_cover.jpg");
            });


            //Chapter
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasKey(c => c.chapter_id);

             
                entity.Property(c => c.chapter_id)
                .ValueGeneratedOnAdd();
               


                entity.Property(c => c.title)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(c => c.content)
                .IsRequired(); ;

                entity.HasIndex(c => new { c.novelID, c.chapter_number })
                .IsUnique()
                .HasDatabaseName("IX_Chapter_Novel_ChapterNumber");

                entity.HasIndex(c => new { c.series_Id, c.chapter_number })
                .HasDatabaseName("IX_Chapter_Series_ChapterNumber")
                .IsUnique();

                entity.Property(c => c.created_at)
                 .HasColumnType("datetime(6)")
                 .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                 .ValueGeneratedOnAdd()
                 .IsRequired();

                

            });

            //Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.category_id);
                

                entity.Property(c => c.category_id)
                .ValueGeneratedOnAdd();

                entity.Property(c => c.category_name)
               .IsRequired(); 
            });

            //NovelStatus
            modelBuilder.Entity<NovelStatus>(entity =>
            {
                entity.HasKey(ns => ns.statusId);

                entity.Property(ns => ns.statusId)
                .ValueGeneratedOnAdd();

                entity.Property(ns => ns.statusName)
                .IsRequired();

            });


            //Tags
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.tagId);

                entity.Property(t => t.tagId)
                .ValueGeneratedOnAdd();

                entity.Property(t => t.tagName)
                .IsRequired();

            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateChapterParents();
            ValidateNovelSeriesRules();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ValidateChapterParents()
        {
            var entries = ChangeTracker.Entries<Chapter>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var c in entries)
            {
                var hasNovel = c.novelID.HasValue;
                var hasSeries = c.series_Id.HasValue;
                if (hasNovel == hasSeries)
                    throw new InvalidOperationException("Chapter must have exactly one parent: novelID xor series_Id.");

                if (hasSeries)
                {
                    var series = this.Novel_Series.Find(c.series_Id.Value);
                    if (series != null && series.type != type.TRADITIONAL)
                        throw new InvalidOperationException("Cannot add a chapter directly to a non-TRADITIONAL series.");
                }

                if (hasNovel)
                {
                    var novel = this.Novels.Find(c.novelID.Value);
                    if (novel != null && novel.series_Id.HasValue)
                    {
                        var parentSeries = this.Novel_Series.Find(novel.series_Id.Value);
                        if (parentSeries != null && parentSeries.type == type.TRADITIONAL)
                            throw new InvalidOperationException("Cannot add chapter to a Novel that belongs to a TRADITIONAL series.");
                    }
                }
            }
        }

        private void ValidateNovelSeriesRules()
        {
            var novelEntries = ChangeTracker.Entries<Novel>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var n in novelEntries)
            {
                if (n.series_Id.HasValue)
                {
                    var series = this.Novel_Series.Find(n.series_Id.Value);
                    if (series != null && series.type == type.TRADITIONAL)
                        throw new InvalidOperationException("Cannot create or attach a Novel under a TRADITIONAL series.");
                }
            }
        }



    }


}
