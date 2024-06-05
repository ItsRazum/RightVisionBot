using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.Common;
using RightVisionBot.User;

namespace RightVisionBot.Back.Commands.Admin
{
    class Cancel
    {
        private static sql database = Program.database;
        public static async Task Participation(ITelegramBotClient botClient, Message message)
        {
            long newMessage = long.Parse(message.Text.ToLower().Replace("аннулировать участие ", ""));
            if (RvMember.Get(newMessage) == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{newMessage}';", "");
                Data.RvMembers.Remove(RvMember.Get(newMessage));

                RvUser.Get(newMessage).Permissions.Add(Permission.SendMemberForm);
                await botClient.SendTextMessageAsync(message.Chat, $"Участие пользователя Id:{newMessage} аннулировано");
                await botClient.SendTextMessageAsync(newMessage,
                    string.Format(Language.GetPhrase("Member_Messages_FormCanceled",
                        RvUser.Get(newMessage).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал участие Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
            }
        }

        public static async Task Critic(ITelegramBotClient botClient, Message message)
        {
            long newMessage = long.Parse(message.Text.ToLower().Replace("аннулировать судейство ", ""));

            if (RvCritic.Get(newMessage) == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "");
                Data.RvCritics.Remove(RvCritic.Get(newMessage));
                RvUser.Get(newMessage).Permissions.Add(Permission.SendCriticForm);

                await botClient.SendTextMessageAsync(message.Chat, $"Судейство пользователя Id:{newMessage} аннулировано");
                await botClient.SendTextMessageAsync(newMessage,
                    string.Format(Language.GetPhrase("Critic_Messages_FormCanceled",
                        RvUser.Get(newMessage).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал судейство Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
            }
        }
    }
}
