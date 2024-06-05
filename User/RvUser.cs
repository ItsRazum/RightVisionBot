using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.Types;
using RightVisionBot.UI;
using System.Text;
using System.Timers;
using RightVisionBot.Common;

namespace RightVisionBot.User;

public class RvUser
{
    public long UserId;

    private string? _name;
    public string? Name { get => _name; set { _name = value; NewString(value, nameof(Name)); } }

    private Status _status = Status.User;
    public Status Status { get => _status; set { _status = value; NewString(value.ToString(), nameof(Status)); } }

    private UserPermissions _permissions = new();
    public UserPermissions Permissions { get => _permissions; set { _permissions = value; NewString(value.ToString(), "permissions"); } }

    private RvPunishments _punishments = new();
    public RvPunishments Punishments { get => _punishments; set { _punishments = value; NewString(value.ToString(), nameof(Punishments)); } }

    private string _lang = "ru";
    public string Lang { get => _lang; set { _lang = value; NewString(value, nameof(Lang)); } }

    private RvLocation _rvLocation = RvLocation.MainMenu;
    public RvLocation RvLocation { get => _rvLocation; set { _rvLocation = value; NewString(value.ToString(), nameof(RvLocation)); } }

    private Role _role;
    public Role Role { get => _role; set { _role = value; NewRole(value.ToString(), nameof(Role)); } }

    private string _category = "none";
    public string Category { get => _category; set { _category = value; NewString(value, nameof(Category)); } }

    public Rewards Rewards { get; set; }

    public System.Timers.Timer Cooldown;
    private System.Timers.Timer? CounterCooldown { get; set; }
    private int _counter;

    public RvUser(long userId, string lang, Status status, RvLocation rvLocation, Role role, string category, string username, bool writeToDb)
    {
        _name = username;
        Name = username;
        UserId = userId;
        Lang = lang;
        Status = status;
        RvLocation = rvLocation;
        Role = role;
        Category = category;
        Cooldown = new System.Timers.Timer(0.1);
        Rewards = new Rewards(userId);

        if (writeToDb)
        {
            Program.database.Read(
                $"INSERT INTO RV_Users(`username`, `userId`, `lang`, `status`, `rvLocation`, `role`, `category`) VALUES ('{username}', {userId}, '{lang}', '{status}', '{rvLocation}', '{role}', '{category}');",
                "");
            ResetPermissions();
        }

        Data.RvUsers.Add(this);
    }

    public void ResetPermissions()
    {
        Permissions = new UserPermissions(Common.Permissions.Layouts[Status] + Common.Permissions.Layouts[Role], UserId);
        Program.database.Read($"UPDATE `RV_Users` SET `permissions` = '{Permissions}' WHERE `userId` = {UserId}", "");
    }

    private void NewString(string value, string property)
    {
        UpdateDatabase(property, value);
        _counter++;
        if (_counter == 1)
        {
            CounterCooldown = new System.Timers.Timer(30000);
            CounterCooldown.Elapsed += CounterCooldownElapsed;
            CounterCooldown.Start();
        }

        if (Cooldown is { Enabled: true }) return;
        Cooldown = new System.Timers.Timer(TimerInterval());
        Cooldown.Elapsed += CooldownElapsed;
        Cooldown.Start();
    }

    private void UpdateDatabase(string property, string value) => Program.database.Read($"UPDATE `RV_Users` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");

    private void NewRole(string value, string property)
    {
        Permissions += Common.Permissions.Layouts[Role];
        NewString(value, property);
    }

    public bool Has(Permission permission) => Permissions.Contains(permission);

    private int TimerInterval()
    {
        return _counter switch
        {
            < 10 => 1000,
            < 20 => 5000,
            < 25 => 10000,
            _ => 500
        };
    }

    private void CooldownElapsed(object? sender, ElapsedEventArgs e) => Cooldown?.Stop();
    private void CounterCooldownElapsed(object? sender, ElapsedEventArgs e)
    {
        CounterCooldown?.Stop();
        _counter = 0;
    }

