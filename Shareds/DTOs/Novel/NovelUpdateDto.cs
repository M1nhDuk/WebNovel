using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class NovelUpdateDto
    {
        public int series_Id {  get; set; }
        public string? title { get; set; }
        public string? cover_images { get; set; }

    }
}
