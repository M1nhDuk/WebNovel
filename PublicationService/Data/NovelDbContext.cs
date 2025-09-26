using Microsoft.EntityFrameworkCore;
using NovelService.Models;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace NovelService.Data
{
    public class NovelDbContext: DbContext
    {
        public NovelDbContext(DbContextOptions<NovelDbContext> options) : base(options)
        {

        }

        public DbSet<Novel> Novels { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NovelTag> Novel_Tags { get; set; }
        public DbSet<NovelStatus> Novel_Statuses { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Relationship

            // Novel - Chapter : 1 -> many
            modelBuilder.Entity<Novel>()
                .HasMany(n => n.Chapters)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.novelID)
                .OnDelete(DeleteBehavior.Cascade);

            // Novel - Category : many -> one
            modelBuilder.Entity<Novel>()
                .HasOne(n => n.category)
                .WithMany(c => c.Novels)
                .HasForeignKey(c => c.category_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Novel - NovelStatus : many -> one
            modelBuilder.Entity<Novel>()
                .HasOne(n => n.status)
                .WithMany(c => c.Novels)
                .HasForeignKey(n => n.status_id)
                .OnDelete(DeleteBehavior.Restrict);

            // NovelTag (join table) cấu hình
            modelBuilder.Entity<NovelTag>()
                .HasKey(c => c.novelTagId);
            modelBuilder.Entity<NovelTag>()
                .HasOne(nt => nt.Novel)
                .WithMany(n => n.NovelTags)
                .HasForeignKey(nt => nt.novelID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<NovelTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NovelTags)
                .HasForeignKey(nt => nt.tagID)
                .OnDelete(DeleteBehavior.Restrict);

            // Ngăn duplicate (1 novel - 1 tag chỉ 1 lần)
            modelBuilder.Entity<NovelTag>()
                .HasIndex(nt => new { nt.novelID, nt.tagID })
                .IsUnique();

            modelBuilder.Entity<Novel>()
                .Property(n => n.uploader_id)
                .IsRequired();

            //Ràng buộc

            //Novel
            modelBuilder.Entity<Novel>(entity =>
            {
                entity.HasKey(c => c.novel_Id);
                

                entity.Property(c => c.novel_Id)
                .ValueGeneratedOnAdd();

                entity.Property(c => c.title)
                .IsRequired()
                .HasMaxLength(250);

                entity.Property<string>(c => c.description)
                .IsRequired();

                entity.Property(c => c.created_at)
                 .HasColumnType("datetime(6)")
                 .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                 .ValueGeneratedOnAdd()
                 .IsRequired();

                entity.Property(c => c.updated_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.cover_images)
                    .HasDefaultValue("/images/cover/default_cover.jpg");
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

            
    }


}
