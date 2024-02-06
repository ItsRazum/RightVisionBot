using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
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

            InlineKeyboardMarkup keyboard = message.Chat.Type == ChatType.Private ? Keyboard.ProfileOptions(RvUser.Get(userId)) : InlineKeyboardMarkup.Empty();
            RvUser rvUser = RvUser.Get(getId);
            if (message.Chat.Type == ChatType.Private)
                Program.updateRvLocation(userId, RvLocation.Profile);
            await botClient.SendTextMessageAsync(message.Chat, ProfileFormat(message, rvUser), replyMarkup: keyboard);
        }

        private static string GetSendingStatus(Message message, RvUser rvUser)
        {
            if (message.Chat.Type == ChatType.Private)
                return Language.GetPhrase(!rvUser.Permissions.Contains(Permission.Sending) ? "Profile_Sending_Status_Inactive" : "Profile_Sending_Status_Active", rvUser.Lang);
            else
                return string.Empty;
        }

        private static string RequestTrackAccess(Message message)
            => message.ReplyToMessage == null ? RvMember.Get(message.From.Id).TrackStr : RvMember.Get(message.ReplyToMessage.From.Id).TrackStr;

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
                    GetCandidateStatus(userId, "Critic"));
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
                case Status.Member: return Language.GetPhrase("Profile_Member_Header", rvUser.Lang);
                case Status.Critic: return Language.GetPhrase("Profile_Critic_Header", rvUser.Lang);
                case Status.CriticAndMember: return Language.GetPhrase("Profile_CriticAndMember_Header", rvUser.Lang);
                default:
                    return Language.GetPhrase("Profile_User_Layout", rvUser.Lang);
            }
        }

        private static string RoleAsString(RvUser rvUser)
            => rvUser.Role switch
            {
                Role.Admin => "Главный организатор",
                Role.Moderator => "Модератор",
                Role.TechAdmin => "Техадмин",
                Role.Developer => "Разработчик",
                Role.Curator => "Куратор",
                Role.Designer => "Дизайнер",
                Role.Translator => "Переводчик",
                Role.SeniorModerator => "Старший Модератор",
                _ =>
                    string.Empty
            };

        private static string CategoryFormat(long userId)
            => RvUser.Get(userId).Category switch
            {
                "bronze" => "🥉Bronze",
                "steel" => "🥈Steel",
                "gold" => "🥇Gold",
                "brilliant" => "💎Brilliant",
                _ => string.Empty
            };

        private static string RewardsFormat(RvUser rvUser)
        {
            string header = Language.GetPhrase("Profile_Form_Rewards", rvUser.Lang);
            StringBuilder sb = new StringBuilder();
            if (rvUser.Rewards.FirstOrDefault() == null)
                return "\n" + header + "Кажется, здесь ничего нет!";
            else
            {
                foreach (var dictionary in rvUser.Rewards)
                    foreach (var reward in dictionary)
                        sb.Append("|" + reward.Value + "\n");

                return "\n" + header + sb;
            }
        }

        public static string ProfileFormat(Message message, RvUser rvUser)
        {
            long userId = message.From.Id;
            string lang = RvUser.Get(userId).Lang;
            long getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;
            string sending = message.Chat.Type == ChatType.Private ? string.Format(Language.GetPhrase("Profile_Sending_Status", lang), GetSendingStatus(message, rvUser)) : string.Empty;
            string optional = sending + GetFormsStatus(message) + RewardsFormat(rvUser);
            string header = message.ReplyToMessage == null ?
                Language.GetPhrase("Profile_Private_Header", lang) :
                string.Format(Language.GetPhrase("Profile_Global_Header", lang), message.ReplyToMessage.From.FirstName);

            switch (rvUser.Status)
            {
                case Status.User:
                    return header + RoleFormat(rvUser) + optional;

                case Status.Member:
                    string memberLayout = string.Format("\n" + Language.GetPhrase("Profile_Member_Layout", lang),
                        /*0*/CategoryFormat(getId),
                        /*1*/RvMember.Get(getId).Country,
                        /*2*/RvMember.Get(getId).City,
                        /*3*/RequestTrackAccess(message)) + optional;
                    return header + RoleFormat(rvUser) + memberLayout;

                case Status.Critic:
                    string criticLayout = string.Format("\n" + Language.GetPhrase("Profile_Critic_Layout", lang),
                        /*0*/CategoryFormat(getId)) + optional;
                    return header + RoleFormat(rvUser) + criticLayout;

                case Status.CriticAndMember:
                    string criticAndMemberLayout = string.Format("\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", lang),
                        /*0*/CategoryFormat(getId),
                        /*1*/RvMember.Get(getId).Country,
                        /*2*/RvMember.Get(getId).City,
                        /*3*/RequestTrackAccess(message)) + optional;
                    return header + RoleFormat(rvUser) + criticAndMemberLayout;
                default:
                    return "Неизвестная ошибка";
            }
        }

        public static async Task PermissionsList(ITelegramBotClient botClient, Update update, RvUser rvUser, string type)
        {
            int standartCount = 10;
            StringBuilder fullList = new();
            StringBuilder blockedList = new();
            StringBuilder addedList = new();
            HashSet<Permission> blockedPerms = new();
            HashSet<Permission> addedPerms = new();
            HashSet<Permission> layout = new();
            var minimize = Keyboard.Minimize(rvUser);
            var maximize = Keyboard.Maximize(rvUser);
            var back = Keyboard.InlineBack(rvUser, RvLocation.Profile);
            InlineKeyboardMarkup keyboard = type == "maximize" ? minimize : maximize;
            if (rvUser.Permissions.Count < 10)
                keyboard = back;

            switch (rvUser.Status)
            {
                case Status.Critic:
                    layout = PermissionLayouts.Critic;
                    break;
                case Status.CriticAndMember:
                    layout = PermissionLayouts.CriticAndMember;
                    break;
                case Status.Member:
                    layout = PermissionLayouts.Member;
                    break;
                default:
                    layout = PermissionLayouts.User;
                    break;
            }

            switch (rvUser.Role)
            {
                case Role.Admin:
                    layout = Permissions.AddPermissions(layout, PermissionLayouts.Admin); break;
                case Role.Curator:
                    layout = Permissions.AddPermissions(layout, PermissionLayouts.Curator); break;
                case Role.Moderator:
                    layout = Permissions.AddPermissions(layout, PermissionLayouts.Moderator); break;
                case Role.Developer:
                    layout = Permissions.AddPermissions(layout, PermissionLayouts.Developer); break;
            }

            int count = rvUser.Permissions.Count <= standartCount ? rvUser.Permissions.Count : standartCount;

            if (type == "maximize")
            {
                foreach (var permission in rvUser.Permissions)
                    fullList.AppendLine("• " + permission);
                keyboard = minimize;
            }
            else
            {
                foreach (var permission in rvUser.Permissions.Take(count))
                    fullList.AppendLine("• " + permission);
                if (rvUser.Permissions.Count > 10)
                    fullList.AppendLine("...");
            }

            //Составление списка добавленных прав
            foreach (var permission in rvUser.Permissions)
                if (!layout.Contains(permission))
                {
                    addedPerms.Add(permission);
                    addedList.AppendLine("+ " + permission);
                }

            //Составление списка отобранных прав
            foreach (var permission in layout)
                if (!rvUser.Permissions.Contains(permission))
                {
                    blockedPerms.Add(permission);
                    blockedList.AppendLine("- " + permission);
                }

            string addedFormat = addedPerms.Count == 0 ? string.Empty : $"Добавленные права:\n{addedList}\n";
            string blockedFormat = blockedPerms.Count == 0 ? string.Empty : $"Отобранные права:\n{blockedList}";
            string permissionsFormat = "Список всех твоих прав доступа:\n\n" +
                                       $"Полный список:\n{fullList}\n" +
                                       addedFormat +
                                       blockedFormat;
            await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, permissionsFormat, replyMarkup: keyboard);
        }

        public static async Task PunishmentsList(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            StringBuilder sb = new();
            foreach (var pun in rvUser.Punishments)
            {
                string type = pun.Type == RvPunishment.PunishmentType.Ban ? "🔒Бан " : "🔇Мут ";
                string group = pun.GroupId == -1002074764678 ? "в группе участников от " : "в группе судей от ";

                sb.AppendLine(type + group + pun.From.ToString("dd.MM.yyyy") + ", " + pun.From.ToString("HH:mm") + ":\n"
                              + $"🪧Причина: {pun.Reason}\n"
                              + $"⏱Срок окончания: {pun.To.ToString("dd.MM.yyyy") + ", " + pun.To.ToString("HH:mm")}\n");
            }

            try
            {
                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat,
                    update.CallbackQuery.Message.MessageId, sb.ToString(),
                    replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.Profile));
            }
            catch (Exception ex) when(ex.Message.Contains("Bad Request: message text is empty"))
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Кажется, у тебя нет никаких наказаний, отличная работа!", showAlert:true);
            }

        }
    }
}
