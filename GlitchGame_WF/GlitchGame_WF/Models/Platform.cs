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

        public Platform(int x, int y, int width, int height, bool isPhantom = false)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsPhantom = isPhantom;
        }

        public void Draw(Graphics g)
        {
            using var brush = new SolidBrush(IsPhantom ? Color.FromArgb(130, 180, 180, 180) : Color.Crimson);
            g.FillRectangle(brush, X, Y, Width, Height);
        }
    }
}