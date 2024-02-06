using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Grant
    {
        public static async Task Role(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if (_rvUser.Has(Permission.Grant))
                try
                {
                    RvUser rvUser;
                    Role newRole;
                    if (message.ReplyToMessage != null)
                    {
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                        newRole = Enum.Parse<Role>(message.Text.Replace("назначить ", ""));
                    }
                    else
                    {
                        string[] args = message.Text.Replace("назначить ", "").Split(' ');
                        rvUser = RvUser.Get(long.Parse(args[0]));
                        newRole = Enum.Parse<Role>(args[1]);
                    }

                    rvUser.Role = newRole;
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь назначен!");
                    await botClient.SendTextMessageAsync(rvUser.UserId, $"Уважаемый пользователь!\nПоздравляю с назначением на должность: {rvUser.Role}\nГордись своим положением и приноси пользу RightVision и всему гачи в целом!");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемая должность не найдена!");
                }

            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task Perm(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if (_rvUser.Has(Permission.Grant))
                try
                {
                    Permission newPermission;
                    RvUser rvUser;
                    if (message.ReplyToMessage != null)
                    {
                        newPermission = Enum.Parse<Permission>(message.Text.Replace("+permission ", ""));
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                    }
                    else
                    {
                        string[] args = message.Text.Replace("+permission ", "").Split(' ');
                        newPermission = Enum.Parse<Permission>(args[1]);
                        rvUser = RvUser.Get(long.Parse(args[0]));
                    }

                    try { rvUser.AddPermissions(array: new[] { newPermission }); }
                    catch { await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!"); }

                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь получает право: Permission.{newPermission}");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемое право не найдено!");
                }
            else
                Permissions.NoPermission(message.Chat);
        }
    }
}
