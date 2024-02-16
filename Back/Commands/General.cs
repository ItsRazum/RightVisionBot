using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back.Commands;

public class General
{
    public static async Task Registration(ITelegramBotClient botClient, Message message)
    {
        string? msgText = message.Text;
        if (msgText != null)
        {
            switch (msgText.ToLower())
            {
                case "/start":
                    if (message.Chat.Type == ChatType.Private)
                        await botClient.SendTextMessageAsync(message.Chat, "Choose lang:",
                            replyMarkup: Keyboard.СhooseLang);
                    break;
                case "🇷🇺ru / cis":
                    if (message.Chat.Type == ChatType.Private)
                        await HubClass.Hub(botClient, message, "ru");
                    break;
                case "🇺🇦ua":
                    if (message.Chat.Type == ChatType.Private)
                        await HubClass.Hub(botClient, message, "ua");
                    break;
                case "🇰🇿kz":
                    if (message.Chat.Type == ChatType.Private)
                        await HubClass.Hub(botClient, message, "kz");
                    break;
            }
        }
    }

    public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Update update)
    {
        var message = update.Message;
        string? msgText = message.Text;
        switch (msgText.ToLower())
        {
            case "/profile":
                if (message.ReplyToMessage != null && message.ReplyToMessage.From.IsBot)
                    await botClient.SendTextMessageAsync(message.Chat, "🧾 Мой профиль RightVision:\n———\n🪪Статус: БОТ!!!!!\n🎖Категория участия: 🤓Душнила\n📍Место проживания: Хостинг за 150р\n💿Трек: Never Gonna Give You Up");
                else
                    await UserProfile.Profile(message);
                break;
            case "/about":
                await botClient.SendTextMessageAsync(message.Chat, "RightVision Bot\n" +
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
                                                                   "- @banan41ck - за рисование аватарки для бота");
                break;
            default:
                if (rvUser.RvLocation == RvLocation.MemberForm)
                    Forms.Member.Form(botClient, message);
                else if (rvUser.RvLocation == RvLocation.CriticForm)
                    Forms.Critic.Form(botClient, message);
                break;
        }

        if (message.Text == Language.GetPhrase("Keyboard_Choice_MainMenu", rvUser.Lang) && message.Chat.Type == ChatType.Private)
        {
            string fullName = message.From.FirstName + " " + message.From.LastName; 
            await botClient.SendTextMessageAsync(message.Chat, "✅", replyMarkup: Keyboard.remove);
            await HubClass.Hub(botClient, message, rvUser.Lang);
        }
            
    }
}