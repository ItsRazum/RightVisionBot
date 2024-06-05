using RightVisionBot.Back;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using RightVisionBot.Common;
using RightVisionBot.User;

//код главного меню
namespace RightVisionBot.UI
{
    class HubClass
    {
        public static async Task Hub(ITelegramBotClient botClient, Message message, string lang)
        {
            RvUser rvUser;
            long userId = message.From.Id;
            string fullName = message.From.FirstName + " " + message.From.LastName;

            if (RvUser.Get(userId) == null)
            {
                rvUser = new RvUser(
                    userId,
                    lang,
                    Status.User,
                    RvLocation.MainMenu,
                    Role.None,
                    "none",
                    message.From.FirstName,
                    true
                );
                Data.RvUsers.Add(rvUser);
                await botClient.SendTextMessageAsync(-4074101060,
                    $"Зарегистрирован новый пользователь @{message.From.Username} с языком {lang}",
                    disableNotification: true);
            }
            else
            {
                rvUser = RvUser.Get(userId);
                rvUser.Lang = lang;
                Program.UpdateRvLocation(userId, RvLocation.MainMenu);
                await botClient.SendTextMessageAsync(-4074101060,
                    $"Пользователь @{message.From.Username} открыл главное меню на языке {lang}\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}",
                    disableNotification: true);
            }

            string[] langs = new[] { "🇷🇺RU / CIS", "🇺🇦UA", "🇰🇿KZ" };
            if (langs.Contains(message.Text))
                await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_LanguageSelected", RvUser.Get(userId).Lang), message.Text), replyMarkup: Keyboard.remove);
            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_Greetings", RvUser.Get(userId).Lang), fullName), replyMarkup: Keyboard.Hub(rvUser));
        }

        public static string SendingStatus(RvUser rvUser) => Language.GetPhrase(rvUser.Has(Permission.Sending) ? "Keyboard_Choice_Sending_Unsubscribe" : "Keyboard_Choice_Sending_Subscribe", rvUser.Lang);

        public static void SubscribeSending(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var userId = callback.From.Id;
            var rvUser = RvUser.Get(userId);

            rvUser.Permissions.Add(Permission.Sending);
            botClient.EditMessageReplyMarkupAsync(callback.Message.Chat, callback.Message.MessageId, Keyboard.Hub(rvUser));
            botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Keyboard_Choice_Sending_Subscribe_Success", rvUser.Lang));
            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} подписался на новостную рассылку", disableNotification: true);
        }

        public static void UnsubscribeSending(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var userId = callback.From.Id;
            var rvUser = RvUser.Get(userId);
            
            rvUser.Permissions.Remove(Permission.Sending);
            botClient.EditMessageReplyMarkupAsync(callback.Message.Chat, callback.Message.MessageId, Keyboard.Hub(rvUser));
            botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe_Success", rvUser.Lang));
            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отписался от новостной рассылки", disableNotification: true);
        }
    }
}
