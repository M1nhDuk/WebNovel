using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class NovelUpdateDto
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public string? author { get; set; } = null!;
        public string? artist { get; set; }
        public string? cover_images { get; set; }
        public string? note { get; set; }

        //ID
        public int? category_id { get; set; }
        public int? status_id { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
