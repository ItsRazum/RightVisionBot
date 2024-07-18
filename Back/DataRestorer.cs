using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Microsoft.Extensions.Logging;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.Types;
using RightVisionBot.User;

//система восстановления данных после перезагрузки бота, а также синхронизация с базой данных
namespace RightVisionBot.Back
{
    internal class Data
    {
        public static volatile List<RvUser> RvUsers = new();
        public static volatile List<RvCritic> RvCritics = new();
        public static volatile List<RvMember> RvMembers = new();
        public static volatile List<RvExMember> RvExMembers = new();
    }

    class DataRestorer
    {
        static sql database = Program.database;

        public static void RestoreUsers()
        {
            Language.Build(new[] { "ru", "ua", "kz" });

            Console.WriteLine("Восстановление данных...");
            {
                string[] columns = { "userId", "lang", "status", "rvLocation", "role", "category", "name" };
                var uRead = database.ExtRead("SELECT * FROM `RV_Users`", columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var user = new RvUser
                    (
                        long.Parse(userDb[columns[0]]), 
                        userDb[columns[1]], 
                        Enum.Parse<Status>(userDb[columns[2]]), 
                        Enum.Parse<RvLocation>(userDb[columns[3]]), 
                        Enum.Parse<Role>(userDb[columns[4]]), 
                        userDb[columns[5]],
                        userDb[columns[6]],
                        false
                    );
                    i++;
                    Data.RvUsers.Add(user);
                }
                Console.WriteLine($"Данные пользователей восстановлены ({i})\n");
            }
            Console.WriteLine("Восстановление прав доступа...");
            {
                var users = database.ExtRead("SELECT `userId`, `permissions` FROM `RV_Users`;",
                    new[] { "userId", "permissions" });
                foreach (var user in users)
                {
                    long userId = long.Parse(user["userId"]);
                    RvUser rvUser = RvUser.Get(userId);
                    if (user["permissions"] == "None" || user["permissions"] == "")
                    {
                        UserPermissions permissions = new(Permissions.Empty);

                        permissions += Permissions.Layouts[rvUser.Role] + Permissions.Layouts[rvUser.Status];

                        database.Read($"UPDATE `RV_Users` SET `permissions` = '{rvUser.Permissions}' WHERE `userId` = {userId}", "");
                        rvUser.Permissions = permissions;
                    }
                    else
                    {
                        var permissionStrings = user["permissions"].Split(";");
                        List<Permission> perms = new();
                        List<Permission> blockedPerms = new();
                        foreach (var permission in permissionStrings)
                        {
                            if (permission == "") continue;
                            if (!permission.StartsWith("::"))
                                perms.Add(Enum.Parse<Permission>(permission));
                            else
                                blockedPerms.Add(Enum.Parse<Permission>(permission[2..]));
                        }

                        RvUser.Get(userId).Permissions = new UserPermissions(perms);
                        RvUser.Get(userId).Permissions.Removed = blockedPerms;
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
                    
                    if (!string.IsNullOrEmpty(fromCritics) && fromCritics is not "denied" and "unfinished")
                        rvUser.Category = fromCritics;

                    if (!string.IsNullOrEmpty(fromMembers) && fromMembers is not "denied" and "unfinished")
                        rvUser.Category = fromMembers;
                }
            }
            Console.WriteLine("Категории восстановлены\n");
            
            Console.WriteLine("Восстановление наград");
            {
                var rewards = database.ExtRead("SELECT `userId`, `rewards` FROM `RV_Users`;", new[] { "userId", "rewards" });
                foreach (var user in rewards)
                {
                    var rewardStrings = user["rewards"].Split(";");
                    foreach (var rewardString in rewardStrings)
                    {
                        var rewardValues = rewardString.Split(":");
                        if (string.IsNullOrEmpty(rewardValues[0])) continue;

                        Reward reward = new(rewardValues[0], rewardValues[2]);
                        RvUser.Get(long.Parse(user["userId"])).Rewards.Add(reward);
                    }
                }
            }
            Console.WriteLine("Награды восстановлены\n");

            Console.WriteLine("Восстановление наказаний");
            {
                var punishments = database.ExtRead("SELECT `userId`, `punishments` FROM `RV_Users`;", new[] { "userId", "punishments" });
                foreach (var user in punishments)
                {
                    var rvUser = RvUser.Get(long.Parse(user["userId"]));

                    var punishmentsString = user["punishments"];
                        var allPunishments = punishmentsString.Split(",");

                    foreach (var punishment in allPunishments)
                    {
                        if (punishment == "") continue;

                        var punishmentArgs = punishment.Split(";");
                        RvPunishment pun = new(
                            Enum.Parse<RvPunishment.PunishmentType>(punishmentArgs[0]),
                            long.Parse(punishmentArgs[1]),
                            punishmentArgs[2],
                            DateTime.Parse(punishmentArgs[3], CultureInfo.GetCultureInfo("en-US").DateTimeFormat),
                            DateTime.Parse(punishmentArgs[4], CultureInfo.GetCultureInfo("en-US").DateTimeFormat)
                        );
                        rvUser.Punishments.Add(pun);
                    }
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
                    _ = new RvCritic
                        (
                            long.Parse(userDb[columns[2]]),
                            userDb[columns[0]],
                            userDb[columns[1]],
                            userDb[columns[3]],
                            userDb[columns[4]],
                            userDb[columns[5]],
                            userDb[columns[6]],
                            long.Parse(userDb[columns[7]]),
                            userDb[columns[8]],
                            long.Parse(userDb[columns[9]])
                        );
                    i++;
                }

                Console.WriteLine($"Данные судей восстановлены ({i})\n");
            }
            Console.WriteLine("Восстановление данных участников...");
            {
                string[] columns = { "name", "telegram", "userId", "link", "rate", "track", "curator", "status" };
                var usersQuery = "SELECT * FROM `RV_Members`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    _ = new RvMember
                        (
                            long.Parse(userDb[columns[2]]), 
                            userDb[columns[0]], 
                            userDb[columns[1]], 
                            userDb[columns[3]], 
                            userDb[columns[4]], 
                            userDb[columns[5]], 
                            long.Parse(userDb[columns[6]]), 
                            userDb[columns[7]]
                        );
                    i++;
                    RvUser.Get(long.Parse(userDb[columns[2]])).Name = userDb[columns[0]];
                }

                Console.WriteLine($"Данные участников восстановлены ({i})\n");
            }

            Console.WriteLine("Восстановление данных бывших участников...");
            {
                string[] columns = { "name", "telegram", "userId", "link", "rate", "track", "curator", "status" };
                var usersQuery = "SELECT * FROM `RV_ExMembers`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    _ = new RvExMember
                    (
                        long.Parse(userDb[columns[2]]),
                        userDb[columns[0]],
                        userDb[columns[1]],
                        userDb[columns[3]],
                        userDb[columns[4]],
                        userDb[columns[5]],
                        long.Parse(userDb[columns[6]]),
                        userDb[columns[7]]
                    );
                    i++;
                }

                Console.WriteLine($"Данные бывших участников восстановлены ({i})\n");
            }

