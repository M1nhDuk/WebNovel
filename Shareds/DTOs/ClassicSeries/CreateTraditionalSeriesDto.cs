using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required]
        [StringLength(10, ErrorMessage = "ISBN-10 limit 10 character")]
        public string? iSBN_10 { get; set; }

        [Required]
        [StringLength(13, ErrorMessage = "ISBN-13 limit 13 character")]
        public required string iSBN_13 { get; set; }

        [Required]
        public string? publisher { get; set; }
        public DateTime? publish_date { get; set; }

        [Required]
        public string? edition { get; set; }

        public List<ChapterDetailDto> Chapters { get; set; } = new();
    }
}
