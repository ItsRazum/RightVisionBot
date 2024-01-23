using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back;
using RightVisionBot.Common;
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
        public static ITelegramBotClient botClient = Program.botClient;
        public static sql database = Program.database;

        public static async Task Profile(Message message)
        {
            long userId = message.From.Id;
            long getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;

            RvUser rvUser = RvUser.Get(getId);
            if (message.Chat.Type == ChatType.Private)
                Program.updateRvLocation(userId, RvLocation.Profile);
            await botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, rvUser));
        }

        private static string GetSendingStatus(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
                return Language.GetPhrase(Data.Subscribers.Contains(message.From.Id) ? "Profile_Sending_Status_Inactive" : "Profile_Sending_Status_Active", RvUser.Get(message.From.Id).Lang);
            else
                return string.Empty;
        }

        private static string RequestTrackAccess(Message message) 
            => message.ReplyToMessage == null ? RvMember.Get(message.From.Id).Track : RvMember.Get(message.ReplyToMessage.From.Id).Track;

        private static string GetCandidateStatus(long userId, string role)
        {
            try
            {
                var query = $"SELECT `status` FROM RV_{role}s WHERE `userId` = {userId};";
                var IdList = database.Read(query, "status");
                switch (IdList.FirstOrDefault())
                {
                    case "denied":
                        return Language.GetPhrase("Profile_Form_Status_Blocked", RvUser.Get(userId).Lang);
                        break;
                    case "waiting":
                        return Language.GetPhrase("Profile_Form_Status_Waiting", RvUser.Get(userId).Lang);
                        break;
                    case "unfinished":
                        return Language.GetPhrase("Profile_Form_Status_Unfinished", RvUser.Get(userId).Lang);
                        break;
                    case null:
                        return Language.GetPhrase("Profile_Form_Status_Allowed", RvUser.Get(userId).Lang);
                        break;
                    case "bronze":
                    case "steel":
                    case "gold":
                    case "brilliant":
                        return Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang);
                        break;
                    default:
                        return Language.GetPhrase("Profile_Form_Status_UnderConsideration", RvUser.Get(userId).Lang);
                        break;
                }
            }
            catch
            {
                return "Не удалось получить статус!";
            }
        }

        private static string GetFormsStatus(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                long userId = message.From.Id;
                return string.Format(Language.GetPhrase("Profile_Forms", RvUser.Get(userId).Lang),
                    GetCandidateStatus(userId, "Member"),
                    GetCandidateStatus(userId, "Critic")) + "\n";
            }
            else
                return string.Empty;
        }

        private static string RoleFormat(RvUser rvUser) =>
            rvUser.Role == Role.None 
                ? GetUserStatus(rvUser) 
                : GetUserStatus(rvUser) + "\n" + string.Format(Language.GetPhrase("Profile_Role", rvUser.Lang), RoleAsString(rvUser));

        private static string GetUserStatus(RvUser rvUser)
        {
            switch (rvUser.Status)
            {
                case Status.Member:          return Language.GetPhrase("Profile_Member_Header", rvUser.Lang);
                case Status.Critic:          return Language.GetPhrase("Profile_Critic_Header", rvUser.Lang);
                case Status.CriticAndMember: return Language.GetPhrase("Profile_CriticAndMember_Header", rvUser.Lang);
                default:
                    return Language.GetPhrase("Profile_User_Layout", rvUser.Lang);
            }
        }

        private static string RoleAsString(RvUser rvUser)
        {
            switch (rvUser.Role)
            {
                case Role.Admin:      return "Главный организатор\n";
                case Role.Moderator:  return "Модератор\n";
                case Role.TechAdmin:  return "Техадмин\n";
                case Role.Developer:  return "Разработчик\n";
                case Role.Curator:    return "Куратор\n";
                case Role.Designer:   return "Дизайнер\n";
                case Role.Translator: return "Переводчик\n";
                default:
                    return string.Empty;
            }
        }

        private static string CategoryFormat(long userId)
        {

            return RvUser.Get(userId).Category switch
            {
                "bronze" =>    "🥉Bronze",
                "steel" =>     "🥈Steel",
                "gold" =>      "🥇Gold",
                "brilliant" => "💎Brilliant",
                _ => string.Empty
            };
        }

        private static ReplyKeyboardMarkup KeyboardFormat(long userId, Status status)
        {
            return status switch
            {
                Status.Member => Keyboard.ForMember(userId),
                Status.Critic => Keyboard.ForCritic(userId),
                Status.CriticAndMember => Keyboard.ForCriticAndMember(userId),
                _ => Keyboard.ForOther(userId)
            };
        }

        private static string RewardsFormat(RvUser rvUser)
        {
            string header = Language.GetPhrase("Profile_Form_Rewards", rvUser.Lang);
            StringBuilder sb = new StringBuilder();
            if (rvUser.Rewards.FirstOrDefault() == null)
                return header + "Кажется, здесь ничего нет!";
            else
            {
                foreach (var dictionary in rvUser.Rewards)
                    foreach (var reward in dictionary)
                        sb.Append("|" + reward.Value + "\n");

                return header + sb;
            }
        }

        private static string ProfileFormat(Message message, RvUser rvUser)
        {
            long userId = message.From.Id;
            string lang = RvUser.Get(userId).Lang;
            long getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;
            string sending = message.Chat.Type == ChatType.Private ? string.Format(Language.GetPhrase("Profile_Sending_Status", lang), GetSendingStatus(message)) : string.Empty;
            string optional = sending + GetFormsStatus(message) + RewardsFormat(rvUser);
            string header = message.ReplyToMessage == null ?
                Language.GetPhrase("Profile_Private_Header", lang) : 
                string.Format(Language.GetPhrase("Profile_Global_Header", lang), message.ReplyToMessage.From.FirstName);

            if (message.Chat.Type == ChatType.Private)
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Messages_Loading", lang), replyMarkup: KeyboardFormat(userId, rvUser.Status));



            switch (rvUser.Status)
            {
                case Status.User:
                    return header + RoleFormat(rvUser) + optional;

                case Status.Member:
                    string memberLayout = string.Format(Language.GetPhrase("Profile_Member_Layout", lang),
                        /*0*/CategoryFormat(getId),
                        /*1*/RvMember.Get(getId).Country,
                        /*2*/RvMember.Get(getId).City,
                        /*3*/RequestTrackAccess(message)) + optional;
                    return header + RoleFormat(rvUser) + memberLayout;

                case Status.Critic:
                    string criticLayout = string.Format(Language.GetPhrase("Profile_Critic_Layout", lang),
                        /*0*/CategoryFormat(getId)) + optional;
                    return header + RoleFormat(rvUser) + criticLayout;

                case Status.CriticAndMember:
                    string criticAndMemberLayout = string.Format(Language.GetPhrase("Profile_CriticAndMember_Layout", lang),
                        /*0*/CategoryFormat(getId),
                        /*1*/RvMember.Get(getId).Country,
                        /*2*/RvMember.Get(getId).City,
                        /*3*/RequestTrackAccess(message)) + optional;
                    return header + RoleFormat(rvUser) + criticAndMemberLayout;
                default:
                    return "Неизвестная ошибка";
            }
        }
    }
}
