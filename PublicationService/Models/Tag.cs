namespace NovelService.Models
{
    public class Tag
    {
        public int tagId { get; set; }
        public string tagName { get; set; }

        public ICollection<NovelTag> NovelTags { get; set; } = new List<NovelTag>();
    }
}
