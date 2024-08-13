using RightVisionBot.Back;
using RightVisionBot.Tracks;

//корень судей, обработка всех событий от судей
namespace RightVisionBot.User
{
    class RvCritic
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

        private string _about = "0";
        public string About { get => _about; set { _about = value; NewString(value, nameof(About)); } }

        private string _whyYou = "0";
        public string WhyYou { get => _whyYou; set { _whyYou = value; NewString(value, nameof(WhyYou)); } }

        private long _curator = 0;
        public long Curator { get => _curator; set { _curator = value; NewString(value.ToString(), nameof(Curator)); } }
        private string _status = "0";
        public string Status { get => _status; set { _status = value; NewString(value, nameof(Status)); } }

        private long _preListeningArtist = 0;
        public long PreListeningArtist { get => _preListeningArtist; set { _preListeningArtist = value; NewString(value.ToString(), nameof(PreListeningArtist)); } }

        public CriticVote CriticRate = new();

        private void NewString(string value, string property) => Program.database.Read($"UPDATE `RV_Critics` SET `{property.ToLower()}` = '{value}' WHERE `userId` = {UserId}", "");

        public RvCritic(long userId, string telegram)
        {
            UserId = userId;
            Telegram = telegram;

            Program.database.Read($"INSERT INTO `RV_Critics` (`telegram`, `userId`) VALUES ('{Telegram}', '{UserId}');", "");
            Data.RvCritics.Add(this);
        }

        public RvCritic(long userId, string name, string telegram, string link, string rate, string about,string whyYou,long curator, string status, long preListeningArtist)
        {
            UserId = userId;
            Name = name;
            Telegram = telegram;
            Link = link;
            Rate = rate;
            About = about;
            WhyYou = whyYou;
            Curator = curator;
            Status = status;
            PreListeningArtist = preListeningArtist;

            Data.RvCritics.Add(this);
        }

        public static RvCritic Get(long userId)
        {
            foreach (RvCritic critic in Data.RvCritics)
                if (critic.UserId == userId)
                    return critic;

            return null;
        }
    }
}
