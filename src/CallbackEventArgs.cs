namespace Soulfire.Bot
{
    public class CallbackEventArgs
    {
        public string? Command { get; set; }
        public long UserId { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
    }
}
