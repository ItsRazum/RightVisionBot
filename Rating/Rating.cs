using RightVisionBot.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Rating
{
    [Serializable]
    internal class Rating
    {
        public List<Category> Categories { get; set; } = new();
    }
}
