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
        var msgText = message.Text;
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

    public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Message message)
    {
        string? msgText = message.Text;
        switch (msgText.ToLower())
        {
            case "/profile":
            case "/profile@rightvisionbot":
                if (message.ReplyToMessage != null && message.ReplyToMessage.From.IsBot)
                    await botClient.SendTextMessageAsync(message.Chat, "🧾 Мой профиль RightVision:\n———\n🪪Статус: БОТ!!!!!\n🎖Категория участия: 🤓Душнила\n📍Место проживания: Хостинг за 150р\n💿Трек: Never Gonna Give You Up");
                else
                {
                    var getId = message.ReplyToMessage == null ? message.From.Id : message.ReplyToMessage.From.Id;
                    await botClient.SendTextMessageAsync(message.Chat, UserProfile.Profile(message), replyMarkup: Keyboard.ProfileOptions(RvUser.Get(getId), message, RvUser.Get(message.From.Id).Lang));
                }

                break;
            case "/about":
                await botClient.SendTextMessageAsync(message.Chat, Program.About);
                break;
            case "//rmkboard":
                if (rvUser.Role == Role.Admin)
                    await botClient.SendTextMessageAsync(message.Chat, "Отключено", replyMarkup: Keyboard.remove);
                break;
            default:
                switch (rvUser.RvLocation)
                {
                    case RvLocation.MemberForm:
                        await Forms.Member.Form(botClient, message);
                        break;
                    case RvLocation.CriticForm:
                        await Forms.Critic.Form(botClient, message);
                        break;
                }
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