using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Commands.Admin
{
    class Admin
    {
        public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            switch (message.Text.ToLower())
            {
                case "очистка от зайцев":
                    await Restriction.Cleaning(botClient, message);
                    break;
                case "сбросить права":
                    await Permissions.Reset(botClient, message, rvUser);
                    break;
                case "авторизовать":
                    if (message.From.Id == 901152811 && message.ReplyToMessage != null)
                    {
                        var CuratorId = Program.database.Read($"SELECT * FROM `RV_Curators` WHERE `userId` = '{message.ReplyToMessage.From.Id}';", "id");
                        if (CuratorId != null)
                        {
                            Program.database.Read($"INSERT INTO `RV_Curators` (`id`) VALUES ('{message.ReplyToMessage.From.Id}');", "");
                            await botClient.SendTextMessageAsync(message.Chat, "Пользователь авторизован. Теперь он может брать кураторство над кандидатами (судьи и участники)");
                        }
                        else
                            await botClient.SendTextMessageAsync(message.Chat, "Пользователь уже авторизован!");
                    }
                    break;
            }

            if (message.Text.StartsWith("/news "))
                await News.Common(botClient, message, rvUser);

            else if (message.Text.ToLower().StartsWith("назначить "))
                await Grant.Role(botClient, message, rvUser);
            else if (message.Text.ToLower().StartsWith("разжаловать"))
                await Degrade.Role(botClient, message, rvUser);

            else if (message.Text.StartsWith("+permission "))
                await Grant.Perm(botClient, message, rvUser);
            else if (message.Text.StartsWith("-permission "))
                await Degrade.Perm(botClient, message, rvUser);

            else if (message.Text.StartsWith("+reward "))
                await Reward.Give(botClient, message);

            else if (message.Text.StartsWith("/tech "))
                await News.Tech(botClient, message, rvUser);

            else if (message.Text.StartsWith("/membernews "))
                await News.Member(botClient, message, rvUser);

            else if (message.Text.StartsWith("/ban"))
                await Restriction.Ban(botClient, rvUser, message);

            else if (message.Text.StartsWith("/mute"))
                await Restriction.Mute(botClient, rvUser, message);
            else if (message.Text.StartsWith("/unmute"))
                await Unbans.Unmute(botClient, rvUser, message);

            else if (message.Text.StartsWith("/blacklist on "))
                await Restriction.Blacklist(botClient, rvUser, message);

            else if (message.Text.ToLower().StartsWith("заблокировать ") && rvUser.Has(Permission.Block))
            {
                if (message.Text.ToLower().StartsWith("заблокировать участие "))
                    await Block.Participation(botClient, message);
                else if (message.Text.ToLower().StartsWith("заблокировать судейство "))
                    await Block.Critic(botClient, message);
            }

            else if (message.Text.ToLower().StartsWith("аннулировать ") && rvUser.Has(Permission.Cancel))
            {
                if (message.Text.ToLower().StartsWith("аннулировать участие "))
                    await Cancel.Participation(botClient, message);
                else if (message.Text.ToLower().StartsWith("аннулировать судейство "))
                    await Cancel.Critic(botClient, message);
            }
        }
    }
}