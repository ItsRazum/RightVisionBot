using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Callbacks
{
    class Critic
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            long criticId;
            var callback = update.CallbackQuery;
            var message = callback?.Message;
            var chat = message?.Chat;
            var from = callback?.From;
            long userId = from.Id;
            var callbackQuery = callback?.Data;
            string fullname = callback?.From.FirstName + callback?.From.LastName;

            if (RvCritic.Get(userId) != null && RvCritic.Get(userId).PreListeningArtist != 0)
                await PreListeningCallbacks(botClient, callback);

            switch (callbackQuery)
            {
                case "c_send":
                    CriticRoot.EnterName(botClient, update);
                    break;
                case "c_openmenu":
                    RvCritic rvCritic = RvCritic.Get(userId);
                    if (rvUser.RvLocation == RvLocation.PreListening)
                    {
                        long artistId = rvCritic.PreListeningArtist;
                        RvMember.Get(artistId).Track.Status = "waiting";
                        rvCritic.PreListeningArtist = 0;

                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} закрыл предварительное прослушивание\n=====\nId:{callback.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                    }
                    if (rvUser.Has(Permission.CriticMenu))
                    {
                        Program.UpdateRvLocation(userId, RvLocation.CriticMenu);
                        await botClient.EditMessageTextAsync(chat, message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {rvCritic.Status}", replyMarkup: Keyboard.criticMenu);
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} открыл судейское меню \n=====\nId:{from.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                    }
                    else
                        await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);
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

                    RvCritic.Get(criticId).Curator = from.Id;
                    await botClient.EditMessageTextAsync(message.Chat, callback.Message.MessageId, $"{message.Text}\n\nОтветственный за судью: {from.FirstName}", replyMarkup: Keyboard.CCategories(criticId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} взял кураторство над судьёй Id:{criticId}\n=====\nId:{userId}\nЯзык: {RvUser.Get(userId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);

            else if(callbackQuery.StartsWith("c_deny-"))
                if (rvUser.Has(Permission.Curate))
                {
                    criticId = long.Parse(callbackQuery.Replace("c_deny-", ""));
                    var rvCritic = RvCritic.Get(criticId);

                    rvCritic.Curator = from.Id;
                    rvCritic.Status = "denied";
                    Data.RvCritics.Remove(rvCritic);
                    RvUser.Get(criticId).ResetPermissions();

                    await botClient.EditMessageTextAsync(message.Chat, message.MessageId, $"{message.Text}\n\nОтветственный за судью: {from.FirstName}\n❌Заявка отклонена!");
                    await botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang), fullname), replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.MainMenu));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username}  отклонил кандидатуру судьи Id: {criticId} \n=====\nId: {userId}\nЯзык: {RvUser.Get(userId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);


            else if (callbackQuery.StartsWith("c_bronze-"))
                CriticRoot.SetCriticCategory(botClient, update, "🥉Bronze");
            else if (callbackQuery.StartsWith("c_silver-"))
                CriticRoot.SetCriticCategory(botClient, update, "🥈Silver");
            else if (callbackQuery.StartsWith("c_gold-"))
                CriticRoot.SetCriticCategory(botClient, update, "🥇Gold");
            else if (callbackQuery.StartsWith("c_brilliant-"))
                CriticRoot.SetCriticCategory(botClient, update, "💎Brilliant");


            else if (callbackQuery.StartsWith("c_deny2-"))
                if (rvUser.Has(Permission.Curate))
                {
                    criticId = long.Parse(callbackQuery.Replace("c_deny2-", ""));
                    if (callback.From.Id == RvCritic.Get(criticId).Curator)
                    {
                        var rvCritic = RvCritic.Get(criticId);

                        rvCritic.Curator = from.Id;
                        rvCritic.Status = "denied";
                        Data.RvCritics.Remove(rvCritic);
                        RvUser.Get(criticId).ResetPermissions();

                        await botClient.EditMessageTextAsync(message.Chat, message.MessageId, $"{message.Text}\n❌Заявка отклонена!");
                        await botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang), fullname), replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.MainMenu));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} отклонил кандидатуру критика Id:{criticId}\n=====\nId:{from.Id}\nЯзык: {RvUser.Get(userId).Lang}", disableNotification: true);
                    }
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);
        }

        private static async Task PreListeningCallbacks(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var message = callback.Message;
            var chat = message.Chat;
            var from = callback.From;
            var callbackQuery = callback.Data;
            long userId = callback.From.Id;
            RvMember artistRvMember;

            switch (callbackQuery)
            {
                case "c_editcategory":
                    await botClient.EditMessageTextAsync(chat, message.MessageId, "Выбери категорию", replyMarkup: Keyboard.PreListeningCategories);
                    break;
                case "c_blockremix":
                    artistRvMember = RvMember.Get(RvCritic.Get(userId).PreListeningArtist);
                    await botClient.EditMessageTextAsync(chat, message.MessageId, $"Ты уверен, что хочешь заблокировать ремикс \"{artistRvMember.TrackStr}\"?", replyMarkup: Keyboard.YesNo);
                    break;
                case "c_blockremix_yes":
                    artistRvMember = RvMember.Get(RvCritic.Get(userId).PreListeningArtist);
                    artistRvMember.Status = "denied";
                    await botClient.AnswerCallbackQueryAsync(callback.Id, "Ремикс заблокирован!");
                    await botClient.SendTextMessageAsync(artistRvMember.UserId, Language.GetPhrase("Member_Messages_PreListening_Blocked", RvUser.Get(artistRvMember.UserId).Lang));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} заблокировал ремикс {artistRvMember.TrackStr} \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    await PreListening.NextTrack(botClient, callback);
                    break;
                case "c_blockremix_no":
                    await botClient.EditMessageTextAsync(chat, message.MessageId, "Выбери действие", replyMarkup: Keyboard.actions);
                    break;
                case "c_acceptremix":
                    Console.WriteLine("userId: " + userId);
                    Console.WriteLine("PreListeningArtist: " + RvCritic.Get(userId).PreListeningArtist);
                    Console.WriteLine("RvMember: " + RvMember.Get(RvCritic.Get(userId).PreListeningArtist));
                    artistRvMember = RvMember.Get(RvCritic.Get(userId).PreListeningArtist);
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{from.Username} одобрил ремикс {artistRvMember.TrackStr} \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    await botClient.EditMessageTextAsync(message.Chat, message.MessageId, "Ремикс допущен к дальнейшему оцениванию!");
                    artistRvMember.Track.Status = "ok";
                    await PreListening.NextTrack(botClient, callback);
                    break;
            }

            if (callbackQuery.StartsWith("c_changeTo_"))
            {
                artistRvMember = RvMember.Get(RvCritic.Get(userId).PreListeningArtist);
                string category = callbackQuery.Substring(11);
                RvMember.Get(RvCritic.Get(userId).PreListeningArtist).Status = category;
                MemberRoot.ChangeMemberCategory(artistRvMember.UserId, artistRvMember.Status);
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Смена категории прошла успешно!");
                await botClient.EditMessageTextAsync(chat, message.MessageId, "Выбери действие", replyMarkup: Keyboard.actions);
                await botClient.SendTextMessageAsync(artistRvMember.UserId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(userId).Lang), category));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} сменил категорию ремикса {RvMember.Get(RvCritic.Get(userId).PreListeningArtist).TrackStr} на {category} \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }
    }
}
