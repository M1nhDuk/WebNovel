using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    //Chapter data khi hover
    public class ChapterSummaryDto
    {
        public int chapter_id { get; set; }
        public int chapter_number { get; set; } //Unique
        public string title { get; set; }

     
    }
}
