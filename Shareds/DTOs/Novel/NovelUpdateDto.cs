using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Novel
{
    public class NovelUpdateDto
    {
       // public int series_Id {  get; set; }

        [Required]
        public string? title { get; set; }
        public string? cover_images { get; set; }

    }
}
