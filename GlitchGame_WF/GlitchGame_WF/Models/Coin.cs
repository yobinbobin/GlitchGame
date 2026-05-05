// Coin.cs - пустой файл, готов к написанию с нуля
using System.Drawing;

namespace GlitchGame_WF.Models
{
    public class Coin
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Size { get; } = 20;
        public bool Collected { get; set; }
        public bool IsFake { get; }
        public bool ActsAsPlatform { get; }

        public Coin(int x, int y, bool isFake = false, bool actsAsPlatform = false)
        {
            X = x;
            Y = y;
            IsFake = isFake;
            ActsAsPlatform = actsAsPlatform;
        }

        public void Draw(Graphics g, Image? sprite = null, float renderScale = 1f)
        {
            if (Collected)
                return;

            if (sprite is not null)
            {
                int s = (int)(Size * renderScale);
                g.DrawImage(sprite, X, Y, s, s);
                return;
            }

            var color = IsFake ? Color.FromArgb(220, 190, 110) : Color.Goldenrod;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, X, Y, Size, Size);
        }
    }
}