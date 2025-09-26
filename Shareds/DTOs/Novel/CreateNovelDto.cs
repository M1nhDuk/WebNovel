using Shareds.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class CreateNovelDto
    {
        [Required]
        [MaxLength(250)]
        public string title { get; set; } = null!;

        [Required]
        public string description { get; set; } = null!;
        public string author { get; set; } = null!;
        public string? artist { get; set; }
        public string? cover_images { get; set; }      
        public string? note { get; set; }

        //ID
        [Required]
        public int status_id { get; set; }
        [Required]
        public int uploader_id { get; set; }
        [Required]
        public int? category_id { get; set; }
        public List<int>? TagIds { get; set; }

        public List<ChapterCreateDto>? Chapters { get; set; }
    }
}