    public string ProfilePrivate()
    {
        var header = Language.GetPhrase("Profile_Private_Header", Lang);

        string roleFormat = Role == Role.None
                ? Language.GetUserStatusString(Status, Lang)
                : Language.GetUserStatusString(Status, Lang) + "\n" + string.Format(Language.GetPhrase("Profile_Role", Lang), Language.GetUserRoleString(Role, Lang));
        
        string formsStatus = string.Format(Language.GetPhrase("Profile_Forms", Lang),
            UserProfile.GetCandidateStatus(UserId, "Member"),
            UserProfile.GetCandidateStatus(UserId, "Critic"));

        string category = UserProfile.CategoryFormat(Category);
        StringBuilder rewards = new();
        rewards.AppendLine(Language.GetPhrase("Profile_Form_Rewards", Lang));
        foreach (var reward in Rewards.Collection)
            rewards.AppendLine("| " + reward.Value.Icon + reward.Key + " – " + reward.Value.Description);
        string sending = string.Format
        (Language.GetPhrase("Profile_Sending_Status", Lang),
         Language.GetPhrase(!Has(Permission.Sending) ? "Profile_Sending_Status_Inactive" : "Profile_Sending_Status_Active", Lang));

        string optional = sending + formsStatus + rewards;
        string profile = Status switch
        {
            Status.User => header + roleFormat + optional,
            Status.Member => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Member_Layout", Lang),
                /*0*/ category,
                /*1*/ RvMember.Get(UserId).TrackStr) + optional,
            Status.ExMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Member_Layout", Lang),
                /*0*/ category,
                /*1*/ RvExMember.Get(UserId).TrackStr) + optional,
            Status.Critic => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Critic_Layout", Lang),
                /*0*/category) + optional,
            Status.CriticAndMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", Lang),
                /*0*/category,
                /*1*/RvMember.Get(UserId).TrackStr) + optional,
            Status.CriticAndExMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", Lang),
                /*0*/category,
                /*1*/RvExMember.Get(UserId).TrackStr) + optional,
            _ => "Неизвестная ошибка"
        };
        return profile;
    }

    public string ProfilePublic()
    {
        var header = string.Format(Language.GetPhrase("Profile_Global_Header", Lang), Name);
        string roleFormat = Role == Role.None
        ? Language.GetUserStatusString(Status, Lang)
        : Language.GetUserStatusString(Status, Lang) + "\n" + string.Format(Language.GetPhrase("Profile_Role", Lang), Language.GetUserRoleString(Role, Lang));
        string category = UserProfile.CategoryFormat(Category);
        StringBuilder rewards = new();
        rewards.AppendLine(Language.GetPhrase("Profile_Form_Rewards", Lang));
        foreach (var reward in Rewards.Collection)
            rewards.AppendLine("| " + reward.Value.Icon + reward.Key + " – " + reward.Value.Description);
        
        string optional = rewards.ToString();
        string profile = Status switch
        {
            Status.User => header + roleFormat + optional,
            Status.Member => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Member_Layout", Lang),
                /*0*/ category,
                /*1*/ "Скрыт") + optional,
            Status.ExMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Member_Layout", Lang),
                /*0*/ category,
                /*1*/ RvExMember.Get(UserId).TrackStr) + optional,
            Status.Critic => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_Critic_Layout", Lang),
                /*0*/category) + optional,
            Status.CriticAndMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", Lang),
                /*0*/category,
                /*1*/"Скрыт") + optional,
            Status.CriticAndExMember => header + roleFormat + string.Format(
                "\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", Lang),
                /*0*/category,
                /*1*/RvExMember.Get(UserId).TrackStr) + optional,
            _ => "Неизвестная ошибка"
        };
        return profile;
    }

    public static RvUser Get(long userId)
    {
        foreach (var user in Data.RvUsers)
            if (userId == user.UserId)
                return user;

        return null;
    }
}