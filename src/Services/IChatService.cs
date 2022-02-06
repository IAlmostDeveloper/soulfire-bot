using Soulfire.Bot.Core.InlineButtons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soulfire.Bot.Services
{
    public interface IChatService
    {
        event EventHandler<ChatMessageEventArgs> ChatMessage;
        event EventHandler<CallbackEventArgs>? Callback;

        Task<string> BotUserName();
        Task<bool> SendMessage(long chatId, string? message, IEnumerable<IEnumerable<InlineButton>>? buttons = null);
        Task<bool> UpdateMessage(long chatId, int messageId, string newText, IEnumerable<IEnumerable<InlineButton>>? buttons = null);
        Task<string> GetChatMemberName(long chatId, int userId);
    }
}
