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
            var callbackUserId = callback.From.Id;
            var callbackQuery = callback.Data;
            var lang = RvUser.Get(callbackUserId).Lang;
            var track = Track.GetTrack(callbackUserId);

            switch (callbackQuery)
            {
                case "t_CheckTrack":
                    try
                    {
                        var trackFile = new InputFileId(track.Track);
                        await botClient.SendDocumentAsync(callbackUserId, trackFile,
                            caption: Language.GetPhrase("Profile_Track_Track_HereItIs", lang));
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Profile_Track_Track_NotSent", lang), showAlert: true); }

                    break;
                case "t_CheckImage":
                    try
                    {
                        var image = new InputFileId(track.Image);
                        await botClient.SendPhotoAsync(callbackUserId, image,
                            caption: Language.GetPhrase("Profile_Track_Image_HereItIs", lang));
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Profile_Track_Image_NotSent", lang), showAlert: true); }

                    break;
                case "t_CheckText":
                    try
                    {
                        var text = new InputFileId(track.Text);
                        await botClient.SendDocumentAsync(callbackUserId, text,
                            caption: Language.GetPhrase("Profile_Track_Text_HereItIs", lang));
                    }
                    catch
                    { await botClient.AnswerCallbackQueryAsync(callback.Id, Language.GetPhrase("Profile_Track_Text_NotSent", lang), showAlert: true); }
                    break;
            }
        }
    }
}
