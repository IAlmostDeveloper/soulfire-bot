using static Soulfire.Bot.Core.Enumerations;

namespace Soulfire.Bot.Core.InlineButtons
{
    public class InlineButton
    {
        public InlineButton(string label, string value, InlineButtonType type = InlineButtonType.Callback)
        {
            Label = label;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Надпись
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Значение кнопки. В зависимости от типа это может быть callback_data, url либо switch_inline_query параметр
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Тип кнопки
        /// </summary>
        public InlineButtonType Type { get; set; }

    }
}
