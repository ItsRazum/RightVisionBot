using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.Tracks;
using RightVisionBot.User;

namespace RightVisionBot.Back.Callbacks
{
    class TrackCard
    {
        public static async Task Callbacks(ITelegramBotClient botClient, Update update)
        {
            var callback = update.CallbackQuery;
            long callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;

            switch (callbackQuery)
            {
                case "t_CheckTrack":
                    try
                    {
                        var track = new InputFileId(RvMember.Get(callbackUserId).Track.Track);
                        await botClient.SendDocumentAsync(callbackUserId, track,
                            caption: "Это файл твоего ремикса, который ты скидывал!");
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, "Ты ещё не скидывал ремикс!", showAlert: true); }

                    break;
                case "t_CheckImage":
                    try
                    {
                        var image = new InputFileId(Track.GetTrack(callbackUserId).Image);
                        await botClient.SendPhotoAsync(callbackUserId, image,
                            caption: "Это обложка ремикса, которую ты скидывал!");
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, "Ты ещё не скидывал обложку!", showAlert: true); }

                    break;
                case "t_CheckText":
                    try
                    {
                        var text = new InputFileId(Track.GetTrack(callbackUserId).Text);
                        await botClient.SendDocumentAsync(callbackUserId, text,
                            caption: "Это файл текста твоего ремикса, который ты скидывал!");
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, "Ты ещё не скидывал текст!", showAlert: true); }
                    break;
            }
        }
    }
}
