using RightVisionBot.Back;
using RightVisionBot.User;
using Telegram.Bot.Types;

namespace RightVisionBot.Common;

public enum Status
{
    User,
    Member,
    ExMember,
    Critic,
    CriticAndMember,
    CriticAndExMember
}

public enum Role
{
    None,
    Designer,
    Translator,
    Moderator,
    SeniorModerator,
    TechAdmin,
    Curator,
    Developer,
    Admin
}

public enum RvLocation
{
    MainMenu,
    Profile,
    TrackCard,
    CriticMenu,
    CriticForm,
    MemberForm,
    PreListening,
    Evaluation,
    EditTrack,
    Blacklist
}