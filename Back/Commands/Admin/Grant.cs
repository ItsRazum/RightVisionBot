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
        public static async Task Execute(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if(_rvUser.Has(Permission.Grant))
                if (message.ReplyToMessage != null)
                { 
                    RvUser rvUser = RvUser.Get(message.ReplyToMessage.From.Id); 
                    rvUser.Role = Enum.Parse<Role>(message.Text.ToLower().Replace("назначить ", ""));
                    await botClient.SendTextMessageAsync(message.Chat, "Права назначены!");
                    await botClient.SendTextMessageAsync(message.ReplyToMessage.From.Id, $"Уважаемый пользователь!\nТы назначаешься на должность: {rvUser.Role}");
                }
                else
                {
                    string[] args = message.Text.ToLower().Replace("назначить", "").Split(' ');
                    RvUser rvUser = RvUser.Get(long.Parse(args[0]));
                    rvUser.Role = Enum.Parse<Role>(args[1]);
                    await botClient.SendTextMessageAsync(message.Chat, "Права назначены!");
                    await botClient.SendTextMessageAsync(rvUser.UserId, $"Уважаемый пользователь!\nТы назначаешься на должность: {rvUser.Role}");
                }
            else
                Permissions.NoPermission(message);
        }
    }
}
