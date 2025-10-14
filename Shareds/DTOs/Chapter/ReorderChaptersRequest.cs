using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    public class ReorderChaptersRequest
    {
        public int? novel_Id { get; set; }
        public int? series_Id { get; set; }
        public List<ChapterOrderItem> Chapters { get; set; } = new();
    }

    public class ChapterOrderItem
    {
        public int chapter_id { get; set; }   // ID chương
        public int new_position { get; set; }    // Thứ tự mới
    }
}
