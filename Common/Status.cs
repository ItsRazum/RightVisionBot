namespace RightVisionBot.Common;

public enum Status
{
    User,
    Member,
    Critic,
    CriticAndMember
}

public enum Role
{
    None,
    Designer,
    Translator,
    Moderator,
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
    Blacklist
}