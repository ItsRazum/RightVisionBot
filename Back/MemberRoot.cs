using RightVisionBot.Common;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back
{
    class MemberRoot
    {
        static sql database = Program.database;

        public static void EnterName(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message ?? update.CallbackQuery.Message;
            long userId = RvUser.Get(message.From.Id) == null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            if (RvUser.Get(userId).Has(Permission.SendMemberForm))
            {
                string telegram = RvUser.Get(message.From.Id) == null ? update.CallbackQuery?.From.Username : update.Message?.From?.Username;
                if (RvMember.Get(userId) == null && message.Chat.Type == ChatType.Private)
                {
                    _ = new RvMember(userId, "@" + telegram);
                    ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang)) }) { ResizeKeyboard = true };
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterName", RvUser.Get(userId).Lang), replyMarkup: backButton);
                    Program.UpdateRvLocation(userId, RvLocation.MemberForm);
                    botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} начал заполнение заявки на участие\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                }
            }
        }

        public static async Task SetMemberCategory(ITelegramBotClient botClient, Update update, string category)
        {
            var callback = update.CallbackQuery;
            var message = callback.Message;
            var chat = message.Chat;
            var callbackQuery = callback.Data;
            var callbackRvUser = RvUser.Get(callback.From.Id);
            var fullname = callback.From.FirstName + callback.From.LastName;
            long memberId = 0;

            string category2 = "0";
            switch (category)
            {
                case "🥉Bronze":
                    category2 = "bronze";
                    memberId = long.Parse(callbackQuery.Replace("m_bronze-", ""));
                    break;
                case "🥈Silver":
                    category2 = "silver";
                    memberId = long.Parse(callbackQuery.Replace("m_silver-", ""));
                    break;
                case "🥇Gold":
                    category2 = "gold";
                    memberId = long.Parse(callbackQuery.Replace("m_gold-", ""));
                    break;
                case "💎Brilliant":
                    category2 = "brilliant";
                    memberId = long.Parse(callbackQuery.Replace("m_brilliant-", ""));
                    break;
            }


            if (callback.From.Id == RvMember.Get(memberId).Curator)
            {
                await botClient.EditMessageTextAsync(chat, message.MessageId, $"{message.Text}\nКатегория: {category}\n\n✅Заявка на участие принята! Отныне кандидат является полноценным участником RightVision!");
                await botClient.SendTextMessageAsync(memberId, string.Format(Language.GetPhrase("Member_Messages_FormAccepted", RvUser.Get(memberId).Lang), category, fullname));
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} выдал категорию {category2} участнику Id:{memberId}\n=====\nId:{callback.From.Id}\nЯзык: {callbackRvUser.Lang}\nЛокация: {callbackRvUser.RvLocation}", disableNotification: true);
                RvMember.Get(memberId).Status = category2;
                RvUser.Get(memberId).Category = category2;
                Program.UpdateStatus(memberId);
            }
        }

        public static void ChangeMemberCategory(long userId, string category)
        {
            database.Read($"DELETE FROM RV_C{RvMember.Get(userId).Status} WHERE `userId` = {userId};", "");
            database.Read($"INSERT INTO RV_C{category} (userId, track, status) VALUES ('{userId}', '{RvMember.Get(userId).Track}', 'ok')", "");
        }
    }
}
