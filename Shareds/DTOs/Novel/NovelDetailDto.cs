using Shareds.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class NovelDetailDto
    {
        public int novel_Id { get; set; }
        public string title { get; set; } 
        public string description { get; set; }
        public int uploader_id { get; set; }
        public string author { get; set; } = null!;
        public string? artist { get; set; }
        public string? cover_images { get; set; }
        public int word_count { get; set; }
        public int view { get; set; }
        public string? note { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        //ID
        public int category_id { get; set; }
        public string? categoryName { get; set; }

        public int status_id { get; set; }
        public string? statusName { get; set; }


        //List
        public List<string> Tags { get; set; } = new();
        public List<ChapterSummaryDto> Chapters { get; set; } = new();
        
    }
}
