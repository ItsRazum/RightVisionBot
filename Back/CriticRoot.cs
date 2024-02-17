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

namespace RightVisionBot.Back
{
    class CriticRoot
    {
        private static sql database = Program.database;

        public static void EnterName(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message ?? update.CallbackQuery?.Message;
            long userId = RvUser.Get(message.From.Id) == null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            string telegram = RvUser.Get(message.From.Id) == null ? update.CallbackQuery?.From.Username : update.Message?.From.Username;
            RvUser rvUser = RvUser.Get(userId);
            //botClient.SendTextMessageAsync(update.Message.Chat, Language.GetPhrase("Critic_Messages_EnrollmentClosed", RvUser.Get(update.Message.From.Id).Lang));
            if (RvCritic.Get(userId) == null)
            {
                Program.UpdateRvLocation(userId, RvLocation.CriticForm);
                RvCritic critic = new();
                critic.UserId = userId;
                critic.Telegram = "@" + telegram;

                var query = $"INSERT INTO `RV_Critics` (`telegram`, `userId`) VALUES ('{critic.Telegram}', '{critic.UserId}');";
                database.Read(query, "");
                Data.RvCritics.Add(critic);
                var removeKeyboard = new ReplyKeyboardRemove();
                ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang)) }) { ResizeKeyboard = true };
                botClient.EditMessageTextAsync(message.Chat, update.CallbackQuery.Message.MessageId, Language.GetPhrase("Critic_Messages_EnterName", rvUser.Lang), replyMarkup: Keyboard.CancelForm(rvUser, Status.Critic));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} начал заполнение заявки на судейство", disableNotification: true);
            }
        }

        public static void SetCriticCategory(ITelegramBotClient botClient, Update update, string category)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = update.CallbackQuery.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;
            long criticId = 0;

            string category2 = "0";
            switch (category)
            {
                case "🥉Bronze":
                    category2 = "bronze";
                    criticId = long.Parse(callbackQuery.Replace("c_bronze-", ""));
                    break;
                case "🥈Steel":
                    category2 = "steel";
                    criticId = long.Parse(callbackQuery.Replace("c_steel-", ""));
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
                RvCritic.Get(criticId).Status = category2;
                Program.UpdateStatus(criticId);
            }
        }
    }
}
