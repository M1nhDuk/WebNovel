using System;

public class NovelListDto
{
	public NovelListDto()
	{
        public int NovelId { get; set; }
        public string Title { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string StatusName { get; set; } = null!;
    }
}
