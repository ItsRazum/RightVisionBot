using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Reward
    {
        public static async Task Give(ITelegramBotClient botClient, Message message)
        {
            if (RvUser.Get(message.From.Id).Has(Permission.Rewarding))
                try
                {
                    string command = message.Text.Replace("+reward ", "");
                    string[] rewardCommand = command.Split(" ");
                    string icon = rewardCommand[1];
                    string text = string.Join(" ", rewardCommand.Skip(2));
                    RvUser rvUser = RvUser.Get(long.Parse(rewardCommand[0]));
                    Types.Reward reward = new(icon, text);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(icon);
                    sb.Append(rvUser.Rewards.Count + 1 + " – " + text);
                    rvUser.Rewards.Add(reward);

                    await botClient.SendTextMessageAsync(message.Chat, "Награда внесена");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Синтаксическая ошибка! Правильное использование команды:\n+reward [id пользователя] [смайлик-иконка] [текст награды]\n\nПример: +reward 901152811 🏆 Награждён\nРезультат: 🏆1 – Награждён");
                }
        }
    }
}
