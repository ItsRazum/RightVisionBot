using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Reward
    {
        public static async Task Give(ITelegramBotClient botClient, Message message)
        {
            try
            {
                long userId = message.From.Id;
                string command = message.Text.Replace("+reward ", "");
                string[] reward = command.Split(" ");
                string icon = reward[1];
                string text = string.Join(" ", reward.Skip(2));
                RvUser rvUser = RvUser.Get(long.Parse(reward[0]));
                StringBuilder sb = new StringBuilder();
                sb.Append(icon);
                sb.Append(rvUser.Rewards.Count + 1 + " – " + text);
                rvUser.AddReward(sb.ToString());

                await botClient.SendTextMessageAsync(message.Chat, "Награда внесена");
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat, "Синтаксическая ошибка! Правильное использование команды:\n+reward [id пользователя] [смайлик-иконка] [текст награды]\n\nПример: +reward 901152811 🏆 Награждён\nРезультат: 🏆1 – Награждён");
            }
        }
    }
}
