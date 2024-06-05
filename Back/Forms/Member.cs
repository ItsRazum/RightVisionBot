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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back.Forms
{
    class Member
    {
        public static async Task Form(ITelegramBotClient botClient, Message message)
        {
            var database = Program.database;
            var userId = message.From.Id;
            var member = RvMember.Get(userId);
            var rvUser = RvUser.Get(userId);
            var chooseRate = Keyboard.ChooseRate(rvUser.Lang);
            var backButton = Keyboard.BackButton(rvUser.Lang);
            var mainMenu =   Keyboard.MainMenu(rvUser.Lang);
            var memberAcceptOrDeny = Keyboard.MemberAcceptOrDeny;
            var back = Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang);

            if (member != null && member.UserId == userId && message.Chat.Type == ChatType.Private)
            {
                if (message.Text == "0" || message.Text.Contains('\''))
                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", rvUser.Lang));
                else
                    if (member.Name == "0")
                        if (message.Text == back)
                        {
                            database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{userId}';", "");
                            Data.RvMembers.Remove(RvMember.Get(userId));
                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на участие\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                            await botClient.SendTextMessageAsync(message.Chat, "✅", replyMarkup: Keyboard.remove);
                            await HubClass.Hub(botClient, message, rvUser.Lang);
                        }
                        else
                        {
                            member.Name = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterLink", rvUser.Lang), member.Name), replyMarkup: backButton);
                        }

                    else if (member.Name != "0" && member.Link == "0")
                        if (message.Text == back)
                        {
                            member.Name = "0";
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterName", rvUser.Lang), replyMarkup: backButton);
                        }
                        else
                        {
                            member.Link = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", rvUser.Lang), replyMarkup: chooseRate);
                        }

                    else if (member.Link != "0" && member.Rate == "0")
                        if (message.Text == back)
                        {
                            member.Link = "0";
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterLink", rvUser.Lang), replyMarkup: backButton);
                        }
                        else
                        {
                            member.Rate = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterTrack", rvUser.Lang), replyMarkup: backButton);
                        }

                    else if (member.Rate != "0" && member.TrackStr == "0")
                        if (message.Text == back)
                        {
                            member.Rate = "0";
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", rvUser.Lang), replyMarkup: chooseRate);
                        }
                        else
                        {
                            member.TrackStr = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_FormSubmitted", rvUser.Lang), replyMarkup: mainMenu);
                            await botClient.SendTextMessageAsync(-1001968408177,
                                $"Пришла новая заявка на участие!\n\n" +
                                $"Имя: {member.Name}\n" +
                                $"Тег: {member.Telegram}\n" +
                                $"Ссылка на канал: {member.Link}\n" +
                                $"Субъективная оценка навыков: {member.Rate}\n" +
                                $"Его трек внесён в базу и пока что держится в тайне!\n" +
                                $"\n" +
                                $"Тот, кто возьмёт кураторство над участником, обязан будет проверить канал и выдать категорию! В случае, если ссылки нету - провести лично с ним проверку мастерства и также выдать категорию!",
                                replyMarkup: memberAcceptOrDeny(userId));
                            RvUser.Get(member.UserId).Permissions.Remove(Permission.SendMemberForm);
                            member.Status = "waiting";
                        }
                
            }
        }
    }
}
