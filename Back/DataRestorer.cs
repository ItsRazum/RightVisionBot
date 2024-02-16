using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Microsoft.Extensions.Logging;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.User;

//система восстановления данных после перезагрузки бота, а также синхронизация с базой данных
namespace RightVisionBot.Back
{
    class Data
    {
        public static volatile List<RvUser> RvUsers = new();
        

        public static volatile List<RvCritic> RvCritics = new();
        public static volatile List<RvMember> RvMembers = new();


    }

    class DataRestorer
    {
        static sql database = Program.database;

        public static void RestoreUsers()
        {
            Console.WriteLine("Восстановление данных...");
            {
                string[] columns = { "userId", "lang", "status", "rvLocation", "role", "category" };
                var usersQuery = "SELECT * FROM `RV_Users`";
                List<Dictionary<string, string>> uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var user = new RvUser()
                    {
                        UserId = long.Parse(userDb[columns[0]]),
                        Lang = userDb[columns[1]],
                        Status = Enum.Parse<Status>(userDb[columns[2]]),
                        RvLocation = Enum.Parse<RvLocation>(userDb[columns[3]]),
                        Role = Enum.Parse<Role>(userDb[columns[4]]),
                        Category = userDb[columns[5]],
                        Cooldown = new System.Timers.Timer(1)
                    };
                    i++;
                    Data.RvUsers.Add(user);
                }

                Console.WriteLine($"Данные пользователей восстановлены ({i})\n");
            }
            Console.WriteLine("Восстановление прав доступа...");
            {
                var permissions = database.ExtRead("SELECT `userId`, `permissions` FROM `RV_Users`;",
                    new[] { "userId", "permissions" });
                foreach (var permission in permissions)
                {
                    long userId = long.Parse(permission["userId"]);
                    RvUser rvUser = RvUser.Get(userId);
                    HashSet<Permission> perms = new HashSet<Permission>();
                    if (permission["permissions"] == "None")
                    {
                        switch (rvUser.Status)
                        {
                            case Status.User:
                                perms = PermissionLayouts.User;
                                break;
                            case Status.Critic:
                                perms = PermissionLayouts.Critic;
                                break;
                            case Status.Member:
                                perms = PermissionLayouts.Member;
                                break;
                            case Status.CriticAndMember:
                                perms = PermissionLayouts.CriticAndMember;
                                break;
                        }

                        StringBuilder sb = new StringBuilder();
                        foreach (var perm in perms)
                            sb.Append(perm + ";");
                        database.Read($"UPDATE `RV_Users` SET `Permissions` = '{sb}' WHERE `userId` = {userId}", "");
                        switch (rvUser.Role)
                        {
                            case Role.Admin:
                                rvUser.AddPermissions(hashSet: PermissionLayouts.Admin);
                                break;
                            case Role.Curator:
                                rvUser.AddPermissions(hashSet: PermissionLayouts.Curator);
                                break;
                            case Role.Moderator:
                                rvUser.AddPermissions(hashSet: PermissionLayouts.Moderator);
                                break;
                            case Role.Developer:
                                rvUser.AddPermissions(hashSet: PermissionLayouts.Developer);
                                break;
                        }
                    }
                    else
                    {
                        string[] permissionStrings = permission["permissions"].Split(";");

                        foreach (var _permission in permissionStrings)
                            if (_permission != "")
                                perms.Add(Enum.Parse<Permission>(_permission));

                        RvUser.Get(userId).Permissions = perms;
                    }
                }

                Console.WriteLine("Права доступа восстановлены\n");
            }
            Console.WriteLine("Восстановление категорий");
            {
                var categories = database.ExtRead("SELECT `userId`, `category` FROM `RV_Users`;", new[] { "userId", "category" });
                foreach (var category in categories)
                {
                    long userId = long.Parse(category["userId"]);
                    RvUser rvUser = RvUser.Get(userId);
                    var fromMembers = database.Read($"SELECT `status` FROM `RV_Members` WHERE `userId` = '{userId}'", "status").FirstOrDefault();
                    var fromCritics = database.Read($"SELECT `status` FROM `RV_Critics` WHERE `userId` = '{userId}'", "status").FirstOrDefault();
                    switch (fromCritics)
                    {
                        case "denied":
                        case "unfinished":
                        case null:
                            break;
                        default:
                            rvUser.Category = fromCritics;
                            break;
                    }

                    switch (fromMembers)
                    {
                        case "denied":
                        case "unfinished":
                        case null:
                            break;
                        default:
                            rvUser.Category = fromMembers;
                            break;
                    }
                }
            }
            Console.WriteLine("Категории восстановлены\n");

