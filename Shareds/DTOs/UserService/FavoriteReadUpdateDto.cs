using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class FavoriteReadUpdateDto
    {
        //Reset bộ đếm Favorite
        [Required]
        public int SeriesId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int LatestChapterCount { get; set; }
    }
}
