using Shareds.DTOs.NovelSeries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.ClassicSeries
{
    public class UpdateClassicSeriesDto : UpdateNovelService
    {
        [Required]
        [StringLength(10, ErrorMessage = "ISBN-10 limit 10 character")]
        public string? iSBN_10 { get; set; }

        [Required]
        [StringLength(13, ErrorMessage = "ISBN-13 limit 13 character")]
        public string? iSBN_13 { get; set; }

        [Required]
        public string? publisher { get; set; }
        public DateTime? publish_date { get; set; }

        [Required]
        public string? edition { get; set; }
    }
}
