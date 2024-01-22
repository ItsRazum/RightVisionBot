using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Commands
{
    class Critic
    {
        public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            sql database = Program.database;
            string? msgText = message.Text;
            long userId = message.From.Id;

            switch (msgText.ToLower())
            {
                case "открыть судейское меню":
                    if (rvUser.Has(Permission.CriticMenu))
                    {
                        Program.updateRvLocation(userId, RvLocation.CriticMenu);
                        await botClient.SendTextMessageAsync(message.Chat, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл судейское меню \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    else
                        Permissions.NoPermission(message);
                    break;
                case "оценивание ремиксов":
                    TrackEvaluation.Start(botClient, message);
                    break;
                case "начать оценивание":
                    TrackEvaluation.First(botClient, message);
                    break;
                case "предварительное прослушивание":
                    PreListening.Start(botClient, message);
                    break;
                case "начать предварительное прослушивание":
                    PreListening.PreListenTrack(botClient, message);
                    break;
                case "сменить категорию":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                        await botClient.SendTextMessageAsync(message.Chat, "Выбери категорию", replyMarkup: Keyboard.categories);
                    break;
                case "🥉bronze":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, RvMember.Get(PreListening.Get(userId).ArtistId).Status);
                        RvMember.Get(PreListening.Get(userId).ArtistId).Status = "bronze";
                        await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: Keyboard.actions);
                        await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(userId).Lang), "🥉Bronze"));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {RvMember.Get(PreListening.Get(userId).ArtistId).Track} на Bronze \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    break;
                case "🥈steel":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, RvMember.Get(PreListening.Get(userId).ArtistId).Status);
                        RvMember.Get(PreListening.Get(userId).ArtistId).Status = "steel";
                        await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: Keyboard.actions);
                        await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(userId).Lang), "🥈Steel"));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {RvMember.Get(PreListening.Get(userId).ArtistId).Track} на Steel \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    break;
                case "🥇gold":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, RvMember.Get(PreListening.Get(userId).ArtistId).Status);
                        RvMember.Get(PreListening.Get(userId).ArtistId).Status = "gold";
                        await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: Keyboard.actions);
                        await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(userId).Lang), "🥇Gold"));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {RvMember.Get(PreListening.Get(userId).ArtistId).Track} на Gold \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    break;
                case "💎brilliant":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, RvMember.Get(PreListening.Get(userId).ArtistId).Status);
                        RvMember.Get(PreListening.Get(userId).ArtistId).Status = "brilliant";
                        await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: Keyboard.actions);
                        await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", RvUser.Get(userId).Lang), "💎Brilliant"));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {RvMember.Get(PreListening.Get(userId).ArtistId).Track} на Brilliant \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    break;
                case "заблокировать ремикс":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        ReplyKeyboardMarkup yesno = new(new[]
                            {
                                            new[] { new KeyboardButton("✅Да") },
                                            new[] { new KeyboardButton("❌Нет") }
                                        })
                        { ResizeKeyboard = true };
                        await botClient.SendTextMessageAsync(message.Chat,
                            "Ты уверен, что хочешь заблокировать этот ремикс?", replyMarkup: yesno);
                    }
                    break;
                case "✅да":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening && message.Chat.Type == ChatType.Private)
                    {
                        RvMember.Get(PreListening.Get(userId).ArtistId).Status = "denied";
                        database.Read($"UPDATE `RV_Tracks` SET `status` = 'denied' WHERE `userId` = '{PreListening.Get(userId).ArtistId}'", "");
                        await botClient.SendTextMessageAsync(message.Chat, "Ремикс заблокирован!");
                        await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, Language.GetPhrase("Member_Messages_PreListening_Blocked", RvUser.Get(userId).Lang));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал ремикс {RvMember.Get(PreListening.Get(userId).ArtistId).Track} \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                        PreListening.NextTrack(botClient, message);
                    }
                    break;
                case "❌нет":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening && message.Chat.Type == ChatType.Private)
                    { botClient.SendTextMessageAsync(message.Chat, "Выбери действие", replyMarkup: Keyboard.actions); }
                    break;
                case "одобрить ремикс":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} одобрил ремикс {RvMember.Get(PreListening.Get(userId).ArtistId).Track} \n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                        await botClient.SendTextMessageAsync(message.Chat, "Ремикс допущен к дальнейшему оцениванию!");
                        database.Read($"UPDATE `RV_Tracks` SET `status` = 'ok' WHERE `userId` = '{PreListening.Get(userId).ArtistId}'", "");
                        database.Read($"UPDATE `RV_C{RvMember.Get(PreListening.Get(userId).ArtistId).Status}` SET `status` = 'ok' WHERE `userId` = {PreListening.Get(userId).ArtistId}", "");
                        PreListening.NextTrack(botClient, message);
                    }
                    break;

                case "завершить прослушивание":
                    if (RvUser.Get(userId).RvLocation == RvLocation.PreListening)
                    {
                        await botClient.SendTextMessageAsync(message.Chat,
                            "Ты вышел из режима прослушивания!");
                        UserProfile.Profile(message);
                        database.Read(
                            $"UPDATE `RV_Tracks` SET `status` = 'waiting' WHERE `userId` = {PreListening.Get(userId).ArtistId}",
                            "");
                        database.Read($"DELETE FROM `RV_PreListening` WHERE `listenerId` = '{PreListening.Get(userId).ListenerId}';", "");
                        PreListening.preListeners.Remove(PreListening.Get(userId));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} закрыл предварительное прослушивание\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                    }
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "10":
                    if (message.ReplyToMessage != null)
                    {
                        int Rate = int.Parse(message.Text);
                        TrackEvaluation.Get(userId).General = Rate;
                        await botClient.EditMessageReplyMarkupAsync(message.Chat, message.ReplyToMessage.MessageId, TrackEvaluation.RatingSystem(message.From.Id));
                    }
                    break;
            }
        }
    }
}
