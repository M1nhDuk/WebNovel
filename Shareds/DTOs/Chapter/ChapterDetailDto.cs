using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    //xem chi tiết 1 chapter
    public class ChapterDetailDto
    {
        public int chapter_id { get; set; }
        public int chapter_number { get; set; }
        public string title { get; set; } 
        public string content { get; set; } 
        public DateTime created_at { get; set; }

        public int word_count { get; set; }
    }
}
