using System.Collections.Generic;
using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using GlitchGame_WF.Models;

namespace GlitchGame_WF
{
    public partial class Form1 : Form
    {
        private enum GameUiState
        {
            StartScreen,
            Playing,
            Paused
        }

        private Controller.GameController _gameController;
        private HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private Timer _timer;
        private GameUiState _uiState = GameUiState.StartScreen;
        private Rectangle _startButtonBounds = Rectangle.Empty;
        private Rectangle _restartButtonBounds = Rectangle.Empty;
        private Rectangle _celebrationRestartBounds = Rectangle.Empty;
        private readonly Random _fxRandom = new Random();

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;

            _gameController = new Controller.GameController();

            _timer = new Timer { Interval = 16 };
            _timer.Tick += GameLoop;
            _timer.Start();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (_uiState != GameUiState.Playing)
            {
                Invalidate();
                return;
            }

            _gameController.HandleInput(_pressedKeys);
            _gameController.Update();

            if (_gameController.CheckWin())
            {
                _gameController.NextLevel();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(Color.Black);

            if (_uiState == GameUiState.StartScreen)
            {
                DrawStartScreen(e.Graphics);
                return;
            }

            using var sceneBuffer = new Bitmap(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));
            using (var sceneGraphics = Graphics.FromImage(sceneBuffer))
            {
                sceneGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                sceneGraphics.Clear(Color.Black);
                DrawScene(sceneGraphics);
            }

            // Визуальные пост-эффекты "глюков" временно отключены.
            e.Graphics.DrawImage(sceneBuffer, 0, 0, sceneBuffer.Width, sceneBuffer.Height);
        }

        private void DrawScene(Graphics g)
        {
            DrawLevelBackground(g);

            if (_gameController.IsCelebrationLevel)
            {
                DrawFireworksBackground(g);
                _gameController.Draw(g);
                DrawCelebrationReplayButton(g);
            }
            else
            {
                _gameController.Draw(g);
            }
            DrawMovementHints(g);
            DrawCharacterSpeech(g);

            if (_uiState == GameUiState.Paused)
            {
                DrawPauseMenu(g);
            }
        }

        private void DrawLevelBackground(Graphics g)
        {
            var background = _gameController.BackgroundSprite;
            if (background is null)
                return;

            g.DrawImage(background, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_uiState == GameUiState.StartScreen && _startButtonBounds.Contains(e.Location))
            {
                StartNewGame();
                return;
            }

            if (_uiState == GameUiState.Paused && _restartButtonBounds.Contains(e.Location))
            {
                _gameController.RestartCurrentLevel();
                _pressedKeys.Clear();
                _uiState = GameUiState.Playing;
                Invalidate();
                return;
            }

            if (_uiState == GameUiState.Playing &&
                _gameController.IsCelebrationLevel &&
                _celebrationRestartBounds.Contains(e.Location))
            {
                _gameController.StartFromFirstLevel();
                _pressedKeys.Clear();
                _uiState = GameUiState.Playing;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                TogglePause();
                return;
            }

            if (_uiState == GameUiState.StartScreen && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                StartNewGame();
                return;
            }

            if (_uiState != GameUiState.Playing)
                return;

            _pressedKeys.Add(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _pressedKeys.Remove(e.KeyCode);
        }

        private void StartNewGame()
        {
            _gameController.StartFromFirstLevel();
            _pressedKeys.Clear();
            _uiState = GameUiState.Playing;
            Invalidate();
        }

        private void TogglePause()
        {
            if (_uiState == GameUiState.StartScreen)
                return;

            if (_uiState == GameUiState.Playing)
            {
                _pressedKeys.Clear();
                _uiState = GameUiState.Paused;
            }
            else if (_uiState == GameUiState.Paused)
            {
                _uiState = GameUiState.Playing;
            }

            Invalidate();
        }

        private void DrawMovementHints(Graphics g)
        {
            const int keySize = 34;
            const int spacing = 8;
            const int margin = 10;

            int panelWidth = keySize * 3 + spacing * 2;
            int panelX = ClientSize.Width - panelWidth - margin;
            int topY = margin;

            DrawArrowKey(g, panelX + keySize + spacing, topY, keySize, "↑", IsUpPressed());
            DrawArrowKey(g, panelX, topY + keySize + spacing, keySize, "←", IsLeftPressed());
            DrawArrowKey(g, panelX + keySize + spacing, topY + keySize + spacing, keySize, "↓", IsDownPressed());
            DrawArrowKey(g, panelX + (keySize + spacing) * 2, topY + keySize + spacing, keySize, "→", IsRightPressed());
        }

        private void DrawArrowKey(Graphics g, int x, int y, int size, string arrow, bool isPressed)
        {
            var fillColor = isPressed ? Color.LimeGreen : Color.FromArgb(45, 45, 45);
            var borderColor = isPressed ? Color.White : Color.Gray;
            var textColor = isPressed ? Color.Black : Color.WhiteSmoke;

            using var fillBrush = new SolidBrush(fillColor);
            using var borderPen = new Pen(borderColor, 2);
            using var textBrush = new SolidBrush(textColor);
            using var font = new Font("Arial", 15, FontStyle.Bold);

            var rect = new Rectangle(x, y, size, size);
            g.FillRectangle(fillBrush, rect);
            g.DrawRectangle(borderPen, rect);

            var textSize = g.MeasureString(arrow, font);
            float textX = x + (size - textSize.Width) / 2f;
            float textY = y + (size - textSize.Height) / 2f - 1f;
            g.DrawString(arrow, font, textBrush, textX, textY);
        }

        private bool IsLeftPressed() => _pressedKeys.Contains(Keys.Left) || _pressedKeys.Contains(Keys.A);
        private bool IsRightPressed() => _pressedKeys.Contains(Keys.Right) || _pressedKeys.Contains(Keys.D);
        private bool IsUpPressed() => _pressedKeys.Contains(Keys.Up) || _pressedKeys.Contains(Keys.W) || _pressedKeys.Contains(Keys.Space);
        private bool IsDownPressed() => _pressedKeys.Contains(Keys.Down) || _pressedKeys.Contains(Keys.S);

        private void DrawCharacterSpeech(Graphics g)
        {
            var speech = _gameController.GetCharacterSpeech();
            if (string.IsNullOrWhiteSpace(speech))
                return;

            using var speechFont = new Font("Arial", 10, FontStyle.Regular);
            using var textBrush = new SolidBrush(Color.Black);
            using var bubbleBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255));
            using var borderPen = new Pen(Color.FromArgb(30, 30, 30), 1.6f);

