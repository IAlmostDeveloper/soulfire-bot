using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Soulfire.Bot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Soulfire.Bot.Core.InlineButtons;
using Telegram.Bot.Types.InlineQueryResults;
using Soulfire.Bot.Dtos;
using System.Text.RegularExpressions;

namespace Soulfire.Bot.Services
{
    public class TelegramService : IChatService, IDisposable
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramService> _logger;
        private bool disposedValue;
        public event EventHandler<ChatMessageEventArgs>? ChatMessage;
        public event EventHandler<CallbackEventArgs>? Callback;

        private readonly NewsService _newsService;

        public async Task<string> BotUserName() => $"@{(await _botClient.GetMeAsync()).Username}";

        public TelegramService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<TelegramService> logger, NewsService newsService)
        {
            _newsService = newsService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            var apikey = configuration["Telegram:ApiKey"];
            _botClient = new TelegramBotClient(apikey);
            _botClient.OnMessage += OnMessage;
            _botClient.OnCallbackQuery += OnCallbackQuery;
            _botClient.OnInlineQuery += OnInlineQuery;
            RegisterCommands();
            _botClient.StartReceiving();
        }

        /// This method registers all the commands with the bot on telegram
        /// so that the user gets some sort of intelisense
        private void RegisterCommands()
        {
            var commands = _serviceProvider
                .GetServices<IBotCommand>()
                .Where(x => !x.InternalCommand)
                .Select(x => new BotCommand
                {
                    Command = x.Command,
                    Description = x.Description
                });
            _botClient.SetMyCommandsAsync(commands).GetAwaiter().GetResult();
        }

        private async void OnMessage(object? sender, MessageEventArgs e)
        {
            _logger.LogTrace("Message received from '{Username}': '{Message}'", e.Message.From.Username ?? e.Message.From.FirstName, e.Message.Text);

            if (e.Message.Entities == null || !e.Message.Entities.Any(x => x.Type == MessageEntityType.BotCommand))
            {
                _logger.LogTrace("No command was specified");
                return;
            }

            if (e.Message.Entities.Count(x => x.Type == MessageEntityType.BotCommand) > 1)
            {
                _logger.LogTrace("More than one command was specified");
                if (sender is TelegramBotClient client)
                {
                    await client.SendTextMessageAsync(e.Message.Chat.Id, "Please only send one command at a time.");
                }
                return;
            }

            var botCommand = e.Message.Entities.Single(x => x.Type == MessageEntityType.BotCommand);
            var command = e.Message.Text.Substring(botCommand.Offset, botCommand.Length);
            command = command.Replace(await BotUserName(), string.Empty);

            ChatMessage?.Invoke(this, new ChatMessageEventArgs
            {
                Text = e.Message.Text.Replace(command, string.Empty).Trim(),
                UserId = e.Message.From.Id,
                ChatId = e.Message.Chat.Id,
                MessageId = e.Message.MessageId,
                Command = command
            });
        }

        private async void OnCallbackQuery(object? sender, CallbackQueryEventArgs e)
        {
            _logger.LogTrace("Callback received from '{Username}': '{Message}'", e.CallbackQuery.From.Username ?? e.CallbackQuery.From.FirstName, e.CallbackQuery.Data);

            if (sender is TelegramBotClient client)
            {
                // This removes the keyboard, but we could also update one here...
                await client.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, null);

                Callback?.Invoke(this, new CallbackEventArgs
                {
                    ChatId = e.CallbackQuery.Message.Chat.Id,
                    UserId = e.CallbackQuery.From.Id,
                    MessageId = e.CallbackQuery.Message.MessageId,
                    Command = e.CallbackQuery.Data
                });
            }
        }

        public async void OnInlineQuery(object? sender, InlineQueryEventArgs e)
        {
            _logger.LogTrace("Inline query received from '{Username}': '{Message}'", e.InlineQuery.From.Username ?? e.InlineQuery.From.FirstName, e.InlineQuery.Query);
            var articles = !string.IsNullOrWhiteSpace(e.InlineQuery.Query) ? await _newsService.GetArticlesByKeyword(e.InlineQuery.Query) : await _newsService.GetTopHeadlines();
            var results = GetInlineQueryResults(articles.Articles.Select(x =>
            {
                return new InlineQueryResultArticleDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = EscapeText(x.Title),
                    Description = EscapeText(x.Description),
                    InputMessageContent = string.Format("{0}\n<a href=\"{1}\">{1}</a>",
                    EscapeText(x.Description != null ? x.Description : x.Title),
                    x.Url),
                    ThumbUrl = x.UrlToImage
                };
            }));
            await _botClient.AnswerInlineQueryAsync(e.InlineQuery.Id, results);
        }

        public async Task<bool> UpdateMessage(long chatId, int messageId, string? newText, IEnumerable<IEnumerable<InlineButton>>? buttons = null)
        {
            try
            {
                newText = EscapeText(newText);

                _logger.LogTrace("Updating message {MessageId}: {NewText}", messageId, newText);

                await _botClient.EditMessageTextAsync(new ChatId(chatId), messageId, newText, parseMode: ParseMode.MarkdownV2, replyMarkup: GetInlineKeyboard(buttons));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating message");
                return false;
            }
        }

        private IEnumerable<InlineQueryResultBase> GetInlineQueryResults(IEnumerable<InlineQueryResultArticleDto> articleDtos)
        {
            var queryResult = new List<InlineQueryResultArticle>();
            foreach (var articleDto in articleDtos)
            {
                var article = new InlineQueryResultArticle(articleDto.Id, articleDto.Title, new InputTextMessageContent(articleDto.InputMessageContent)
                {
                    ParseMode = ParseMode.Html
                });
                article.Description = articleDto.Description;
                if (articleDto.ThumbUrl != null)
                    article.ThumbUrl = articleDto.ThumbUrl;

                queryResult.Add(article);
            }

            return queryResult;
        }

        private InlineKeyboardMarkup? GetInlineKeyboard(IEnumerable<IEnumerable<InlineButton>>? buttons)
        {
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (buttons != null)
            {
                var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
                foreach (var buttonsLine in buttons)
                {
                    var inlineButtonsLine = new List<InlineKeyboardButton>();
                    foreach (var button in buttonsLine)
                    {
                        switch (button.Type)
                        {
                            case Core.Enumerations.InlineButtonType.Callback:
                                inlineButtonsLine.Add(InlineKeyboardButton.WithCallbackData(button.Label, button.Value));
                                break;
                            case Core.Enumerations.InlineButtonType.Url:
                                inlineButtonsLine.Add(InlineKeyboardButton.WithUrl(button.Label, button.Value));
                                break;
                            case Core.Enumerations.InlineButtonType.SwitchToInlineQuery:
                                inlineButtonsLine.Add(InlineKeyboardButton.WithSwitchInlineQuery(button.Label, button.Value));
                                break;
                            case Core.Enumerations.InlineButtonType.SwitchToInlineQueryCurrentChat:
                                inlineButtonsLine.Add(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(button.Label, button.Value));
                                break;
                            default:
                                break;
                        }
                    }
                    inlineKeyboardButtons.Add(inlineButtonsLine);
                }
                inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
            }
            return inlineKeyboard;
        }

        public async Task<bool> SendMessage(long chatId, string? message, IEnumerable<IEnumerable<InlineButton>>? buttons = null)
        {
            try
            {
                message = EscapeText(message);

                _logger.LogTrace("Sending message to {ChatId}: {Message}", chatId, message);

                await _botClient.SendTextMessageAsync(new ChatId(chatId), message, parseMode: ParseMode.MarkdownV2, replyMarkup: GetInlineKeyboard(buttons));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending message");
                return false;
            }
        }

        public async Task<string> GetChatMemberName(long chatId, int userId)
        {
            var chatMember = await _botClient.GetChatMemberAsync(new ChatId(chatId), userId);
            return chatMember.User.Username ?? chatMember.User.FirstName;
        }

        private string EscapeText(string? source)
        {
            if (source != null)
            {
                var regex = new Regex(@"<(\w*|\/\w*)>");
                source = regex.Replace(source, string.Empty);

                var charactersToEscape = new[] { "_", "*", "[", "]", "(", ")", "~", "`","<", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
                foreach (var item in charactersToEscape)
                {
                    source = source.Replace(item, $@"\{item}");
                }
            }
            return source;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_botClient != null)
                    {
                        _botClient.OnMessage -= OnMessage;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TelegramService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
