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
        static sql database = Program.database;

        public static async Task Hub(ITelegramBotClient botClient, Message message, string lang)
        {
            RvUser rvUser;
            long userId = message.From.Id;
            string fullName = message.From.FirstName + " " + message.From.LastName;

            if (RvUser.Get(userId) != null)
            {
                rvUser = RvUser.Get(userId);
                rvUser.Lang = lang;
                Program.UpdateRvLocation(userId, RvLocation.MainMenu);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл главное меню на языке {lang}\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
            }
            else
            {
                rvUser = new()
                { UserId = userId, Lang = lang };
                Data.RvUsers.Add(rvUser);
                var userToDB = $"INSERT INTO `RV_Users` (`userId`, `lang`, `permissions`) VALUES ('{rvUser.UserId}', '{rvUser.Lang}', '{rvUser.PermissionsAsString(rvUser.Permissions)}')";
                database.Read(userToDB, "");
                await botClient.SendTextMessageAsync(-4074101060, $"Зарегистрирован новый пользователь @{message.From.Username} с языком {lang}", disableNotification: true);
            }

            string[] langs = new[] { "🇷🇺RU / CIS", "🇺🇦UA", "🇰🇿KZ" };
            if (langs.Contains(message.Text))
                await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_LanguageSelected", RvUser.Get(userId).Lang), message.Text), replyMarkup: Keyboard.remove);
            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_Greetings", RvUser.Get(userId).Lang), fullName), replyMarkup: Keyboard.Hub(rvUser));
        }

        public static void SelectRole(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            string buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic", RvUser.Get(userId).Lang);
            string buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member", RvUser.Get(userId).Lang);


            if (Program.StringExists("RV_Critics", userId))
                buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic_Already_Sent", RvUser.Get(userId).Lang);

            if (Program.StringExists("RV_Members", userId))
                buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member_Already_Sent", RvUser.Get(userId).Lang);


            ReplyKeyboardMarkup chooseRole = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(buttonCriticText),
                        new KeyboardButton(buttonMemberText)
                    },
                    new [] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", RvUser.Get(userId).Lang)) }
                })
            { ResizeKeyboard = true };
            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_ChooseRole", RvUser.Get(userId).Lang), parseMode: ParseMode.Html, replyMarkup: chooseRole);
            Program.UpdateRvLocation(userId, RvLocation.MainMenu);
        }

        public static string SendingStatus(RvUser rvUser)
        {
            if (rvUser.Has(Permission.Sending))
                return Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", rvUser.Lang);
            else 
                return Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", rvUser.Lang);
        }

        public static void SubscribeSending(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var userId = callback.From.Id;
            var rvUser = RvUser.Get(userId);

            rvUser.AddPermissions(array: new [] { Permission.Sending });
            botClient.EditMessageReplyMarkupAsync(callback.Message.Chat, callback.Message.MessageId, Keyboard.Hub(rvUser));
            botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Keyboard_Choice_Sending_Subscribe_Success", rvUser.Lang));
            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} подписался на новостную рассылку", disableNotification: true);
        }

        public static void UnsubscribeSending(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var userId = callback.From.Id;
            var rvUser = RvUser.Get(userId);
            
            rvUser.RemovePermission(Permission.Sending);
            botClient.EditMessageReplyMarkupAsync(callback.Message.Chat, callback.Message.MessageId, Keyboard.Hub(rvUser));
            botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe_Success", rvUser.Lang));
            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отписался от новостной рассылки", disableNotification: true);
        }
    }
}
