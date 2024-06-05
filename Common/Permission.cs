using System.Text;
using RightVisionBot.Types;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Common;

class Permissions
{
    public static UserPermissions User = new()
    {
        Permission.Messaging,            Permission.OpenProfile,
        Permission.SendCriticForm,       Permission.SendMemberForm
    };

    public static UserPermissions Critic = new(User + new UserPermissions
    {
        Permission.CriticMenu,           Permission.CriticChat,
        Permission.ChattingInCriticChat, Permission.Evaluation
    });

    public static UserPermissions Member = new(User + new UserPermissions
    {
        Permission.TrackCard,            Permission.MemberChat,
        Permission.ChattingInMemberChat,
    });

    public static UserPermissions ExMember = new(User + new UserPermissions
    {
        Permission.MemberChat,
        Permission.ChattingInMemberChat,
    });

    public static UserPermissions CriticAndMember = new(User + Member + Critic);

    public static UserPermissions CriticAndExMember = new(User + ExMember + Critic);

    public static UserPermissions Moderator = new(User + new UserPermissions
    {
        Permission.Mute,                Permission.Unmute,
        Permission.Cancel,              Permission.News,
        Permission.MemberNews
    });

    public static UserPermissions SeniorModerator = new(Moderator + new UserPermissions
    {
        Permission.Ban,                 Permission.Unban,
        Permission.BlacklistOn,         Permission.BlacklistOff,
        Permission.EditPermissions,     Permission.Block
    });

    public static UserPermissions Curator = new()
    {
        Permission.PreListening,        Permission.Curate,
        Permission.Rewarding
    };

    public static UserPermissions Empty = new();

    public static UserPermissions Developer = new()
    {
        Permission.Audit
    };

    public static UserPermissions Admin = new(User + CriticAndMember + SeniorModerator + Curator + Developer + new UserPermissions()
    {
        Permission.Degrade,             Permission.DegradePermission,
        Permission.GivePermission,      Permission.Grant,
        Permission.TechNews
    });

    private static readonly ITelegramBotClient BotClient = Program.botClient;

    public static readonly Dictionary<Enum, UserPermissions> Layouts = new()
    {
        { Status.User, User },
        { Status.Critic, Critic },
        { Status.Member, Member },
        { Status.ExMember, ExMember },
        { Status.CriticAndMember, CriticAndMember },
        { Status.CriticAndExMember, CriticAndExMember },

        { Role.Admin, Admin },
        { Role.Curator, Curator },
        { Role.Developer, Developer },
        { Role.Moderator, Moderator },
        { Role.SeniorModerator, SeniorModerator },

        { Role.TechAdmin, Empty },
        { Role.Designer, Empty },
        { Role.Translator, Empty },
        { Role.None, Empty }
    };

    public static void NoPermission(Chat chat) => BotClient.SendTextMessageAsync(chat, "Извини, но у тебя нет права совершать это действие!");

    public static async Task Reset(ITelegramBotClient botClient, Message message, RvUser rvUser)
    {
        if (message.ReplyToMessage != null && rvUser.Has(Permission.DegradePermission) && rvUser.Has(Permission.GivePermission))
        {
            var repliedRvUser = RvUser.Get(message.ReplyToMessage.From.Id);
            
            repliedRvUser.ResetPermissions();
            string statusLayout = repliedRvUser.Status.ToString();
            string roleLayout = repliedRvUser.Role.ToString();

            await botClient.SendTextMessageAsync(message.Chat, $"Выполнен сброс прав до стандартных для пользователя.\n\nИспользованные шаблоны:\n{statusLayout}\n{roleLayout}");
        }
        else
            NoPermission(message.Chat);
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
    BlacklistOff
}