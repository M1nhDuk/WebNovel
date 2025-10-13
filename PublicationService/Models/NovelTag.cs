namespace NovelService.Models
{
    public class NovelTag
    {
        public int novelTagId { get; set; }

        //Forgein Key
        public int series_Id { get; set; }
        public NovelSeries NovelSeries { get; set; }

        public int tagID { get; set; }
        public Tag Tag { get; set; }
       

    }
}
