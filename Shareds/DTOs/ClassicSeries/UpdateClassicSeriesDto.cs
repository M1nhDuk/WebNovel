using Shareds.DTOs.NovelSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.ClassicSeries
{
    public class UpdateClassicSeriesDto : UpdateNovelService
    {
        public string? ISBN_10 { get; set; }
        public string? ISBN_13 { get; set; }
        public string? publisher { get; set; }
        public DateTime? publish_date { get; set; }
        public string? edition { get; set; }
    }
}
