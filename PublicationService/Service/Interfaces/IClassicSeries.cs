using Shareds.DTOs.Chapter;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Service.Interfaces
{
    public interface IClassicSeries
    {
        Task<ClassicSeriesDetailDto> CreateTraditionalSeriesAsync(CreateTraditionalSeriesDto dto, Guid uploaderId);

        Task<ClassicSeriesDetailDto?> UpdateClassicSeriesAsync(int seriesId, UpdateClassicSeriesDto dto, Guid uploaderId, string userRole);

    }
}
