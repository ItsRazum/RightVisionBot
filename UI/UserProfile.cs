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

            InlineKeyboardMarkup keyboard = Keyboard.ProfileOptions(RvUser.Get(getId), message);
            RvUser rvUser = RvUser.Get(getId);
            if (message.Chat.Type == ChatType.Private)
                Program.UpdateRvLocation(userId, RvLocation.Profile);
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
                return IdList.FirstOrDefault() switch
                {
                    "denied" => Language.GetPhrase("Profile_Form_Status_Blocked", RvUser.Get(userId).Lang),
                    "waiting" => Language.GetPhrase("Profile_Form_Status_Waiting", RvUser.Get(userId).Lang),
                    "unfinished" => Language.GetPhrase("Profile_Form_Status_Unfinished", RvUser.Get(userId).Lang),
                    null => Language.GetPhrase("Profile_Form_Status_Allowed", RvUser.Get(userId).Lang),
                    "bronze" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "steel" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "gold" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "brilliant" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    _ => Language.GetPhrase("Profile_Form_Status_UnderConsideration", RvUser.Get(userId).Lang)
                };
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
            return rvUser.Status switch
            {
                Status.Member => Language.GetPhrase("Profile_Member_Header", rvUser.Lang),
                Status.Critic => Language.GetPhrase("Profile_Critic_Header", rvUser.Lang),
                Status.CriticAndMember => Language.GetPhrase("Profile_CriticAndMember_Header", rvUser.Lang),
                _ => Language.GetPhrase("Profile_User_Layout", rvUser.Lang)
            };
        }

        private static string RoleAsString(RvUser rvUser)
            => rvUser.Role switch
            {
                Role.Admin => Language.GetPhrase("Profile_Role_Admin", rvUser.Lang),
                Role.Moderator => Language.GetPhrase("Profile_Role_Moderator", rvUser.Lang),
                Role.TechAdmin => Language.GetPhrase("Profile_Role_TechAdmin", rvUser.Lang),
                Role.Developer => Language.GetPhrase("Profile_Role_Developer", rvUser.Lang),
                Role.Curator => Language.GetPhrase("Profile_Role_Curator", rvUser.Lang),
                Role.Designer => Language.GetPhrase("Profile_Role_Designer", rvUser.Lang),
                Role.Translator => Language.GetPhrase("Profile_Role_Translator", rvUser.Lang),
                Role.SeniorModerator => Language.GetPhrase("Profile_Role_SeniorModerator", rvUser.Lang),
                _ => string.Empty
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
            string header = message.ReplyToMessage == null
                ? Language.GetPhrase("Profile_Private_Header", lang)
                : string.Format(Language.GetPhrase("Profile_Global_Header", lang),
                    RvCritic.Get(message.ReplyToMessage.From.Id) == null
                        ? RvMember.Get(message.ReplyToMessage.From.Id).Name
                        : RvCritic.Get(message.ReplyToMessage.From.Id).Name);

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

        public static async Task PermissionsList(CallbackQuery callback, RvUser rvUser, string type)
        {
            string nameInHeader;
            if (RvCritic.Get(rvUser.UserId) != null)
                nameInHeader = RvCritic.Get(rvUser.UserId).Name;
            else if (RvMember.Get(rvUser.UserId) != null)
                nameInHeader = RvMember.Get(rvUser.UserId).Name;
            else
                nameInHeader = callback.From.FirstName;

            string header = callback.From.Id == rvUser.UserId
                ? Language.GetPhrase("Profile_Permissions_Header", rvUser.Lang)
                : string.Format(Language.GetPhrase("Profile_Permissions_Header_Global", rvUser.Lang), nameInHeader);

            int standartCount = 10;
            StringBuilder fullList = new();
            StringBuilder blockedList = new();
            StringBuilder addedList = new();
            HashSet<Permission> blockedPerms = new();
            HashSet<Permission> addedPerms = new();
            HashSet<Permission> layout = new();
            var minimize = Keyboard.Minimize(rvUser);
            var maximize = Keyboard.Maximize(rvUser);
            var back = Keyboard.PermissionsBack(rvUser);
            InlineKeyboardMarkup keyboard = type == "maximize" ? minimize : maximize;
            if (rvUser.Permissions.Count < 10)
                keyboard = back;

            layout = rvUser.Status switch
            {
                Status.Critic => PermissionLayouts.Critic,
                Status.CriticAndMember => PermissionLayouts.CriticAndMember,
                Status.Member => PermissionLayouts.Member,
                _ => PermissionLayouts.User
            };

            layout = Permissions.AddPermissions(layout, rvUser.Role switch
            {
                Role.Admin => Permissions.AddPermissions(layout, PermissionLayouts.Admin),
                Role.Curator => Permissions.AddPermissions(layout, PermissionLayouts.Curator),
                Role.Moderator => Permissions.AddPermissions(layout, PermissionLayouts.Moderator),
                Role.Developer => Permissions.AddPermissions(layout, PermissionLayouts.Developer),
                Role.SeniorModerator => Permissions.AddPermissions(layout, PermissionLayouts.SeniorModerator),
                _ => PermissionLayouts.Empty
            });

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

            string addedFormat = addedPerms.Count == 0 ? string.Empty : $"{Language.GetPhrase("Profile_Permissions_AddedList", rvUser.Lang)}\n{addedList}\n";
            string blockedFormat = blockedPerms.Count == 0 ? string.Empty : $"{Language.GetPhrase("Profile_Permissions_BlockedList", rvUser.Lang)}\n{blockedList}";
            string permissionsFormat = $"{header}\n\n" +
                                       $"{Language.GetPhrase("Profile_Permissions_FullList", rvUser.Lang)}\n{fullList}\n" +
                                       addedFormat +
                                       blockedFormat;
            await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, permissionsFormat, replyMarkup: keyboard);
        }

        public static async Task PunishmentsList(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            string groupMember = Language.GetPhrase("Profile_Punishment_InMembers", rvUser.Lang);
            string groupCritic = Language.GetPhrase("Profile_Punishment_InMembers", rvUser.Lang);
            string reason = Language.GetPhrase("Profile_Punishment_Reason", rvUser.Lang);
            string dateTo = Language.GetPhrase("Profile_Punishment_DateTo", rvUser.Lang);

            StringBuilder sb = new();
            foreach (var pun in rvUser.Punishments)
            {
                string type = pun.Type == RvPunishment.PunishmentType.Ban ? "🔒Бан " : "🔇Мут ";
                string group = pun.GroupId == -1002074764678 ? groupMember : groupCritic;

                sb.AppendLine(type + group + pun.From.ToString("dd.MM.yyyy") + ", " + pun.From.ToString("HH:mm") + ":\n"
                              + $"🪧{reason} {pun.Reason}\n"
                              + $"⏱{dateTo} {pun.To.ToString("dd.MM.yyyy") + ", " + pun.To.ToString("HH:mm")}\n");
            }

            try
            {
                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat,
                    update.CallbackQuery.Message.MessageId, sb.ToString(),
                    replyMarkup: Keyboard.PermissionsBack(rvUser));
            }
            catch (Exception ex) when(ex.Message.Contains("Bad Request: message text is empty"))
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, Language.GetPhrase("Profile_NoPunishments", rvUser.Lang), showAlert: true);
            }

        }
    }
}
