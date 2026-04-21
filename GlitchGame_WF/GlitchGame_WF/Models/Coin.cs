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

        public Coin(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(Graphics g)
        {
            if (Collected)
                return;
            using var brush = new SolidBrush(Color.Goldenrod);
            g.FillEllipse(brush, X, Y, Size, Size);
        }
    }
}