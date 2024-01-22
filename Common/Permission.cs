using RightVisionBot.Back;
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

        public static HashSet<Permission> Moderator = new()
        {
            Permission.Ban,                  Permission.Mute,
            Permission.Blacklist,            Permission.Block
        };

        public static HashSet<Permission> Curator = new()
        {
            Permission.PreListening,         Permission.Curate,
            Permission.Rewarding             
        };

        public static HashSet<Permission> Developer = new()
        {
            Permission.Audit
        };

    public static HashSet<Permission> Admin = new(CriticAndMember)
        {
            Permission.Messaging,            Permission.Ban, 
            Permission.Mute,                 Permission.Blacklist,
            Permission.PreListening,         Permission.EditPermissions,
            Permission.Cancel,               Permission.Block,
            Permission.Rewarding,            Permission.Grant
        };
    }

class Permissions
{
    private static ITelegramBotClient botClient = Program.botClient;

    public static void NoPermission(Message message) => botClient.SendTextMessageAsync(message.Chat, "Извини, но у тебя нет права совершать это действие!");
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
    Blacklist,
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
}