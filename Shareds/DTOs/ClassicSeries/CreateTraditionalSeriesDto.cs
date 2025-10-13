using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;
using Shareds.DTOs.NovelSeries;

namespace Shareds.DTOs.ClassicSeries
{
    public class CreateTraditionalSeriesDto : CreateSeriesDto
    {
        public string? ISBN_10 { get; set; }
        public required string ISBN_13 { get; set; }
        public string? publisher { get; set; }
        public DateTime? publish_date { get; set; }
        public string? edition { get; set; }

        public List<ChapterDetailDto> Chapters { get; set; } = new();
    }
}
