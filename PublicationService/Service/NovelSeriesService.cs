using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.Novel;
using Shareds.DTOs.Chapter;

using Shareds.DTOs;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.UserService;


using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;


namespace NovelService.Service
{
    public class NovelSeriesService : INovelSeriesService
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<NovelSeriesService> _logger;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _userServiceUrl;

        public NovelSeriesService(
            NovelDbContext context, 
            ILogger<NovelSeriesService> logger, 
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userServiceUrl = configuration["ServiceUrls:UserService"] ??
                              throw new InvalidOperationException("ServiceUrls:UserService không được cấu hình");
        }


        //Create
        public async Task<NovelSeriesDetailDto> CreateSeriesAsync(CreateSeriesDto dto, Guid uploaderId)
        {
            if (string.IsNullOrEmpty(dto.series_title)) throw  new InvalidOperationException("Series title is required");

            if (string.IsNullOrEmpty(dto.description)) throw new InvalidOperationException("Description is required");


            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var series = new NovelSeries
                {
                    series_title = dto.series_title,
                    author = dto.author,
                    artist = dto.artist,
                    description = dto.description,
                    cover_images = dto.cover_images,
                    type = type.Series,
                    note = dto.note,
                    uploader_id = uploaderId,                         
                    category_id = dto.category_id ?? throw new ArgumentNullException(nameof(dto.category_id)),
                    status_id = dto.status_id,
                    views = 0,
                    word_count = 0
                };

                _context.Novel_Series.Add(series);
                await _context.SaveChangesAsync(); 

                int totalSeriesWordCount = 0;
                series.word_count = totalSeriesWordCount;
                _context.Novel_Series.Update(series);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                return await GetByIdAsync(series.series_Id)
                       ?? throw new InvalidOperationException("Failed to return created series");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "CreateSeriesAsync failed");
                throw;
            }
        }

       

        //Update
        public async Task<NovelSeriesDetailDto?> UpdateSeriesAsync(int seriesId, UpdateNovelService dto, Guid uploaderId) 
        {
            var series = await _context.Novel_Series.FirstOrDefaultAsync(s => s.series_Id == seriesId);

            if (series == null)
                throw new InvalidOperationException("Series not found");

            if (series.uploader_id != uploaderId)
                throw new UnauthorizedAccessException("You are not allowed to update this series");

            var oldNote = series.note;
            var oldDescription = series.description;


            // Update các field nếu có giá trị
            if (!string.IsNullOrWhiteSpace(dto.series_title))
                series.series_title = dto.series_title;

            if (!string.IsNullOrWhiteSpace(dto.author))
                series.author = dto.author;

            if (!string.IsNullOrWhiteSpace(dto.artist))
                series.artist = dto.artist;

            if (!string.IsNullOrWhiteSpace(dto.description))
                series.description = dto.description;

            if (!string.IsNullOrWhiteSpace(dto.cover_images))
                series.cover_images = dto.cover_images;

            if (!string.IsNullOrWhiteSpace(dto.note))
                series.note = dto.note;

            if (dto.category_id.HasValue)
                series.category_id = dto.category_id.Value;

            if (dto.status_id.HasValue)
                series.status_id = dto.status_id.Value;

            if (dto.TagIds != null)
            {
                var oldTagIds = await _context.Novel_Tags
                    .Where(nt => nt.series_Id == series.series_Id)
                    .Select(nt => nt.tagID)
                    .ToListAsync();

                var newTagIds = dto.TagIds.Distinct().ToList();

                // Xóa tag không còn
                var removeTags = oldTagIds.Except(newTagIds).ToList();
                var toRemove = _context.Novel_Tags
                    .Where(nt => nt.series_Id == series.series_Id && removeTags.Contains(nt.tagID));
                _context.Novel_Tags.RemoveRange(toRemove);

                // Thêm tag mới
                var addTags = newTagIds.Except(oldTagIds).ToList();
                foreach (var tagId in addTags)
                {
                    _context.Novel_Tags.Add(new NovelTag { series_Id = series.series_Id, tagID = tagId });
                }
            }

            series.updated_at = DateTime.UtcNow;

            _context.Novel_Series.Update(series);
            await _context.SaveChangesAsync();

            bool hasAnnouncement = (dto.note != null && dto.note != oldNote) ||
                                     (dto.description != null && dto.description != oldDescription);

            if (hasAnnouncement)
            {
                _logger.LogInformation("Series {SeriesId} có cập nhật. Gửi thông báo cho followers...", seriesId);
                await NotifyFollowersOfUpdate(seriesId, series.series_title, uploaderId);
            }

            return await GetByIdAsync(series.series_Id);
        }


        //Delete
        public async Task<bool> DeleteSeriesById(int id, Guid uploader_Id) // chưa valid quyền quản tri (uploaderID)
        {
            var series = await _context.Novel_Series
                .Include(s => s.Novel)
                    .ThenInclude(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.series_Id == id);

            if (series == null)
                throw new InvalidOperationException("Series not found");

            //kiểm tra quyền
            if (series.uploader_id != uploader_Id)
                throw new UnauthorizedAccessException("You are not allowed to delete this series");


            var notificationDto = new CreateNotificationDto
            {
                UserId = series.uploader_id,
                Type = "SeriesDeleted",
                Message = $"Your series have been deleted '{series.series_title}'.",
                LinkUrl = null 

            };
            await SendNotificationAsync(notificationDto);

            _context.Novel_Series.Remove(series);

            await _context.SaveChangesAsync();
            return true;
        }



        //View(Get) dùng chung cho cả 2 type
        public async Task<NovelSeriesDetailDto?> GetByIdAsync(int id)
        {
            var seriesBase = await _context.Novel_Series.FindAsync(id);


            if (seriesBase == null)
            {
                return null;
            }

            if (seriesBase.type == type.TRADITIONAL)
            {
                var ts = await _context.ClassicSeries
                    .Include(x => x.Chapters)
                    .Include(x => x.status)
                    .Include(x => x.category)
                    .Include(x => x.NovelTags)
                        .ThenInclude(nt => nt.Tag)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.series_Id == id);

                if (ts == null) return null;

                return new ClassicSeriesDetailDto
                {
                    series_Id = ts.series_Id,
                    series_title = ts.series_title,
                    author = ts.author,
                    artist = ts.artist,
                    description = ts.description,
                    cover_images = ts.cover_images,
                    word_count = ts.word_count,
                    views = ts.views,
                    note = ts.note,
                    created_at = ts.created_at,
                    category_id = ts.category_id,
                    categoryName = ts.category?.category_name,
                    status_id = ts.status_id,
                    statusName = ts.status?.statusName,
                    type = "TRADITIONAL",
                    Tags = ts.NovelTags.Select(t => t.Tag.tagName).ToList(),
                    uploader_id = ts.uploader_id,

                    // Các trường riêng của ClassicSeries
                    ISBN_10 = ts.ISBN_10,
                    ISBN_13 = ts.ISBN_13,
                    publisher = ts.publisher,
                    publish_date = ts.publish_date,
                    edition = ts.edition,

                    Chapters = ts.Chapters.OrderBy(c => c.chapter_number).Select(c => new ChapterDetailDto
                    {
                        novelID = c.novelID,
                        chapter_id = c.chapter_id,
                        title = c.title,
                        chapter_number = c.chapter_number,
                        word_count = c.word_count,
                        created_at = c.created_at,
                        content = c.content,
                    }).ToList()
                };
            }
            else
            {
                var s = await _context.Novel_Series
                     .Include(x => x.status)
                    .Include(x => x.category)
                    .Include(x => x.NovelTags)
                        .ThenInclude(nt => nt.Tag)
                    .Include(x => x.Novel) // Include danh sách Novel
                        .ThenInclude(n => n.Chapters) // Include chapter của từng Novel
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.series_Id == id);

                if (s == null) return null;

                return new NovelSeriesDetailDto
                {
                    series_Id = s.series_Id,
                    series_title = s.series_title,
                    author = s.author,
                    artist = s.artist,
                    description = s.description,
                    cover_images = s.cover_images,
                    word_count = s.word_count,
                    views = s.views,
                    note = s.note,
                    created_at = s.created_at,
                    updated_at = s.updated_at,
                    uploader_id = s.uploader_id,
                    type = "Series",

                    category_id = s.category_id,
                    categoryName = s.category?.category_name,
                    status_id = s.status_id,
                    statusName = s.status?.statusName,

                    // tags: chỉ lấy tên
                    Tags = s.NovelTags.Select(t => t.Tag.tagName).ToList(),


                    Novels = s.Novel.OrderBy(n => n.novel_number).Select(n => new NovelDetailDto
                    {
                        series_Id = n.series_Id,
                        novel_Id = n.novel_Id,
                        title = n.title,
                        novel_number = n.novel_number,
                        cover_images = n.cover_images,

                        // map chapter
                        Chapters = n.Chapters.OrderBy(c => c.chapter_number).Select(c => new ChapterDetailDto
                        {
                            novelID = c.novelID,
                            chapter_id = c.chapter_id,
                            title = c.title,
                            chapter_number = c.chapter_number,
                            word_count = c.word_count,
                            created_at = c.created_at,
                            content = c.content,

                        }).ToList()


                    }).ToList()
                };
            }     
        }

        //View all
        public async Task<PagedResult<SeriesListDto>> GetAllSeriesAsync(
                int pageNumber,
                int pageSize,
                SeriesFilterDto filter,
                SeriesSortBy sortBy = SeriesSortBy.Title,
                bool isAscending = true)
        {
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 30) pageSize = 30;
        
            if (pageSize < 1) pageNumber = 1;   

            var query = _context.Novel_Series
                .Include(s => s.category)
                .Include(s => s.status)
                .Include(s => s.NovelTags)
                    .ThenInclude(nt => nt.Tag)
                .AsQueryable();

            filter ??= new SeriesFilterDto();

            // Gọi filter service
            query = Filter(query, filter);

            //Sorting
            query = Sorting(query, sortBy, isAscending);

      

            var totalCount = await query.CountAsync();

            var series = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SeriesListDto
                {
                    series_Id = s.series_Id,
                    series_title = s.series_title,
                    cover_images = s.cover_images,
                    category_id = s.category_id,
                    categoryName = s.category!.category_name,
                    status_id = s.status_id,
                    statusName = s.status!.statusName,
                    Tags = s.NovelTags.Select(nt => nt.Tag.tagName).ToList()
                })
                .ToListAsync();

            return new PagedResult<SeriesListDto>
            {
                Items = series,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        //View By User
        public async Task<PagedResult<SeriesListDto>> GetSeriesByUploaderAsync(Guid uploaderId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 20) pageSize = 20;

            var query = _context.Novel_Series
                .Where(s => s.uploader_id == uploaderId)
                .Include(s => s.category)
                .Include(s => s.status)
                .Include(s => s.NovelTags)
                    .ThenInclude(nt => nt.Tag)
                .OrderByDescending(s => s.updated_at);

            var totalCount = await query.CountAsync();

            var seriesList = await query
                .Skip((pageNumber - 1) * pageSize) // Bỏ qua các trang trước
                .Take(pageSize)
                .Select(s => new SeriesListDto
                {
                    series_Id = s.series_Id,
                    series_title = s.series_title,
                    cover_images = s.cover_images,
                    category_id = s.category_id,
                    categoryName = s.category != null ? s.category.category_name : null,
                    status_id = s.status_id,
                    statusName = s.status != null ? s.status.statusName : null,
                    Tags = s.NovelTags.Select(nt => nt.Tag.tagName).ToList() ?? new List<string>()
                })
                .ToListAsync();

            return new PagedResult<SeriesListDto>
            {
                Items = seriesList,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };


        }

        


