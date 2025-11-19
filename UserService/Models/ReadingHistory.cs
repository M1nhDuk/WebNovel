using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class ReadingHistory
    {
        [Key]
        public Guid HistoryId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int SeriesId { get; set; }

        public int ChapterId { get; set; }

        [Required]
        public DateTime LastAccessedAt { get; set; } 

        //Enrich từ NovelService
        [NotMapped]
        public string? SeriesTitle { get; set; }
        [NotMapped]
        public string? SeriesCoverImage { get; set; }
    }
}
