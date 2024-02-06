﻿using System;
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
            var callbackQuery = callback.Data;

            switch (callbackQuery)
            {
                case "r_lower":
                    if (TrackEvaluation.Get(callbackUserId).General is 0 or 1)
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку меньше 1 нельзя!", showAlert: true);
                    else
                    {
                        TrackEvaluation.Get(callbackUserId).General--;
                        await botClient.EditMessageTextAsync(chatId: callback.Message.Chat, messageId: update.CallbackQuery.Message.MessageId, text: callback.Message.Text, replyMarkup: TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_higher":
                    if (TrackEvaluation.Get(callbackUserId).General == 10)
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку больше 10 нельзя!", showAlert: true);
                    else
                    {
                        TrackEvaluation.Get(callbackUserId).General++;
                        await botClient.EditMessageTextAsync(chatId: callback.Message.Chat, messageId: update.CallbackQuery.Message.MessageId, text: callback.Message.Text, replyMarkup: TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_enter":
                    {
                        string property = "";
                        var voter = TrackEvaluation.Get(callbackUserId);

                        if (voter.Rate1 == 0)
                        {
                            property = "Rate1"; 
                            voter.Rate1 = voter.General;
                        }
                        else if (voter.Rate2 == 0)
                        {
                            property = "Rate2"; 
                            voter.Rate2 = voter.General;
                        }
                        else if (voter.Rate3 == 0)
                        {
                            property = "Rate3"; 
                            voter.Rate3 = voter.General;
                        }
                        else if (voter.Rate4 == 0)
                        {
                            property = "Rate4"; 
                            voter.Rate4 = voter.General;
                        }

                        await botClient.EditMessageTextAsync(
                            chatId: callback.Message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: TrackEvaluation.RatesNot0(callbackUserId) ? $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\nТвоя оценка творческого исполнения: {voter.Rate4}" : TrackEvaluation.EnterVote(callbackUserId, property),
                            replyMarkup: TrackEvaluation.RatesNot0(callbackUserId) ? Keyboard.finalActions : TrackEvaluation.RatingSystem(callbackUserId));
                    }
                    break;
                case "r_back":
                    await botClient.EditMessageTextAsync(
                        chatId: callback.Message.Chat,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: TrackEvaluation.RollBackVote(callbackUserId),
                        replyMarkup: TrackEvaluation.RatesNot0(callbackUserId) ? Keyboard.finalActions : TrackEvaluation.RatingSystem(callbackUserId));
                    break;
                case "r_count":
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Если ты хочешь вручную вписать оценку - напиши цифру в чат от 1 до 10 самостоятельно!", showAlert: true);
                    break;
                case "r_change1":
                case "r_change2":
                case "r_change3":
                case "r_change4":
                    TrackEvaluation.ChangeRate(botClient, update);
                    break;
                case "r_send":
                    {
                        var voter = TrackEvaluation.Get(callbackUserId);
                        double finalRate = (voter.Rate1 + voter.Rate2 + voter.Rate3 + voter.Rate4) / 4.0;

                        await botClient.EditMessageTextAsync(
                            chatId: callback.Message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: update.CallbackQuery.Message.Text + $"\n\n{voter.Rate1}+{voter.Rate2}+{voter.Rate3}+{voter.Rate4}={voter.Rate1 + voter.Rate2 + voter.Rate3 + voter.Rate4} / 4\nИтоговая оценка: {finalRate}",
                            replyMarkup: Keyboard.NextTrack);
                        database.Read($"UPDATE `RV_C{RvCritic.Get(callbackUserId).Status}` SET `{callbackUserId}` = {finalRate} WHERE `userId` = {TrackEvaluation.Get(callbackUserId).ArtistId};", "");
                    }
                    break;
                case "r_nexttrack":
                    await TrackEvaluation.NextTrack(botClient, callback, RvUser.Get(callbackUserId));
                    break;
                case "r_exit":

                    break;
            }
        }
    }
}
