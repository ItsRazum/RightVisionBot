using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.Tracks;
using RightVisionBot.User;

namespace RightVisionBot.Back.Callbacks
{
    class Evaluation
    {
        private static sql database = Program.database;
        
        public static async Task Callbacks(ITelegramBotClient botClient, Update update)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var message = callback.Message;
            var callbackQuery = callback.Data;
            CriticVote vote = TrackEvaluation.Get(callbackUserId);
            switch (callbackQuery)
            {
                case "r_lower":
                    if (vote.General is 0 or 1)
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку меньше 1 нельзя!", showAlert: true);
                    else
                    {
                        vote.General--;
                        await botClient.EditMessageTextAsync(message.Chat, message.MessageId, message.Text, replyMarkup: TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_higher":
                    if (vote.General == 10)
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку больше 10 нельзя!", showAlert: true);
                    else
                    {
                        vote.General++;
                        await botClient.EditMessageTextAsync(message.Chat, message.MessageId, message.Text, replyMarkup: TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_enter":
                    { 
                        var property = "";

                        if (vote.Rate1 == 0)
                        {
                            property = "Rate1"; 
                            vote.Rate1 = vote.General;
                        }
                        else if (vote.Rate2 == 0)
                        {
                            property = "Rate2"; 
                            vote.Rate2 = vote.General;
                        }
                        else if (vote.Rate3 == 0)
                        {
                            property = "Rate3"; 
                            vote.Rate3 = vote.General;
                        }
                        else if (vote.Rate4 == 0)
                        {
                            property = "Rate4"; 
                            vote.Rate4 = vote.General;
                        }

                        await botClient.EditMessageTextAsync(
                            chatId: message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: TrackEvaluation.RatesNot0(callbackUserId) ? $"Твоя оценка инструментала: {vote.Rate1}\nТвоя оценка гачивокала: {vote.Rate2}\nТвоя оценка технического исполнения: {vote.Rate3}\nТвоя оценка творческого исполнения: {vote.Rate4}" : TrackEvaluation.EnterRate(callbackUserId, property),
                            replyMarkup: TrackEvaluation.RatesNot0(callbackUserId) ? Keyboard.finalActions : TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_back":
                    await botClient.EditMessageTextAsync(
                        chatId: message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.RollBackRate(callbackUserId, vote),
                        replyMarkup: TrackEvaluation.RatesNot0(callbackUserId) ? Keyboard.finalActions : TrackEvaluation.RatingSystem(callbackUserId));
                    break;
                case "r_count":
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Если ты хочешь вручную вписать оценку - напиши цифру в чат от 1 до 10 самостоятельно!", showAlert: true);
                    break;
                case "r_change1":
                case "r_change2":
                case "r_change3":
                case "r_change4":
                    TrackEvaluation.ChangeRate(botClient, update);
                    break;
                case "r_send":
                    {
                        var critic = RvCritic.Get(callbackUserId);
                        double finalRate = (vote.Rate1 + vote.Rate2 + vote.Rate3 + vote.Rate4) / 4.0;

                        if(!database.Read(
                            $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " +
                            $"WHERE TABLE_NAME = 'RV_C{critic.UserId}' " +
                            $"AND TABLE_SCHEMA = 'RightVision'",
                            "COLUMN_NAME")
                            .Contains(critic.UserId.ToString()))
                        {
                            database.Read($"ALTER TABLE `RV_C{critic.Status}` ADD `1305332660` DOUBLE NOT NULL DEFAULT '-1' AFTER `status`;", "");
                        }

                        database.Read($"UPDATE `RV_C{critic.Status}` SET `{critic.UserId}` = {finalRate} WHERE `userId` = {vote.ArtistId};", "");

                        await botClient.EditMessageTextAsync(
                            chatId: message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: update.CallbackQuery.Message.Text + $"\n\n{vote.Rate1}+{vote.Rate2}+{vote.Rate3}+{vote.Rate4}={vote.Rate1 + vote.Rate2 + vote.Rate3 + vote.Rate4} / 4\nИтого: {finalRate}\nОценка успешно занесена в базу данных, ты можешь приступить к оцениванию следующего трека",
                            replyMarkup: Keyboard.NextTrack);
                    }
                    break;
                case "r_nexttrack":
                    await TrackEvaluation.NextTrack(botClient, callback, vote);
                    break;
            }
        }
    }
}
