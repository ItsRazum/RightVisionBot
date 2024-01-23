using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Callbacks
{
    class Member
    {
        private static sql database = Program.database;

        public static async Task Callbacks(ITelegramBotClient botClient, Update update)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;
            var CuratorId = database.Read($"SELECT * FROM `RV_Curators` WHERE `userId` = '{callback.From.Id}';", "id");
            string curatorId = CuratorId.FirstOrDefault();

            switch (callbackQuery)
            {
                case "m_accept":
                    if (curatorId == null)
                        return;
                    else
                    {
                        Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                        long memberId = long.Parse(match.Groups[1].Value);

                        RvMember.Get(memberId).Curator = callback.From.Id;
                        var query =
                            $"UPDATE `RV_Members` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {memberId};";
                        database.Read(query, "");
                        await botClient.EditMessageTextAsync(callback.Message.Chat,
                            update.CallbackQuery.Message.MessageId,
                            $"{{m_Message}}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}",
                            replyMarkup: Keyboard.mCategories);
                        await botClient.SendTextMessageAsync(-4074101060,
                            $"Пользователь @{update.CallbackQuery.From.Username} взял кураторством над участником Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                            disableNotification: true);
                    }

                    break;
                case "m_deny":
                    if (curatorId == null)
                        return;
                    else
                    {
                        Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                        long memberId = long.Parse(match.Groups[1].Value);

                        RvMember.Get(memberId).Curator = callback.From.Id;
                        var query =
                            $"UPDATE `RV_Members` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {memberId};";
                        database.Read(query, "");
                        await botClient.EditMessageTextAsync(callback.Message.Chat,
                            update.CallbackQuery.Message.MessageId,
                            $"{{m_Message}}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}\n❌Заявка отклонена!");
                        await botClient.SendTextMessageAsync(memberId,
                            string.Format(
                                Language.GetPhrase("Member_Messages_FormDenied", RvUser.Get(memberId).Lang),
                                fullname));
                        var updateMemberStatus =
                            $"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = {memberId};";
                        database.Read(updateMemberStatus, "");
                        await botClient.SendTextMessageAsync(-4074101060,
                            $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                            disableNotification: true);
                    }

                    break;


                case "m_deny2":
                    {
                        Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                        long memberId = long.Parse(match.Groups[1].Value);
                        if (callback.From.Id == RvMember.Get(memberId).Curator)
                        {
                            await botClient.EditMessageTextAsync(callback.Message.Chat,
                                update.CallbackQuery.Message.MessageId,
                                $"{callback.Message.Text}\n❌Заявка отклонена!");
                            await botClient.SendTextMessageAsync(memberId,
                                string.Format(
                                    Language.GetPhrase("Member_Messages_FormDenied", RvUser.Get(memberId).Lang),
                                    fullname));
                            var updateMemberStatus =
                                $"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = {memberId};";
                            database.Read(updateMemberStatus, "");
                            await botClient.SendTextMessageAsync(-4074101060,
                                $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}",
                                disableNotification: true);
                        }
                    }
                    break;
                case "m_bronze":
                    MemberRoot.SetMemberCategory(botClient, update, "🥉Bronze");
                    break;
                case "m_steel":
                    MemberRoot.SetMemberCategory(botClient, update, "🥈Steel");
                    break;
                case "m_gold":
                    MemberRoot.SetMemberCategory(botClient, update, "🥇Gold");
                    break;
                case "m_brilliant":
                    MemberRoot.SetMemberCategory(botClient, update, "💎Brilliant");
                    break;
                case "m_send":
                    MemberRoot.EnterName(botClient, update);
                    break;
            }
        }
    }
}
