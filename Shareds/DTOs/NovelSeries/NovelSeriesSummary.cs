using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    //Hiển thị thông tin khi hover vào bìa sách (list)
    public class NovelSeriesSummary
    {
        public int series_Id { get; set; }
        public string series_title { get; set; }
        public string description { get; set; }
        public string? cover_images { get; set; }
        public int word_count { get; set; }
        public int views { get; set; }
    }
}
