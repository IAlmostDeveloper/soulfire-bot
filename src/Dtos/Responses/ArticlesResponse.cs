using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulfire.Bot.Dtos.Responses
{
    public class ArticlesResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public IEnumerable<Article> Articles { get; set; }
    }
}
