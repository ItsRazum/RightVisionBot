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
        public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            sql database = Program.database;
            string? msgText = message.Text;
            long userId = message.From.Id;

            if (message.Text == Language.GetPhrase("Profile_Track_SendTrack", rvUser.Lang) + "♂" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Instruction", rvUser.Lang), replyMarkup: Keyboard.backButton(userId));

            else if (message.Text == Language.GetPhrase("Profile_Track_SendImage", rvUser.Lang) + "🖼" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendImage_Instruction", rvUser.Lang), replyMarkup: Keyboard.backButton(userId));

            else if (message.Text == Language.GetPhrase("Profile_Track_SendText", rvUser.Lang) + "📝" && message.Chat.Type == ChatType.Private)
                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendText_Instruction", rvUser.Lang), replyMarkup: Keyboard.backButton(userId));
        }
    }
}
