using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService.ReadingHistory
{
    public class ReadingHistoryDto
    {
        public Guid HistoryId { get; set; }
        public int SeriesId { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;

        public int ChapterId { get; set; }
        //Lấy từ novel service
        public string? SeriesCoverImage { get; set; }
    }
}
