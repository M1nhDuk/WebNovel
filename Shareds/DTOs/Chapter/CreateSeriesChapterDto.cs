using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    public class CreateSeriesChapterDto
    {
        public int chapter_number { get; set; }
        public string title { get; set; } = null!;
        public string content { get; set; } = null!;
    }
}
