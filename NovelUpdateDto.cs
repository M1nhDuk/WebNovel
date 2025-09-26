using System;

public class NovelUpdateDto
{
	public NovelUpdateDto()
	{
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? StatusId { get; set; }
        public string Author { get; set; };
        public string? Artist { get; set; };
        public string? CoverImage { get; set; };
        public string? Cover_image { get; set; };
        public List<int>? TagIds { get; set; }
    }
}
