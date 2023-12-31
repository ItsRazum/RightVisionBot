﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1;
using RightVisionBot.Back;

//корень участников, обработка всех событий от участников
namespace RightVisionBot.User
{
    class Member
    {
        private string _name = "0";
        public string Name { get => _name; set { _name = value; newString(value, nameof(Name)); } }

        private string _telegram = "0";
        public string Telegram { get => _telegram; set { _telegram = value; newString(value, nameof(Telegram)); } }

        private long _userId = 0;
        public long UserId { get => _userId; set { _userId = value; newLong(value, nameof(UserId)); } }

        private string _country = "0";
        public string Country { get => _country; set { _country = value; newString(value, nameof(Country)); } }

        private string _city = "0";
        public string City { get => _city; set { _city = value; newString(value, nameof(City)); } }

        private string _link = "0";
        public string Link { get => _link; set { _link = value; newString(value, nameof(Link)); } }

        private string _rate = "0";
        public string Rate { get => _rate; set { _rate = value; newString(value, nameof(Rate)); } }

        private string _track = "0";
        public string Track { get => _track; set { _track = value; newString(value, nameof(Track)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; newLong(value, nameof(Curator)); } }

        private string _status = "0";
        public string Status { get => _status; set { _status = value; newString(value, nameof(Status)); } }

        private string newString(string value, string property)
        {
            _OnPropertyChanged(property, value);
            return value;
        }

        private long newLong(long value, string property)
        {
            newString(value.ToString(), property);
            return value;
        }

        public event Action<string> OnPropertyChanged = delegate { };
        private void _OnPropertyChanged(string property, string value)
        {
            OnPropertyChanged(property);
            UpdateDatabase(property, value);
        }

        private void UpdateDatabase(string property, string value)
        {
            sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");
            switch (property)
            {
                case "track":
                    database.Read($"UPDATE `RV_TrackRating` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
                    break;
            }
            database.Read($"UPDATE `RV_Members` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
        }
    }


    class MemberRoot
    {
        static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");
        public static volatile List<Member> newMembers = new();


        public static void EnterName(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message ?? update.CallbackQuery.Message;
            long userId = Program.GetUser(message.From.Id) == null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            string telegram = Program.GetUser(message.From.Id) == null ? update.CallbackQuery.From.Username : update.Message.From.Username;
            botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnrollmentClosed", Program.GetUser(userId).lang));
            /*
            if (GetMember(userId) == null)
            {
                Member member = new();
                member.UserId = userId;
                member.Telegram = "@" + telegram;

                var query = $"INSERT INTO `RV_Members` (`telegram`, `userId`) VALUES ('{member.Telegram}', '{member.UserId}');";

                database.Read(query, "");
                newMembers.Add(member);
                var removeKeyboard = new ReplyKeyboardRemove();
                ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", Program.GetUser(userId).lang)) }) { ResizeKeyboard = true };
                botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterName", Program.GetUser(userId).lang), Program.GetUser(userId).userName), replyMarkup: backButton);
                Program.updateLocation(userId, "memberform");
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал заполнение заявки на участие\n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
            }
            */
        }

        public static Member GetMember(long userId)
        {
            foreach (Member member in newMembers)
            {
                if (member.UserId == userId)
                    return member;
            }

            return null;
        }

        public static void SetMemberCategory(ITelegramBotClient botClient, Update update, string category)
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
            long memberId = long.Parse(match.Groups[1].Value);
            if (callback.From.Id == GetMember(memberId).Curator)
            {
                botClient.EditMessageTextAsync(callback.Message.Chat, update.CallbackQuery.Message.MessageId, $"{callback.Message.Text}\nКатегория: {category}\n\n✅Заявка на участие принята! Отныне кандидат является полноценным участником RightVision!");
                botClient.SendTextMessageAsync(memberId, string.Format(Language.GetPhrase("Member_Messages_FormAccepted", Program.GetUser(memberId).lang), category, fullname));
                botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{update.CallbackQuery.From.Username} выдал категорию {category2} участнику Id:{memberId}\n=====\nId:{update.CallbackQuery.From.Id}\nЯзык: {Program.GetUser(update.CallbackQuery.From.Id).lang}\nЛокация: {Program.GetUser(update.CallbackQuery.From.Id).location}", disableNotification: true);
                var updateMemberStatus = $"UPDATE `RV_Members` SET `status` = '{category2}' WHERE `userId` = '{memberId}'; INSERT INTO `RV_Tracks` (`userId`, `track`) VALUES ('{memberId}', '{GetMember(memberId).Track}')";
                database.Read(updateMemberStatus, "");
                Program.UpdateStatus(memberId);
            }
        }

        public static void ChangeMemberCategory(long userId, string category)
        {
            database.Read($"DELETE FROM RV_C{MemberRoot.GetMember(userId).Status} WHERE userId = '{userId}';", "");
            database.Read($"INSERT INTO RV_C{category} (userId, track, status) VALUES ('{userId}', '{MemberRoot.GetMember(userId).Track}', 'checked'", "");
        }
    }
}
