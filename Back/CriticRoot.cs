using RightVisionBot.Common;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back
{
    class CriticRoot
    {
        public static void EnterName(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message ?? update.CallbackQuery?.Message;
            long userId = RvUser.Get(message.From.Id) == null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            string telegram = RvUser.Get(message.From.Id) == null ? update.CallbackQuery?.From.Username : update.Message?.From.Username;
            RvUser rvUser = RvUser.Get(userId);
            //botClient.SendTextMessageAsync(update.Message.Chat, Language.GetPhrase("Critic_Messages_EnrollmentClosed", RvUser.Get(update.Message.From.Id).Lang));
            if (RvCritic.Get(userId) == null && message.Chat.Type == ChatType.Private)
            {
                Program.UpdateRvLocation(userId, RvLocation.CriticForm);
                _ = new RvCritic(userId, "@" + telegram);

                botClient.EditMessageTextAsync(message.Chat, update.CallbackQuery.Message.MessageId, Language.GetPhrase("Critic_Messages_EnterName", rvUser.Lang), replyMarkup: Keyboard.CancelForm(rvUser, Status.Critic));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} начал заполнение заявки на судейство", disableNotification: true);
            }
        }

        public static void SetCriticCategory(ITelegramBotClient botClient, Update update, string category)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = update.CallbackQuery.Data;
            var fullname = callback.From.FirstName + callback.From.LastName;
            long criticId = 0;

            var category2 = "0";
            switch (category)
            {
                case "🥉Bronze":
                    category2 = "bronze";
                    criticId = long.Parse(callbackQuery.Replace("c_bronze-", ""));
                    break;
                case "🥈Silver":
                    category2 = "silver";
                    criticId = long.Parse(callbackQuery.Replace("c_silver-", ""));
                    break;
                case "🥇Gold":
                    category2 = "gold";
                    criticId = long.Parse(callbackQuery.Replace("c_gold-", ""));
                    break;
                case "💎Brilliant":
                    category2 = "brilliant";
                    criticId = long.Parse(callbackQuery.Replace("c_brilliant-", ""));
                    break;
            }

            if (callback.From.Id == RvCritic.Get(criticId).Curator)
            {
                botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"{callback.Message.Text}\nКатегория: {category}\n\nКандидат был приглашён в эту беседу!");
                botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormAccepted", RvUser.Get(criticId).Lang), category, fullname));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} выдал категорию {category2} судье Id:{criticId}", disableNotification: true);
                
                RvUser.Get(criticId).ResetPermissions();
                RvCritic.Get(criticId).Status = category2;
                RvUser.Get(criticId).Category = category2;
                Program.UpdateStatus(criticId);
            }
        }
    }
}
