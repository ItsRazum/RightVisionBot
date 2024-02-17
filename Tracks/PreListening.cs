using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Tracks
{
    class PreListening
    {
        static sql database = Program.database;
        public static async Task Start(ITelegramBotClient botClient, CallbackQuery callback)
        {
            long userId = callback.From.Id;
            if (RvUser.Get(userId).Has(Permission.Curate))
            {
                Program.UpdateRvLocation(userId, RvLocation.PreListening);
                InlineKeyboardMarkup actions = new(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("Начать предварительное прослушивание", "c_startprelistening") },
                        new[] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang), "c_openmenu") }
                    });
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, Language.GetPhrase("Keyboard_Choice_Critic_Menu_PreListening_Instruction", RvUser.Get(userId).Lang), replyMarkup: actions);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл меню предварительного прослушивания\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
            else await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но тебе нельзя проводить предварительное прослушивание!", showAlert: true);
        }

        public static async Task PreListenTrack(ITelegramBotClient botClient, CallbackQuery callback)
        {
            ReplyKeyboardMarkup back = new(new[]
            { new[] { new KeyboardButton("Назад") } })
            { ResizeKeyboard = true };
            long userId = callback.From.Id;
            var actions = Keyboard.actions;
            var artistId = from rvMember in Data.RvMembers where (rvMember.Status == "waiting" && rvMember.Track.Image != null && rvMember.Track.Track != null) select rvMember.UserId;
            if (!artistId.Any())
            {
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Свободные треки для прослушивания не найдены!", showAlert: true);
                Program.UpdateRvLocation(userId, RvLocation.CriticMenu);
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл судейское меню \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }

            else
            {
                RvCritic.Get(userId).PreListeningArtist = artistId.First();
                var trackCard = RvMember.Get(artistId.First()).Track;
                var trackName = RvMember.Get(artistId.First()).TrackStr;
                database.Read($"INSERT INTO `RV_PreListening` (`listenerId`, `artistId`) VALUES ('{userId}', '{RvCritic.Get(userId).PreListeningArtist}');", "");
                trackCard.Status = "checked";
                await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(trackCard.Track), caption: $"Название: {trackName}\nКатегория: {RvMember.Get(RvCritic.Get(userId).PreListeningArtist).Status}");
                await botClient.SendPhotoAsync(callback.Message.Chat, new InputFileId(trackCard.Image), caption: "Обложка ремикса");
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Выбери действие", replyMarkup: actions);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.Message.From.Username} начал предварительное прослушивание\n=====\nId:{callback.Message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }

        public static async Task NextTrack(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var actions = Keyboard.actions;
            long userId = callback.From.Id;
            RvMember.Get(RvCritic.Get(userId).PreListeningArtist).Track.Status = "ok";

            var artistId = from rvMember in Data.RvMembers where (rvMember.Status == "waiting" && rvMember.Track.Image != null && rvMember.Track.Track != null) select rvMember.UserId;
            var trackName = RvMember.Get(artistId.First()).TrackStr;
            if (!artistId.Any())
            {
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Свободные треки для прослушивания не найдены!", showAlert: true);
                Program.UpdateRvLocation(userId, RvLocation.CriticMenu);
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл судейское меню \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
            else
            {
                RvCritic.Get(userId).PreListeningArtist = artistId.First();
                var artist = RvMember.Get(RvCritic.Get(userId).PreListeningArtist).Track;

                artist.Status = "checked";
                await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(artist.Track), caption: $"Название: {trackName}\nКатегория: {RvMember.Get(RvCritic.Get(userId).PreListeningArtist).Status}");
                await botClient.SendPhotoAsync(callback.Message.Chat, new InputFileId(artist.Image), caption: "Обложка ремикса");
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Выбери действие", replyMarkup: actions);
            }
        }
    }
}
