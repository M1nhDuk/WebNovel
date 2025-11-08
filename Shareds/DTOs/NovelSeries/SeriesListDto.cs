using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class SeriesListDto
    {
        //Hiển thị thông tin trên bìa sách
        public int series_Id { get; set; }
        public string series_title { get; set; }
        public string? cover_images { get; set; }

        public string type { get; set; }

        //Id
        public int category_id { get; set; }
        public string? categoryName { get; set; }

        public int status_id { get; set; }
        public string? statusName { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
