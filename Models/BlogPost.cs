namespace British_Kingdom_back.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public int ProfilId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public DateTime PostDate { get; set; }
        public int ReadingTime { get; set; }
        public string ContentJson { get; set; } = string.Empty;
    }
}
