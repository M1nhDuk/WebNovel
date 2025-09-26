namespace NovelService.Models
{
    public class NovelStatus
    {
        public int statusId { get; set; }
        public string statusName { get; set; }

        //Thiết lập quan hệ giữa các bảng
        public ICollection<Novel> Novels { get; set; } = new List<Novel>();
    }
}
