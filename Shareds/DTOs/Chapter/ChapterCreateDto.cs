using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    public class ChapterCreateDto
    {
        public int? novelID { get; set; }

        public int? series_id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int chapter_number { get; set; }
     }
}
