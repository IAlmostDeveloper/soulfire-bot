using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulfire.Bot.Dtos
{
    public class InlineQueryResultArticleDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? InputMessageContent { get; set; }
        public string Description { get; set; }
        public string ThumbUrl { get; set; }
    }
}
