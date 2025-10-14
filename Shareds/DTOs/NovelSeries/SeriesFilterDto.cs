using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class SeriesFilterDto
    {
        public string? Type { get; set; }
        public List<int>? StatusId { get; set; }
        public List<int>? CategoryId { get; set; }
        public string? FirstLetter { get; set; }
        public List<int>? TagId { get; set; }
    }
}
