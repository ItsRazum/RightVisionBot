using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

//система отображения профилей пользователей
namespace RightVisionBot.UI
{
    class UserProfile
    {
        public static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");

        public static void Profile(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            long userId = message.From.Id;
            long getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;

            ReplyKeyboardMarkup keyboardForMember = new(new[]
            {
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_EditTrack", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_SendTrack", Program.GetUser(getId).lang)) }
                })
            { ResizeKeyboard = true };

            ReplyKeyboardMarkup keyboardForOther = new(new[]
            {
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", Program.GetUser(getId).lang)) } })
            { ResizeKeyboard = true };

            ReplyKeyboardMarkup keyboardForCritic = new(new[]
                {
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", Program.GetUser(getId).lang)) }
                })
                { ResizeKeyboard = true };

            ReplyKeyboardMarkup keyboardForCriticAndMember = new(new[]
                {
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_EditTrack", Program.GetUser(getId).lang)) },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_SendTrack", Program.GetUser(getId).lang)) }
                })
                { ResizeKeyboard = true };

            string? status = database.Read($"SELECT `status` FROM `RV_Users` WHERE `id` = {getId};", "status").FirstOrDefault();
            Program.updateLocation(userId, "profile");
            switch (status)
            {
                case "member":
                    if (message.Chat.Type == ChatType.Private)
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Messages_Loading", Program.GetUser(userId).lang), replyMarkup: keyboardForMember);
                    botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, "member"));
                    break;
                case "critic":
                    if (message.Chat.Type == ChatType.Private)
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Messages_Loading", Program.GetUser(userId).lang), replyMarkup: keyboardForCritic);
                    botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, "critic"));
                    break;
                case "user":
                    if (message.Chat.Type == ChatType.Private)
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Messages_Loading", Program.GetUser(userId).lang), replyMarkup: keyboardForOther);
                    botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, "user"));
                    break;
                case "criticAndMember":
                    if (message.Chat.Type == ChatType.Private)
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Messages_Loading", Program.GetUser(userId).lang), replyMarkup: keyboardForCriticAndMember);
                    botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, "criticAndMember"));
                    break;
            }
        }

        private static string GetSendingStatus(long userId, Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                var query = $"SELECT * FROM RV_Sending WHERE `id` = {userId};";
                var IdList = database.Read(query, "id");
                string id = IdList.FirstOrDefault();

                return Language.GetPhrase(id == null ? "Profile_Sending_Status_Inactive" : "Profile_Sending_Status_Active", Program.GetUser(userId).lang);
            }
            else
                return string.Empty;
        }

        private static string RequestTrackAccess(Message message)
        { return message.Chat.Type == ChatType.Private ? MemberRoot.GetMember(message.From.Id).Track : "Скрыт"; }

        private static string GetCandidateStatus(long userId, string role)
        {
            var query = $"SELECT `status` FROM RV_{role}s WHERE `userId` = {userId};";
            var IdList = database.Read(query, "status");
            switch (IdList.FirstOrDefault())
            {
                case "denied":
                    return Language.GetPhrase("Profile_Form_Status_Blocked", Program.GetUser(userId).lang);
                    break;
                case "waiting":
                    return Language.GetPhrase("Profile_Form_Status_Waiting", Program.GetUser(userId).lang);
                    break;
                case "unfinished":
                    return Language.GetPhrase("Profile_Form_Status_Unfinished", Program.GetUser(userId).lang);
                    break;
                case null:
                    return Language.GetPhrase("Profile_Form_Status_Allowed", Program.GetUser(userId).lang);
                    break;
                case "bronze":
                case "steel":
                case "gold":
                case "brilliant":
                    return Language.GetPhrase("Profile_Form_Status_Accepted", Program.GetUser(userId).lang);
                    break;
                default:
                    return Language.GetPhrase("Profile_Form_Status_UnderConsideration", Program.GetUser(userId).lang);
                    break;
            }
        }

        private static string GetFormsStatus(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                long userId = message.From.Id;
                return string.Format(Language.GetPhrase("Profile_Forms",
                        Program.GetUser(userId).lang),
                    GetCandidateStatus(userId, "Member"),
                    GetCandidateStatus(userId, "Critic"));
            }
            else
                return string.Empty;
        }

        private static string CategoryFormat(long userId, string role)
        {
            var query = $"SELECT `status` FROM RV_{role}s WHERE `userId` = {userId};";
            var statusList = database.Read(query, "status");
            string status = statusList.FirstOrDefault();
            switch (status)
            {
                case "bronze":
                    return "🥉Bronze";
                    break;
                case "steel":
                    return "🥈Steel";
                    break;
                case "gold":
                    return "🥇Gold";
                    break;
                case "brilliant":
                    return "💎Brilliant";
                    break;
                default:
                    return string.Empty;
                    break;
            }
        }

        private static string ProfileFormat(Message message, string role)
        {
            long userId = message.From.Id;
            long getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;
            string header = message.ReplyToMessage == null
                ? Language.GetPhrase("Profile_Private_Header", Program.GetUser(getId).lang)
                : string.Format(Language.GetPhrase("Profile_Global_Header", Program.GetUser(userId).lang), message.ReplyToMessage.From.FirstName);
            string userLayout;
            string memberLayout;
            string criticLayout;
            string criticAndMemberLayout;
            string sending = message.Chat.Type == ChatType.Private ? string.Format(Language.GetPhrase("Profile_Sending_Status", Program.GetUser(getId).lang), GetSendingStatus(getId, message)) : string.Empty;


            switch (role)
            {
                case "user":
                    userLayout = Language.GetPhrase("Profile_User_Layout", Program.GetUser(userId).lang);
                    return header + userLayout + sending + GetFormsStatus(message);
                    break;

                case "member":
                    memberLayout = string.Format(Language.GetPhrase("Profile_Member_Layout", Program.GetUser(userId).lang),
                        /*0*/CategoryFormat(getId, "Member"),
                        /*1*/MemberRoot.GetMember(getId).Country,
                        /*2*/MemberRoot.GetMember(getId).City,
                        /*3*/RequestTrackAccess(message));
                    return header + memberLayout + sending + GetFormsStatus(message);
                    break;

                case "critic":
                    criticLayout = string.Format(Language.GetPhrase("Profile_Critic_Layout", Program.GetUser(userId).lang),
                        /*0*/CategoryFormat(getId, "Critic"));
                    return header + criticLayout + sending + GetFormsStatus(message);
                    break;

                case "criticAndMember":
                    criticAndMemberLayout = string.Format(Language.GetPhrase("Profile_CriticAndMember_Layout", Program.GetUser(userId).lang),
                        /*0*/CategoryFormat(getId, "Member"),
                        /*1*/MemberRoot.GetMember(getId).Country,
                        /*2*/MemberRoot.GetMember(getId).City,
                        /*3*/RequestTrackAccess(message));
                    return header + criticAndMemberLayout + sending + GetFormsStatus(message);
                    break;
                default:
                    return "Неизвестная ошибка";
            }
        }
    }
}
