using System.Text;

namespace RightVisionBot.Types
{
    public class Reward
    {
        public Reward(string icon, string description)
        {
            Icon = icon;
            Description = description;
        }

        public string Icon { get; set; }
        public string Description { get; set; }
    }

    public class Rewards
    {
        private long? UserId { get; }
        public Dictionary<int, Reward> Collection = new();


        public Rewards(long? userId = null)
        {
            if (userId != null)
                UserId = userId;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var reward in Collection)
                sb.Append(reward.Value.Icon + ":" + reward.Key + ":" + reward.Value.Description + ";");

            return sb.ToString();
        }

        public int Count => Collection.Count;

        public void Add(Reward reward)
        {
            Collection.Add(Collection.Count + 1, reward);
            if (UserId != null)
                Program.database.Read($"UPDATE RV_Users SET rewards = '{this}' WHERE userId = {UserId};", "");
        }
    }
}
