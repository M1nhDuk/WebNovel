using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelSeries
{
    public class SeriesPagingRequest
    {
        public int PageNumber { get; set; } = 1;   // mặc định page 1
        public int PageSize { get; set; } = 30;    // mặc định 30 item/trang
        public string? Keyword { get; set; }
    }
}
