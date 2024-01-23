using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Commands.Admin
{
    class Degrade
    {
        public static async Task Role(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if (_rvUser.Has(Permission.Grant))
            {
                try
                {
                    RvUser rvUser;
                    if (message.ReplyToMessage != null)
                    {
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                        rvUser.Role = Enum.Parse<Role>(message.Text.Replace("разжаловать ", ""));
                    }
                    else
                    {
                        string[] args = message.Text.Replace("разжаловать ", "").Split(' ');
                        rvUser = RvUser.Get(long.Parse(args[0]));
                        rvUser.Role = Common.Role.None;
                    }

                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь назначен!");
                    await botClient.SendTextMessageAsync(rvUser.UserId, $"Уважаемый пользователь!\nТы назначаешься на должность: {rvUser.Role}");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемая должность не найдена!");
                }
            }

            else
                Permissions.NoPermission(message);
        }

        public static async Task Perm(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if (_rvUser.Has(Permission.Grant))
            {
                try
                {
                    RvUser rvUser;
                    if (message.ReplyToMessage != null)
                    {
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                        Permission deletedPermission = Enum.Parse<Permission>(message.Text.Replace("-permission ", ""));
                        if (rvUser.Has(deletedPermission))
                        {
                            rvUser.RemovePermission(deletedPermission);
                            await botClient.SendTextMessageAsync(message.Chat,
                                $"С пользователя успешно снято право Permission.{deletedPermission}!");
                        }
                    }
                    else
                    {
                        string[] args = message.Text.Replace("-permission ", "").Split(' ');
                        rvUser = RvUser.Get(long.Parse(args[0]));
                        rvUser.Role = Enum.Parse<Role>(args[1]);
                    }
                    await botClient.SendTextMessageAsync(message.Chat, "Право выдано!");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемое право не найдено!");
                }
            }
            else
                Permissions.NoPermission(message);
        }
    }
}
