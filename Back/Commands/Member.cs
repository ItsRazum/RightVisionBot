using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back.Commands
{
    class Member
    {
        public static async Task Commands(ITelegramBotClient botClient, string lang, Message message)
        {
            sql database = Program.database;
            string? msgText = message.Text;
            long userId = message.From.Id;

            if (message.Text == Language.GetPhrase("Profile_Track_SendTrack", lang) + "♂" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Instruction", lang), replyMarkup: Keyboard.BackButton(lang));

            else if (message.Text == Language.GetPhrase("Profile_Track_SendImage", lang) + "🖼" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendImage_Instruction", lang), replyMarkup: Keyboard.BackButton(lang));

            else if (message.Text == Language.GetPhrase("Profile_Track_SendText", lang) + "📝" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendText_Instruction", lang), replyMarkup: Keyboard.BackButton(lang));
        }
    }
}
