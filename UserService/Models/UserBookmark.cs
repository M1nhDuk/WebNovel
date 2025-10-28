using Org.BouncyCastle.Bcpg;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractionService.Models
{
    public class UserBookmark
    {
        [Key]
        public Guid BookmarkId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int SeriesId { get; set; }
        [Required]
        public int ChapterId { get; set; }

        [Required]
        [MaxLength(255)]
        public string LocationIdentifier { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ContextSnippet { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        //Enrich 
        [NotMapped]
        public string? SeriesTitle { get; set; }
        [NotMapped]
        public string? SeriesCoverImage { get; set; }
        [NotMapped]
        public string? ChapterTitle { get; set; }
        [NotMapped]
        public int ChapterNumber { get; set; }

    }
}
