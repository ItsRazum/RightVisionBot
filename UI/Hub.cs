using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using RightVisionBot.UI;

//код главного меню
namespace RightVisionBot.UI
{
    class HubClass
    {
        static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");
        public static void Hub(ITelegramBotClient botClient, Update update, string lang)
        {
            var message = update.Message;
            long userId = message.From.Id;

            if (Program.GetUser(userId) != null)
            {
                Program.GetUser(userId).lang = lang;
                string updateUserLang = $"UPDATE `RV_Users` SET `lang` = '{Program.GetUser(userId).lang}' WHERE `Id` = {Program.GetUser(userId).userId}";
                database.Read(updateUserLang, "");
                Program.updateLocation(userId, "mainmenu");
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл главное меню на языке {lang}\n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
            }
            else
            {
                RV_User RV_User = new RV_User();
                RV_User = new();
                RV_User.userName = message.From.FirstName + " " + message.From.LastName;
                RV_User.userId = userId;
                RV_User.lang = lang;
                RV_User.location = "mainmenu";
                Program.users.Add(RV_User);
                var userToDB = $"INSERT INTO `RV_Users` (`username`, `id`, `lang`) VALUES ('{Program.GetUser(userId).userName}', '{Program.GetUser(userId).userId}', '{Program.GetUser(userId).lang}')";
                database.Read(userToDB, "");
                botClient.SendTextMessageAsync(-4074101060, $"Зарегистрирован новый пользователь @{message.From.Username} с языком {lang}", disableNotification: true);
            }
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", Program.GetUser(userId).lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", Program.GetUser(userId).lang) + "📨")
                    },
                    new[] { new KeyboardButton(SendingStatus(userId) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", Program.GetUser(userId).lang) + "👤") }
                })
            { ResizeKeyboard = true };

            string[] langs = new[] { "🇷🇺RU / CIS", "🇺🇦UA", "🇰🇿KZ", "🇬🇧EN" };
            if (langs.Contains(message.Text))
            { botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_LanguageSelected", Program.GetUser(userId).lang), message.Text)); }
            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Messages_Greetings", Program.GetUser(userId).lang), Program.GetUser(userId).userName), replyMarkup: Hub);
        }

        public static void SelectRole(ITelegramBotClient botClient, Update update)
        {
            long userId = update.Message.From.Id;
            string buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic", Program.GetUser(userId).lang);
            string buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member", Program.GetUser(userId).lang);


            if (Program.StringExists("RV_Critics", userId) == true)
                buttonCriticText = Language.GetPhrase("Keyboard_Choice_Critic_Already_Sent", Program.GetUser(userId).lang);

            if (Program.StringExists("RV_Members", userId) == true)
                buttonMemberText = Language.GetPhrase("Keyboard_Choice_Member_Already_Sent", Program.GetUser(userId).lang);


            ReplyKeyboardMarkup chooseRole = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(buttonCriticText),
                        new KeyboardButton(buttonMemberText)
                    },
                    new []
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", Program.GetUser(userId).lang))
                    }
                })
            { ResizeKeyboard = true };
            botClient.SendTextMessageAsync(update.Message.Chat, string.Format(Language.GetPhrase("Messages_ChooseRole", Program.GetUser(userId).lang), Program.GetUser(userId).userName), parseMode: ParseMode.Html, replyMarkup: chooseRole);
            Program.updateLocation(userId, "mainmenu");
        }

        public static string SendingStatus(long userId)
        {
            var query = $"SELECT * FROM RV_Sending WHERE `id` = '{userId}';";
            var IdList = database.Read(query, "id");
            string id = IdList.FirstOrDefault();

            if (userId.ToString() == id) return Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", Program.GetUser(userId).lang);
            else return Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", Program.GetUser(userId).lang);
        }

        public static void SubscribeSending(ITelegramBotClient botClient, Update update)
        { var message = update.Message;
            long userId = message.From.Id;
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", Program.GetUser(userId).lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", Program.GetUser(userId).lang) + "📨")
                    },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", Program.GetUser(userId).lang) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", Program.GetUser(userId).lang) + "👤") }
                })
                { ResizeKeyboard = true };


            var query = $"SELECT * FROM RV_Sending WHERE `id` = '{userId}';";
            var IdList = database.Read(query, "id");
            string id = IdList.FirstOrDefault();
            if (userId.ToString() != id)
            {
                var toDB = $"INSERT INTO `RV_Sending` (`id`) VALUES ('{userId}');";
                database.Read(toDB, "");
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Keyboard_Choice_Sending_Subscribe_Success", Program.GetUser(userId).lang), replyMarkup: Hub);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} подписался на новостную рассылку", disableNotification: true);
            }
        }

        public static void UnsubscribeSending(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            long userId = message.From.Id;
            ReplyKeyboardMarkup Hub = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_About", Program.GetUser(userId).lang) + "❓"),
                        new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Apply", Program.GetUser(userId).lang) + "📨")
                    },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", Program.GetUser(userId).lang) + "📬") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MyProfile", Program.GetUser(userId).lang) + "👤") }
                })
                { ResizeKeyboard = true };


            var query = $"SELECT * FROM RV_Sending WHERE `id` = '{userId}';";
            var IdList = database.Read(query, "id");
            string id = IdList.FirstOrDefault();
            if (userId.ToString() == id)
            {
                var delFromDB = $"DELETE FROM `RV_Sending` WHERE `id` = '{userId}';";
                database.Read(delFromDB, "");
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe_Success", Program.GetUser(userId).lang), replyMarkup: Hub);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отписался от новостной рассылки", disableNotification: true);
            }
        }
    }
}
