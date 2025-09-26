namespace NovelService.Models
{
    public class Category
    {
        public int category_id { get; set; }
        public required string category_name { get; set; }

     
        //Thiết lập quan hệ giữa các bảng
        public ICollection<Novel> Novels { get; set; } = new List<Novel>();
    }
}
