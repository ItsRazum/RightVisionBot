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
            database.Read($"UPDATE `RV_PreListening` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {ListenerId}", "");
        }
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
            //botClient.SendTextMessageAsync(update.Message.Chat, Language.GetPhrase("Critic_Messages_EnrollmentClosed", RvUser.Get(update.Message.From.Id).Lang));
            if (RvCritic.Get(userId) == null)
            {

                RvCritic critic = new();
                critic.UserId = userId;
                critic.Telegram = "@" + telegram;

                var query = $"INSERT INTO `RV_Critics` (`telegram`, `userId`) VALUES ('{critic.Telegram}', '{critic.UserId}');";
                database.Read(query, "");
                newCritics.Add(critic);
                var removeKeyboard = new ReplyKeyboardRemove();
                ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", RvUser.Get(userId).Lang)) }) { ResizeKeyboard = true };
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterName", RvUser.Get(userId).Lang), replyMarkup: backButton);
                Program.updateRvLocation(userId, RvLocation.CriticForm);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал заполнение заявки на судейство", disableNotification: true);
            }
        }

        public static void SetCriticCategory(ITelegramBotClient botClient, Update update, string category)
        {
            var callback = update.CallbackQuery;
            var callbackQuery = update.CallbackQuery.Data;
            string fullname = callback.From.FirstName + callback.From.LastName;

            string category2 = "0";
            switch (category)
            {
                case "🥉Bronze":
                    category2 = "bronze";
                    break;
                case "🥈Steel":
                    category2 = "steel";
                    break;
                case "🥇Gold":
                    category2 = "gold";
                    break;
                case "💎Brilliant":
                    category2 = "brilliant";
                    break;

            }

            Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
            long criticId = long.Parse(match.Groups[1].Value);
            if (callback.From.Id == RvCritic.Get(criticId).Curator)
            {
                botClient.EditMessageTextAsync(callback.Message.Chat,
                    update.CallbackQuery.Message.MessageId,
                    $"{callback.Message.Text}\nКатегория: {category}\n\nКандидат был приглашён в эту беседу!");
                botClient.SendTextMessageAsync(criticId, string.Format(Language.GetPhrase("Critic_Messages_FormAccepted", RvUser.Get(criticId).Lang), category, fullname));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} выдал категорию {category2} судье Id:{criticId}", disableNotification: true);
                var updateCriticStatus = $"UPDATE `RV_Critics` SET `status` = '{category2}' WHERE `userId` = {criticId};";
                database.Read(updateCriticStatus, "");
                Program.UpdateStatus(criticId);
            }
        }
    }

    class PreListening
    {
        public static volatile List<PreListener> preListeners = new();
        static sql database = Program.database;
        public static void Start(ITelegramBotClient botClient, Message message)
        {
            long userId = message.From.Id;
            List<string> CuratorId = database.Read($"SELECT * FROM `RV_Curators` WHERE `id` = '{userId}';", "id");
            string curatorId = CuratorId.FirstOrDefault();
            if (curatorId != null)
            {
                Program.updateRvLocation(userId, RvLocation.PreListening);
                ReplyKeyboardMarkup actions = new(new[]
                    {
                        new[] { new KeyboardButton("Начать предварительное прослушивание") },
                        new[] { new KeyboardButton("Назад") }
                    })
                { ResizeKeyboard = true };
                botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Keyboard_Choice_Critic_Menu_PreListening_Instruction", RvUser.Get(userId).Lang), replyMarkup: actions);
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл меню предварительного прослушивания\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }

        public static void PreListenTrack(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup back = new(new[]
            { new[] { new KeyboardButton("Назад") } })
            { ResizeKeyboard = true };
            long userId = message.From.Id;
            ReplyKeyboardMarkup actions = Keyboard.actions;
            var artistId = database.Read($"SELECT `userId` FROM `RV_Tracks` WHERE `status` = 'waiting' AND `userId` != '{userId}' AND `track` IS NOT NULL AND `image` IS NOT NULL LIMIT 1", "userId");
            var trackName = database.Read($"SELECT `track` FROM `RV_Members` WHERE `userId` = '{artistId.FirstOrDefault()}' LIMIT 1", "track");
            var trackCard = database.ExtRead($"SELECT `track`, `image` FROM `RV_Tracks` WHERE `userId` = '{artistId.FirstOrDefault()}'", new[] { "track", "image" });
            if (artistId.FirstOrDefault() == null)
                botClient.SendTextMessageAsync(message.Chat, "Свободные треки для прослушивания не найдены!", replyMarkup: back);
            else
            {
                PreListener preListener = new() 
                { ArtistId = long.Parse(artistId.FirstOrDefault()), ListenerId = userId };
                preListeners.Add(preListener);
                database.Read($"INSERT INTO `RV_PreListening` (`listenerId`, `artistId`) VALUES ('{Get(userId).ListenerId}', '{Get(userId).ArtistId}');", "");
                foreach (var track in trackCard)
                {
                    database.Read($"UPDATE `RV_Tracks` SET `status` = 'checked' WHERE `userId` = {artistId.FirstOrDefault()};", "");
                    botClient.SendDocumentAsync(message.Chat, new InputFileId(track["track"].ToString()), caption: $"Название: {trackName.FirstOrDefault()}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                    botClient.SendPhotoAsync(message.Chat, new InputFileId(track["image"].ToString()), caption: "Обложка ремикса");
                    botClient.SendTextMessageAsync(message.Chat, "Выбери действие", replyMarkup: actions);
                }
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал предварительное прослушивание\n=====\nId:{message.From.Id}\nЯзык: {RvUser.Get(userId).Lang}\nЛокация: {RvUser.Get(userId).RvLocation}", disableNotification: true);
            }
        }

        public static void NextTrack(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup back = new(new[]
                    { new[] { new KeyboardButton("Назад") } })
                { ResizeKeyboard = true };
            ReplyKeyboardMarkup actions = Keyboard.actions;
            long userId = message.From.Id;

            var artistId = database.Read($"SELECT `userId` FROM `RV_Tracks` WHERE `status` = 'waiting' AND `userId` != '{userId}' AND `track` IS NOT NULL AND `image` IS NOT NULL LIMIT 1", "userId");
            var trackName = database.Read($"SELECT `track` FROM `RV_Members` WHERE `userId` = {artistId.FirstOrDefault()};", "track");
            var trackCard = database.ExtRead($"SELECT `track`, `image` FROM `RV_Tracks` WHERE `userId` = {artistId.FirstOrDefault()}", new[] { "track", "image" });
            if (artistId.FirstOrDefault() == null)
                botClient.SendTextMessageAsync(message.Chat, "Свободные треки для прослушивания не найдены!", replyMarkup: back);
            else
            {
                Get(userId).ArtistId = long.Parse(artistId.FirstOrDefault());
                foreach (var track in trackCard)
                {
                    database.Read($"UPDATE `RV_Tracks` SET `status` = 'checked' WHERE `userId` = {artistId.FirstOrDefault()};", "");
                    botClient.SendDocumentAsync(message.Chat, new InputFileId(track["track"].ToString()), caption: $"Название: {trackName.FirstOrDefault()}\nКатегория: {RvMember.Get(Get(userId).ArtistId).Status}");
                    botClient.SendPhotoAsync(message.Chat, new InputFileId(track["image"].ToString()), caption: "Обложка ремикса");
                    botClient.SendTextMessageAsync(message.Chat, "Выбери действие", replyMarkup: actions);
                }
            }
        }

        public static PreListener Get(long userId)
        {
            foreach (var preListener in preListeners)
            {
                if (preListener.ListenerId == userId)
                    return preListener;
            }

            return null;
        }
    }
}
