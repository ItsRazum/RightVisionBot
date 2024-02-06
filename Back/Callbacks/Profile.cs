using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.UI;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.User;

namespace RightVisionBot.Back.Callbacks
{
    class Profile
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;

            switch (callbackQuery)
            {
                case "menu_permissions":
                case "permissions_minimize":
                    await UserProfile.PermissionsList(botClient, update, rvUser, "minimize");
                    break;
                case "permissions_maximize":
                    await UserProfile.PermissionsList(botClient, update, rvUser, "maximize");
                    break;
                case "menu_history":
                    await UserProfile.PunishmentsList(botClient, update, rvUser);
                    break;
                case "menu_forms":
                    if (!rvUser.Has(Permission.SendCriticForm) && !rvUser.Has(Permission.SendCriticForm))
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "К сожалению, тебе больше нельзя подавать заявки!");
                    else
                        await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "До участия в RightVision всего два шага! Подай заявку на участие или на судейство прямо сейчас!", replyMarkup: Keyboard.Forms(rvUser, rvUser.RvLocation));
                    break;
                case "menu_cancelCritic":
                    Program.database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{callbackUserId}';", "");
                    CriticRoot.newCritics.Remove(RvCritic.Get(callbackUserId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отменил заполнение заявки на судейство\n=====\nId:{callbackUserId}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
                    goto case "menu_forms";
                case "menu_cancelMember":
                    Program.database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{callbackUserId}';", "");
                    MemberRoot.newMembers.Remove(RvMember.Get(callbackUserId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отменил заполнение заявки на участие\n=====\nId:{callbackUserId}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
                    goto case "menu_forms";
            }
        }
    }
}
