using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovelService.Models
{
    public class Chapter
    {
        public int chapter_id { get; set; }
        public  string title { get; set; }
        public  string content { get; set; }
        public int word_count { get; set; }
        public int chapter_number { get; set; }         //unique
        public DateTime created_at { get; set; }

        //Foregein Key
        public int novelID { get; set; }
        public Novel? Novel { get; set; }
    }
}
