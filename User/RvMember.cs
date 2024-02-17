using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1;
using RightVisionBot.Back;
using RightVisionBot.Common;
using RightVisionBot.Tracks;

//корень участников, обработка всех событий от участников
namespace RightVisionBot.User
{
    class RvMember
    {
        public long UserId;

        private string _name = "0";
        public string Name { get => _name; set { _name = value; newString(value, nameof(Name)); } }

        private string _telegram = "0";
        public string Telegram { get => _telegram; set { _telegram = value; newString(value, nameof(Telegram)); } }

        private string _country = "0";
        public string Country { get => _country; set { _country = value; newString(value, nameof(Country)); } }

        private string _city = "0";
        public string City { get => _city; set { _city = value; newString(value, nameof(City)); } }

        private string _link = "0";
        public string Link { get => _link; set { _link = value; newString(value, nameof(Link)); } }

        private string _rate = "0";
        public string Rate { get => _rate; set { _rate = value; newString(value, nameof(Rate)); } }

        private string _track = "0";
        public string TrackStr { get => _track; set { _track = value; newString(value, nameof(Track)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; newLong(value, nameof(Curator)); } }

        private string _status = "0";
        public string Status { get => _status; set { _status = value; newString(value, nameof(Status)); } }

        public TrackInfo? Track { get; set; }

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
            sql database = Program.database;
            switch (property)
            {
                case "track":
                    database.Read($"UPDATE `RV_TrackRating` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
                    break;
            }
            database.Read($"UPDATE `RV_Members` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
        }

        public static RvMember Get(long userId)
        {
            foreach (RvMember member in Data.RvMembers)
            {
                if (member.UserId == userId)
                    return member;
            }

            return null;
        }
    }
}
