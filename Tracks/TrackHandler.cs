using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

//система чтения, записи и отправки треков
namespace RightVisionBot.Tracks
{
    class TrackInfo
    {
        public long UserId { get; set; }
        public string? Track = null;
        public string? Image = null;
        public string? Text = null;
    }

    class Track
    {
        public static volatile List<TrackInfo> Tracks = new List<TrackInfo>();
        private static sql database = Program.database;
        public static void Send(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            if (GetTrack(userId) != null)
                    TrackCard(true, botClient, message);
            else
            {
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_CreatingCard", RvUser.Get(userId).Lang));
                    TrackInfo track = new() { UserId = userId };
                    Tracks.Add(track);
                    database.Read($"INSERT INTO `RV_Tracks`(`userId`) VALUES ({userId});", "");
                    TrackCard(false, botClient, message);
            }
        }

        public static void TrackCard(bool isExists, ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            InlineKeyboardMarkup inline = new(
                new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckTrack", RvUser.Get(userId).Lang) + "♂", "t_CheckTrack")  },
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckImage", RvUser.Get(userId).Lang) + "🖼", "t_CheckImage") },
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckText", RvUser.Get(userId).Lang) + "📝", "t_CheckText") }
                });
            /*
            ReplyKeyboardMarkup keyboard = new(new[]
                {
                    new[] { new KeyboardButton(Language.GetPhrase("Profile_Track_SendTrack", RvUser.Get(userId).Lang) + "♂") },
                    new[] { new KeyboardButton(Language.GetPhrase("Profile_Track_SendImage", RvUser.Get(userId).Lang) + "🖼") },
                    new[] { new KeyboardButton(Language.GetPhrase("Profile_Track_SendText", RvUser.Get(userId).Lang) + "📝") },
                    new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", RvUser.Get(userId).Lang)) }
                })
                { ResizeKeyboard = true };
            */
            ReplyKeyboardMarkup keyboard = new(new[]
                    { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", RvUser.Get(userId).Lang)) } })
                { ResizeKeyboard = true };

            Program.updateRvLocation(userId, RvLocation.TrackCard);
            switch (isExists)
            {
                case false:
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_CreatingCard_Success", RvUser.Get(userId).Lang), replyMarkup: keyboard);
                    botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} создал карточку ремикса\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    break;
                case true:
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_Card_HereItIs", RvUser.Get(userId).Lang), replyMarkup: keyboard);
                    botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл свою карточку ремикса\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    break;
            }
            botClient.SendTextMessageAsync(message.Chat, 
                $"{RvMember.Get(userId).Track}\n\n" 
                + string.Format(Language.GetPhrase("Profile_Track_Card", RvUser.Get(userId).Lang) , IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))
                + $"\n\n{CardStatus(userId, IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))}", replyMarkup: inline);
        }

        public static string IsTrackSent(long userId)
        {
            if (GetTrack(userId) != null)
                return Language.GetPhrase(GetTrack(userId).Track != null? "Profile_Track_TrackSent" : "Profile_Track_TrackNotSent", RvUser.Get(userId).Lang);

            return null;
        }

        public static string IsImageSent(long userId)
        {
            if (GetTrack(userId) != null)
                return Language.GetPhrase(GetTrack(userId).Image != null? "Profile_Track_ImageSent" : "Profile_Track_ImageNotSent", RvUser.Get(userId).Lang);

            return null;
        }

        public static string IsTextSent(long userId)
        {
            if (GetTrack(userId) != null)
                return Language.GetPhrase(GetTrack(userId).Text != null? "Profile_Track_TrackSent" : "Profile_Track_TrackNotSent", RvUser.Get(userId).Lang);

            return null;
        }

        public static TrackInfo GetTrack(long userId)
        {
            foreach (var trackInfo in Tracks)
            {
                if (trackInfo.UserId == userId) 
                    return trackInfo;
            }
            return null;
        }

        public static void SendFilesByOne(ITelegramBotClient botClient, int number)
        {
            List<Dictionary<string, object>> files;
            if(number == 1) files = database.ExtRead($"SELECT * FROM RV_Tracks LIMIT 1 OFFSET 1;", new[] { "userId", "track", "image", "text" });
            else files = database.ExtRead($"SELECT * FROM RV_Tracks LIMIT 1 OFFSET {number - 1};", new[] { "userId", "track", "image", "text" });

            foreach (var file in files)
            {
                var track = database.Read($"SELECT `track` FROM `RV_Members` WHERE `userId` = '{file["userId"]}';", "track");
                try
                {
                    botClient.SendDocumentAsync(-1001968408177, new InputFileId(file["track"].ToString()),
                        caption:
                        $"Название: {track.FirstOrDefault()}\nКатегория: {RvMember.Get(long.Parse(file["userId"].ToString())).Status}");
                    botClient.SendPhotoAsync(-1001968408177, new InputFileId(file["image"].ToString()),
                        caption:
                        $"Название: {track.FirstOrDefault()}\nКатегория: {RvMember.Get(long.Parse(file["userId"].ToString())).Status}");
                    botClient.SendDocumentAsync(-1001968408177, new InputFileId(file["text"].ToString()),
                        caption:
                        $"Название: {track.FirstOrDefault()}\nКатегория: {RvMember.Get(long.Parse(file["userId"].ToString())).Status}");
                }
                catch
                {
                    try
                    {
                        botClient.SendDocumentAsync(-1001968408177, new InputFileId(file["track"].ToString()),
                            caption:
                            $"Название: {track.FirstOrDefault()}\nКатегория: {RvMember.Get(long.Parse(file["userId"].ToString())).Status}");
                        botClient.SendPhotoAsync(-1001968408177, new InputFileId(file["image"].ToString()),
                            caption:
                            $"Название: {track.FirstOrDefault()}\nКатегория: {RvMember.Get(long.Parse(file["userId"].ToString())).Status}");
                    }
                    catch
                    {
                        botClient.SendTextMessageAsync(-1001968408177, "Текста и обложки нет, пропускаю...");
                    }
                }
            }
        }

        public static string CardStatus(long userId, string track, string image, string text)
        {
            if (track == Language.GetPhrase("Profile_Track_TrackSent", RvUser.Get(userId).Lang)
                && image == Language.GetPhrase("Profile_Track_ImageSent", RvUser.Get(userId).Lang)
                && text  == Language.GetPhrase("Profile_Track_TextSent", RvUser.Get(userId).Lang))
                return Language.GetPhrase("Profile_Track_Card_Full", RvUser.Get(userId).Lang);
            else
                return string.Empty;
        }
    }
}
