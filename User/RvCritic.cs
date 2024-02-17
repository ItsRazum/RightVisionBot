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

        private long _preListeningArtist = 0;
        public long PreListeningArtist { get => _preListeningArtist; set { _preListeningArtist = value; newLong(value, nameof(PreListeningArtist)); } }

        public CriticVote CriticRate;

        private string newString(string value, string property)
        { _OnPropertyChanged(property, value); return value; }

        private long newLong(long value, string property)
        { newString(value.ToString(), property); return value; }

        public event Action<string> OnPropertyChanged = delegate { };   
        private void _OnPropertyChanged(string property, string value)
        { OnPropertyChanged(property); UpdateDatabase(property, value); }

        private void UpdateDatabase(string property, string value) => Program.database.Read($"UPDATE `RV_Critics` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");

        public static RvCritic Get(long userId)
        {
            foreach (RvCritic critic in Data.RvCritics)
                if (critic.UserId == userId)
                    return critic;

            return null;
        }
    }
}
