using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class CreateSeriesDto
    {
        [Required]
        public string series_title { get; set; }
        public string? artist { get; set; }
        public string? author { get; set; }

        [Required]
        public string description { get; set; }
        public string? cover_images { get; set; }
        public string? note { get; set; }

        //ID
        [Required]
        public int status_id { get; set; }

        [Required]
        public int? category_id { get; set; }

        public List<int>? TagIds { get; set; }
    }
}
