using System.Collections.Generic;

namespace Soulfire.Bot.Dtos.Responses
{
    public class NewsSourcesReponse
    {
        public string Status { get; set; }
        public IEnumerable<Source> Sources { get; set; }
    }
}
