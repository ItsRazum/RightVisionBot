using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Commands
{
    class Critic
    {
        public static async Task Commands(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            sql database = Program.database;
            string? msgText = message.Text;
            long userId = message.From.Id;

            switch (msgText.ToLower())
            {
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "10":
                    if (message.ReplyToMessage != null)
                    {
                        int Rate = int.Parse(message.Text);
                        TrackEvaluation.Get(userId).General = Rate;
                        await botClient.EditMessageReplyMarkupAsync(message.Chat, message.ReplyToMessage.MessageId, TrackEvaluation.RatingSystem(message.From.Id));
                    }
                    break;
            }
        }
    }
}
