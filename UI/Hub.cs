using RightVisionBot.Back;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using RightVisionBot.Common;

//код главного меню
namespace RightVisionBot.UI
{
    class HubClass
    {
        static sql database = Program.database;

        public static void Hub(ITelegramBotClient botClient, Message message, string lang)
        {
            long userId = message.From.Id;
            string fullName = message.From.FirstName + " " + message.From.LastName;

            if (RvUser.Get(userId) != null)
            {
                RvUser.Get(userId).Lang = lang;
                string updateUserLang = $"UPDATE `RV_Users` SET `lang` = '{RvUser.Get(userId).Lang}' WHERE `userId` = {RvUser.Get(userId).UserId}";
                database.Read(updateUserLang, "");
                Program.updateRvLocation(userId, RvLocation.MainMenu);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл главное меню на языке {lang}\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
            else
            {
                RvUser rvUser = new()
                { UserId = userId, Lang = lang };
                Program.users.Add(rvUser);
                var userToDB = $"INSERT INTO `RV_Users` (`id`, `lang`) VALUES ('{rvUser.UserId}', '{rvUser.Lang}')";
                database.Read(userToDB, "");
                botClient.SendTextMessageAsync(-4074101060, $"Зарегистрирован новый пользователь @{message.From.Username} с языком {lang}", disableNotification: true);
            }
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", RvUser.Get(userId).Lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", RvUser.Get(userId).Lang) + "📨")
                    },
                    new[] { new KeyboardButton(SendingStatus(userId) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", RvUser.Get(userId).Lang) + "👤") }
                })
            { ResizeKeyboard = true };

            string[] langs = new[] { "🇷🇺RU / CIS", "🇺🇦UA", "🇰🇿KZ", "🇬🇧EN" };
            if (langs.Contains(message.Text))
            { botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_LanguageSelected", RvUser.Get(userId).Lang), message.Text)); }
            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_Greetings", RvUser.Get(userId).Lang), fullName), replyMarkup: Hub);
        }

        public static void SelectRole(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            string buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic", RvUser.Get(userId).Lang);
            string buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member", RvUser.Get(userId).Lang);


            if (Program.StringExists("RV_Critics", userId) == true)
                buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic_Already_Sent", RvUser.Get(userId).Lang);

            if (Program.StringExists("RV_Members", userId) == true)
                buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member_Already_Sent", RvUser.Get(userId).Lang);


            ReplyKeyboardMarkup chooseRole = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(buttonCriticText),
                        new KeyboardButton(buttonMemberText)
                    },
                    new []
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", RvUser.Get(userId).Lang))
                    }
                })
            { ResizeKeyboard = true };
            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_ChooseRole", RvUser.Get(userId).Lang), parseMode: ParseMode.Html, replyMarkup: chooseRole);
            Program.updateRvLocation(userId, RvLocation.MainMenu);
        }

        public static string SendingStatus(long userId)
        {
            var query = $"SELECT * FROM RV_Sending WHERE `id` = '{userId}';";
            var IdList = database.Read(query, "id");
            string id = IdList.FirstOrDefault();

            if (userId.ToString() == id) return Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", RvUser.Get(userId).Lang);
            else return Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", RvUser.Get(userId).Lang);
        }

        public static void SubscribeSending(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", RvUser.Get(userId).Lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", RvUser.Get(userId).Lang) + "📨")
                    },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", RvUser.Get(userId).Lang) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", RvUser.Get(userId).Lang) + "👤") }
                })
                { ResizeKeyboard = true };

            var IdList = database.Read($"SELECT * FROM RV_Sending WHERE `id` = '{userId}';", "id");
            string id = IdList.FirstOrDefault();

            if (userId.ToString() != id)
            {
                var toDB = $"INSERT INTO `RV_Sending` (`id`) VALUES ('{userId}');";
                database.Read(toDB, "");
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Keyboard_Choice_Sending_Subscribe_Success", RvUser.Get(userId).Lang), replyMarkup: Hub);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} подписался на новостную рассылку", disableNotification: true);
            }
        }

        public static void UnsubscribeSending(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", RvUser.Get(userId).Lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", RvUser.Get(userId).Lang) + "📨")
                    },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", RvUser.Get(userId).Lang) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", RvUser.Get(userId).Lang) + "👤") }
                })
                { ResizeKeyboard = true };

            var IdList = database.Read($"SELECT * FROM RV_Sending WHERE `id` = '{userId}';", "id");
            string id = IdList.FirstOrDefault();
            if (userId.ToString() == id)
            {
                var delFromDB = $"DELETE FROM `RV_Sending` WHERE `id` = '{userId}';";
                database.Read(delFromDB, "");
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe_Success", RvUser.Get(userId).Lang), replyMarkup: Hub);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отписался от новостной рассылки", disableNotification: true);
            }
        }
    }
}
