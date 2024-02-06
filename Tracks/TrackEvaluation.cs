﻿using System;
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
// ReSharper disable All




// Система оценивания треков
namespace RightVisionBot.Tracks
{
    class CriticVote
    {
        public long UserId;

        private long _artistId = 0;
        public long ArtistId { get => _artistId; set { _artistId = value; newLong(value, nameof(ArtistId)); } }
        private int _general = 0;
        public int General { get => _general; set { _general = value; newLong(value, nameof(General)); } }
        private int _rate1 = 0;
        public int Rate1 { get => _rate1; set { _rate1 = value; newLong(value, nameof(Rate1)); } }
        private int _rate2 = 0;
        public int Rate2 { get => _rate2; set { _rate2 = value; newLong(value, nameof(Rate2)); } }
        private int _rate3 = 0;
        public int Rate3 { get => _rate3; set { _rate3 = value; newLong(value, nameof(Rate3)); } }
        private int _rate4 = 0;
        public int Rate4 { get => _rate4; set { _rate4 = value; newLong(value, nameof(Rate4)); } }

        private string newString(string value, string property)
        {
            _OnPropertyChanged(property, value);
            return value;
        }

        private long newLong(long value, string property)
        {
            newString(value.ToString(), property);
            return value;
        }

        public event Action<string> OnPropertyChanged = delegate { };

        private void _OnPropertyChanged(string property, string value)
        {
            OnPropertyChanged(property);
            UpdateDatabase(property, value);
        }

        private void UpdateDatabase(string property, string value)
        {
            sql database = Program.database;
            database.Read($"UPDATE `RV_Rates` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
        }

    }

