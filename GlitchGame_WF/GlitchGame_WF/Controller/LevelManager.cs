using System;
using System.Collections.Generic;
using GlitchGame_WF.Models;

namespace GlitchGame_WF.Controller
{
    public class LevelManager
    {
        private readonly List<LevelDefinition> _definitions;

        public int CurrentLevelNumber { get; private set; } = 1;
        public int MaxLevel => _definitions.Count;

        public LevelManager()
        {
            _definitions = BuildDefinitions();
        }

        public Level GetCurrentLevel() => BuildLevel(CurrentLevelNumber);

        public Level GoToNextLevel()
        {
            CurrentLevelNumber = CurrentLevelNumber >= MaxLevel ? 1 : CurrentLevelNumber + 1;
            return BuildLevel(CurrentLevelNumber);
        }

        public Level GoToLevel(int levelNumber)
        {
            if (levelNumber < 1 || levelNumber > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(levelNumber));

            CurrentLevelNumber = levelNumber;
            return BuildLevel(CurrentLevelNumber);
        }

        private Level BuildLevel(int levelNumber)
        {
            var definition = _definitions[levelNumber - 1];
            const float verticalSpacingMultiplier = 1.5f;
            var level = new Level
            {
                GroundY = definition.GroundY,
                StartX = definition.StartX,
                StartY = definition.StartY
            };

            foreach (var p in definition.Platforms)
            {
                int y = p.Y;
                if (y != definition.GroundY)
                {
                    int delta = definition.GroundY - y;
                    y = definition.GroundY - (int)Math.Round(delta * verticalSpacingMultiplier);
                }
                // Не даём платформам уехать за верх экрана после увеличения расстояний
                y = Math.Max(20, y);
                level.Platforms.Add(new Platform(p.X, y, p.Width, p.Height, p.IsPhantom, p.IsCollectible));
            }

            foreach (var c in definition.Coins)
            {
                int y = c.Y;
                if (y != definition.GroundY)
                {
                    int delta = definition.GroundY - y;
                    y = definition.GroundY - (int)Math.Round(delta * verticalSpacingMultiplier);
                }
                // То же для монет (иначе могут оказаться вне видимой области)
                y = Math.Max(20, y);
                level.Coins.Add(new Coin(c.X, y, c.IsFake, c.ActsAsPlatform));
            }

            return level;
        }

        private static List<LevelDefinition> BuildDefinitions()
        {
            return new List<LevelDefinition>
            {
                new LevelDefinition(
                    startX: 50,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(90, 430, 170, 40),
                        new PlatformDef(350, 360, 130, 40),
                        new PlatformDef(560, 300, 140, 40),
                        new PlatformDef(300, 240, 90, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(130, 400),
                        new CoinDef(395, 330),
                        new CoinDef(595, 270)
                    }),

                new LevelDefinition(
                    startX: 35,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(30, 430, 90, 40),
                        new PlatformDef(190, 470, 110, 40),
                        new PlatformDef(330, 410, 100, 40),
                        new PlatformDef(480, 340, 100, 40),
                        new PlatformDef(640, 270, 100, 40),
                        new PlatformDef(760, 200, 80, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(55, 400),
                        new CoinDef(225, 440),
                        new CoinDef(360, 380),
                        new CoinDef(515, 310),
                        new CoinDef(790, 170)
                    }),

                new LevelDefinition(
                    startX: 70,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(90, 440, 90, 40),
                        new PlatformDef(220, 390, 90, 40),
                        new PlatformDef(120, 330, 90, 40),
                        new PlatformDef(300, 280, 90, 40),
                        new PlatformDef(480, 340, 100, 40),
                        new PlatformDef(620, 280, 90, 40),
                        new PlatformDef(740, 220, 90, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(110, 410),
                        new CoinDef(250, 360),
                        new CoinDef(150, 300),
                        new CoinDef(335, 250),
                        new CoinDef(770, 190)
                    }),

                new LevelDefinition(
                    startX: 60,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(140, 450, 60, 40),
                        new PlatformDef(250, 390, 55, 40),
                        new PlatformDef(360, 350, 50, 40),
                        new PlatformDef(470, 320, 50, 40),
                        new PlatformDef(580, 290, 50, 40),
                        new PlatformDef(700, 250, 55, 40),
                        new PlatformDef(780, 210, 45, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(160, 420),
                        new CoinDef(268, 360),
                        new CoinDef(488, 290),
                        new CoinDef(598, 260),
                        new CoinDef(792, 180)
                    }),

                new LevelDefinition(
                    startX: 40,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(90, 440, 130, 40, isPhantom: true),
                        new PlatformDef(260, 380, 120, 40),
                        new PlatformDef(420, 330, 120, 40, isPhantom: true),
                        new PlatformDef(560, 270, 120, 40),
                        new PlatformDef(710, 210, 120, 40, isPhantom: true)
                    },
                    coins: new[]
                    {
                        new CoinDef(125, 410),
                        new CoinDef(295, 350),
                        new CoinDef(455, 300),
                        new CoinDef(595, 240),
                        new CoinDef(745, 180)
                    }),

                new LevelDefinition(
                    startX: 40,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(80, 420, 120, 40),
                        new PlatformDef(260, 360, 100, 40),
                        new PlatformDef(420, 420, 100, 40),
                        new PlatformDef(590, 330, 130, 40),
                        new PlatformDef(740, 260, 90, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(105, 390),
                        new CoinDef(280, 330),
                        new CoinDef(450, 390, isFake: true),
                        new CoinDef(620, 300),
                        new CoinDef(770, 230, isFake: true),
                        new CoinDef(320, 220)
                    }),

                new LevelDefinition(
                    startX: 60,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(130, 460, 120, 40),
                        new PlatformDef(280, 390, 140, 40),
                        new PlatformDef(80, 320, 120, 40),
                        new PlatformDef(260, 260, 90, 40),
                        new PlatformDef(470, 290, 160, 40),
                        new PlatformDef(700, 360, 120, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(165, 430),
                        new CoinDef(320, 360),
                        new CoinDef(120, 290),
                        new CoinDef(500, 260),
                        new CoinDef(730, 330),
                        new CoinDef(300, 230)
                    }),

                new LevelDefinition(
                    startX: 420,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(50, 420, 120, 40),
                        new PlatformDef(710, 420, 120, 40),
                        new PlatformDef(240, 340, 160, 40),
                        new PlatformDef(470, 340, 160, 40),
                        new PlatformDef(350, 240, 180, 40)
                    },
                    coins: new[]
                    {
                        new CoinDef(90, 390),
                        new CoinDef(750, 390),
                        new CoinDef(275, 310),
                        new CoinDef(505, 310),
                        new CoinDef(410, 210)
                    }),

