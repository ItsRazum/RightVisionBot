using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Callbacks
{
    class Member
    {
        private static sql database = Program.database;

        public static async Task Callbacks(ITelegramBotClient botClient, Update update, RvUser rvUser)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;

            switch (callbackQuery)
            {
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
                case "m_openmenu":
                    await Track.Send(botClient, callback: callback);
                    break;
                case "m_edittrack":
                    Program.UpdateRvLocation(callbackUserId, RvLocation.EditTrack);
                    await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, Language.GetPhrase("Profile_Member_Track_EnterNewTrack", rvUser.Lang), replyMarkup: Keyboard.InlineBack(rvUser, RvLocation.Profile));
                    break;
            }

            if (callbackQuery.StartsWith("m_accept-"))
                if (rvUser.Has(Permission.Curate))
                {
                    long memberId = long.Parse(callbackQuery.Replace("m_accept-", ""));

                    RvMember.Get(memberId).Curator = callback.From.Id;
                    await botClient.EditMessageTextAsync(callback.Message.Chat, update.CallbackQuery.Message.MessageId, $"{callback.Message.Text}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}", replyMarkup: Keyboard.mCategories(memberId));
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} взял кураторством над участником Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);

                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);

            else if (callbackQuery.StartsWith("m_deny-"))
                if (rvUser.Has(Permission.Curate))
                {
                    long memberId = long.Parse(callbackQuery.Replace("m_deny-", ""));

                    RvMember.Get(memberId).Curator = callback.From.Id;
                    await botClient.EditMessageTextAsync(callback.Message.Chat, update.CallbackQuery.Message.MessageId, $"{callback.Message.Text}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}\n❌Заявка отклонена!");
                    await botClient.SendTextMessageAsync(memberId, string.Format(Language.GetPhrase("Member_Messages_FormDenied", RvUser.Get(memberId).Lang), fullname));
                    RvMember.Get(memberId).Status = "denied";
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);

            else if (callbackQuery.StartsWith("m_deny2-"))
            {
                long memberId = long.Parse(callbackQuery.Replace("m_deny2-", ""));

                if (callback.From.Id == RvMember.Get(memberId).Curator)
                {
                    await botClient.EditMessageTextAsync(callback.Message.Chat, update.CallbackQuery.Message.MessageId, $"{callback.Message.Text}\n❌Заявка отклонена!");
                    await botClient.SendTextMessageAsync(memberId, string.Format(Language.GetPhrase("Member_Messages_FormDenied", RvUser.Get(memberId).Lang), fullname));
                    RvMember.Get(memberId).Status = "denied";
                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(callbackUserId).Lang}", disableNotification: true);
                }
                else
                    await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Messages_NoPermission", rvUser.Lang), showAlert: true);
            }
        }
    }
}
