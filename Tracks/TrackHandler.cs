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
using static System.Runtime.InteropServices.JavaScript.JSType;

//система чтения, записи и отправки треков
namespace RightVisionBot.Tracks
{
    public class TrackInfo
    {
        public long UserId;

        private string _status = "waiting";
        public string Status { get => _status; set { _status = value; NewString(value, nameof(Status)); } }

        private string? _track = null;
        public string? Track { get => _track; set { _track = value; NewString(value, nameof(Track)); } }

        private string? _image = null;
        public string? Image { get => _image; set { _image = value; NewString(value, nameof(Image)); } }

        private string? _text = null;
        public string? Text { get => _text; set { _text = value; NewString(value, nameof(Text)); } }

        private void NewString(string? value, string property) => Program.database.Read($"UPDATE `RV_Tracks` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
    }

    public class Track
    {
        public static volatile List<TrackInfo> Tracks = new();
        private static sql database = Program.database;
        public static async Task Send(ITelegramBotClient botClient, Message? message = null, CallbackQuery? callback = null)
        {
            long userId = message != null ? message.From.Id : callback.From.Id;
            if (GetTrack(userId) != null)
                TrackCard(true, botClient, message, callback);
            else
            {
                await botClient.SendTextMessageAsync(callback.Message.Chat, Language.GetPhrase("Profile_Track_CreatingCard", RvUser.Get(userId).Lang));
                TrackInfo track = new() { UserId = userId };
                RvMember.Get(userId)!.Track = track;
                Tracks.Add(track);
                database.Read($"INSERT INTO `RV_Tracks`(`userId`) VALUES ({userId});", "");
                TrackCard(false, botClient, callback:callback);
            }
        }

        public static void TrackCard(bool isExists, ITelegramBotClient botClient, Message? message = null, CallbackQuery? callback = null)
        {
            long userId = message != null? message.From.Id : callback.From.Id;
            RvUser rvUser = RvUser.Get(userId);
            InlineKeyboardMarkup inline = new(
                new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckTrack", rvUser.Lang) + "♂", "t_CheckTrack")  },
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckImage", rvUser.Lang) + "🖼", "t_CheckImage") },
                    new[] { InlineKeyboardButton.WithCallbackData("🔎" + Language.GetPhrase("Profile_Track_CheckText", rvUser.Lang) + "📝", "t_CheckText") },
                    new[] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_profile"),  }
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
                    { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", rvUser.Lang)) } })
            { ResizeKeyboard = true };

            Program.UpdateRvLocation(userId, RvLocation.TrackCard);
            if (!isExists)
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} создал карточку ремикса\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);

            if (message != null)
                botClient.SendTextMessageAsync(message.Chat, 
                    $"{RvMember.Get(userId).TrackStr}\n\n"
                    + string.Format(Language.GetPhrase("Profile_Track_Card", rvUser.Lang), IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))
                    + $"\n\n{CardStatus(RvUser.Get(userId).Lang, IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))}", replyMarkup: inline);
            else
                botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                    $"{RvMember.Get(userId).TrackStr}\n\n"
                    + string.Format(Language.GetPhrase("Profile_Track_Card", rvUser.Lang), IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))
                    + $"\n\n{CardStatus(RvUser.Get(userId).Lang, IsTrackSent(userId), IsImageSent(userId), IsTextSent(userId))}", replyMarkup: inline);
        }

        public static string? IsTrackSent(long userId) => GetTrack(userId) != null ? Language.GetPhrase(GetTrack(userId).Track != null ? "Profile_Track_TrackSent" : "Profile_Track_TrackNotSent", RvUser.Get(userId).Lang) : null;
        public static string? IsImageSent(long userId) => GetTrack(userId) != null ? Language.GetPhrase(GetTrack(userId).Image != null ? "Profile_Track_ImageSent" : "Profile_Track_ImageNotSent", RvUser.Get(userId).Lang) : null;
        public static string? IsTextSent(long userId) =>  GetTrack(userId) != null ? Language.GetPhrase(GetTrack(userId).Text != null  ? "Profile_Track_TrackSent" : "Profile_Track_TrackNotSent", RvUser.Get(userId).Lang) : null;

        public static TrackInfo? GetTrack(long userId)
        {
            foreach (var trackInfo in Data.RvMembers)
                if (trackInfo.Track != null && trackInfo.Track.UserId == userId)
                    return trackInfo.Track;

            return null;
        }

        public static void SendFilesByOne(ITelegramBotClient botClient, int number)
        {
            List<string> user;
            if (number == 1) user = database.Read("SELECT * FROM RV_Tracks LIMIT 1;", "userId");
            else user = database.Read($"SELECT * FROM RV_Tracks LIMIT 1 OFFSET {number - 1};", "userId");

            RvMember member = RvMember.Get(long.Parse(user.FirstOrDefault()));
            try
            {
                botClient.SendDocumentAsync(-1001968408177, new InputFileId(member.Track.Track),
                    caption: $"Название: {member.TrackStr}\nКатегория: {member.Status}");
                botClient.SendPhotoAsync(-1001968408177, new InputFileId(member.Track.Image),
                    caption: $"Название: {member.TrackStr}\nКатегория: {member.Status}");
                botClient.SendDocumentAsync(-1001968408177, new InputFileId(member.Track.Text),
                    caption: $"Название: {member.TrackStr}\nКатегория: {member.Status}");
            }
            catch
            {
                try
                {
                    botClient.SendDocumentAsync(-1001968408177, new InputFileId(member.Track.Track),
                        caption: $"Название: {member.TrackStr}\nКатегория: {member.Status}");
                    botClient.SendPhotoAsync(-1001968408177, new InputFileId(member.Track.Image),
                        caption: $"Название: {member.TrackStr}\nКатегория: {member.Status}");
                }
                catch
                {
                    botClient.SendTextMessageAsync(-1001968408177, "Текста и обложки нет, пропускаю...");
                }
            }
        }

        public static string CardStatus(string lang, string track, string image, string text)
        {
            if (track == Language.GetPhrase("Profile_Track_TrackSent", lang)
                && image == Language.GetPhrase("Profile_Track_ImageSent", lang)
                && text == Language.GetPhrase("Profile_Track_TextSent", lang))
                return Language.GetPhrase("Profile_Track_Card_Full", lang);
            else
                return string.Empty;
        }
    }
}
