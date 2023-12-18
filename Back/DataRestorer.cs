using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Tracks;
using RightVisionBot.Tracks;
using RightVisionBot.User;
using Telegram.Bot.Types;

//система восстановления данных после перезагрузки бота, а также синхронизация с базой данных
namespace RightVisionBot.Back
{
    class Data
    {
        public static volatile List<long> Subscribers = new();
    }
    class DataRestorer
    {
        static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");
        public static void RestoreUsers()
        {
            Console.WriteLine("Восстановление данных...");
            {
                string[] columns = new[] { "username", "id", "lang", "location" };
                var usersQuery = "SELECT * FROM `RV_Users`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var user = new RV_User
                    {
                        userName = userDb["username"].ToString(),
                        userId = long.Parse(userDb["id"].ToString()),
                        lang = userDb["lang"].ToString(),
                        location = userDb["location"].ToString(),
                    };
                    i++;
                    Program.users.Add(user);
                }
                Console.WriteLine($"Данные пользователей восстановлены ({i})\n");
            }
            Console.WriteLine("Начался процесс восстановления данных критиков");
            {
                string[] columns = new[] { "name", "telegram", "userId", "link", "rate", "about", "whyyou", "curator", "status" };
                var usersQuery = "SELECT * FROM `RV_Critics`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var critic = new Critic
                    {
                        Name = userDb[columns[0]].ToString(),
                        Telegram = userDb[columns[1]].ToString(),
                        UserId = long.Parse(userDb[columns[2]].ToString()),
                        Link = userDb[columns[3]].ToString(),
                        Rate = userDb[columns[4]].ToString(),
                        About = userDb[columns[5]].ToString(),
                        WhyYou = userDb[columns[6]].ToString(),
                        Curator = long.Parse(userDb[columns[7]].ToString()),
                        Status = userDb[columns[8]].ToString()
                    };
                    i++;
                    CriticRoot.newCritics.Add(critic);
                }
                Console.WriteLine($"Данные критиков восстановлены ({i})\n");
            }
            Console.WriteLine("Начался процесс восстановления данных пользователей");
            {
                string[] columns = new[] { "name", "telegram", "userId", "country", "city", "link", "rate", "track", "curator", "status" };
                var usersQuery = "SELECT * FROM `RV_Members`";
                var uRead = database.ExtRead(usersQuery, columns);
                int i = 0;
                foreach (var userDb in uRead)
                {
                    var member = new Member
                    {
                        Name = userDb[columns[0]].ToString(),
                        Telegram = userDb[columns[1]].ToString(),
                        UserId = long.Parse(userDb[columns[2]].ToString()),

                        Country = userDb[columns[3]].ToString(),
                        City = userDb[columns[4]].ToString(),
                        Link = userDb[columns[5]].ToString(),
                        Rate = userDb[columns[6]].ToString(),
                        Track = userDb[columns[7]].ToString(),
                        Curator = long.Parse(userDb[columns[8]].ToString()),
                        Status = userDb[columns[9]].ToString()
                    };
                    i++;
                    MemberRoot.newMembers.Add(member);
                }
                Console.WriteLine($"Данные участников восстановлены ({i})\n");
            }
            Console.WriteLine("Актуализация данных в общей таблице");
            {
                var idList = database.Read("SELECT `id` FROM `RV_Users`;", "id");
                foreach (var ids in idList)
                {
                    long id = long.Parse(ids);
                    var fromCritics = database.Read(
                        $"SELECT `userId` FROM `RV_Critics` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                        "userId");
                    var fromMembers = database.Read(
                        $"SELECT `userId` FROM `RV_Members` WHERE `userId` = {id} AND `status` != 'denied' AND `status` != 'waiting' AND `status` != 'unfinished';",
                        "userId");
                    if (fromMembers.FirstOrDefault() != null && fromCritics.FirstOrDefault() == null)
                        database.Read(
                            $"UPDATE `RV_Users` SET `status` = 'member' WHERE `id` = {id};",
                            "");
                    else if (fromMembers.FirstOrDefault() == null && fromCritics.FirstOrDefault() != null)
                        database.Read(
                            $"UPDATE `RV_Users` SET `status` = 'critic' WHERE `id` = {id};",
                            "");
                    else if (fromMembers.FirstOrDefault() != null && fromCritics.FirstOrDefault() != null)
                        database.Read(
                            $"UPDATE `RV_Users` SET `status` = 'criticAndMember' WHERE `id` = {id};",
                            "");
                    else
                    {
                        database.Read(
                            $"UPDATE `RV_Users` SET `status` = 'user' WHERE `id` = {id};",
                            "");
                    }
                }
                Console.WriteLine("Актуализация завершена");
            }
            Console.WriteLine("Восстановление списка подписчиков...\n");
            {
                var idList = database.Read("SELECT `id` FROM `RV_Sending`;", "id");
                foreach (var ids in idList)
                {
                    long id = long.Parse(ids);
                    Data.Subscribers.Add(id);
                }
            }
            Console.WriteLine("Завершено. Восстановление карточек треков участников...");
            {
                string[] columns = new[] { "userId", "track", "image", "text" };
                var cardList = database.ExtRead("SELECT * FROM `RV_Tracks`", columns);
                int i = 0;
                foreach (var card in cardList)
                {
                    var trackCard = new TrackInfo()
                    {
                        UserId = long.Parse(card[columns[0]].ToString()),
                        Track = String.IsNullOrEmpty(card[columns[1]].ToString()) ? null : card[columns[1]].ToString(),
                        Image = String.IsNullOrEmpty(card[columns[2]].ToString()) ? null : card[columns[2]].ToString(),
                        Text = String.IsNullOrEmpty(card[columns[3]].ToString()) ? null : card[columns[3]].ToString()
                    };
                    Track.Tracks.Add(trackCard);
                    i++;
                }

                Console.WriteLine($"Восстановлено {i} карточек\n");
            }
            Console.WriteLine("Восстановление данных предварительного прослушивания");
            {
                string[] columns = new[] { "listenerId", "artistId" };
                var listeners = database.ExtRead("SELECT * FROM `RV_PreListening`", columns);
                int i = 0;
                foreach (var listener in listeners)
                {
                    var lsnr = new PreListener()
                    {
                        ListenerId = long.Parse(listener[columns[0]].ToString()),
                        ArtistId = long.Parse(listener[columns[1]].ToString())
                    };
                    PreListening.preListeners.Add(lsnr);
                    i++;
                }

                Console.WriteLine($"Восстановлено {i} прослушиваний\n");
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
                        UserId = long.Parse(rate["userId"].ToString()),
                        ArtistId = long.Parse(rate["artistId"].ToString()),
                        Rate1 = int.Parse(rate["rate1"].ToString()),
                        Rate2 = int.Parse(rate["rate2"].ToString()),
                        Rate3 = int.Parse(rate["rate3"].ToString()),
                        Rate4 = int.Parse(rate["rate4"].ToString())
                    };
                    TrackEvaluation.Rates.Add(_rate);
                    i++;
                }

