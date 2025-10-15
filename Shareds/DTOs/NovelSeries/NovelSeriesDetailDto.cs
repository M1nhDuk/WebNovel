using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class NovelSeriesDetailDto
    {
        public int series_Id { get; set; }
        public string series_title { get; set; }
        public string? artist { get; set; }
        public string? author { get; set; }
        public string description { get; set; }
        public string? cover_images { get; set; }
        public int word_count { get; set; }
        public int views { get; set; }
        public string? note { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public string type { get; set; }


        //ID
        public int uploader_id { get; set; }
        public string uploader_name { get; set; } = string.Empty;
        public string? uploader_avatar { get; set; }

        public int category_id { get; set; }
        public string? categoryName { get; set; }

        public int status_id { get; set; }
        public string? statusName { get; set; }

        
        //List
        public List<string> Tags { get; set; } = new();

        public List<NovelDetailDto> Novels { get; set; } = new();
    }
}
