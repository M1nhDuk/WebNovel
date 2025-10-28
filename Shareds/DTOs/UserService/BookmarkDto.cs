using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class BookmarkDto
    {
        public Guid BookmarkId { get; set; }
        public int SeriesId { get; set; }
        public int ChapterId { get; set; }
        public string LocationIdentifier { get; set; } = string.Empty;
        public string? ContextSnippet { get; set; }
        public DateTime CreatedAt { get; set; } 

        //Enriched Data
        public string? SeriesTitle { get; set; }
        public string? SeriesCoverImage { get; set; }
        public string? ChapterTitle { get; set; }
        public int ChapterNumber { get; set; }
    }
}
