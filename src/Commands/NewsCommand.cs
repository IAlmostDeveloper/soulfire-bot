using Soulfire.Bot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Soulfire.Bot.Commands
{
    public class NewsCommand : IBotCommand
    {
        private readonly NewsService _service;
        public NewsCommand(NewsService service)
        {
            _service = service;
        }
        public string Command => "news";

        public string Description => "Command for getting news from headlines";

        public bool InternalCommand => false;

        public async Task Execute(IChatService chatService, long chatId, long userId, int messageId, string commandText)
        {
            var newsList = await _service.GetTopHeadlines();
            var buttons = newsList.Articles.ToDictionary(x => x.Title.Substring(0, 10), x => x.Description.Substring(0, 10));
            await chatService.SendMessage(chatId, "Top news:", buttons);
        }
    }
}
