using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back
{
    class Document
    {
        public static async Task Handling(ITelegramBotClient botClient, Message message, RvUser rvUser)
        {
            sql database = Program.database;
            if (message is { Audio: not null, Chat.Type: ChatType.Private } && rvUser.RvLocation == RvLocation.TrackCard)
            {
                var fileId = message.Audio.FileId;
                long userId = message.From.Id;
                Track.GetTrack(userId).Track = fileId;

                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Success", rvUser.Lang));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал свой ремикс\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                await Track.Send(botClient, message: message);
            }

            if (message is { Photo: not null, Chat.Type: ChatType.Private })
            {
                var fileId = message.Photo.LastOrDefault()?.FileId;
                long userId = message.From.Id;

                Track.GetTrack(userId).Image = fileId;
                if (Track.GetTrack(userId).Track != null)
                    database.Read($"UPDATE `RV_C{RvMember.Get(userId).Status}` SET `status` = 'waiting' WHERE `userId` = {userId};", "");

                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendImage_Success", rvUser.Lang));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал обложку своего ремикса\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                await Track.Send(botClient, message: message);
            }

            if ((message is { Document: not null, Chat.Type: ChatType.Private }) && rvUser.RvLocation == RvLocation.TrackCard)
            {
                var fileName = message.Document.FileName;
                if (fileName.EndsWith(".txt"))
                {
                    var fileId = message.Document.FileId;
                    long userId = message.From.Id;

                    Track.GetTrack(userId).Text = fileId;
                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendText_Success", rvUser.Lang));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал текст своего ремикса\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                    await Track.Send(botClient, message);

                }
                else if ((fileName.EndsWith(".wav") || fileName.EndsWith(".mp3") || fileName.EndsWith(".flac")) && rvUser.RvLocation == RvLocation.TrackCard)
                {
                    var fileId = message.Document.FileId;
                    long userId = message.From.Id;

                    Track.GetTrack(userId).Track = fileId;
                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Success", rvUser.Lang));
                    if (Track.GetTrack(userId).Image != null)
                        database.Read($"UPDATE `RV_C{RvMember.Get(userId).Status}` SET `status` = 'waiting' WHERE `userId` = {userId};", "");
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал свой ремикс\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                    await Track.Send(botClient, message);
                }
            }
        }
    }
}
