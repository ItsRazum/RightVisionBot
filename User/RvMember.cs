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
using Telegram.Bot.Types.Enums;

//корень участников, обработка всех событий от участников
namespace RightVisionBot.User
{
    class RvMember
    {
        public long UserId;

        private string _name = "0";
        public string Name { get => _name; set { _name = value; NewString(value, nameof(Name)); } }

        private string _telegram = "0";
        public string Telegram { get => _telegram; set { _telegram = value; NewString(value, nameof(Telegram)); } }

        private string _link = "0";
        public string Link { get => _link; set { _link = value; NewString(value, nameof(Link)); } }

        private string _rate = "0";
        public string Rate { get => _rate; set { _rate = value; NewString(value, nameof(Rate)); } }

        private string _track = "0";
        public string TrackStr { get => _track; set { _track = value; NewString(value, nameof(Track)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; NewString(value.ToString(), nameof(Curator)); } }

        private string _status = "0";
        public string Status { get => _status; set { _status = value; NewString(value, nameof(Status)); } }

        public TrackInfo? Track { get; set; }

        public RvMember(long userId, string telegram)
        {
            UserId = userId;
            Telegram = telegram;

            Program.database.Read($"INSERT INTO `RV_Members` (`telegram`, `userId`) VALUES ('{Telegram}', '{UserId}');", "");
            Data.RvMembers.Add(this);
        }

        public RvMember(long userId, string name, string telegram, string link, string rate, string trackStr, long curator, string status)
        {
            UserId = userId;
            Name = name;
            Telegram = telegram;
            Link = link;
            Rate = rate;
            TrackStr = trackStr;
            Curator = curator;
            Status = status;

            Data.RvMembers.Add(this);
        }

        private void NewString(string value, string property)
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
                if (member.UserId == userId)
                    return member;

            return null;
        }
    }

    class RvExMember
    {
        public long UserId;

        private string _name = "0";
        public string Name { get => _name; set { _name = value; NewString(value, nameof(Name)); } }

        private string _telegram = "0";
        public string Telegram { get => _telegram; set { _telegram = value; NewString(value, nameof(Telegram)); } }

        private string _link = "0";
        public string Link { get => _link; set { _link = value; NewString(value, nameof(Link)); } }

        private string _rate = "0";
        public string Rate { get => _rate; set { _rate = value; NewString(value, nameof(Rate)); } }

        private string _track = "0";
        public string TrackStr { get => _track; set { _track = value; NewString(value, nameof(Track)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; NewString(value.ToString(), nameof(Curator)); } }

        private string _status = "0";
        public string Status { get => _status; set { _status = value; NewString(value, nameof(Status)); } }

        public RvExMember(long userId, string name, string telegram, string link, string rate, string trackStr, long curator, string status)
        {
            UserId = userId;
            Name = name;
            Telegram = telegram;
            Link = link;
            Rate = rate;
            TrackStr = trackStr;
            Curator = curator;
            Status = status;

            Data.RvExMembers.Add(this);
        }

        private void NewString(string value, string property)
        {
            sql database = Program.database;
            database.Read($"UPDATE `RV_ExMembers` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");
        }

        public static RvExMember Get(long userId)
        {
            foreach (RvExMember member in Data.RvExMembers)
                if (member.UserId == userId)
                    return member;

            return null;
        }
    }
}