            Console.WriteLine("Восстановление наград");
            {
                var rewards = database.ExtRead("SELECT `userId`, `rewards` FROM `RV_Users`;", new[] { "userId", "rewards" });
                foreach (var user in rewards)
                {
                    string[] rewardStrings = user["rewards"].Split(";");
                    foreach (var reward in rewardStrings)
                        if (reward != "" && reward != "None")
                            RvUser.Get(long.Parse(user["userId"])).AddReward(reward);
                }
            }
            Console.WriteLine("Награды восстановлены\n");

            Console.WriteLine("Восстановление наказаний");
            {
                var punishments = database.ExtRead("SELECT `userId`, `punishments` FROM `RV_Users`;", new[] { "userId", "punishments" });
                foreach (var user in punishments)
                {
                    RvUser rvUser = RvUser.Get(long.Parse(user["userId"]));

                    string punishmentsString = user["punishments"];
                    List<RvPunishment> punishmentsList = new List<RvPunishment>();
                        string[] allPunishments = punishmentsString.Split(",");

                    foreach (string punishment in allPunishments)
                    {
                        if (punishment != "")
                        {
                            string[] punishmentArgs = punishment.Split(";");
                            RvPunishment pun = new RvPunishment();

                            pun.Type = Enum.Parse<RvPunishment.PunishmentType>(punishmentArgs[0]);
                            pun.GroupId = long.Parse(punishmentArgs[1]);
                            pun.Reason = punishmentArgs[2];
                            pun.From = DateTime.Parse(punishmentArgs[3], CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
                            pun.To =   DateTime.Parse(punishmentArgs[4], CultureInfo.GetCultureInfo("en-US").DateTimeFormat);

                            punishmentsList.Add(pun);
                        }
                    }
                    rvUser.Punishments = punishmentsList;
                }
            }
            Console.WriteLine("Наказания восстановлены\n");

            Console.WriteLine("Восстановление данных судей...");
            {
                string[] columns = { "name", "telegram", "userId", "link", "rate", "about", "whyyou", "curator", "status", "prelisteningartist" };
                var usersQuery = "SELECT * FROM `RV_Critics`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var critic = new RvCritic
                    {
                        UserId = long.Parse(userDb[columns[2]]),
                        Curator = long.Parse(userDb[columns[7]]),
                        PreListeningArtist = long.Parse(userDb[columns[9]]),
                        Name = userDb[columns[0]],
                        Telegram = userDb[columns[1]],
                        Link = userDb[columns[3]],
                        Rate = userDb[columns[4]],
                        About = userDb[columns[5]],
                        WhyYou = userDb[columns[6]],
                        Status = userDb[columns[8]],
                    };
                    i++;
                    Data.RvCritics.Add(critic);
                }

                Console.WriteLine($"Данные судей восстановлены ({i})\n");
            }
            Console.WriteLine("Восстановление данных участников...");
            {
                string[] columns = { "name", "telegram", "userId", "country", "city", "link", "rate", "track", "curator", "status" };
                var usersQuery = "SELECT * FROM `RV_Members`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var member = new RvMember
                    {
                        UserId = long.Parse(userDb[columns[2]]),
                        Curator = long.Parse(userDb[columns[8]]),
                        Name = userDb[columns[0]],
                        Telegram = userDb[columns[1]],
                        Country = userDb[columns[3]],
                        City = userDb[columns[4]],
                        Link = userDb[columns[5]],
                        Rate = userDb[columns[6]],
                        TrackStr = userDb[columns[7]],
                        Status = userDb[columns[9]]
                    };
                    i++;
                    Data.RvMembers.Add(member);
                }

