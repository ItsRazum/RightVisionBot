using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Block
    {
        private static sql database = Program.database;
        public static async Task Participation(ITelegramBotClient botClient, Message message)
        {
            string newMessage = message.Text.ToLower().Replace("заблокировать участие ", "");
            if (RvMember.Get(message.From.Id) == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = '{newMessage}';", "");
                await botClient.SendTextMessageAsync(message.Chat, $"Участие пользователя Id:{newMessage} заблокировано");
                await botClient.SendTextMessageAsync(long.Parse(newMessage),
                    string.Format(Language.GetPhrase("Member_Messages_FormBlocked",
                        RvUser.Get(long.Parse(newMessage)).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал участие Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
                try
                {
                    await botClient.SendTextMessageAsync(-1002074764678, $"Пользователь Id:{newMessage} получает бан на участие в RightVision за нарушение правил ивента.");
                    await botClient.BanChatMemberAsync(-1002074764678, long.Parse(newMessage));
                }
                catch { }
            }
        }

        public static async Task Critic(ITelegramBotClient botClient, Message message)
        {
            string newMessage = message.Text.ToLower().Replace("заблокировать судейство ", "");
            var criticAsList = database.Read($"SELECT `userId` FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "userId");
            if (criticAsList.FirstOrDefault() == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = '{newMessage}';", "");

                await botClient.SendTextMessageAsync(message.Chat, $"Судейство пользователя Id:{newMessage} заблокировано");
                await botClient.SendTextMessageAsync(long.Parse(newMessage),
                    string.Format(Language.GetPhrase("Critic_Messages_FormBlocked",
                        RvUser.Get(long.Parse(newMessage)).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал судейство Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
            }
        }
    }
}
