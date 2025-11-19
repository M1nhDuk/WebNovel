using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class UserFavoriteDto
    {
        public int SeriesId { get; set; }
        public DateTime AddedAt { get; set; }
        public int LastKnowChapter {  get; set; }

        public string? SeriesTitle { get; set; }
        public string? SeriesCoverImage { get; set; }
        public int CurrentChapterCount { get; set; }


    }
}