            const int bubbleWidth = 330;
            const int bubblePadding = 10;

            int playerCenterX = (int)(_gameController.PlayerX + _gameController.PlayerWidth / 2f);
            int bubbleX = Math.Max(8, Math.Min(ClientSize.Width - bubbleWidth - 8, playerCenterX - bubbleWidth / 2));
            int maxTextWidth = bubbleWidth - bubblePadding * 2;
            var textSize = g.MeasureString(speech, speechFont, maxTextWidth);

            int bubbleHeight = (int)Math.Ceiling(textSize.Height) + bubblePadding * 2;
            int bubbleY = (int)_gameController.PlayerY - bubbleHeight - 20;
            bubbleY = Math.Max(8, bubbleY);

            var bubbleRect = new Rectangle(bubbleX, bubbleY, bubbleWidth, bubbleHeight);
            g.FillRectangle(bubbleBrush, bubbleRect);
            g.DrawRectangle(borderPen, bubbleRect);

            var textRect = new RectangleF(
                bubbleRect.X + bubblePadding,
                bubbleRect.Y + bubblePadding,
                maxTextWidth,
                bubbleRect.Height - bubblePadding * 2);
            g.DrawString(speech, speechFont, textBrush, textRect);

            int tailBaseX = Math.Max(bubbleRect.Left + 12, Math.Min(bubbleRect.Right - 12, playerCenterX));
            int tailY = bubbleRect.Bottom;
            Point[] tailPoints =
            {
                new Point(tailBaseX - 8, tailY),
                new Point(tailBaseX + 8, tailY),
                new Point(playerCenterX, (int)_gameController.PlayerY - 2)
            };
            g.FillPolygon(bubbleBrush, tailPoints);
            g.DrawPolygon(borderPen, tailPoints);
        }

        private void DrawFireworksBackground(Graphics g)
        {
            var now = DateTime.UtcNow;
            int seed = now.Second * 1000 + now.Millisecond / 20;
            var rand = new Random(seed);

            for (int i = 0; i < 6; i++)
            {
                int cx = rand.Next(40, Math.Max(41, ClientSize.Width - 40));
                int cy = rand.Next(30, Math.Max(31, ClientSize.Height / 2));
                int radius = rand.Next(20, 58);
                var color = Color.FromArgb(
                    170,
                    rand.Next(80, 256),
                    rand.Next(80, 256),
                    rand.Next(80, 256));

                using var pen = new Pen(color, 2f);
                for (int ray = 0; ray < 12; ray++)
                {
                    double angle = ray * Math.PI / 6.0;
                    int x2 = cx + (int)(Math.Cos(angle) * radius);
                    int y2 = cy + (int)(Math.Sin(angle) * radius);
                    g.DrawLine(pen, cx, cy, x2, y2);
                }
            }
        }

        private void DrawCelebrationReplayButton(Graphics g)
        {
            _celebrationRestartBounds = new Rectangle(ClientSize.Width - 190, ClientSize.Height - 60, 170, 36);
            using var fillBrush = new SolidBrush(Color.FromArgb(190, 30, 95, 160));
            using var borderPen = new Pen(Color.WhiteSmoke, 1.4f);
            using var font = new Font("Arial", 10, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);

            g.FillRectangle(fillBrush, _celebrationRestartBounds);
            g.DrawRectangle(borderPen, _celebrationRestartBounds);

            const string text = "Сыграть заново";
            var textSize = g.MeasureString(text, font);
            float x = _celebrationRestartBounds.X + (_celebrationRestartBounds.Width - textSize.Width) / 2f;
            float y = _celebrationRestartBounds.Y + (_celebrationRestartBounds.Height - textSize.Height) / 2f;
            g.DrawString(text, font, textBrush, x, y);
        }

        private Bitmap ApplyRenderDegradation(Bitmap source, RenderDegradation degradation)
        {
            var working = new Bitmap(source.Width, source.Height);
            using (var g = Graphics.FromImage(working))
            {
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            if (degradation.PixelScale > 1)
            {
                int lowW = Math.Max(1, source.Width / degradation.PixelScale);
                int lowH = Math.Max(1, source.Height / degradation.PixelScale);

                using var lowRes = new Bitmap(lowW, lowH);
                using (var gLow = Graphics.FromImage(lowRes))
                {
                    gLow.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    gLow.DrawImage(working, 0, 0, lowW, lowH);
                }

                using var repixelated = new Bitmap(source.Width, source.Height);
                using (var gUp = Graphics.FromImage(repixelated))
                {
                    gUp.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gUp.PixelOffsetMode = PixelOffsetMode.Half;
                    gUp.DrawImage(lowRes, 0, 0, source.Width, source.Height);
                }

                working.Dispose();
                working = new Bitmap(repixelated);
            }

            ApplyColorDegrade(working, degradation.DesaturationAmount);
            DrawScanlines(working, degradation.ScanlineAlpha);
            DrawNoise(working, degradation.NoiseDots);
            ApplyChannelShift(working, degradation.ChannelShiftPixels);

            return working;
        }

        private static void ApplyColorDegrade(Bitmap image, float desaturationAmount)
        {
            desaturationAmount = Math.Max(0f, Math.Min(1f, desaturationAmount));
            float rw = 0.3086f;
            float gw = 0.6094f;
            float bw = 0.0820f;
            float s = 1f - desaturationAmount;

            var matrix = new ColorMatrix(new[]
            {
                new[] { rw + (1f - rw) * s, rw * (1f - s), rw * (1f - s), 0f, 0f },
                new[] { gw * (1f - s), gw + (1f - gw) * s, gw * (1f - s), 0f, 0f },
                new[] { bw * (1f - s), bw * (1f - s), bw + (1f - bw) * s, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { -0.02f, -0.02f, -0.02f, 0f, 1f }
            });

            using var temp = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(temp))
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(matrix);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            using var gWrite = Graphics.FromImage(image);
            gWrite.DrawImage(temp, 0, 0);
        }

        private void DrawScanlines(Bitmap image, int alpha)
        {
            if (alpha <= 0)
                return;

            using var g = Graphics.FromImage(image);
            using var pen = new Pen(Color.FromArgb(Math.Min(alpha, 140), 0, 0, 0), 1f);
            for (int y = 0; y < image.Height; y += 2)
            {
                g.DrawLine(pen, 0, y, image.Width, y);
            }
        }

        private void DrawNoise(Bitmap image, int dots)
        {
            if (dots <= 0)
                return;

            using var g = Graphics.FromImage(image);
            for (int i = 0; i < dots; i++)
            {
                int x = _fxRandom.Next(image.Width);
                int y = _fxRandom.Next(image.Height);
                int size = _fxRandom.Next(1, 3);
                int a = _fxRandom.Next(40, 130);
                var c = _fxRandom.NextDouble() > 0.5
                    ? Color.FromArgb(a, 255, 255, 255)
                    : Color.FromArgb(a, 0, 0, 0);
                using var brush = new SolidBrush(c);
                g.FillRectangle(brush, x, y, size, size);
            }
        }

        private static void ApplyChannelShift(Bitmap image, int shift)
        {
            if (shift <= 0)
                return;

            using var source = new Bitmap(image);
            using var g = Graphics.FromImage(image);
            g.Clear(Color.Black);

            using var redAttr = BuildTintAttributes(1f, 0f, 0f, 0.28f);
            using var blueAttr = BuildTintAttributes(0f, 0f, 1f, 0.24f);
            g.DrawImage(source, new Rectangle(-shift, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, redAttr);
            g.DrawImage(source, new Rectangle(shift, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, blueAttr);
            g.DrawImage(source, 0, 0, image.Width, image.Height);
        }

        private static ImageAttributes BuildTintAttributes(float r, float g, float b, float alpha)
        {
            var matrix = new ColorMatrix(new[]
            {
                new[] { r, 0f, 0f, 0f, 0f },
                new[] { 0f, g, 0f, 0f, 0f },
                new[] { 0f, 0f, b, 0f, 0f },
                new[] { 0f, 0f, 0f, alpha, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix);
            return attributes;
        }

        private void DrawStartScreen(Graphics g)
        {
            using var titleFont = new Font("Arial", 30, FontStyle.Bold);
            using var textFont = new Font("Arial", 14, FontStyle.Regular);
            using var brush = new SolidBrush(Color.White);
            using var hintBrush = new SolidBrush(Color.LightGray);

            var title = "GLITCH PLATFORMER";
            var subtitle = "Собери все монеты и переживи глюки уровней";
            var titleSize = g.MeasureString(title, titleFont);
            var subtitleSize = g.MeasureString(subtitle, textFont);

            float centerX = ClientSize.Width / 2f;
            g.DrawString(title, titleFont, brush, centerX - titleSize.Width / 2f, 140);
            g.DrawString(subtitle, textFont, hintBrush, centerX - subtitleSize.Width / 2f, 200);

            _startButtonBounds = new Rectangle((int)centerX - 110, 280, 220, 56);
            DrawMenuButton(g, _startButtonBounds, "Спасти игру");

            var controlsHint = "Enter/Space - начать, Esc - пауза во время игры";
            var controlsSize = g.MeasureString(controlsHint, textFont);
            g.DrawString(controlsHint, textFont, hintBrush, centerX - controlsSize.Width / 2f, 370);
        }

        private void DrawPauseMenu(Graphics g)
        {
            using var overlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            g.FillRectangle(overlayBrush, ClientRectangle);

            using var titleFont = new Font("Arial", 24, FontStyle.Bold);
            using var hintFont = new Font("Arial", 12, FontStyle.Regular);
            using var titleBrush = new SolidBrush(Color.White);
            using var hintBrush = new SolidBrush(Color.LightGray);

            var title = "Пауза";
            var titleSize = g.MeasureString(title, titleFont);
            float centerX = ClientSize.Width / 2f;
            float topY = 180;
            g.DrawString(title, titleFont, titleBrush, centerX - titleSize.Width / 2f, topY);

            _restartButtonBounds = new Rectangle((int)centerX - 135, (int)topY + 70, 270, 56);
            DrawMenuButton(g, _restartButtonBounds, "Начать уровень заново");

            var hint = "Esc - продолжить";
            var hintSize = g.MeasureString(hint, hintFont);
            g.DrawString(hint, hintFont, hintBrush, centerX - hintSize.Width / 2f, topY + 145);
        }

        private void DrawMenuButton(Graphics g, Rectangle bounds, string text)
        {
            using var fillBrush = new SolidBrush(Color.FromArgb(35, 120, 200));
            using var borderPen = new Pen(Color.White, 2f);
            using var font = new Font("Arial", 13, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);

            g.FillRectangle(fillBrush, bounds);
            g.DrawRectangle(borderPen, bounds);
            
            var textSize = g.MeasureString(text, font);
            float x = bounds.X + (bounds.Width - textSize.Width) / 2f;
            float y = bounds.Y + (bounds.Height - textSize.Height) / 2f;
            g.DrawString(text, font, textBrush, x, y);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