    class TrackEvaluation
    {
        private static string Count(long userId) => Get(userId).General.ToString();
        public static InlineKeyboardMarkup RatingSystem(long userId)
        {
            var keyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("<", "r_lower"),
                    InlineKeyboardButton.WithCallbackData(Count(userId), "r_count"),
                    InlineKeyboardButton.WithCallbackData(">", "r_higher")
                },
                secondaryActions(userId)
            }
            );
            return keyboard;
        }

        public static InlineKeyboardButton[] secondaryActions(long userId)
        {
            var withBack = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Сменить прошлую оценку", "r_back"),
                InlineKeyboardButton.WithCallbackData("Подтвердить", "r_enter")
            };
            var withoutBack = new InlineKeyboardButton[]
                { InlineKeyboardButton.WithCallbackData("Подтвердить", "r_enter") };

            return Get(userId).Rate1 == 0 ? withoutBack : withBack;
        }

        public static volatile List<CriticVote> Rates = new();
        public static sql database = Program.database;

        public static async Task Start(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            long userId = callback.From.Id;

            if (rvUser.Has(Permission.Evaluation))
            {
                Program.updateRvLocation(userId, RvLocation.Evaluation);
                InlineKeyboardMarkup actions = new(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("Начать оценивание", "c_startlistening"),  },
                        new[] { InlineKeyboardButton.WithCallbackData("Назад", "c_openmenu"),  }
                    });
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "Добро пожаловать в режим оценивания треков! Сейчас ты можешь выдать оценку ремиксам своей категории, которые прошли предварительные прослушивания. Оценивание будет включать в себя 4 фактора:" +
                                                             "\n1. Инструментал (шлепки + мелодии)" +
                                                             "\n2. Гачивокал" +
                                                             "\n3. Смысловая продуманность" +
                                                             "\n4. Общее звучание" +
                                                             "\n\nКогда ты начнёшь - я скину тебе первый свободный трек, и ты поочерёдно через инлайн-кнопки выставишь ему все оценки!", replyMarkup: actions);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.Message.From.Username} открыл меню оценивания\n=====\nId:{callback.Message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);

            }
            else
                Permissions.NoPermission(callback.Message.Chat);
        }

        public static async Task First(ITelegramBotClient botClient, CallbackQuery callback, RvUser rvUser)
        {
            if (rvUser.Has(Permission.Evaluation))
            {
                long userId = callback.From.Id;

                var artistId = database.Read($"SELECT `userId` FROM `RV_C{RvCritic.Get(userId).Status}` WHERE `status` = 'ok' AND `userId` != {userId} AND `{userId}` = -1 LIMIT 1", "userId").FirstOrDefault();
                if (String.IsNullOrEmpty(artistId))
                {
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Свободные треки для оценивания не найдены!", showAlert: true);
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                }
                else
                {
                    var trackName = RvMember.Get(long.Parse(artistId)).TrackStr;
                    var trackCard = RvMember.Get(long.Parse(artistId)).Track;
                    CriticVote vote = new()
                    {
                        UserId = userId,
                        ArtistId = artistId.FirstOrDefault(),
                        General = 0,
                    };
                    Rates.Add(vote);
                    database.Read($"INSERT INTO `RV_Rates` (`userId`, `artistId`) VALUES ({userId}, {artistId.FirstOrDefault()});", "");
                    await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(trackCard.Track), caption: $"Название: {trackName.FirstOrDefault()}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                    await botClient.SendTextMessageAsync(callback.Message.Chat, "Выдай оценку инструменталу", replyMarkup: Keyboard.Evaluation(userId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} начал оценивание ремикса\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                }
            }
            else
                Permissions.NoPermission(callback.Message.Chat);
        }

        public static string EnterVote(long userId, string property)
        {
            CriticVote voter = Get(userId);
            voter.General = 0;

            switch (property)
            {
                case "Rate1": return $"Твоя оценка инструментала: {voter.Rate1}\n\nВыдай оценку гачивокалу";
                case "Rate2": return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\n\nВыдай оценку техническому исполнению";
                case "Rate3": return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\n\nВыдай оценку творческому исполнению";
                default:      return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\nТвоя оценка творческого исполнения: {voter.Rate4}";
            }
        }

        public static string ChangeVote(long userId)
        {
            CriticVote voter = Get(userId);
            if      (voter.Rate1 == 0) { voter.Rate1 = voter.General; voter.General = 0; return $"Выдай оценку инструменталу"; }
            else if (voter.Rate2 == 0) { voter.Rate2 = voter.General; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\n\nВыдай оценку гачивокалу"; }
            else if (voter.Rate3 == 0) { voter.Rate3 = voter.General; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\n\nВыдай оценку техническому исполнению"; }
            else    {voter.Rate4 = voter.General; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\n\nВыдай оценку творческому исполнению"; }
        }

        public static string RollBackVote(long userId)
        {
            CriticVote voter = Get(userId);
            if      (voter.Rate4 != 0) { voter.Rate4 = 0; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\n\nВыдай оценку творческому исполнению"; }
            else if (voter.Rate3 != 0) { voter.Rate3 = 0; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\n\nВыдай оценку техническому исполнению"; }
            else if (voter.Rate2 != 0) { voter.Rate2 = 0; voter.General = 0; return $"Твоя оценка инструментала: {voter.Rate1}\n\nВыдай оценку гачивокалу"; }
            else    {voter.Rate1 = 0; voter.General = 0; return $"Выдай оценку инструменталу"; }
        }

        public static bool RatesNot0(long userId)
        { return Get(userId).Rate1 != 0 && Get(userId).Rate2 != 0 && Get(userId).Rate3 != 0 && Get(userId).Rate4 != 0; }


        public static void ChangeRate(ITelegramBotClient botClient, Update update)
        {
            var callbackQuery = update.CallbackQuery.Data;
            var callback = update.CallbackQuery;
            var userId = callback.From.Id;
            CriticVote voter = Get(userId);
            switch (callbackQuery)
            {
                case "change1":
                    voter.Rate1 = 0;
                    botClient.EditMessageTextAsync(
                        chatId: callback.Message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.ChangeVote(callback.From.Id),
                        replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                    break;
                case "change2":
                    voter.Rate2 = 0;
                    botClient.EditMessageTextAsync(
                        chatId: callback.Message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.ChangeVote(callback.From.Id),
                        replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                    break;
                case "change3":
                    voter.Rate3 = 0;
                    botClient.EditMessageTextAsync(
                        chatId: callback.Message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.ChangeVote(callback.From.Id),
                        replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                    break;
                case "change4":
                    voter.Rate4 = 0;
                    botClient.EditMessageTextAsync(
                        chatId: callback.Message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.ChangeVote(callback.From.Id),
                        replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                    break;
            }
        }

        public static async Task NextTrack(ITelegramBotClient botClient, CallbackQuery callback, RvUser rvUser)
        {
            ReplyKeyboardMarkup back = new(new[]
                    { new[] { new KeyboardButton("Назад") } })
            { ResizeKeyboard = true };
            long userId = callback.From.Id;
            var artistId = database.Read($"SELECT `userId` FROM `RV_C{RvCritic.Get(userId).Status}` WHERE `status` = 'ok' AND `userId` != {userId} AND `{userId}` = -1 LIMIT 1", "userId").FirstOrDefault();
            if (string.IsNullOrEmpty(artistId))
            {
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Свободные треки для оценивания не найдены!", replyMarkup: back);
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
            }
            else
            {
                var trackName = RvMember.Get(long.Parse(artistId)).TrackStr;
                var trackCard = RvMember.Get(long.Parse(artistId)).Track;
                Get(userId).General = 0;  Get(userId).Rate1 = 0; 
                Get(userId).Rate2 = 0;    Get(userId).Rate3 = 0; 
                Get(userId).Rate4 = 0;    Get(userId).ArtistId = long.Parse(artistId);
                await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(trackName), caption: $"Название: {trackName}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Выдай оценку инструменталу", replyMarkup: Keyboard.Evaluation(userId));
                database.Read($"UPDATE `RV_Rates` SET `artistId` = {artistId.FirstOrDefault()} WHERE `userId` = {userId}", "");
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} начал оценивание ремикса\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }

        public static CriticVote Get(long userId)
        {
            foreach (var vote in Rates)
                if (vote.UserId == userId)
                    return vote;

            return null;
        }
    }
}
