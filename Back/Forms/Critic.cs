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
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Forms
{
    class Critic
    {
        private static InlineKeyboardMarkup criticAcceptOrDeny = Keyboard.criticAcceptOrDeny;
        public static void Form(ITelegramBotClient botClient, Message message)
        {
            sql database = Program.database;
            long userId = message.From.Id;
            var chooseRate = Keyboard.chooseRate(userId);
            var backButton = Keyboard.backButton(userId);
            var MainMenu   = Keyboard.MainMenu(userId);
            var critic = RvCritic.Get(userId);
            string back = Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang);

            if (critic != null && critic.UserId == userId)
            {
                if (message.Text == "0" || message.Text.Contains("'"))
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", RvUser.Get(userId).Lang));
                else
                    if (critic.Name == "0")
                        if (message.Text == back)
                        {
                            database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{userId}';", "");
                            CriticRoot.newCritics.Remove(critic);
                            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на судейство\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                            HubClass.SelectRole(botClient, message);
                        }
                        else
                        {
                            critic.Name = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", RvUser.Get(userId).Lang), message.Text), replyMarkup: backButton);
                        }

                    else if (critic.Name != "0" && critic.Link == "0")
                        if (message.Text == back)
                        {
                            critic.Name = "0";
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterName", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }
                        else if (!message.Text.StartsWith("https://"))
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_IncorrectFormat", RvUser.Get(userId).Lang));
                        else
                        {
                            critic.Link = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterRate", RvUser.Get(userId).Lang), replyMarkup: chooseRate);
                        }

                    else if (critic.Link != "0" && critic.Rate == "0")
                        if (message.Text == back)
                        {
                            critic.Link = "0";
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", RvUser.Get(userId).Lang), RvMember.Get(userId).Name), replyMarkup: backButton);
                        }
                        else
                        {
                            critic.Rate = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterAboutYou", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }

                    else if (critic.Rate != "0" && critic.About == "0")
                        if (message.Text == back)
                        {
                            critic.Link = "0";
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterRate", RvUser.Get(userId).Lang), critic.Name), replyMarkup: chooseRate);
                        }
                        else
                        {
                            critic.About = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterWhyYou", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }

                    else if (critic.About != "0" && critic.WhyYou == "0")
                        if (message.Text == back)
                        {
                            critic.About = "0";
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterAboutYou", RvUser.Get(userId).Lang), critic.Name), replyMarkup: backButton);
                        }
                        else
                        {
                            critic.WhyYou = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_FormSubmitted", RvUser.Get(userId).Lang), replyMarkup: MainMenu);
                            botClient.SendTextMessageAsync(-1001968408177,
                                $"Пришла новая заявка на должность судьи!\n\n" +
                                $"Id: {critic.UserId}\n" +
                                $"Имя: {critic.Name}\n" +
                                $"Тег: {critic.Telegram}\n" +
                                $"Ссылка на канал: {critic.Link}\n" +
                                $"Субъективная оценка навыков: {critic.Rate}\n" +
                                $"Что написал о себе: {critic.About}\n" +
                                $"Почему мы должны его принять: {critic.WhyYou}\n",
                                replyMarkup: criticAcceptOrDeny);
                        }
            }
        }
    }
}
