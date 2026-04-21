using System.Collections.Generic;

namespace GlitchGame_WF.Models
{
    public class Level
    {
        public List<Platform> Platforms { get; set; } = new List<Platform>();
        public List<Coin> Coins { get; set; } = new List<Coin>();
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int GroundY { get; set; }
    }
}