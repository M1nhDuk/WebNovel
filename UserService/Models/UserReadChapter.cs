namespace UserService.Models
{
    public class UserReadChapter
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int SeriesId { get; set; }
        public int ChapterId { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
