namespace NovelService.Models
{
    public class Novel
    {
        public int novel_Id { get; set; }
        public string title { get; set; }
        public string? cover_images { get; set; }
        public DateTime updated_at { get; set; }
        public int novel_number { get; set; }

        //public string uploader_id { get; set; }
            
        //Foregein Key
        public int? series_Id { get; set; }
        public NovelSeries? NovelSeries { get; set; }


        //Thiết lập quan hệ giữa các bảng
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    }
}
