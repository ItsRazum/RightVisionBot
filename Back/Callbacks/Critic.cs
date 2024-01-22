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
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Callbacks
{
    class Critic
    {
        private static sql database = Program.database;

        public static async Task Callbacks(ITelegramBotClient botClient, Update update)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;
            var CuratorId = database.Read($"SELECT * FROM `RV_Curators` WHERE `id` = '{callback.From.Id}';", "id");
            string curatorId = CuratorId.FirstOrDefault();

            switch (callbackQuery)
            {
                case "c_accept":
                    {
                        if (curatorId == null)
                            return;
                        else
                        {
                            Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                            long criticId = long.Parse(match.Groups[1].Value);

                            RvCritic.Get(criticId).Curator = callback.From.Id;
                            var query =
                                $"UPDATE `RV_Critics` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {criticId};";
                            database.Read(query, "");
                            await botClient.EditMessageTextAsync(callback.Message.Chat,
                                update.CallbackQuery.Message.MessageId,
                                $"{callback.Message.Text}\n\nОтветственный за судью: {update.CallbackQuery.From.FirstName}",
                                replyMarkup: Keyboard.cCategories);
                            await botClient.SendTextMessageAsync(-4074101060,
                                $"Пользователь @{update.CallbackQuery.From.Username} взял кураторство над судьёй Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                                disableNotification: true);
                        }
                    }
                    break;
                case "c_deny":
                    {
                        if (curatorId == null)
                            return;
                        else
                        {
                            Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                            long criticId = long.Parse(match.Groups[1].Value);

                            RvCritic.Get(criticId).Curator = callback.From.Id;
                            var query =
                                $"UPDATE `RV_Critics` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {criticId};";
                            database.Read(query, "");
                            await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                                $"{callback.Message.Text}\n\nОтветственный за судью: {update.CallbackQuery.From.FirstName}\n❌Заявка отклонена!");
                            await botClient.SendTextMessageAsync(criticId,
                                string.Format(
                                    Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang),
                                    fullname));
                            var updateCriticStatus =
                                $"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = {criticId};";
                            database.Read(updateCriticStatus, "");
                            await botClient.SendTextMessageAsync(-4074101060,
                                $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру судьи Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                                disableNotification: true);
                        }
                    }
                    break;

                case "c_deny2":
                    {
                        Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                        long criticId = long.Parse(match.Groups[1].Value);
                        if (callback.From.Id == RvCritic.Get(criticId).Curator)
                        {
                            botClient.EditMessageTextAsync(callback.Message.Chat,
                                update.CallbackQuery.Message.MessageId,
                                $"{callback.Message.Text}\n❌Заявка отклонена!");
                            botClient.SendTextMessageAsync(criticId,
                                string.Format(
                                    Language.GetPhrase("Critic_Messages_FormDenied", RvUser.Get(criticId).Lang),
                                    fullname));
                            var updateCriticStatus =
                                $"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = {criticId};";
                            database.Read(updateCriticStatus, "");
                            await botClient.SendTextMessageAsync(-4074101060,
                                $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру критика Id:{criticId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                                disableNotification: true);
                        }
                    }
                    break;
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
                    await botClient.SendTextMessageAsync(update.Message.Chat,
                        Language.GetPhrase("Critic_Messages_EnrollmentClosed",
                            RvUser.Get(update.CallbackQuery.Message.From.Id).Lang));
                    break;
            }
        }
    }
}
