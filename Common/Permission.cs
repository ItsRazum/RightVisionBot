using System.Text;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Common;

class PermissionLayouts
    {
        public static HashSet<Permission> User = new()
        {
            Permission.Messaging,            Permission.OpenProfile, 
            Permission.SendCriticForm,       Permission.SendMemberForm
        };

        public static HashSet<Permission> Critic = new(User)
        {
            Permission.CriticMenu,           Permission.CriticChat, 
            Permission.ChattingInCriticChat, Permission.Evaluation
        };

        public static HashSet<Permission> Member = new(User)
        {
            Permission.TrackCard,            Permission.MemberChat, 
            Permission.ChattingInMemberChat, 
        };

        public static HashSet<Permission> CriticAndMember = new(User)
        {
            Permission.CriticMenu,           Permission.CriticChat,
            Permission.ChattingInCriticChat, Permission.Evaluation,
            Permission.TrackCard,            Permission.MemberChat,
            Permission.ChattingInMemberChat,
        };

        public static HashSet<Permission> Moderator = new(User)
        {
            Permission.Mute,                 Permission.Unmute,
            Permission.Cancel
        };

        public static HashSet<Permission> SeniorModerator = new(Moderator)
        {
            Permission.Ban,                  Permission.Unban,
            Permission.BlacklistOn,          Permission.BlacklistOff,
            Permission.EditPermissions,      Permission.Block
        };

    public static HashSet<Permission> Curator = new()
        {
            Permission.PreListening,         Permission.Curate,
            Permission.Rewarding             
        };

    public static HashSet<Permission> Empty = new();

    public static HashSet<Permission> Developer = new()
        {
            Permission.Audit
        };

    public static HashSet<Permission> Admin = new(CriticAndMember)
        {
            Permission.Sending,              Permission.News,
            Permission.MemberNews,           Permission.TechNews,
            Permission.PreListening,         Permission.Curate,
            Permission.Ban,                  Permission.Mute,
            Permission.EditPermissions,      Permission.BlacklistOn,
            Permission.Block,                Permission.Cancel,
            Permission.Audit,                Permission.Rewarding,
            Permission.Grant,                Permission.GivePermission,
            Permission.Degrade,              Permission.DegradePermission,
            Permission.Unban,                Permission.Unmute,
            Permission.BlacklistOff,
        };
    }

class Permissions
{
    private static ITelegramBotClient botClient = Program.botClient;

    public static void NoPermission(Chat chat) => botClient.SendTextMessageAsync(chat, "Извини, но у тебя нет права совершать это действие!");

    public static async Task Reset(ITelegramBotClient botClient, Message message, RvUser rvUser)
    {
        if (message.ReplyToMessage != null && rvUser.Has(Permission.DegradePermission) && rvUser.Has(Permission.GivePermission))
        {
            RvUser repliedRvUser = RvUser.Get(message.ReplyToMessage.From.Id);
            string statusLayout = string.Empty;
            string roleLayout = string.Empty;
            switch (repliedRvUser.Status)
            {
                case Status.User:
                    repliedRvUser.Permissions = PermissionLayouts.User;
                    statusLayout = "User";
                    break;
                case Status.Critic:
                    repliedRvUser.Permissions = PermissionLayouts.Critic;
                    statusLayout = "Critic";
                    break;
                case Status.Member:
                    repliedRvUser.Permissions = PermissionLayouts.Member;
                    statusLayout = "Member";
                    break;
                case Status.CriticAndMember:
                    repliedRvUser.Permissions = PermissionLayouts.CriticAndMember;
                    statusLayout = "CriticAndMember";
                    break;
            }

            switch (repliedRvUser.Role)
            {
                case Role.Admin:
                    repliedRvUser.AddPermissions(hashSet: PermissionLayouts.Admin);
                    roleLayout = "Admin";
                    break;
                case Role.Curator:
                    repliedRvUser.AddPermissions(hashSet: PermissionLayouts.Curator);
                    roleLayout = "Curator";
                    break;
                case Role.Moderator:
                    repliedRvUser.AddPermissions(hashSet: PermissionLayouts.Moderator);
                    roleLayout = "Moderator";
                    break;
                case Role.Developer:
                    repliedRvUser.AddPermissions(hashSet: PermissionLayouts.Developer);
                    roleLayout = "Developer";
                    break;
                case Role.SeniorModerator:
                    repliedRvUser.AddPermissions(hashSet: PermissionLayouts.SeniorModerator);
                    roleLayout = "SeniorModerator";
                    break;
            }

            await botClient.SendTextMessageAsync(message.Chat, $"Выполнен сброс прав до стандартных для пользователя.\n\nИспользованные шаблоны:\n{statusLayout}\n{roleLayout}");
        }
        else
            Permissions.NoPermission(message.Chat);
    }

    public static HashSet<Permission> AddPermissions(HashSet<Permission> permissions, HashSet<Permission> permissionsToAdd)
    {
        HashSet<Permission> newPermissions = new(permissions);
        foreach (var permission in permissionsToAdd)
            newPermissions.Add(permission);

        return newPermissions;
    }
}

public enum Permission
{
    /// <summary>Право на общение с ботом</summary>
    Messaging,
    /// <summary>Право на рассылку</summary>
    Sending,
    /// <summary>Право на отправку общих новостей</summary>
    News,
    /// <summary>Право на отправку новостей для участников</summary>
    MemberNews,
    /// <summary>Право на отправку технических новостей</summary>
    TechNews,
    /// <summary>Право на открытие профиля</summary>
    OpenProfile,
    /// <summary>Право на отправку заявки на судейство</summary>
    SendCriticForm,
    /// <summary>Право на отправку заявки на участие</summary>
    SendMemberForm,
    /// <summary>Право на доступ к чату участников</summary>
    MemberChat,
    /// <summary>Право на отправку сообщений в чате участников</summary>
    ChattingInMemberChat,
    /// <summary>Право на нахождение в чате судей</summary>
    CriticChat,
    /// <summary>Право на отправку сообщений в чате судей</summary>
    ChattingInCriticChat,
    /// <summary>Право на открытие судейского меню</summary>
    CriticMenu,
    /// <summary>Право на открытие карточки ремикса</summary>
    TrackCard,
    /// <summary>Право на оценивание ремиксов</summary>
    Evaluation,
    /// <summary>Право на предварительное прослушивание</summary>
    PreListening,
    /// <summary>Право курировать кандидатов</summary>
    Curate,
    /// <summary>Право банить</summary>
    Ban,
    /// <summary>Право мутить</summary>
    Mute,
    /// <summary>Право изменять права других пользователей</summary>
    EditPermissions,
    /// <summary>Право на отправку пользователя в чёрный список</summary>
    BlacklistOn,
    /// <summary>Право на блокировку любой из кандидатур пользователя</summary>
    Block,
    /// <summary>Право на аннулирование любой из кандидатур пользователя</summary>
    Cancel,
    /// <summary>Право на чтение журнала аудита</summary>
    Audit,
    /// <summary>Право на выдачу наград</summary>
    Rewarding,
    /// <summary>Право на назначение пользователя на должность</summary>
    Grant,
    /// <summary>Право на выдачу привилегии</summary>
    GivePermission,
    /// <summary>Право на снятие пользователя с должности</summary>
    Degrade,
    /// <summary>Право на снятие с пользователя права</summary>
    DegradePermission,
    /// <summary>Право на разбан пользователя</summary>
    Unban,
    /// <summary>Право на размут пользователя</summary>
    Unmute,
    /// <summary>Право на удаление пользователя из чёрного списка</summary>
    BlacklistOff,
}