                Console.WriteLine($"Восстановлено {i} оцениваний");
            }

            Console.WriteLine("Актуализация данных в таблицах рейтинга треков");

            string[] categories = new[] { "bronze", "steel", "gold", "brilliant" };

            foreach (var category in categories)
            {
                var values = database.ExtRead($"SELECT m.userId, m.track, m.status AS category, COALESCE(t.status, 'notfinished') AS status FROM RV_Members m LEFT JOIN RV_Tracks t ON m.userId = t.userId WHERE m.status = '{category}' AND t.status = 'ok';", new[] { "userId", "track", "category", "status" });

                foreach (var user in values)
                {
                    string userId = user["userId"].ToString();
                    string track = user["track"].ToString();
                    string userStatus = user["status"].ToString();
                    string categoryName = user["category"].ToString();

                    bool userExists = database.Read($"SELECT `userId` FROM RV_C{categoryName.ToLower()} WHERE userId = '{userId}';", "userId").FirstOrDefault() != null;

                    if (!userExists)
                    {
                        string insertQuery = $"INSERT INTO RV_C{categoryName.ToLower()} (userId, track, status) VALUES ('{userId}', '{track}', '{userStatus}');";
                        database.Read(insertQuery, "");
                    }
                    else
                    {
                        string updateQuery = $"UPDATE RV_C{categoryName.ToLower()} SET track = '{track}', status = '{userStatus}' WHERE userId = '{userId}';";
                        database.Read(updateQuery, "");
                        if (userStatus != "ok")
                        {
                            database.Read($"DELETE FROM `RV_C{categoryName.ToLower()}` WHERE `userId` = {userId};", "");
                        }
                    }
                }
            }
        }
    }
}
