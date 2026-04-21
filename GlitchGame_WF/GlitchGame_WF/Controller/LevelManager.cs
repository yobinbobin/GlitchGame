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
            var level = new Level
            {
                GroundY = definition.GroundY,
                StartX = definition.StartX,
                StartY = definition.StartY
            };

            foreach (var p in definition.Platforms)
            {
                level.Platforms.Add(new Platform(p.X, p.Y, p.Width, p.Height, p.IsPhantom));
            }

            foreach (var c in definition.Coins)
            {
                level.Coins.Add(new Coin(c.X, c.Y));
            }

            return level;
        }

        private static List<LevelDefinition> BuildDefinitions()
        {
            return new List<LevelDefinition>
            {
                new LevelDefinition(
                    startX: 50,
                    startY: 460,
                    platforms: new[]
                    {
                        new PlatformDef(0, 500, 882, 20),
                        new PlatformDef(90, 430, 170, 20),
                        new PlatformDef(350, 360, 130, 20),
                        new PlatformDef(560, 300, 140, 20),
                        new PlatformDef(300, 240, 90, 20)
                    },
                    coins: new[]
                    {
                        new CoinDef(130, 400),
                        new CoinDef(395, 330),
                        new CoinDef(595, 270)
                    }),

                new LevelDefinition(
                    startX: 35,
                    startY: 460,
                    platforms: new[]
                    {
                        new PlatformDef(0, 500, 882, 20),
                        new PlatformDef(30, 430, 90, 20),
                        new PlatformDef(190, 470, 110, 20),
                        new PlatformDef(330, 410, 100, 20),
                        new PlatformDef(480, 340, 100, 20),
                        new PlatformDef(640, 270, 100, 20),
                        new PlatformDef(760, 200, 80, 20)
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
                    startY: 460,
                    platforms: new[]
                    {
                        new PlatformDef(0, 500, 882, 20),
                        new PlatformDef(90, 440, 90, 20),
                        new PlatformDef(220, 390, 90, 20),
                        new PlatformDef(120, 330, 90, 20),
                        new PlatformDef(300, 280, 90, 20),
                        new PlatformDef(480, 340, 100, 20),
                        new PlatformDef(620, 280, 90, 20),
                        new PlatformDef(740, 220, 90, 20)
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
                    startY: 460,
                    platforms: new[]
                    {
                        new PlatformDef(0, 500, 882, 20),
                        new PlatformDef(140, 450, 60, 20),
                        new PlatformDef(250, 390, 55, 20),
                        new PlatformDef(360, 350, 50, 20),
                        new PlatformDef(470, 320, 50, 20),
                        new PlatformDef(580, 290, 50, 20),
                        new PlatformDef(700, 250, 55, 20),
                        new PlatformDef(780, 210, 45, 20)
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
                    startY: 460,
                    platforms: new[]
                    {
                        new PlatformDef(0, 500, 882, 20),
                        new PlatformDef(90, 440, 130, 20, isPhantom: true),
                        new PlatformDef(260, 380, 120, 20),
                        new PlatformDef(420, 330, 120, 20, isPhantom: true),
                        new PlatformDef(560, 270, 120, 20),
                        new PlatformDef(710, 210, 120, 20, isPhantom: true)
                    },
                    coins: new[]
                    {
                        new CoinDef(125, 410),
                        new CoinDef(295, 350),
                        new CoinDef(455, 300),
                        new CoinDef(595, 240),
                        new CoinDef(745, 180)
                    })
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

            public PlatformDef(int x, int y, int width, int height, bool isPhantom = false)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                IsPhantom = isPhantom;
            }
        }

        private readonly struct CoinDef
        {
            public int X { get; }
            public int Y { get; }

            public CoinDef(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
