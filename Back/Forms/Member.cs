using RightVisionBot.Common;
using RightVisionBot.UI;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Forms
{
    class Member
    {
        public static void Form(ITelegramBotClient botClient, Message message)
        {
            var database = Program.database;
            var userId = message.From.Id;
            var chooseRate = Keyboard.chooseRate(RvUser.Get(userId).Lang);
            var backButton = Keyboard.backButton(RvUser.Get(userId).Lang);
            var mainMenu =   Keyboard.MainMenu(RvUser.Get(userId).Lang);
            var memberAcceptOrDeny = Keyboard.memberAcceptOrDeny;
            var member = RvMember.Get(userId);
            var back = Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang);

            if (member != null && member.UserId == userId)
            {
                if (message.Text == "0" || message.Text.Contains('\''))
                    botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", RvUser.Get(userId).Lang));
                else
                    if (member.Name == "0")
                        if (message.Text == back)
                        {
                            database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{userId}';", "");
                            MemberRoot.newMembers.Remove(RvMember.Get(userId));
                            botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на участие\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
                            HubClass.SelectRole(botClient, message);
                        }
                        else
                        {
                            member.Name = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCountry", RvUser.Get(userId).Lang), message.Text), replyMarkup: backButton);
                        }

                    else if (member.Name != "0" && member.Country == "0")
                        if (message.Text == back)
                        {
                            member.Name = "0";
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterName", RvUser.Get(userId).Lang));
                        }
                        else
                        {
                            member.Country = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCity", RvUser.Get(userId).Lang), member.Name), replyMarkup: backButton);
                        }

                    else if (member.Country != "0" && member.City == "0")
                        if (message.Text == back)
                        {
                            member.Country = "0";
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCountry", RvUser.Get(userId).Lang), member.Name), replyMarkup: backButton);
                        }
                        else
                        {
                            member.City = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterLink", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }

                    else if (member.City != "0" && member.Link == "0")
                        if (message.Text == back)
                        {
                            member.City = "0";
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCity", RvUser.Get(userId).Lang), member.Name), replyMarkup: backButton);
                        }
                        else
                        {
                            member.Link = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", RvUser.Get(userId).Lang), replyMarkup: chooseRate);
                        }

                    else if (member.Link != "0" && member.Rate == "0")
                        if (message.Text == back)
                        {
                            member.Link = "0";
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterLink", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }
                        else
                        {
                            member.Rate = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterTrack", RvUser.Get(userId).Lang), replyMarkup: backButton);
                        }

                    else if (member.Rate != "0" && member.TrackStr == "0")
                        if (message.Text == back)
                        {
                            member.Rate = "0";
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", RvUser.Get(userId).Lang), replyMarkup: chooseRate);
                        }
                        else
                        {
                            member.TrackStr = message.Text;
                            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_FormSubmitted", RvUser.Get(userId).Lang), replyMarkup: mainMenu);
                            botClient.SendTextMessageAsync(-1001968408177,
                                $"Пришла новая заявка на участие!\n\n" +
                                $"Имя: {member.Name}\n" +
                                $"Тег: {member.Telegram}\n" +
                                $"Страна проживания: {member.Country}\n" +
                                $"Город: {member.City}\n" +
                                $"Ссылка на канал: {member.Link}\n" +
                                $"Субъективная оценка навыков: {member.Rate}\n" +
                                $"Его трек внесён в базу и пока что держится в тайне!\n" +
                                $"\n" +
                                $"Тот, кто возьмёт кураторство над участником, обязан будет проверить канал и выдать категорию! В случае, если ссылки нету - провести лично с ним проверку мастерства и также выдать категорию!",
                                replyMarkup: memberAcceptOrDeny(userId));
                            RvUser.Get(member.UserId).RemovePermission(Permission.SendMemberForm);
                    }
                
            }
        }
    }
}
