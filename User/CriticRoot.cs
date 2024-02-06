using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

//корень судей, обработка всех событий от судей
namespace RightVisionBot.User
{
    class RvCritic
    {
        public long UserId;

        private string _name = "0";
        public string Name { get => _name; set { _name = value; newString(value, nameof(Name)); } }

        private string _telegram = "0";
        public string Telegram { get => _telegram; set { _telegram = value; newString(value, nameof(Telegram)); } }

        private string _link = "0";
        public string Link { get => _link; set { _link = value; newString(value, nameof(Link)); } }

        private string _rate = "0";
        public string Rate { get => _rate; set { _rate = value; newString(value, nameof(Rate)); } }

        private string _about = "0";
        public string About { get => _about; set { _about = value; newString(value, nameof(About)); } }

        private string _whyYou = "0";
        public string WhyYou { get => _whyYou; set { _whyYou = value; newString(value, nameof(WhyYou)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; newLong(value, nameof(Curator)); } }
        private string _status = "0";
        public string Status { get => _status; set { _status = value; newString(value, nameof(Status)); } }

        public PreListener PreListening;

        private string newString(string value, string property)
        { _OnPropertyChanged(property, value); return value; }

        private long newLong(long value, string property)
        { newString(value.ToString(), property); return value; }

        public event Action<string> OnPropertyChanged = delegate { };   
        private void _OnPropertyChanged(string property, string value)
        { OnPropertyChanged(property); UpdateDatabase(property, value); }

        private void UpdateDatabase(string property, string value)
        {
            sql database = Program.database; 
            database.Read($"UPDATE `RV_Critics` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
        }

        public static RvCritic Get(long userId)
        {
            foreach (RvCritic critic in CriticRoot.newCritics)
            {
                if (critic.UserId == userId)
                    return critic;
            }
            return null;
        }
    }

    class PreListener
    {
        public long ListenerId { get; set; }
        public long ArtistId { get; set; }


        private string NewString(string value, string property)
        { _OnPropertyChanged(property, value); return value; }

        private long NewLong(long value, string property)
        { NewString(value.ToString(), property); return value; }

        public event Action<string> OnPropertyChanged = delegate { };
        private void _OnPropertyChanged(string property, string value)
        { OnPropertyChanged(property); UpdateDatabase(property, value); }

        private void UpdateDatabase(string property, string value) => 
            Program.database.Read($"UPDATE `RV_PreListening` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {ListenerId}", "");
    }

    class CriticRoot
    {
        private static sql database = Program.database;
        public static volatile List<RvCritic> newCritics = new();

        public static void EnterName(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message ?? update.CallbackQuery?.Message;
            long userId = RvUser.Get(message.From.Id) == null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            string telegram = RvUser.Get(message.From.Id) == null ? update.CallbackQuery?.From.Username : update.Message?.From.Username;
            RvUser rvUser = RvUser.Get(userId);
            //botClient.SendTextMessageAsync(update.Message.Chat, Language.GetPhrase("Critic_Messages_EnrollmentClosed", RvUser.Get(update.Message.From.Id).Lang));
            if (RvCritic.Get(userId) == null)
            {
                Program.updateRvLocation(userId, RvLocation.CriticForm);
                RvCritic critic = new();
                critic.UserId = userId;
                critic.Telegram = "@" + telegram;

                var query = $"INSERT INTO `RV_Critics` (`telegram`, `userId`) VALUES ('{critic.Telegram}', '{critic.UserId}');";
                database.Read(query, "");
                newCritics.Add(critic);
                var removeKeyboard = new ReplyKeyboardRemove();
                ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang)) }) { ResizeKeyboard = true };
                botClient.EditMessageTextAsync(message.Chat, update.CallbackQuery.Message.MessageId, Language.GetPhrase("Critic_Messages_EnterName", rvUser.Lang), replyMarkup: Keyboard.CancelForm(rvUser, Status.Critic));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} начал заполнение заявки на судейство", disableNotification: true);
            }
        }

        public static void SetCriticCategory(ITelegramBotClient botClient, Update update, string category)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = update.CallbackQuery.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;
            long criticId = 0;

            string category2 = "0";
            switch (category)
            {
                case "🥉Bronze":
                    category2 = "bronze";
                    criticId = long.Parse(callbackQuery.Replace("c_bronze-", ""));
                    break;
                case "🥈Steel":
                    category2 = "steel";
                    criticId = long.Parse(callbackQuery.Replace("c_steel-", ""));
                    break;
                case "🥇Gold":
                    category2 = "gold";
                    criticId = long.Parse(callbackQuery.Replace("c_gold-", ""));
                    break;
                case "💎Brilliant":
                    category2 = "brilliant";
                    criticId = long.Parse(callbackQuery.Replace("c_brilliant-", ""));
                    break;
            }

            if (callback.From.Id == RvCritic.Get(criticId).Curator)
            {
                botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"{callback.Message.Text}\nКатегория: {category}\n\nКандидат был приглашён в эту беседу!");
                botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormAccepted", RvUser.Get(criticId).Lang), category, fullname));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} выдал категорию {category2} судье Id:{criticId}", disableNotification: true);
                RvCritic.Get(criticId).Status = category2;
                Program.UpdateStatus(criticId);
            }
        }
    }

    class PreListening
    {
        static sql database = Program.database;
        public static async Task Start(ITelegramBotClient botClient, CallbackQuery callback)
        {
            long userId = callback.From.Id;
            if (RvUser.Get(userId).Has(Permission.Curate))
            {
                Program.updateRvLocation(userId, RvLocation.PreListening);
                InlineKeyboardMarkup actions = new(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("Начать предварительное прослушивание", "c_startprelistening") },
                        new[] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang), "c_openmenu") }
                    });
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, Language.GetPhrase("Keyboard_Choice_Critic_Menu_PreListening_Instruction", RvUser.Get(userId).Lang), replyMarkup: actions);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл меню предварительного прослушивания\n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
            else await botClient.AnswerCallbackQueryAsync(callback.Id, "Извини, но тебе нельзя проводить предварительное прослушивание!", showAlert: true);
        }

        public static async Task PreListenTrack(ITelegramBotClient botClient, CallbackQuery callback)
        {
            ReplyKeyboardMarkup back = new(new[]
            { new[] { new KeyboardButton("Назад") } })
            { ResizeKeyboard = true };
            long userId = callback.From.Id;
            var actions = Keyboard.actions;
            var artistId = from rvMember in MemberRoot.newMembers where (rvMember.Status == "waiting" && rvMember.Track.Image != null && rvMember.Track.Track != null) select rvMember.UserId;
            if (!artistId.Any())
            {
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Свободные треки для прослушивания не найдены!", showAlert: true);
                Program.updateRvLocation(userId, RvLocation.CriticMenu);
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл судейское меню \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
                
            else
            {
                PreListener preListener = new() 
                { ArtistId = artistId.First(), ListenerId = userId };
                RvCritic.Get(userId).PreListening = preListener;
                var trackCard = RvMember.Get(artistId.First()).Track;
                var trackName = RvMember.Get(artistId.First()).TrackStr;
                database.Read($"INSERT INTO `RV_PreListening` (`listenerId`, `artistId`) VALUES ('{Get(userId).ListenerId}', '{Get(userId).ArtistId}');", "");
                trackCard.Status = "checked";
                await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(trackCard.Track), caption: $"Название: {trackName}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                await botClient.SendPhotoAsync(callback.Message.Chat, new InputFileId(trackCard.Image), caption: "Обложка ремикса");
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Выбери действие", replyMarkup: actions);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.Message.From.Username} начал предварительное прослушивание\n=====\nId:{callback.Message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }

        public static async Task NextTrack(ITelegramBotClient botClient, CallbackQuery callback)
        {
            var actions = Keyboard.actions;
            long userId = callback.From.Id;
            RvMember.Get(Get(userId).ArtistId).Track.Status = "ok";

            var artistId = from rvMember in MemberRoot.newMembers where(rvMember.Status == "waiting" && rvMember.Track.Image != null && rvMember.Track.Track != null) select rvMember.UserId;
            var trackName = RvMember.Get(artistId.First()).TrackStr;
            if (!artistId.Any())
            {
                await botClient.AnswerCallbackQueryAsync(callback.Id, "Свободные треки для прослушивания не найдены!", showAlert: true);
                Program.updateRvLocation(userId, RvLocation.CriticMenu);
                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {RvCritic.Get(userId).Status}", replyMarkup: Keyboard.criticMenu);
                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{callback.From.Username} открыл судейское меню \n=====\nId:{callback.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
            else
            {
                Get(userId).ArtistId = artistId.First();
                var artist = RvMember.Get(Get(userId).ArtistId).Track;
                
                artist.Status = "checked";
                await botClient.SendDocumentAsync(callback.Message.Chat, new InputFileId(artist.Track), caption: $"Название: {trackName}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                await botClient.SendPhotoAsync(callback.Message.Chat, new InputFileId(artist.Image), caption: "Обложка ремикса");
                await botClient.SendTextMessageAsync(callback.Message.Chat, "Выбери действие", replyMarkup: actions);
            }
        }

        public static PreListener Get(long userId)
        {
            foreach (var preListener in CriticRoot.newCritics)
                if (preListener.UserId == userId)
                    return preListener.PreListening;

            return null;
        }
    }
}