                Console.WriteLine($"Данные участников восстановлены ({i})\n");
            }
            Console.WriteLine("Актуализация данных в общей таблице");
            {
                var idList = database.Read("SELECT `userId` FROM `RV_Users`;", "userId");
                foreach (var ids in idList)
                {
                    long id = long.Parse(ids);
                    var fromCritics = database.Read(
                            $"SELECT `userId` FROM `RV_Critics` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                            "userId").FirstOrDefault();
                    var fromMembers = database.Read(
                            $"SELECT `userId` FROM `RV_Members` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                            "userId").FirstOrDefault();

                    if (fromMembers != null && fromCritics == null)
                        RvUser.Get(id).Status = Status.Member;
                    else if (fromMembers == null && fromCritics != null)
                        RvUser.Get(id).Status = Status.Critic;
                    else if (fromMembers != null && fromCritics != null)
                        RvUser.Get(id).Status = Status.CriticAndMember;
                    else
                        RvUser.Get(id).Status = Status.User;
                }

                Console.WriteLine("Актуализация завершена\n");
            }
            Console.WriteLine("Завершено. Восстановление карточек треков участников...");
            {
                string[] columns = { "userId", "track", "image", "text" };
                var cardList = database.ExtRead("SELECT * FROM `RV_Tracks`", columns);
                int i = 0;
                foreach (var card in cardList)
                {
                    var trackCard = new TrackInfo()
                    {
                        UserId = long.Parse(card[columns[0]]),
                        Track = string.IsNullOrEmpty(card[columns[1]]) ? null : card[columns[1]],
                        Image = string.IsNullOrEmpty(card[columns[2]]) ? null : card[columns[2]],
                        Text =  string.IsNullOrEmpty(card[columns[3]]) ? null : card[columns[3]]
                    };
                    RvMember.Get(trackCard.UserId).Track = trackCard; 
                    i++;
                }

                Console.WriteLine($"Восстановлено {i} карточек\n");
            }

            Console.WriteLine("Восстановление оценок судей");
            {
                var rates = database.ExtRead($"SELECT * FROM `RV_Rates`;",
                    new[] { "userId", "artistId", "rate1", "rate2", "rate3", "rate4" });
                int i = 0;
                foreach (var rate in rates)
                {
                    CriticVote _rate = new()
                    {
                        UserId = long.Parse(rate["userId"]),
                        ArtistId = long.Parse(rate["artistId"]),
                        Rate1 = int.Parse(rate["rate1"]),
                        Rate2 = int.Parse(rate["rate2"]),
                        Rate3 = int.Parse(rate["rate3"]),
                        Rate4 = int.Parse(rate["rate4"])
                    };
                    RvCritic.Get(_rate.UserId).CriticRate = _rate;
                    i++;
                }

                Console.WriteLine($"Восстановлено {i} оцениваний");
            }

            Console.WriteLine("Актуализация данных в таблицах рейтинга треков");
            {
                string[] categories = { "bronze", "steel", "gold", "brilliant" };

                foreach (var category in categories)
                {
                    var values = database.ExtRead(
                        $"SELECT m.userId, m.track, m.status AS category, COALESCE(t.status, 'notfinished') AS status FROM RV_Members m LEFT JOIN RV_Tracks t ON m.userId = t.userId WHERE m.status = '{category}' AND t.status = 'ok';",
                        new[] { "userId", "track", "category", "status" });

                    foreach (var user in values)
                    {
                        string userId = user["userId"];
                        string track = user["track"];
                        string userStatus = user["status"];
                        string categoryName = user["category"];

                        bool userExists = database.Read($"SELECT `userId` FROM RV_C{categoryName.ToLower()} WHERE userId = '{userId}';", "userId").FirstOrDefault() != null;

                        if (!userExists)
                            database.Read($"INSERT INTO RV_C{categoryName.ToLower()} (userId, track, status) VALUES ('{userId}', '{track}', '{userStatus}');", "");
                        else
                        {
                            database.Read($"UPDATE RV_C{categoryName.ToLower()} SET track = '{track}', status = '{userStatus}' WHERE userId = '{userId}';", "");
                            if (userStatus != "ok")
                                database.Read($"DELETE FROM `RV_C{categoryName.ToLower()}` WHERE `userId` = {userId};", "");
                        }
                    }
                }
            }
        }
    }
}
