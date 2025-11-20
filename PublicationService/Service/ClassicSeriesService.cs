using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.Novel;
using Shareds.DTOs.NovelSeries;


namespace NovelService.Service
{
    public class ClassicSeriesService : IClassicSeries
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<ClassicSeriesService> _logger;
        private readonly INovelSeriesService _novelSeriesService;

        public ClassicSeriesService(NovelDbContext context, INovelSeriesService novelSeriesService, ILogger<ClassicSeriesService> logger)
        {
            _context = context;
            _novelSeriesService = novelSeriesService;
            _logger = logger;
        }

        // TẠO TRADITIONAL SERIES (TS)
        public async Task<ClassicSeriesDetailDto> CreateTraditionalSeriesAsync(CreateTraditionalSeriesDto dto, Guid uploaderId)
        {
            if (string.IsNullOrEmpty(dto.series_title)) throw new InvalidOperationException("Series title is required");
            if (string.IsNullOrEmpty(dto.description)) throw new InvalidOperationException("Description is required");
            if (string.IsNullOrEmpty(dto.iSBN_13)) throw new InvalidOperationException("iSBN_13 is required for Traditional Series");
            if (string.IsNullOrEmpty(dto.author)) throw new InvalidOperationException("Author is required");


            if (dto.iSBN_13.Length != 13 || (!dto.iSBN_13.StartsWith("978") && !dto.iSBN_13.StartsWith("979")))
            {
                throw new InvalidOperationException("ISBN-13 must be 13 digits and start with 978 or 979.");
            }

            if (!string.IsNullOrEmpty(dto.iSBN_10) && dto.iSBN_10.Length != 10)
            {
                throw new InvalidOperationException("ISBN-10 must be 10 characters.");
            }

            bool isbnExists = await _context.ClassicSeries.AnyAsync(
                s => (s.iSBN_13 == dto.iSBN_13) ||
                     (dto.iSBN_10 != null && s.iSBN_10 == dto.iSBN_10)
            );

            if (isbnExists)
            {
                throw new InvalidOperationException("ISBN-13 or ISBN-10 already exists for another series.");
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var ts = new ClassicSeries
                {
                    series_title = dto.series_title,
                    author = dto.author,
                    artist = dto.artist,
                    description = dto.description,
                    cover_images = string.IsNullOrEmpty(dto.cover_images) ? "/images/covers/default_cover.jpg" : dto.cover_images,
                    note = dto.note,
                    uploader_id = uploaderId,
                    category_id = dto.category_id ?? throw new ArgumentNullException(nameof(dto.category_id)),
                    status_id = dto.status_id,
                    views = 0,
                    word_count = 0,
                    type = type.TRADITIONAL,

                    // ClassicSeries specific
                    iSBN_10 = dto.iSBN_10,
                    iSBN_13 = dto.iSBN_13,
                    publisher = dto.publisher,
                    publish_date = dto.publish_date,
                    edition = dto.edition
                };

                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    foreach (var tagId in dto.TagIds.Distinct())
                    {
                        ts.NovelTags.Add(new NovelTag
                        {
                            tagID = tagId
                        });
                    }
                }


                _context.ClassicSeries.Add(ts);
                await _context.SaveChangesAsync();

                ts.word_count = 0;
                _context.Novel_Series.Update(ts);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                var createdDto = await _novelSeriesService.GetByIdAsync(ts.series_Id);

                return createdDto as ClassicSeriesDetailDto;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Create Traditional Series failed");
                throw;
            }
        }

        //Update
        public async Task<ClassicSeriesDetailDto?> UpdateClassicSeriesAsync(int seriesId, UpdateClassicSeriesDto dto, Guid uploaderId, string userRole)
        {
            var series = await _context.ClassicSeries
                .Include(s => s.NovelTags)
                .FirstOrDefaultAsync(s => s.series_Id == seriesId);

            if (series == null)
            {
                _logger.LogWarning("ClassicSeries with id {SeriesId} not found.", seriesId);
                return null;
            }

            if (series.uploader_id != uploaderId && userRole != "Admin") 
            {
                throw new UnauthorizedAccessException("You are not authorized to update this series.");
            }


            if (dto.iSBN_13 != null && dto.iSBN_13 != series.iSBN_13)
            {
                if (dto.iSBN_13.Length != 13 || (!dto.iSBN_13.StartsWith("978") && !dto.iSBN_13.StartsWith("979")))
                {
                    throw new InvalidOperationException("ISBN-13 must be 13 digits and start with 978 or 979.");
                }

                //Unique Check
                bool isbn13Exists = await _context.ClassicSeries.AnyAsync(
                    s => s.series_Id != seriesId && s.iSBN_13 == dto.iSBN_13
                );
                if (isbn13Exists)
                {
                    throw new InvalidOperationException("ISBN-13 already exists for another series.");
                }
                series.iSBN_13 = dto.iSBN_13; 
            }

  
            if (dto.iSBN_10 != null && dto.iSBN_10 != series.iSBN_10)
            {

                if (dto.iSBN_10.Length != 10)
                {
                    throw new InvalidOperationException("ISBN-10 must be 10 characters.");
                }

                bool isbn10Exists = await _context.ClassicSeries.AnyAsync(
                    s => s.series_Id != seriesId && s.iSBN_10 == dto.iSBN_10
                );
                if (isbn10Exists)
                {
                    throw new InvalidOperationException("ISBN-10 already exists for another series.");
                }
                series.iSBN_10 = dto.iSBN_10;
            }


            await _novelSeriesService.UpdateSeriesAsync(seriesId, dto, uploaderId, userRole);

            //Cập nhật các thuộc tính riêng của ClassicSeries
            series.publisher = dto.publisher ?? series.publisher;
            series.publish_date = dto.publish_date ?? series.publish_date;
            series.edition = dto.edition ?? series.edition;

            await _context.SaveChangesAsync();

            var updatedDto = await _novelSeriesService.GetByIdAsync(seriesId);

            return updatedDto as ClassicSeriesDetailDto;
        }
    }
}

