using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Rating
{
    [Serializable]
    internal class Track
    {
        public Track(long userId, string category, string track)
        {
            UserId = userId;
            Category = category;
            UserTrack = track;
        }

        public long UserId { get; set; }
        public string Category { get; set; }
        public string UserTrack { get; set; }
        public List<Critic> Critics { get; set; } = new();
    }
}
