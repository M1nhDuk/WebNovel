namespace NovelService.Models
{
    public class Novel
    {
        public int novel_Id { get; set; }
        public string title { get; set; }
        public string? artist { get; set; }
        public string? author { get; set; }
        public string description { get; set; }
        public string? cover_images { get; set; }
        public int word_count { get; set; }
        public int views { get; set; }
        public string? note { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


        //Forgein Key
        public int uploader_id { get; set; }
        public int category_id { get; set; }
        public Category? category { get; set; } 
        public int status_id { get; set; }
        public NovelStatus? status { get; set; }

        //Thiết lập quan hệ giữa các bảng
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<NovelTag> NovelTags { get; set; } = new List<NovelTag>();

    }
}
