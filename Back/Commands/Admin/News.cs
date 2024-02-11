using RightVisionBot.Common;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class News
    {
        public static async Task Common(ITelegramBotClient botClient, Message message, RvUser rvUser)
        {
            if (rvUser.Has(Permission.News))
            {
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал новостную рассылку\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
                string newMessage = message.Text.Replace("/news ", "");
                int i = 0, b = 0;
                var subs = from sub in Program.users where sub.Has(Permission.Sending) select sub.UserId;
                foreach (var sub in subs)
                    try { await botClient.SendTextMessageAsync(sub, newMessage); }
                    catch { b++; }

                Console.WriteLine($"Рассылка завершена. {i} получили сообщение, {b} не получили");
            }
        }

        public static async Task Tech(ITelegramBotClient botClient, Message message, RvUser rvUser)
        {
            if (rvUser.Has(Permission.TechNews))
            {
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал техническую рассылку\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
                string newMessage = message.Text.Replace("/tech ", "");
                int i = 0, b = 0;
                foreach (var user in Program.users)
                    try { await botClient.SendTextMessageAsync(user.UserId, newMessage); i++; }
                    catch { b++; }

                Console.WriteLine($"Рассылка завершена. {i} получили сообщение, {b} не получили");
            }
        }

        public static async Task Member(ITelegramBotClient botClient, Message message, RvUser rvUser)
        {
            if (rvUser.Has(Permission.MemberNews))
            {
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал новостную рассылку для гачимейкеров\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(message.From.Id).Lang}", disableNotification: true);
                string newMessage = message.Text.Replace("/membernews ", "");
                int i = 0, b = 0;
                foreach (var member in MemberRoot.newMembers)
                    try { await botClient.SendTextMessageAsync(member.UserId, newMessage); i++; }
                    catch { b++; }

                Console.WriteLine($"Рассылка завершена. {i} получили сообщение, {b} не получили");
            }
            else
                Permissions.NoPermission(message.Chat);
        }
    }
}
