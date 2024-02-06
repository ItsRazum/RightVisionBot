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
            if (_rvUser.Has(Permission.Degrade))
            {
                RvUser rvUser;
                rvUser = RvUser.Get(message.ReplyToMessage != null ? message.ReplyToMessage.From.Id : long.Parse(message.Text.Replace("разжаловать", "")));

                rvUser.Permissions = rvUser.Status switch
                {
                    Status.Critic =>          PermissionLayouts.Critic,
                    Status.Member =>          PermissionLayouts.Member,
                    Status.CriticAndMember => PermissionLayouts.CriticAndMember,
                    _ => PermissionLayouts.User,
                };
                rvUser.Role = Common.Role.None;

                await botClient.SendTextMessageAsync(message.Chat, "Пользователь разжалован!");
                await botClient.SendTextMessageAsync(rvUser.UserId, "Уважаемый пользователь!\nК сожалению, ты был разжалован со своей должности.");
            }

            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task Perm(ITelegramBotClient botClient, Message message, RvUser rvUser2)
        {
            if (rvUser2.Has(Permission.DegradePermission))
            {
                try
                {
                    RvUser rvUser;
                    Permission deletedPermission;
                    if (message.ReplyToMessage != null)
                    {
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                        deletedPermission = Enum.Parse<Permission>(message.Text.Replace("-permission ", ""));
                        if (rvUser.Has(deletedPermission))
                            rvUser.RemovePermission(deletedPermission);
                    }
                    else
                    {
                        string[] args = message.Text.Replace("-permission ", "").Split(' ');
                        rvUser = RvUser.Get(long.Parse(args[0]));
                        deletedPermission = Enum.Parse<Permission>(args[1]);
                        if (rvUser.Has(deletedPermission))
                            rvUser.RemovePermission(deletedPermission);
                    }
                    await botClient.SendTextMessageAsync(message.Chat, $"С пользователя успешно снято право Permission.{deletedPermission}!");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемое право не найдено!");
                }
            }
            else
                Permissions.NoPermission(message.Chat);
        }
    }
}
