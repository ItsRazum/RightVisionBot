using System.Text;
using RightVisionBot.Back;

namespace RightVisionBot.Common;

public class RvUser
{
    public long UserId;

    private Status _status = Status.User;
    public Status Status { get => _status; set { _status = value; newString(value.ToString(), nameof(Status)); } }

    private HashSet<Permission> _permissions = new HashSet<Permission>(PermissionLayouts.User);
    public HashSet<Permission> Permissions { get => _permissions; set { _permissions = value; newPerms(value); } }

    private string _lang = "ru";
    public string Lang { get => _lang; set { _lang = value; newString(value, nameof(Lang)); } }

    private RvLocation _rvLocation = RvLocation.MainMenu;
    public RvLocation RvLocation { get => _rvLocation; set { _rvLocation = value; newString(value.ToString(), nameof(RvLocation)); } }

    private Role _role;
    public Role Role { get => _role; set { _role = value; newRole(value.ToString(), nameof(Role)); } }

    private string _category = "none";
    public string Category { get => _category; set { _category = value; newString(value, nameof(Category)); } }

    private List<Dictionary<int, string>> _rewards = new();
    public List<Dictionary<int, string>> Rewards {get => _rewards; set { _rewards = value; newRewards(value); }}


    private string newString(string value, string property)
    { _OnPropertyChanged(property, value); return value; }

    private HashSet<Permission> newPerms(HashSet<Permission> permissions)
    { _OnPropertyChanged("permissions", PermissionsAsString(permissions)); return permissions; }

    private List<Dictionary<int, string>> newRewards(List<Dictionary<int, string>> rewards)
    { _OnPropertyChanged("rewards", RewardsAsString(rewards)); return rewards; }

    public event Action<string> OnPropertyChanged = delegate { };
    private void _OnPropertyChanged(string property, string value)
    { OnPropertyChanged(property); UpdateDatabase(property, value); }

    private void UpdateDatabase(string property, string value) => Program.database.Read($"UPDATE `RV_Users` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");

    private string PermissionsAsString(HashSet<Permission> permission)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var perm in permission)
            sb.Append(perm + ";");
        return sb.ToString();
    }

    private void newRole(string value, string property)
    {
        switch (Role)
        {
            case Role.Admin:     this.AddPermissions(hashSet:PermissionLayouts.Admin);      break;
            case Role.Moderator: this.AddPermissions(hashSet:PermissionLayouts.Moderator);  break;
            case Role.Curator:   this.AddPermissions(hashSet:PermissionLayouts.Curator);    break;
            case Role.Developer: this.AddPermissions(hashSet:PermissionLayouts.Developer);  break;
        }

        newString(value, property);
    }

    private string RewardsAsString(List<Dictionary<int, string>> rewards)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var dictionary in rewards)
            foreach (var reward in dictionary)
                sb.Append(reward.Value + ";");
        return sb.ToString();
    }

    public bool Has(Role role) => this.Role == role;
    public bool Has(Permission permission) => Permissions.Contains(permission);

    public void AddPermissions(Permission[]? array = null, HashSet<Permission>? hashSet = null)
    {
        HashSet<Permission> newPermissions = new(this.Permissions);
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

    public static RvUser Get(long userId)
    {
        foreach (var user in Program.users)
            if (userId == user.UserId)
                return user;

        return null;
    }
}