            Console.WriteLine("Актуализация данных в общей таблице");
            {
                var idList = database.Read("SELECT `userId` FROM `RV_Users`;", "userId");
                foreach (var ids in idList)
                {
                    long id = long.Parse(ids);
                    var fromCritics = database.Read(
                            $"SELECT `status` FROM `RV_Critics` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                            "status").FirstOrDefault();
                    var fromMembers = database.Read(
                            $"SELECT `status` FROM `RV_Members` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                            "status").FirstOrDefault();
                    var fromExMembers = database.Read(
                        $"SELECT `status` FROM `RV_ExMembers` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                        "status").FirstOrDefault();

                    if (fromMembers != null && fromCritics == null)
                    {
                        RvUser.Get(id).Status = Status.Member;
                        RvUser.Get(id).Category = fromMembers;
                    }
                    else if (fromMembers == null && fromCritics != null && fromExMembers == null)
                    {
                        RvUser.Get(id).Status = Status.Critic;
                        RvUser.Get(id).Category = fromCritics;
                    }
                    else if (fromMembers != null && fromCritics != null)
                    {
                        RvUser.Get(id).Status = Status.CriticAndMember;
                        RvUser.Get(id).Category = fromMembers;
                    }
                    else if (fromExMembers != null && fromCritics == null && fromMembers == null)
                    {
                        RvUser.Get(id).Status = Status.ExMember;
                        RvUser.Get(id).Category = fromExMembers;
                    }
                    else if (fromExMembers != null && fromCritics != null && fromMembers == null)
                    {
                        RvUser.Get(id).Status = Status.CriticAndExMember;
                        RvUser.Get(id).Category = fromExMembers;
                    }
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
                string[] categories = { "bronze", "silver", "gold", "brilliant" };

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
