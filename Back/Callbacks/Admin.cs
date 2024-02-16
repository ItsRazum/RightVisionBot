using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back.Commands.Admin;
using RightVisionBot.Common;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.User;

namespace RightVisionBot.Back.Callbacks
{
    class Admin
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            var message = callback.Message;
            var chat = message.Chat;
            var callbackQuery = callback.Data;
            if (rvUser.Has(Permission.Ban))
            {
                switch (callbackQuery)
                {
                    case "h_kick":
                        await Restriction.KickHares(botClient, chat.Id);
                        break;
                    case "h_notkick":
                        await botClient.EditMessageTextAsync(chat.Id, message.MessageId, "Кик безбилетников отменён!");
                        break;
                }
            }
            else
                Permissions.NoPermission(message.Chat);
        }
    }
}
