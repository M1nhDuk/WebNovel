using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovelService.Models
{
    public class Chapter
    {
        
        public int chapter_id { get; set; }
        public string title { get; set; } = null!;
        public string content { get; set; } = null!;
        public int word_count { get; set; }
        public int chapter_number { get; set; }         //unique
        public DateTime created_at { get; set; }

        //Foregein Key
        public int? series_Id { get; set; }
        public NovelSeries? TS { get; set; }
        public int? novelID { get; set; }
        public Novel? Novel { get; set; }

        
    }
}
