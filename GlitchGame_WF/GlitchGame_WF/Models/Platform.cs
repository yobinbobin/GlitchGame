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

        public void Draw(Graphics g, Image? sprite = null)
        {
            if (Collected)
                return;

            if (sprite is not null)
            {
                g.DrawImage(sprite, X, Y, Width, Height);
                return;
            }

            using var solidBrush = new SolidBrush(Color.Crimson);
            g.FillRectangle(solidBrush, X, Y, Width, Height);
        }
    }
}