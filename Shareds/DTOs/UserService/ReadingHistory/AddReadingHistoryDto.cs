using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService.ReadingHistory
{
    public class AddReadingHistoryDto
    {
        public int SeriesId { get; set; }
        public int ChapterId { get; set; }

        public DateTime? ChapterCreatedAt { get; set; }
    }
}
