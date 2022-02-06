using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulfire.Bot.Dtos.Responses
{
    public class NewsSourcesReponse
    {
        public string Status { get; set; }
        public IEnumerable<Source> Sources { get; set; }
    }
}
