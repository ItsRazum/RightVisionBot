using RightVisionBot.Common;
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
                case "menu_forms":
                    if (!rvUser.Has(Permission.SendCriticForm) && !rvUser.Has(Permission.SendCriticForm))
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "Messages_FormsBlocked");
                    else
                        await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                            Language.GetPhrase("Messages_SendFormRightNow", rvUser.Lang),
                            replyMarkup: Keyboard.Forms(rvUser, rvUser.RvLocation));
                    break;
                case "menu_cancelCritic":
                    Program.database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{callbackUserId}';", "");
                    Data.RvCritics.Remove(RvCritic.Get(callbackUserId));
                    await botClient.SendTextMessageAsync(-4074101060,
                        $"Пользователь @{callback.From.Username} отменил заполнение заявки на судейство\n=====\nId:{callbackUserId}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}",
                        disableNotification: true);
                    goto case "menu_forms";
                case "menu_cancelMember":
                    Program.database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{callbackUserId}';", "");
                    Data.RvMembers.Remove(RvMember.Get(callbackUserId));
                    await botClient.SendTextMessageAsync(-4074101060,
                        $"Пользователь @{callback.From.Username} отменил заполнение заявки на участие\n=====\nId:{callbackUserId}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}",
                        disableNotification: true);
                    goto case "menu_forms";
            }

            if (callbackQuery.StartsWith("menu_permissions-"))
                await UserProfile.PermissionsList(callback, RvUser.Get(long.Parse(callbackQuery.Replace("menu_permissions-", ""))), "minimize");

            else if (callbackQuery.StartsWith("permissions_back-"))
            {
                Telegram.Bot.Types.User from = new()
                {
                    Id = long.Parse(callbackQuery.Replace("permissions_back-", ""))
                };
                Message plugMessage = new()
                {
                    Chat = callback.Message.Chat,
                    From = from,
                    ReplyToMessage = new Message()
                    {
                        Chat = callback.Message.Chat,
                        From = from
                    }
                };
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                    RvUser.Get(plugMessage.From.Id).ProfilePublic(RvUser.Get(callback.From.Id).Lang), replyMarkup: Keyboard.ProfileOptions(RvUser.Get(plugMessage.From.Id), plugMessage, RvUser.Get(callback.From.Id).Lang));
            }

            else if (callbackQuery.StartsWith("permissions_minimize-"))
                await UserProfile.PermissionsList(callback, RvUser.Get(long.Parse(callbackQuery.Replace("permissions_minimize-", ""))), "minimize");

            else if (callbackQuery.StartsWith("permissions_maximize-"))
                await UserProfile.PermissionsList(callback, RvUser.Get(long.Parse(callbackQuery.Replace("permissions_maximize-", ""))), "maximize");

            else if (callbackQuery.StartsWith("menu_history-"))
                await UserProfile.PunishmentsList(update, RvUser.Get(long.Parse(callbackQuery.Replace("menu_history-", ""))));
        }
    }
}
