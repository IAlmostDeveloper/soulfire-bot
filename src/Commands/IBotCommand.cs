using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soulfire.Bot.Commands
{
    public interface IBotCommand
    {
        string Command { get; }
        string Description { get; }
        bool InternalCommand { get; }

        Task Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText);
    }
}
