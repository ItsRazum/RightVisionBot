using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back.Commands.Admin;
using RightVisionBot.Common;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Callbacks
{
    class Admin
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = callback.Data;
            if (rvUser.Has(Permission.Ban))
            {
                switch (callbackQuery)
                {
                    case "h_kick":
                        await Restriction.KickHares(botClient, callback.Message.Chat.Id);
                        break;
                    case "h_notkick":
                        await botClient.EditMessageTextAsync(callback.Message.Chat.Id,
                            callback.Message.MessageId, "Кик безбилетников отменён!");
                        break;
                }
            }
            else
                Permissions.NoPermission(callback.Message.Chat);
        }
    }
}
