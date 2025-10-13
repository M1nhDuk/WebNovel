using Shareds.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace Shareds.DTOs.Novel
    {
        public class CreateNovelDto
        {
            public int series_Id { get; set; }

            [Required]
            [MaxLength(250)]
            public string title { get; set; } = null!;
            public string? cover_images { get; set; }      
      
        }
    }
