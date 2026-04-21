using System.Drawing;

namespace GlitchGame_WF.Models
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityY { get; set; }
        
        public int Width { get; } = 30;
        public int Height { get; } = 40;
    
        private const float Gravity = 0.5f;
        private int _groundY = 500;
        public float VelocityX { get; set; }
        public bool IsGrounded { get; set; }

        private const float BaseMoveSpeed = 5f;
        public void MoveLeft(float speedMultiplier = 1f) => X -= BaseMoveSpeed * speedMultiplier;
        public void MoveRight(float speedMultiplier = 1f) => X += BaseMoveSpeed * speedMultiplier;
        public int Score { get; set; }

        public Player()
        {
            X = 50;
            Y = _groundY - Height;
        }

        public void Draw(Graphics g)
        {
            using var brush = new SolidBrush(Color.Blue);
            g.FillRectangle(brush, (int)X, (int)Y, Width, Height);
        }

        public void ApplyGravity()
        {
            VelocityY += Gravity;
            Y += VelocityY;
            
            // Упёрся в пол
            if (Y > _groundY)
            {
                Y = _groundY;
                VelocityY = 0;
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;  // в воздухе, нельзя прыгать
            }
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                VelocityY = -12f;
            }
        }

        public void ApplyPlatforms(List<Platform> platforms, bool ignorePhantomPlatforms = false)
        {
            foreach(var p in platforms)
            {
                if (ignorePhantomPlatforms && p.IsPhantom)
                    continue;

                if (VelocityY >= 0 && 
                X + Width > p.X &&
                X < p.X + p.Width &&
                Y + Height > p.Y && 
                Y + Height < p.Y + p.Height + VelocityY)
                {
                    Y = p.Y - Height;
                    VelocityY = 0;
                    IsGrounded = true;
                }
                if (VelocityY < 0 &&
                    X + Width > p.X && X < p.X + p.Width &&
                    Y < p.Y + p.Height && Y > p.Y)
                {
                    Y = p.Y + p.Height;
                    VelocityY = 0;
                }
            }
        }

        public void CollectCoins(List<Coin> coins)
        {
            foreach (var c in coins)
            {
                if (!c.Collected)
                {
                    // Проверка пересечения прямоугольников
                    if (X < c.X + c.Size && X + Width > c.X &&
                        Y < c.Y + c.Size && Y + Height > c.Y)
                    {
                        c.Collected = true;
                        Score += 10;
                    }
                }
            }
        }
    }
}