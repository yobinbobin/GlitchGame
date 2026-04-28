using System.Drawing;

namespace GlitchGame_WF.Models
{
    public class Platform
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPhantom { get; set; }
        public bool IsCollectible { get; set; }
        public bool Collected { get; set; }

        public Platform(int x, int y, int width, int height, bool isPhantom = false, bool isCollectible = false)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsPhantom = isPhantom;
            IsCollectible = isCollectible;
        }

        public void Draw(Graphics g)
        {
            if (Collected)
                return;

            using var brush = new SolidBrush(Color.Crimson);
            g.FillRectangle(brush, X, Y, Width, Height);
        }
    }
}