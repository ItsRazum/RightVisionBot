using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.UI;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.User;

namespace RightVisionBot.Back.Callbacks
{
    class MainMenu
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = callback.Data;
            long callbackUserId = callback.From.Id;
            string fullname = callback.From.FirstName + callback.From.LastName;

            await Profile.Callbacks(botClient, update, rvUser);
            switch (callbackQuery)
            {
                case "menu_sending":
                    if (rvUser.Has(Permission.Sending))
                        HubClass.UnsubscribeSending(botClient, callback);
                    else
                        HubClass.SubscribeSending(botClient, callback);
                    break;
                case "menu_about":
                    await botClient.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, Language.GetPhrase("Messages_About", rvUser.Lang), replyMarkup: Keyboard.About(rvUser));
                    break;
                case "menu_main":
                case "menu_back":
                    Program.UpdateRvLocation(callbackUserId, RvLocation.MainMenu);
                    await botClient.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, string.Format(Language.GetPhrase("Messages_Greetings", rvUser.Lang), fullname), replyMarkup: Keyboard.Hub(rvUser));
                    break;
                case "menu_aboutBot":
                    await botClient.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "RightVision Bot\n" +
                        $"Дата сборки: {ConfigReader.BuildDate}\n" +
                        "Разработчик: @NtRazum\n\n" +
                        "Переводчики:\n" +
                        "UA: @crink1337 & @elec7reify\n" +
                        "KZ: @chrkovsky\n" +
                        "\nИспользованный стек:\n" +
                        "- Язык программирования: C#\n" +
                        "Использованные библиотеки:\n" +
                        "- Telegram.Bot\n" +
                        "- MySQL Connector\n" +
                        "Прочее ПО:\n" +
                        "- PhpMyAdmin\n" +
                        "-.NET 7.0\n\n" +
                        "Особые благодарности:\n" +
                        "- @Viktor290906 - за перевод старой версии английского языка\n" +
                        "- @elec7reify - за минимальную, но всё же помощь в разработке\n" +
                        "- @banan41ck - за рисование аватарки для бота", replyMarkup: Keyboard.BackToAbout(rvUser));
                    break;
                case "menu_profile":
                    if (rvUser.Has(Permission.OpenProfile))
                    {
                        Message message = new()
                        {
                            Chat = callback.Message.Chat,
                            From = new Telegram.Bot.Types.User()
                                { Id = callback.From.Id, }
                        };
                        Program.UpdateRvLocation(callbackUserId, RvLocation.Profile);
                        await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                            UserProfile.ProfileFormat(message, RvUser.Get(callbackUserId)),
                            replyMarkup: Keyboard.ProfileOptions(RvUser.Get(callbackUserId), message));
                    }
                    else
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "К сожалению, у тебя нет права открывать профиль.", showAlert: true);
                    break;
            }
        }
    }
}