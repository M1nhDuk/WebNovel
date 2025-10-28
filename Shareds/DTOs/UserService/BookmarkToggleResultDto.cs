using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class BookmarkToggleResultDto
    {
        public bool IsBookmarked { get; set; }

        
        public BookmarkDto? Data { get; set; }
    }
}
