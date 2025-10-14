using Shareds.DTOs.Chapter;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Service.Interfaces
{
    public interface IClassicSeries
    {
        Task<ClassicSeriesDetailDto> CreateTraditionalSeriesAsync(CreateTraditionalSeriesDto dto, int uploaderId);

        Task<ClassicSeriesDetailDto?> GetByIdAsync(int seriesId);

        Task<ClassicSeriesDetailDto?> UpdateClassicSeriesAsync(int seriesId, UpdateClassicSeriesDto dto, int uploaderId);
        
    }
}
