using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs
{
    public class PublicationDetailResponseItem
    {
        public int SeriesId { get; set; }
        public int ChapterId { get; set; }
        public string? SeriesTitle { get; set; }
        public string? SeriesCoverImage { get; set; }
        public string? ChapterTitle { get; set; }
        public int ChapterNumber { get; set; }
    }
}
