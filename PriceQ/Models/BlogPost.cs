namespace PriceQ.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; } // Short summary
        public string Category { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string ImageUrl { get; set; }
    }
}