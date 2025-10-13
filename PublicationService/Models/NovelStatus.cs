namespace NovelService.Models
{
    public class NovelStatus
    {
        public int statusId { get; set; }
        public string statusName { get; set; }

        //Thiết lập quan hệ giữa các bảng
        public ICollection<NovelSeries> NovelSeries { get; set; } = new List<NovelSeries>();
    }
}
