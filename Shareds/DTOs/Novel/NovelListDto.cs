using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs
{
    public class NovelListDto
    {
        public int novel_Id { get; set; }
        public string title { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? cover_images { get; set; }
        public string StatusName { get; set; } = null!;


        //Id
        public int category_id { get; set; }
        public int status_id { get; set; }
        public List<string>? Tags { get; set; }
    }
}
