using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class SeriesSummaryDto
    {
        public int SeriesId { get; set; }
        public string? Title { get; set; }
        public string? CoverImage { get; set; }
        public int TotalChapterCount { get; set; }
    }
}
