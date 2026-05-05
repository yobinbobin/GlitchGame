using System.Drawing;

namespace GlitchGame_WF.Models
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityY { get; set; }
        public bool FacingRight { get; private set; } = true;
        
        public int Width { get; } = 45;
        public int Height { get; } = 60;
    
        private const float Gravity = 0.5f;
        private int _groundY = 500;
        public int GroundY
        {
            get => _groundY;
            set => _groundY = value;
        }
        public float VelocityX { get; set; }
        public bool IsGrounded { get; set; }

        private const float BaseMoveSpeed = 5f;
        public void MoveLeft(float speedMultiplier = 1f)
        {
            FacingRight = false;
            X -= BaseMoveSpeed * speedMultiplier;
        }

        public void MoveRight(float speedMultiplier = 1f)
        {
            FacingRight = true;
            X += BaseMoveSpeed * speedMultiplier;
        }
        public int Score { get; set; }

        public Player()
        {
            X = 50;
            Y = _groundY - Height;
        }

        public void Draw(Graphics g, Image? sprite = null, float renderScale = 1f)
        {
            if (sprite is not null)
            {
                int w = (int)(Width * renderScale);
                int h = (int)(Height * renderScale);
                // Якорим спрайт по "ногам" (нижней границе хитбокса),
                // чтобы при увеличении renderScale игрок визуально не "проваливался" в платформы.
                int drawY = (int)(Y - (h - Height));
                int drawX = (int)X;
                if (FacingRight)
                {
                    g.DrawImage(sprite, drawX, drawY, w, h);
                }
                else
                {
                    var oldTransform = g.Transform;
                    try
                    {
                        g.TranslateTransform(drawX + w, drawY);
                        g.ScaleTransform(-1, 1);
                        g.DrawImage(sprite, 0, 0, w, h);
                    }
                    finally
                    {
                        g.Transform = oldTransform;
                    }
                }
                return;
            }

            using var brush = new SolidBrush(Color.Blue);
            g.FillRectangle(brush, (int)X, (int)Y, Width, Height);
        }

        public void ApplyGravity(bool reverseGravity = false)
        {
            VelocityY += reverseGravity ? -Gravity : Gravity;
            Y += VelocityY;
            
            float groundTopY = _groundY - Height;
            if (!reverseGravity && Y > groundTopY)
            {
                Y = groundTopY;
                VelocityY = 0;
                IsGrounded = true;
            }
            else if (reverseGravity && Y < 0)
            {
                Y = 0;
                VelocityY = 0;
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                VelocityY = -13f;
            }
        }

        public void ApplyPlatforms(List<Platform> platforms, bool ignorePhantomPlatforms = false, bool reverseGravity = false)
        {
            foreach(var p in platforms)
            {
                if (p.Collected)
                    continue;
                if (ignorePhantomPlatforms && p.IsPhantom)
                    continue;

                if (!reverseGravity &&
                    VelocityY >= 0 &&
                    X + Width > p.X &&
                    X < p.X + p.Width &&
                    Y + Height > p.Y &&
                    Y + Height < p.Y + p.Height + VelocityY)
                {
                    Y = p.Y - Height;
                    VelocityY = 0;
                    IsGrounded = true;
                }
                if (!reverseGravity &&
                    VelocityY < 0 &&
                    X + Width > p.X && X < p.X + p.Width &&
                    Y < p.Y + p.Height && Y > p.Y)
                {
                    Y = p.Y + p.Height;
                    VelocityY = 0;
                }

                if (reverseGravity &&
                    VelocityY <= 0 &&
                    X + Width > p.X &&
                    X < p.X + p.Width &&
                    Y < p.Y + p.Height &&
                    Y > p.Y + p.Height + VelocityY - Height)
                {
                    Y = p.Y + p.Height;
                    VelocityY = 0;
                    IsGrounded = true;
                }
            }
        }

        public void ApplyCoinPlatforms(List<Coin> coins, bool reverseGravity = false)
        {
            foreach (var c in coins)
            {
                if (!c.ActsAsPlatform || c.Collected)
                    continue;

                if (!reverseGravity &&
                    VelocityY >= 0 &&
                    X + Width > c.X &&
                    X < c.X + c.Size &&
                    Y + Height > c.Y &&
                    Y + Height < c.Y + c.Size + VelocityY)
                {
                    Y = c.Y - Height;
                    VelocityY = 0;
                    IsGrounded = true;
                }
            }
        }

        public void CollectCoins(List<Coin> coins, bool ignoreFakeCoinScore = false)
        {
            foreach (var c in coins)
            {
                if (!c.Collected)
                {
                    if (X < c.X + c.Size && X + Width > c.X &&
                        Y < c.Y + c.Size && Y + Height > c.Y)
                    {
                        c.Collected = true;
                        if (!ignoreFakeCoinScore || !c.IsFake)
                            Score += 10;
                    }
                }
            }
        }

        public void CollectPlatforms(List<Platform> platforms)
        {
            foreach (var p in platforms)
            {
                if (p.Collected || !p.IsCollectible)
                    continue;

                if (X < p.X + p.Width &&
                    X + Width > p.X &&
                    Y < p.Y + p.Height &&
                    Y + Height > p.Y)
                {
                    p.Collected = true;
                    Score += 10;
                }
            }
        }
    }
}