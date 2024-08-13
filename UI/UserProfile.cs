using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.Types;
using RightVisionBot.User;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

//система отображения профилей пользователей
namespace RightVisionBot.UI
{
    class UserProfile
    {
        public static ITelegramBotClient BotClient = Program.botClient;
        public static sql Database = Program.database;

        public static string Profile(Message message)
        {
            if (message.From != null)
            {
                var userId = message.From.Id;
                var getId = message.ReplyToMessage == null ? userId : message.ReplyToMessage.From.Id;
                var rvUser = RvUser.Get(getId);

                if (message.Chat.Type == ChatType.Private)
                {
                    Program.UpdateRvLocation(userId, RvLocation.Profile);
                    return rvUser.ProfilePrivate();
                }
                else
                    return rvUser.ProfilePublic(RvUser.Get(userId).Lang);
            }
            return string.Empty;
        }

        public static string GetCandidateStatus(long userId, string role)
        {
            try
            {
                var query = $"SELECT `status` FROM RV_{role}s WHERE `userId` = {userId};";
                var idList = Database.Read(query, "status");
                return idList[0] switch
                {
                    "denied" => Language.GetPhrase("Profile_Form_Status_Blocked", RvUser.Get(userId).Lang),
                    "waiting" => Language.GetPhrase("Profile_Form_Status_Waiting", RvUser.Get(userId).Lang),
                    "unfinished" => Language.GetPhrase("Profile_Form_Status_Unfinished", RvUser.Get(userId).Lang),
                    null => Language.GetPhrase("Profile_Form_Status_Allowed", RvUser.Get(userId).Lang),
                    "bronze" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "silver" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
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

        public static string CategoryFormat(string category)
            => category switch
            {
                "bronze" => "🥉Bronze",
                "silver" => "🥈Silver",
                "gold" => "🥇Gold",
                "brilliant" => "💎Brilliant",
                _ => string.Empty
            };

        public static async Task PermissionsList(CallbackQuery callback, RvUser rvUser, string type)
        {
            string nameInHeader;
            if (RvCritic.Get(rvUser.UserId) != null)
                nameInHeader = RvCritic.Get(rvUser.UserId).Name;
            else if (RvMember.Get(rvUser.UserId) != null)
                nameInHeader = RvMember.Get(rvUser.UserId).Name;
            else
                nameInHeader = callback.From.FirstName;

            var header = callback.From.Id == rvUser.UserId
                ? Language.GetPhrase("Profile_Permissions_Header", rvUser.Lang)
                : string.Format(Language.GetPhrase("Profile_Permissions_Header_Global", rvUser.Lang), nameInHeader);

            var standartCount = 10;
            StringBuilder fullList = new();
            StringBuilder blockedList = new();
            StringBuilder addedList = new();
            UserPermissions blockedPerms = new();
            UserPermissions addedPerms = new();
            var minimize = Keyboard.Minimize(rvUser);
            var maximize = Keyboard.Maximize(rvUser);
            var back = Keyboard.PermissionsBack(rvUser);
            var keyboard = type == "maximize" ? minimize : maximize;
            if (rvUser.Permissions.Count < 10)
                keyboard = back;

            UserPermissions layout = new(Permissions.Layouts[rvUser.Status] + Permissions.Layouts[rvUser.Role]);

            if (type == "maximize")
            {
                foreach (var permission in rvUser.Permissions)
                    fullList.AppendLine("• " + permission);
                keyboard = minimize;
            }
            else
            {
                for (var i = 0; i <= standartCount; i++)
                    try
                    {
                        fullList.AppendLine("• " + rvUser.Permissions.Collection[i]);
                    }
                    catch (Exception e) when (e.Message.Contains("Index was out of range"))
                    {
                        break;
                    }
                if (rvUser.Permissions.Count >= 10)
                    fullList.AppendLine("...");
            }

            //Составление списка добавленных прав
            foreach (var permission in rvUser.Permissions)
            {
                if (!Permissions.Layouts[rvUser.Status].Contains(permission) &&
                    !Permissions.Layouts[rvUser.Role].Contains(permission))
                {
                    addedPerms.Add(permission);
                    addedList.AppendLine("+ " + permission);
                }
            }

            //Составление списка отобранных прав
            foreach (var permission in layout.Collection.Where(permission => !rvUser.Permissions.Contains(permission)))
            {
                blockedPerms.Add(permission);
                blockedList.AppendLine("- " + permission);
            }

            var addedFormat = addedPerms.Count == 0 ? string.Empty : $"{Language.GetPhrase("Profile_Permissions_AddedList", rvUser.Lang)}\n{addedList}\n";
            var blockedFormat = blockedPerms.Count == 0 ? string.Empty : $"{Language.GetPhrase("Profile_Permissions_BlockedList", rvUser.Lang)}\n{blockedList}";
            var permissionsFormat = $"{header}\n\n" +
                                    $"{Language.GetPhrase("Profile_Permissions_FullList", rvUser.Lang)}\n{fullList}\n" +
                                    addedFormat +
                                    blockedFormat;
            await BotClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, permissionsFormat, replyMarkup: keyboard);
        }

        public static async Task PunishmentsList(Update update, RvUser rvUser)
        {
            string groupMember = Language.GetPhrase("Profile_Punishment_InMembers", rvUser.Lang);
            string groupCritic = Language.GetPhrase("Profile_Punishment_InMembers", rvUser.Lang);
            string reason = Language.GetPhrase("Profile_Punishment_Reason", rvUser.Lang);
            string dateTo = Language.GetPhrase("Profile_Punishment_DateTo", rvUser.Lang);

            StringBuilder sb = new();
            foreach (var pun in rvUser.Punishments.Collection)
            {
                string type = pun.Type == RvPunishment.PunishmentType.Ban ? "🔒Бан " : "🔇Мут ";
                string group = pun.GroupId == -1002074764678 ? groupMember : groupCritic;

                sb.AppendLine(type + group + pun.From.ToString("dd.MM.yyyy") + ", " + pun.From.ToString("HH:mm") + ":\n"
                              + $"🪧{reason} {pun.Reason}\n"
                              + $"⏱{dateTo} {pun.To.ToString("dd.MM.yyyy") + ", " + pun.To.ToString("HH:mm")}\n");
            }

            try
            {
                await BotClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat,
                    update.CallbackQuery.Message.MessageId, sb.ToString(),
                    replyMarkup: Keyboard.PermissionsBack(rvUser));
            }
            catch (Exception ex) when (ex.Message.Contains("Bad Request: message text is empty"))
            {
                await BotClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, Language.GetPhrase("Profile_NoPunishments", rvUser.Lang), showAlert: true);
            }

        }
    }
}
