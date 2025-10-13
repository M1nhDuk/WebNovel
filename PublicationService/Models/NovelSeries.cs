using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NovelService.Models
{
    public enum type { Series, TRADITIONAL }
    public class NovelSeries
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int series_Id { get; set; }
        public string series_title { get; set; }
        public string? artist { get; set; }
        public string? author { get; set; }
        public string description { get; set; } = null!;
        public string? cover_images { get; set; }
        public int word_count { get; set; }
        public int views { get; set; }
        public string? note { get; set; }
        public required type type { get; set; } = type.Series;
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


        //Forgein Key
        public int uploader_id { get; set; }
        public int category_id { get; set; }
        public Category? category { get; set; }
        public int status_id { get; set; }
        public NovelStatus? status { get; set; }

       

        //Relation ship
        public ICollection<NovelTag> NovelTags { get; set; } = new List<NovelTag>();
        public ICollection<Novel> Novel { get; set; } = new List<Novel>();
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        public ClassicSeries? ClassicSeries { get; set; }
    }
}
