using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.Common;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back.Forms
{
    class Critic
    {
        public static async Task Form(ITelegramBotClient botClient, Message message)
        {
            sql database = Program.database;
            long userId = message.From.Id;
            var critic = RvCritic.Get(userId);
            var rvUser = RvUser.Get(userId);
            var chooseRate = Keyboard.ChooseRate(rvUser.Lang);
            var backButton = Keyboard.BackButton(rvUser.Lang);
            var mainMenu   = Keyboard.MainMenu(rvUser.Lang);
            string back = Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang);

            if (critic != null && critic.UserId == userId && message.Chat.Type == ChatType.Private)
            {
                if (message.Text == "0" || message.Text.Contains("'"))
                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", rvUser.Lang));
                else
                    if (critic.Name == "0")
                        if (message.Text == back)
                        {
                            database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{userId}';", "");
                            Data.RvCritics.Remove(critic);
                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на судейство\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                            await botClient.SendTextMessageAsync(message.Chat, "✅", replyMarkup: Keyboard.remove);
                            await HubClass.Hub(botClient, message, rvUser.Lang);
                        }
                        else
                        {
                            critic.Name = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", rvUser.Lang), critic.Name), replyMarkup: backButton);
                        }
                        
                    else if (critic.Name != "0" && critic.Link == "0")
                        if (message.Text == back)
                        {
                            critic.Name = "0";
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterName", rvUser.Lang), replyMarkup: backButton);
                        }
                        else if (!message.Text.StartsWith("https://"))
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_IncorrectFormat", rvUser.Lang));
                        else
                        {
                            critic.Link = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterRate", rvUser.Lang), replyMarkup: chooseRate);
                        }
                        
                    else if (critic.Link != "0" && critic.Rate == "0")
                        if (message.Text == back)
                        {
                            critic.Link = "0";
                            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", rvUser.Lang), critic.Name), replyMarkup: backButton);
                        }
                        else
                        {
                            critic.Rate = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterAboutYou", rvUser.Lang), replyMarkup: backButton);
                        }
                        
                    else if (critic.Rate != "0" && critic.About == "0")
                        if (message.Text == back)
                        {
                            critic.Link = "0";
                            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterRate", rvUser.Lang), critic.Name), replyMarkup: chooseRate);
                        }
                        else
                        {
                            critic.About = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterWhyYou", rvUser.Lang), replyMarkup: backButton);
                        }
                        
                    else if (critic.About != "0" && critic.WhyYou == "0")
                        if (message.Text == back)
                        {
                            critic.About = "0";
                            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterAboutYou", rvUser.Lang), critic.Name), replyMarkup: backButton);
                        }
                        else
                        {
                            critic.WhyYou = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_FormSubmitted", rvUser.Lang), replyMarkup: mainMenu);
                            await botClient.SendTextMessageAsync(-1001968408177,
                                $"Пришла новая заявка на должность судьи!\n\n" +
                                $"Имя: {critic.Name}\n" +
                                $"Тег: {critic.Telegram}\n" +
                                $"Ссылка на канал: {critic.Link}\n" +
                                $"Субъективная оценка навыков: {critic.Rate}\n" +
                                $"Что написал о себе: {critic.About}\n" +
                                $"Почему мы должны его принять: {critic.WhyYou}\n",
                                replyMarkup: Keyboard.CriticAcceptOrDeny(critic.UserId));
                            RvUser.Get(critic.UserId).Permissions.Remove(Permission.SendCriticForm);
                            critic.Status = "waiting";
                        }
            }
        }
    }
}
