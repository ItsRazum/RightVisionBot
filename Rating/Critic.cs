using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Rating
{
    [Serializable]
    internal class Critic
    {
        public Critic(long userId, string category)
        {
            UserId = userId;
            Category = category;
        }

        public long UserId { get; set; }
        public string Category { get; set; }
    }
}
