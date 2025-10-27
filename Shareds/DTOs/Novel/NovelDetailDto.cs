using Shareds.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace Shareds.DTOs.Novel
    {
        public class NovelDetailDto
        {
            public int? series_Id {  get; set; }
            public int novel_Id { get; set; }

            [Required]
            public string title { get; set; }

            public string author { get; set; }  //map từ NovelSeries
            public string? artist { get; set; } //map từ NovelSeries
            public string? cover_images { get; set; }
            public DateTime updated_at { get; set; }
            public int novel_number { get; set; }

            //ID
            public Guid uploader_id { get; set; } //map từ NovelSeries
            public string uploader_name { get; set; } // enrich sau khi map bằng cách gọi UserService API.
            public string? uploader_avatar { get; set; } //enrich sau khi map bằng cách gọi UserService API

            //List

            public List<ChapterDetailDto> Chapters { get; set; } = new();
        
        }
    }