// -------------------------------------------------------------------------------------------------------------------------------------------- //
//Sorting
        public IQueryable<NovelSeries> Sorting(
           IQueryable<NovelSeries> query,
           SeriesSortBy sortBy,
           bool isAscending)
        {
            return sortBy switch
            {
                SeriesSortBy.Title => isAscending
                    ? query.OrderBy(s => s.series_title)
                    : query.OrderByDescending(s => s.series_title),

                SeriesSortBy.Views => isAscending
                    ? query.OrderBy(s => s.views)
                    : query.OrderByDescending(s => s.views),

                SeriesSortBy.WordCount => isAscending
                    ? query.OrderBy(s => s.word_count)
                    : query.OrderByDescending(s => s.word_count),

                SeriesSortBy.UpdatedAt => isAscending
                    ? query.OrderBy(s => s.updated_at)
                    : query.OrderByDescending(s => s.updated_at),

                _ => query.OrderBy(s => s.series_Id) // fallback
            };

        }

        //Filter
        public IQueryable<NovelSeries> Filter(IQueryable<NovelSeries> query, SeriesFilterDto filter)
        {
            if (filter.StatusId is { Count: > 0 })
                query = query.Where(s => filter.StatusId.Contains(s.status_id));

            if (filter.CategoryId is { Count: > 0 })
                query = query.Where(s => filter.CategoryId.Contains(s.category_id));

            if (!string.IsNullOrEmpty(filter.FirstLetter))
                query = query.Where(s => s.series_title.StartsWith(filter.FirstLetter));

            if (filter.TagId is { Count: > 0 })
                query = query.Where(s => s.NovelTags.Any(nt => filter.TagId.Contains(nt.tagID)));

            if (!string.IsNullOrEmpty(filter.Type) && Enum.TryParse<type>(filter.Type, true, out var seriesType))
            {
                query = query.Where(s => s.type == seriesType);
            }


            return query;
        }


        //Search
        public async Task<PagedResult<SeriesListDto>> SearchSeries(
                string keyword,
                int pageNumber,
                int pageSize)
        {
            var query = _context.Novel_Series
                .Include(s => s.category)
                .Include(s => s.status)
                .Include(s => s.NovelTags)
                    .ThenInclude(nt => nt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(s =>
                    s.series_title.ToLower().Contains(keyword) ||
                    (s.author != null && s.author.ToLower().Contains(keyword)) ||
                    (s.artist != null && s.artist.ToLower().Contains(keyword))
                );
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.series_title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SeriesListDto
                {
                    series_Id = s.series_Id,
                    series_title = s.series_title,
                    cover_images = s.cover_images,
                    category_id = s.category_id,
                    categoryName = s.category!.category_name,
                    status_id = s.status_id,
                    statusName = s.status!.statusName,
                    Tags = s.NovelTags.Select(nt => nt.Tag.tagName).ToList()
                })
                .ToListAsync();

            return new PagedResult<SeriesListDto>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        private async Task NotifyFollowersOfUpdate(int seriesId, string seriesTitle, Guid uploaderId)
        {
            List<Guid> followerIds;
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
     
                var followersUrl = $"{_userServiceUrl}/api/internal/favorites/{seriesId}/followers";

                followerIds = await httpClient.GetFromJsonAsync<List<Guid>>(followersUrl);

                if (followerIds == null || !followerIds.Any())
                {
                    _logger.LogInformation("No Follower for series {SeriesId}", seriesId);
                    return;
                }
            }
            catch (Exception ex)
            {
                return; 
            }

            foreach (var followerId in followerIds)
            {
                // Không gửi thông báo cho chính tác giả
                if (followerId == uploaderId) continue;

                var notificationDto = new CreateNotificationDto
                {
                    UserId = followerId,
                    Type = "SeriesUpdate", // Dùng chuỗi
                    Message = $"Series '{seriesTitle}' bạn theo dõi vừa có thông báo mới.",
                    LinkUrl = $"/series/{seriesId}"
                };

                await SendNotificationAsync(notificationDto);
            }
        }

        private async Task SendNotificationAsync(CreateNotificationDto dto)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var notificationUrl = $"{_userServiceUrl}/api/internal/notifications";

                var response = await httpClient.PostAsJsonAsync(notificationUrl, dto);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Send notice faliuer to User {UserId}. Status: {StatusCode}", dto.UserId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection erro when send noctify to User {UserId}", dto.UserId);
            }
        }
    }
}
