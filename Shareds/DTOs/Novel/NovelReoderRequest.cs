using Shareds.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class NovelReoderRequest
    {
        public int series_Id {  get; set; }
        public List<NovelReorderItem> Novels { get; set; } = new();
    }

    public class NovelReorderItem
    {
        public int novel_id { get; set; }
        public int new_position { get; set; }
    }
}
