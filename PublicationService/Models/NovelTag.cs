namespace NovelService.Models
{
    public class NovelTag
    {
        public int novelTagId { get; set; }

        //Forgein Key
        public int novelID { get; set; }
        public Novel Novel { get; set; }

        public int tagID { get; set; }
        public Tag Tag { get; set; }
       

    }
}
