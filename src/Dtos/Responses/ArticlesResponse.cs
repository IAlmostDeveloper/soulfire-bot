using System.Collections.Generic;

namespace Soulfire.Bot.Dtos.Responses
{
    public class ArticlesResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public List<Article> Articles { get; set; }
    }
}
