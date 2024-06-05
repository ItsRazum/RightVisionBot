using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Rating
{
    [Serializable]
    internal class Category
    {
        public List<Track> Tracks { get; set; } = new();
    }
}