                new LevelDefinition(
                    startX: 50,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40),
                        new PlatformDef(120, 410, 100, 40, isCollectible: true),
                        new PlatformDef(280, 350, 110, 40, isCollectible: true),
                        new PlatformDef(460, 390, 120, 40, isCollectible: true),
                        new PlatformDef(620, 310, 120, 40, isCollectible: true),
                        new PlatformDef(760, 230, 90, 40, isCollectible: true)
                    },
                    coins: new[]
                    {
                        new CoinDef(145, 385, actsAsPlatform: true),
                        new CoinDef(310, 325, actsAsPlatform: true),
                        new CoinDef(495, 365, actsAsPlatform: true),
                        new CoinDef(655, 285, actsAsPlatform: true),
                        new CoinDef(790, 205, actsAsPlatform: true),
                        new CoinDef(380, 250, actsAsPlatform: true)
                    }),

                new LevelDefinition(
                    startX: 426,
                    startY: 440,
                    platforms: new[]
                    {
                        new PlatformDef(-10, 500, 902, 40)
                    },
                    coins: Array.Empty<CoinDef>()
                    )
            };
        }

        private sealed class LevelDefinition
        {
            public int GroundY { get; } = 500;
            public int StartX { get; }
            public int StartY { get; }
            public IReadOnlyList<PlatformDef> Platforms { get; }
            public IReadOnlyList<CoinDef> Coins { get; }

            public LevelDefinition(int startX, int startY, IReadOnlyList<PlatformDef> platforms, IReadOnlyList<CoinDef> coins)
            {
                StartX = startX;
                StartY = startY;
                Platforms = platforms;
                Coins = coins;
            }
        }

        private readonly struct PlatformDef
        {
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }
            public bool IsPhantom { get; }
            public bool IsCollectible { get; }

            public PlatformDef(int x, int y, int width, int height, bool isPhantom = false, bool isCollectible = false)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                IsPhantom = isPhantom;
                IsCollectible = isCollectible;
            }
        }

        private readonly struct CoinDef
        {
            public int X { get; }
            public int Y { get; }
            public bool IsFake { get; }
            public bool ActsAsPlatform { get; }

            public CoinDef(int x, int y, bool isFake = false, bool actsAsPlatform = false)
            {
                X = x;
                Y = y;
                IsFake = isFake;
                ActsAsPlatform = actsAsPlatform;
            }
        }
    }
}
