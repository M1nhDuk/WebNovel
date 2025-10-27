using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    public class CreateSeriesChapterDto
    {
        public int chapter_number { get; set; }

        [Required]
        public string title { get; set; } = null!;

        [Required]
        public string content { get; set; } = null!;
    }
}
