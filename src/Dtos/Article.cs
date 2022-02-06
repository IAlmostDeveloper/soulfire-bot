namespace Soulfire.Bot.Dtos
{
    public class Article
    {
        public Source Source { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }
        public string PublishedAt { get; set; }
    }
}
