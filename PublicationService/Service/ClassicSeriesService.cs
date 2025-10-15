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
    public class ClassicSeriesService: IClassicSeries
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
        public async Task<ClassicSeriesDetailDto> CreateTraditionalSeriesAsync(CreateTraditionalSeriesDto dto, int uploaderId)
        {
            if (string.IsNullOrEmpty(dto.series_title)) throw new InvalidOperationException("Series title is required");
            if (string.IsNullOrEmpty(dto.description)) throw new InvalidOperationException("Description is required");
            if (string.IsNullOrEmpty(dto.ISBN_13)) throw new InvalidOperationException("ISBN_13 is required for Traditional Series");
            if (string.IsNullOrEmpty(dto.author)) throw new InvalidOperationException("Author is required");


            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var ts = new ClassicSeries
                {
                    series_title = dto.series_title,
                    author = dto.author,
                    artist = dto.artist,
                    description = dto.description,
                    cover_images = dto.cover_images,
                    note = dto.note,
                    uploader_id = uploaderId,
                    category_id = dto.category_id ?? throw new ArgumentNullException(nameof(dto.category_id)),
                    status_id = dto.status_id,
                    views = 0,
                    word_count = 0,
                    type = type.TRADITIONAL,

                    // ClassicSeries specific
                    ISBN_10 = dto.ISBN_10,
                    ISBN_13 = dto.ISBN_13,
                    publisher = dto.publisher,
                    publish_date = dto.publish_date,
                    edition = dto.edition
                };

               
                _context.ClassicSeries.Add(ts);
                await _context.SaveChangesAsync();

                // update derived word_count, etc. (nếu cần)
                ts.word_count = 0;
                _context.Novel_Series.Update(ts);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // Gọi GetByIdAsync từ NovelSeriesService đã được tiêm vào
                var createdDto = await _novelSeriesService.GetByIdAsync(ts.series_Id);

                // Ép kiểu an toàn về DTO của ClassicSeries để trả về
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
        public async Task<ClassicSeriesDetailDto?> UpdateClassicSeriesAsync(int seriesId, UpdateClassicSeriesDto dto, int uploaderId)
        {
            var series = await _context.ClassicSeries
                .Include(s => s.NovelTags) 
                .FirstOrDefaultAsync(s => s.series_Id == seriesId);

            if (series == null)
            {
                _logger.LogWarning("ClassicSeries with id {SeriesId} not found.", seriesId);
                return null;
            }

            if (series.uploader_id != uploaderId)
            {
                _logger.LogWarning("User {UploaderId} is not authorized to update series {SeriesId}.", uploaderId, seriesId);
                throw new UnauthorizedAccessException("You are not authorized to update this series.");
            }

            // 1. Cập nhật các thuộc tính chung bằng cách truyền thẳng DTO
            await _novelSeriesService.UpdateSeriesAsync(seriesId, dto, uploaderId);

            // 2. Cập nhật các thuộc tính riêng của ClassicSeries
            series.ISBN_10 = dto.ISBN_10 ?? series.ISBN_10;
            series.ISBN_13 = dto.ISBN_13 ?? series.ISBN_13;
            series.publisher = dto.publisher ?? series.publisher;
            series.publish_date = dto.publish_date ?? series.publish_date;
            series.edition = dto.edition ?? series.edition;

            await _context.SaveChangesAsync();

            var updatedDto = await _novelSeriesService.GetByIdAsync(seriesId);

            return updatedDto as ClassicSeriesDetailDto;
        }




    }
    }

