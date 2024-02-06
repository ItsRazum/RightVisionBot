using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.Common;
using RightVisionBot.User;
using System.Text.RegularExpressions;
using RightVisionBot.Tracks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Callbacks
{
    class Critic
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            long criticId;
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;

            if (PreListening.Get(callbackUserId) != null)
                await PreListeningCallbacks(botClient, callback);

            switch (callbackQuery)
            {
                case "c_bronze":
                    CriticRoot.SetCriticCategory(botClient, update, "🥉Bronze");
                    break;
                case "c_steel":
                    CriticRoot.SetCriticCategory(botClient, update, "🥈Steel");
                    break;
                case "c_gold":
                    CriticRoot.SetCriticCategory(botClient, update, "🥇Gold");
                    break;
                case "c_brilliant":
                    CriticRoot.SetCriticCategory(botClient, update, "💎Brilliant");
                    break;
                case "c_send":
                    CriticRoot.EnterName(botClient, update);
                    break;
                case "c_openmenu":
                    if (rvUser.RvLocation == RvLocation.PreListening)
                    {
                        RvCritic.Get(callbackUserId).PreListening = null;
                        Program.database.Read($"DELETE FROM `RV_PreListening` WHERE `listenerId` = '{PreListening.Get(callbackUserId).ListenerId}';", "");
                        long artistId = RvCritic.Get(rvUser.UserId).PreListening.ArtistId;
                        RvMember.Get(artistId).Track.Status = "waiting";

                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} закрыл предварительное прослушивание\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
                    }
                    if (rvUser.Has(Permission.CriticMenu))
                    {
                        Program.updateRvLocation(callbackUserId, RvLocation.CriticMenu);
                        await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(callbackUserId).Status}", replyMarkup: Keyboard.criticMenu);
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл судейское меню \n=====\nId:{callback.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                    }
                    else
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но у тебя нет права совершать это действие!", showAlert:true);
                    break;
                case "c_prelistening":
                    await PreListening.Start(botClient, callback);
                    break;
                case "c_startprelistening":
                    await PreListening.PreListenTrack(botClient, callback);
                    break;
                case "c_evaluation":
                    await TrackEvaluation.Start(botClient, update, rvUser);
                    break;
                case "c_startlistening":
                    await TrackEvaluation.First(botClient, callback, rvUser);
                    break;
            }

            if (callbackQuery.StartsWith("c_accept-"))
                if (rvUser.Has(Permission.Curate))
                {
                    criticId = long.Parse(callbackQuery.Replace("c_accept-", ""));

                    RvCritic.Get(criticId).Curator = callback.From.Id;
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"{callback.Message.Text}\n\nОтветственный за судью: {callback.From.FirstName}", replyMarkup: Keyboard.cCategories(criticId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} взял кураторство над судьёй Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но у тебя нет права совершать это действие!", showAlert: true);

            else if(callbackQuery.StartsWith("c_deny-"))
                if (rvUser.Has(Permission.Curate))
                {
                    criticId = long.Parse(callbackQuery.Replace("c_deny-", ""));

                    RvCritic.Get(criticId).Curator = callback.From.Id;
                    RvCritic.Get(criticId).Status = "denied";

                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"{callback.Message.Text}\n\nОтветственный за судью: {callback.From.FirstName}\n❌Заявка отклонена!");
                    await botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang), fullname), replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.MainMenu));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отклонил кандидатуру судьи Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но у тебя нет права совершать это действие!", showAlert: true);

            else if (callbackQuery.StartsWith("c_deny2-"))
                if (rvUser.Has(Permission.Curate))
                {
                    criticId = long.Parse(callbackQuery.Replace("c_deny2-", ""));
                    if (callback.From.Id == RvCritic.Get(criticId).Curator)
                    {
                        RvCritic.Get(criticId).Status = "denied";

                        await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"{callback.Message.Text}\n❌Заявка отклонена!");
                        await botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang), fullname), replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.MainMenu));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} отклонил кандидатуру критика Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);
                    }
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но у тебя нет права совершать это действие!", showAlert: true);
        }

        private static async Task PreListeningCallbacks(ITelegramBotClient botClient, CallbackQuery callback)
        {
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;
            RvMember artistRvMember = RvMember.Get(PreListening.Get(callbackUserId).ArtistId);

            switch (callbackQuery)
            {
                case "c_editcategory":
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "Выбери категорию", replyMarkup: Keyboard.PreListeningCategories);
                    break;
                case "c_blockremix":
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Ты уверен, что хочешь заблокировать ремикс \"{artistRvMember.TrackStr}\"?", replyMarkup: Keyboard.YesNo);
                    break;
                case "c_blockremix_yes":
                    artistRvMember.Status = "denied";
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Ремикс заблокирован!");
                    await botClient.SendTextMessageAsync(artistRvMember.UserId, Language.GetPhrase("Member_Messages_PreListening_Blocked", RvUser.Get(artistRvMember.UserId).Lang));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} заблокировал ремикс {artistRvMember.Track} \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
                    await PreListening.NextTrack(botClient, callback);
                    break;
                case "c_blockremix_no":
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "Выбери действие", replyMarkup: Keyboard.actions);
                    break;
                case "c_acceptremix":
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} одобрил ремикс {artistRvMember.Track} \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "Ремикс допущен к дальнейшему оцениванию!");
                    artistRvMember.Track.Status = "ok";
                    await PreListening.NextTrack(botClient, callback);
                    break;
            }

            if (callbackQuery.StartsWith("c_changeTo_"))
            {
                string category = callbackQuery.Substring(11);
                RvMember.Get(PreListening.Get(callbackUserId).ArtistId).Status = category;
                MemberRoot.ChangeMemberCategory(artistRvMember.UserId, artistRvMember.Status);
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Смена категории прошла успешно!");
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, "Выбери действие", replyMarkup: Keyboard.actions);
                await botClient.SendTextMessageAsync(artistRvMember.UserId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(callbackUserId).Lang), category));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} сменил категорию ремикса {RvMember.Get(PreListening.Get(callbackUserId).ArtistId).Track} на Brilliant \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}\nЛокация: {RvUser.Get(callbackUserId).RvLocation}", disableNotification: true);
            }
        }
    }
}
