using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulfire.Bot.Core
{
    public static class Enumerations
    {
        /// <summary>
        /// Перечисление типов инлайн-кнопок в сообщении от бота
        /// </summary>
        public enum InlineButtonType
        {
            // Обычная кнопка с callback'ом
            Callback = 0,
            // Кнопка-ссылка
            Url = 1,
            // Кнопки, активирующий режим Inline Query при нажатии
            SwitchToInlineQuery = 2,
            SwitchToInlineQueryCurrentChat = 3
        }
    }
}
