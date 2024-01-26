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
            if (rvUser.Has(Permission.Ban))
            {
                var callback = update.CallbackQuery;
                long callbackUserId = callback.From.Id;
                var callbackQuery = callback.Data;
                string fullname = callback.From.FirstName + callback.From.LastName;

                switch (callbackQuery)
                {
                    case "h_kick":
                        await Restriction.KickHares(botClient, update.CallbackQuery.Message.Chat.Id);
                        break;
                    case "h_notkick":
                        await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                            callback.Message.MessageId, "Кик безбилетников отменён!");
                        break;
                }
            }
        }
    }
}
