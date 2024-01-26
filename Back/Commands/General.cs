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
        long userId = message.From.Id;
        if (RvUser.Get(userId) == null)
            if (msgText != null)
            {
                switch (msgText.ToLower())
                {
                    case "/start":
                        if (message.Chat.Type == ChatType.Private)
                            await botClient.SendTextMessageAsync(message.Chat, "Choose lang:",
                                replyMarkup: Keyboard.chooseLang);
                        break;
                    case "🇷🇺ru / cis":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "ru");
                        break;
                    case "🇺🇦ua":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "ua");
                        break;
                    case "🇰🇿kz":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "kz");
                        break;
                    case "🇬🇧en":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "en");
                        break;
                }
            }
            else if (RvUser.Get(userId) != null && RvUser.Get(userId).RvLocation != RvLocation.Blacklist)
            {
                switch (msgText.ToLower())
                {
                    case "/start":
                        if (message.Chat.Type == ChatType.Private)
                            await botClient.SendTextMessageAsync(message.Chat, "Choose lang:",
                                replyMarkup: Keyboard.chooseLang);
                        break;
                    case "🇷🇺ru / cis":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "ru");
                        break;
                    case "🇺🇦ua":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "ua");
                        break;
                    case "🇰🇿kz":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "kz");
                        break;
                    case "🇬🇧en":
                        if (message.Chat.Type == ChatType.Private)
                            HubClass.Hub(botClient, message, "en");
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
            case "/admin":
                if (message.From.Id == 901152811)
                {
                    rvUser.AddPermissions(hashSet:PermissionLayouts.Admin);
                    rvUser.Role = Role.Admin;
                    await botClient.SendTextMessageAsync(message.Chat, "Полный доступ получен");
                }
                break;
            default:
                if (rvUser.RvLocation == RvLocation.MemberForm)
                    Forms.Member.Form(botClient, message);
                else if (rvUser.RvLocation == RvLocation.CriticForm)
                    Forms.Critic.Form(botClient, message);
                break;
        }

        if (message.Text == Language.GetPhrase("Keyboard_Choice_Apply", rvUser.Lang) + "📨" && message.Chat.Type == ChatType.Private)
            HubClass.SelectRole(botClient, message);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang) && rvUser.RvLocation == RvLocation.TrackCard)
            Track.Send(botClient, message);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_Critic", rvUser.Lang) && message.Chat.Type == ChatType.Private)
            CriticRoot.EnterName(botClient, update);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_Member", rvUser.Lang) && message.Chat.Type == ChatType.Private)
            MemberRoot.EnterName(botClient, update);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_About", rvUser.Lang) + "❓" && message.Chat.Type == ChatType.Private)
            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_About", rvUser.Lang));

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_MainMenu", rvUser.Lang) && message.Chat.Type == ChatType.Private)
            HubClass.Hub(botClient, message, rvUser.Lang);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", rvUser.Lang) + "📬" && message.Chat.Type == ChatType.Private)
            HubClass.SubscribeSending(botClient, message);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", rvUser.Lang) + "📬" && message.Chat.Type == ChatType.Private)
            HubClass.UnsubscribeSending(botClient, message);

        else if (message.Text == Language.GetPhrase("Keyboard_Choice_MyProfile", rvUser.Lang) + "👤" && message.Chat.Type == ChatType.Private)
            await UserProfile.Profile(message);
    }
}