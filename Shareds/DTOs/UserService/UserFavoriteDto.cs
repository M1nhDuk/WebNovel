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

  
        // Frontend sẽ dùng SeriesId để gọi NovekService
        // và lấy về Title, CoverImage, TotalChapters
   
    }
}
