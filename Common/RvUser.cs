using System.Diagnostics.Metrics;
using System.Text;
using RightVisionBot.Back;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Globalization;
using RightVisionBot.Tracks;

namespace RightVisionBot.Common;

public class RvUser
{
    public long UserId;

    private Status _status = Status.User;
    public Status Status { get => _status; set { _status = value; NewString(value.ToString(), nameof(Status)); } }

    private HashSet<Permission> _permissions = new HashSet<Permission>(PermissionLayouts.User);
    public HashSet<Permission> Permissions { get => _permissions; set { _permissions = value; NewPerms(value); } }

    private List<RvPunishment> _punishments = new List<RvPunishment>();
    public List<RvPunishment> Punishments { get => _punishments; set { _punishments = value; NewPunishments(value); }}

    private string _lang = "ru";
    public string Lang { get => _lang; set { _lang = value; NewString(value, nameof(Lang)); } }

    private RvLocation _rvLocation = RvLocation.MainMenu;
    public RvLocation RvLocation { get => _rvLocation; set { _rvLocation = value; NewString(value.ToString(), nameof(RvLocation)); } }

    private Role _role;
    public Role Role { get => _role; set { _role = value; NewRole(value.ToString(), nameof(Role)); } }

    private string _category = "none";
    public string Category { get => _category; set { _category = value; NewString(value, nameof(Category)); } }

    private List<Dictionary<int, string>> _rewards = new();
    public List<Dictionary<int, string>> Rewards {get => _rewards; set { _rewards = value; NewRewards(value); }}

    public System.Timers.Timer? Cooldown;
    private System.Timers.Timer? CounterCooldown { get; set; }
    private int _counter = 0;


    private string NewString(string value, string property)
    { _OnPropertyChanged(property, value); return value; }

    private HashSet<Permission> NewPerms(HashSet<Permission> permissions)
    { _OnPropertyChanged("permissions", PermissionsAsString(permissions)); return permissions; }

    private List<RvPunishment> NewPunishments(List<RvPunishment> punishments)
    { _OnPropertyChanged("punishments", PunishmentsAsString(punishments)); return punishments; }

    private List<Dictionary<int, string>> NewRewards(List<Dictionary<int, string>> rewards)
    { _OnPropertyChanged("rewards", RewardsAsString(rewards)); return rewards; }

    public event Action<string> OnPropertyChanged = delegate { };

    private void _OnPropertyChanged(string property, string value)
    {
        OnPropertyChanged(property); 
        UpdateDatabase(property, value);
        _counter++;
        if (_counter == 1)
        {
            CounterCooldown = new System.Timers.Timer(30000);
            CounterCooldown.Elapsed += CounterCooldownElapsed;
            CounterCooldown.Start();
        }

        if (Cooldown == null || !Cooldown.Enabled)
        {
            Cooldown = new System.Timers.Timer(TimerInterval());
            Cooldown.Elapsed += CooldownElapsed;
            Cooldown.Start();
        }
    }

    private void UpdateDatabase(string property, string value) => Program.database.Read($"UPDATE `RV_Users` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");

    private string PermissionsAsString(HashSet<Permission> permission)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var perm in permission)
            sb.Append(perm + ";");
        return sb.ToString();
    }

    private string PunishmentsAsString(List<RvPunishment> punishments)
    {
        StringBuilder sb = new();
        foreach (var punishment in punishments)
        {
            StringBuilder one = new();
            one.Append($"{punishment.Type};");
            one.Append(punishment.GroupId + ";");
            one.Append(punishment.Reason + ";");
            one.Append(punishment.From.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ";");
            one.Append(punishment.To.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ",");
            sb.Append(one.ToString());
        }
        return sb.ToString();
    }

    public void AddPunishment(RvPunishment.PunishmentType type, long groupId, string reason, DateTime from, DateTime to)
    {
        List<RvPunishment> newPunishments = new(Punishments)
        {
            new()
            {
                Type = type,
                GroupId = groupId,
                Reason = reason,
                From = from,
                To = to
            }
        };
        newPunishments.Reverse();
        Punishments = newPunishments;
    }

    private void NewRole(string value, string property)
    {
        switch (Role)
        {
            case Role.Admin:     AddPermissions(hashSet:PermissionLayouts.Admin);      break;
            case Role.Moderator: AddPermissions(hashSet:PermissionLayouts.Moderator);  break;
            case Role.Curator:   AddPermissions(hashSet:PermissionLayouts.Curator);    break;
            case Role.Developer: AddPermissions(hashSet:PermissionLayouts.Developer);  break;
        }

        NewString(value, property);
    }

    private string RewardsAsString(List<Dictionary<int, string>> rewards)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var dictionary in rewards)
            foreach (var reward in dictionary)
                sb.Append(reward.Value + ";");
        return sb.ToString();
    }

    public bool Has(Permission permission) => Permissions.Contains(permission);

    public void AddPermissions(Permission[]? array = null, HashSet<Permission>? hashSet = null)
    {
        HashSet<Permission> newPermissions = new(Permissions);
        if (array != null)
            foreach (Permission permission in array)
                newPermissions.Add(permission);

        else if (hashSet != null)
            foreach (Permission permission in hashSet)
                newPermissions.Add(permission);

        this.Permissions = newPermissions;
    }

    public void RemovePermission(Permission permission)
    {
        HashSet<Permission> newPermissions = new(this.Permissions);
        newPermissions.Remove(permission);
        this.Permissions = newPermissions;
    }

    public void AddReward(string reward)
    {
        List<Dictionary<int, string>> newRewards = new(this.Rewards);
        newRewards.Add(new(){ [newRewards.Count+1] = reward} );
        this.Rewards = newRewards;
    }

    private int TimerInterval()
    {
        if (_counter < 10)
            return 1000;
        else if (_counter < 20)
            return 5000;
        else if (_counter < 25)
            return 10000;
        else
            return 500;
    }

    private void CooldownElapsed(object sender, ElapsedEventArgs e) => Cooldown?.Stop();
    private void CounterCooldownElapsed(object sender, ElapsedEventArgs e)
    {
        CounterCooldown?.Stop();
        _counter = 0;
    }

    public static RvUser Get(long userId)
    {
        foreach (var user in Program.users)
            if (userId == user.UserId)
                return user;

        return null;
    }
}

public class RvPunishment
{
    public PunishmentType Type;
    public long GroupId;
    public string? Reason;
    public DateTime From;
    public DateTime To;

    public enum PunishmentType
    {
        Ban,
        Mute
    }
}