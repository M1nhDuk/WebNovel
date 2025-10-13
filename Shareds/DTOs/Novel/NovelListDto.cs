using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs
{
    //Hiển thị thông tin trên bìa sách
    public class NovelListDto
    {
        public int novel_Id { get; set; }
        public string title { get; set; } = null!;
        public string? cover_images { get; set; }

    }
}
