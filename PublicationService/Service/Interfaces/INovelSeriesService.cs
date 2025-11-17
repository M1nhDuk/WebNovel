using NovelService.Models;
using Shareds.DTOs;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Service.Interfaces
{
    public interface INovelSeriesService
    {
        //Create
        Task<NovelSeriesDetailDto> CreateSeriesAsync(CreateSeriesDto createDto, Guid uploader_id);

        //Update
        Task<NovelSeriesDetailDto?> UpdateSeriesAsync(int seriesId, UpdateNovelService dto, Guid uploader_id, string userRole);

        //Delete
        Task<bool> DeleteSeriesById(int seriesId, Guid uploader_id, string userRole);

        // Read
        Task<NovelSeriesDetailDto?> GetByIdAsync(int seriesId);
        Task<PagedResult<SeriesListDto>> GetAllSeriesAsync(int pageNumber, int pageSize, SeriesFilterDto filter, SeriesSortBy sortBy = SeriesSortBy.Title, bool isAscending = true);


        //Sorting
        IQueryable<NovelSeries> Sorting(IQueryable<NovelSeries> query, SeriesSortBy sortBy, bool isAscending);

        //Seach
        Task<PagedResult<SeriesListDto>> SearchSeries(string keyword, int pageNumber, int pageSize);

        //Filter
        IQueryable<NovelSeries> Filter(IQueryable<NovelSeries> query, SeriesFilterDto filter);

        Task<PagedResult<SeriesListDto>> GetSeriesByUploaderAsync(Guid uploaderId, int pageNumber, int pageSize);

    }
}
