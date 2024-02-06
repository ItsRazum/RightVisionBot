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
            string newMessage = message.Text.ToLower().Replace("аннулировать участие ", "");
            if (RvMember.Get(long.Parse(newMessage)) == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{newMessage}';", "");
                MemberRoot.newMembers.Remove(RvMember.Get(long.Parse(newMessage)));

                RvUser.Get(long.Parse(newMessage)).AddPermissions(array: new [] { Permission.SendMemberForm });
                await botClient.SendTextMessageAsync(message.Chat, $"Участие пользователя Id:{newMessage} аннулировано");
                await botClient.SendTextMessageAsync(long.Parse(newMessage),
                    string.Format(Language.GetPhrase("Member_Messages_FormCanceled",
                        RvUser.Get(long.Parse(newMessage)).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал участие Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
            }
        }

        public static async Task Critic(ITelegramBotClient botClient, Message message)
        {
            string newMessage = message.Text.ToLower().ToLower().Replace("аннулировать судейство ", "");

            if (RvCritic.Get(long.Parse(newMessage)) == null)
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
            else
            {
                database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "");
                CriticRoot.newCritics.Remove(RvCritic.Get(long.Parse(newMessage)));
                RvUser.Get(long.Parse(newMessage)).AddPermissions(array: new[] { Permission.SendCriticForm });

                await botClient.SendTextMessageAsync(message.Chat, $"Судейство пользователя Id:{newMessage} аннулировано");
                await botClient.SendTextMessageAsync(long.Parse(newMessage),
                    string.Format(Language.GetPhrase("Critic_Messages_FormCanceled",
                        RvUser.Get(long.Parse(newMessage)).Lang), message.From.FirstName + " " + message.From.LastName));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал судейство Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
            }
        }
    }
}
