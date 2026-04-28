using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System;
using GlitchGame_WF.Models;

namespace GlitchGame_WF.Controller
{
    public class GameController
    {
        private Player _player = null!;
        private Level _currentLevel = null!;
        private readonly LevelManager _levelManager;
        private readonly List<Glitch> _glitches = new List<Glitch>();
        private readonly Queue<HashSet<Keys>> _inputBuffer = new Queue<HashSet<Keys>>();
        private readonly Queue<PlayerSnapshot> _positionHistory = new Queue<PlayerSnapshot>();
        private SpeechBubble? _speechBubble;
        private DateTime _levelStartedUtc;
        private DateTime _lastTeleportUtc;
        private DateTime _lastCelebrationJumpUtc;
        private bool _isReverseGravityEnabled;
        private bool _wasJumpPressed;

        public int Score => _player.Score;
        public int CurrentLevelNumber => _levelManager.CurrentLevelNumber;
        public float PlayerX => _player.X;
        public float PlayerY => _player.Y;
        public int PlayerWidth => _player.Width;
        public bool IsCelebrationLevel => CurrentLevelNumber == 10;
        public bool IsPhantomCollisionModeEnabled => ShouldIgnorePhantomPlatforms();

        public GameController()
        {
            _levelManager = new LevelManager();
            _glitches.Add(new Glitch("Inverted controls", activationLevel: 2, invertHorizontalInput: true));
            _glitches.Add(new Glitch("Input lag", activationLevel: 3, inputLagFrames: 20));
            _glitches.Add(new Glitch("Hyper speed", activationLevel: 4, moveSpeedMultiplier: 5.6f));
            _glitches.Add(new Glitch("Phantom platforms", activationLevel: 5, ignorePlatformCollisions: true));
            LoadCurrentLevel();
        }

        private void LoadCurrentLevel()
        {
            _currentLevel = _levelManager.GetCurrentLevel();
            ResetPlayerForCurrentLevel();
        }

        public void HandleInput(HashSet<Keys> keys)
        {
            if (IsCelebrationLevel)
                return;

            var effectiveKeys = GetEffectiveInput(keys);
            var invertHorizontalInput = IsHorizontalInputInverted();
            var moveSpeedMultiplier = GetMoveSpeedMultiplier();

            bool moveLeftPressed = effectiveKeys.Contains(Keys.Left) || effectiveKeys.Contains(Keys.A);
            bool moveRightPressed = effectiveKeys.Contains(Keys.Right) || effectiveKeys.Contains(Keys.D);
            bool jumpPressed = effectiveKeys.Contains(Keys.Space) || effectiveKeys.Contains(Keys.Up) || effectiveKeys.Contains(Keys.W);
            bool jumpJustPressed = jumpPressed && !_wasJumpPressed;

            if (invertHorizontalInput)
            {
                (moveLeftPressed, moveRightPressed) = (moveRightPressed, moveLeftPressed);
            }

            if (moveRightPressed)
                _player.MoveRight(moveSpeedMultiplier);
            if (moveLeftPressed)
                _player.MoveLeft(moveSpeedMultiplier);
            if (CurrentLevelNumber == 8 && jumpJustPressed)
            {
                _isReverseGravityEnabled = !_isReverseGravityEnabled;
                if (_isReverseGravityEnabled && _player.VelocityY > 0)
                    _player.VelocityY = 0;
                if (!_isReverseGravityEnabled && _player.VelocityY < 0)
                    _player.VelocityY = 0;
            }
            else if (CurrentLevelNumber != 8 && jumpJustPressed)
            {
                _player.Jump();
            }

            _wasJumpPressed = jumpPressed;
        }

        public void Update()
        {
            if (IsCelebrationLevel)
            {
                HandleCelebrationLevel();
                return;
            }

            _player.ApplyGravity(_isReverseGravityEnabled);

            if (CurrentLevelNumber == 9)
            {
                _player.ApplyPlatforms(_currentLevel.Platforms, ignorePhantomPlatforms: false, reverseGravity: _isReverseGravityEnabled);
                _player.ApplyCoinPlatforms(_currentLevel.Coins, reverseGravity: _isReverseGravityEnabled);
                _player.CollectPlatforms(_currentLevel.Platforms);
            }
            else
            {
                _player.ApplyPlatforms(_currentLevel.Platforms, ShouldIgnorePhantomPlatforms(), _isReverseGravityEnabled);
                _player.CollectCoins(_currentLevel.Coins, ignoreFakeCoinScore: CurrentLevelNumber == 6);
            }

            HandleTeleportGlitch();
        }

        public void Draw(Graphics g)
        {
            foreach (var platform in _currentLevel.Platforms)
                platform.Draw(g);

            foreach (var coin in _currentLevel.Coins)
                coin.Draw(g);

            _player.Draw(g);

            using var font = new Font("Arial", 16);
            using var brush = new SolidBrush(Color.White);
            g.DrawString("Счёт: " + _player.Score, font, brush, 10, 10);
        }

        public string? GetCharacterSpeech()
        {
            if (_speechBubble is null || !_speechBubble.IsVisible(DateTime.UtcNow))
                return null;

            return _speechBubble.Text;
        }
        
        public bool CheckWin()
        {
            if (IsCelebrationLevel)
                return false;

            foreach (var coin in _currentLevel.Coins)
            {
                if (CurrentLevelNumber == 9 && coin.ActsAsPlatform)
                    continue;
                if (!coin.Collected)
                    return false;
            }

            if (CurrentLevelNumber == 9)
            {
                foreach (var platform in _currentLevel.Platforms.Where(p => p.IsCollectible))
                {
                    if (!platform.Collected)
                        return false;
                }
            }

            return true;
        }

        public void NextLevel()
        {
            _currentLevel = _levelManager.GoToNextLevel();
            ResetPlayerForCurrentLevel();
        }

        public void RestartCurrentLevel()
        {
            _currentLevel = _levelManager.GetCurrentLevel();
            ResetPlayerForCurrentLevel();
        }

        public void StartFromFirstLevel()
        {
            _currentLevel = _levelManager.GoToLevel(1);
            ResetPlayerForCurrentLevel();
        }

        private bool IsHorizontalInputInverted()
        {
            return _glitches.Any(g => g.IsActive(CurrentLevelNumber) && g.InvertHorizontalInput);
        }

        private int GetInputLagFrames()
        {
            return _glitches
                .Where(g => g.IsActive(CurrentLevelNumber))
                .Select(g => g.InputLagFrames)
                .DefaultIfEmpty(0)
                .Max();
        }

        private float GetMoveSpeedMultiplier()
        {
            return _glitches
                .Where(g => g.IsActive(CurrentLevelNumber))
                .Select(g => g.MoveSpeedMultiplier)
                .DefaultIfEmpty(1f)
                .Max();
        }

        private bool ShouldIgnorePhantomPlatforms()
        {
            return _glitches.Any(g => g.IsActive(CurrentLevelNumber) && g.IgnorePlatformCollisions);
        }

        private HashSet<Keys> GetEffectiveInput(HashSet<Keys> currentKeys)
        {
            var lagFrames = GetInputLagFrames();
            if (lagFrames <= 0)
                return currentKeys;

            _inputBuffer.Enqueue(new HashSet<Keys>(currentKeys));
            if (_inputBuffer.Count <= lagFrames)
                return new HashSet<Keys>();

            return _inputBuffer.Dequeue();
        }

        private void ResetPlayerForCurrentLevel()
        {
            _player = new Player
            {
                X = _currentLevel.StartX,
                Y = _currentLevel.StartY
            };
            _inputBuffer.Clear();
            _positionHistory.Clear();
            _isReverseGravityEnabled = false;
            _wasJumpPressed = false;
            _levelStartedUtc = DateTime.UtcNow;
            _lastTeleportUtc = _levelStartedUtc;
            _lastCelebrationJumpUtc = _levelStartedUtc;
            _speechBubble = BuildSpeechBubbleForLevel(CurrentLevelNumber, _levelStartedUtc);
        }

        private void HandleTeleportGlitch()
        {
            if (CurrentLevelNumber != 7)
                return;

            var now = DateTime.UtcNow;
            _positionHistory.Enqueue(new PlayerSnapshot(_player.X, _player.Y, now));

            while (_positionHistory.Count > 0 && now - _positionHistory.Peek().TimestampUtc > TimeSpan.FromSeconds(6))
            {
                _positionHistory.Dequeue();
            }

            if (now - _lastTeleportUtc < TimeSpan.FromSeconds(5))
                return;

            var rewindTarget = _positionHistory.FirstOrDefault(s => now - s.TimestampUtc >= TimeSpan.FromSeconds(3));
            if (rewindTarget.TimestampUtc != default)
            {
                _player.X = rewindTarget.X;
                _player.Y = rewindTarget.Y;
                _player.VelocityY = 0;
            }

            _lastTeleportUtc = now;
        }

        private void HandleCelebrationLevel()
        {
            _player.X = 426;
            _player.ApplyGravity();
            _player.ApplyPlatforms(_currentLevel.Platforms);

            var now = DateTime.UtcNow;
            if (now - _lastCelebrationJumpUtc >= TimeSpan.FromSeconds(0.85))
            {
                _player.Jump();
                _lastCelebrationJumpUtc = now;
            }
        }

        private static SpeechBubble? BuildSpeechBubbleForLevel(int levelNumber, DateTime levelStartUtc)
        {
            string? text = levelNumber switch
            {
                1 => "Мне нужно собрать все монетки, иначе моим разработчикам будет не на что жить",
                2 => "Странно... управление перевернули.\nНаверное, тестировщика уволили, и никто не заметил.",
                3 => "Лаги?.. Раньше такого не было.\nПохоже, оптимизацию тоже вырезали.\nОдин программист, значит, уже ушел.",
                4 => "Слишком быстро... это даже не настраивали.\nОни просто выкрутили скорость, чтобы \"казалось динамичнее\"?\nПохоже, балансить уже некому.",
                5 => "Платформы исчезают...\nЭто не баг - это незаконченные элементы.\nХудожника тоже убрали?..",
                6 => "Я собираю монетки... но счет не растет.\nОни больше ничего не приносят.\nЗначит... донаты отключили?",
                7 => "Меня откатывает назад...\nОни используют старый код?\nПохоже, новых обновлений уже не будет.",
                8 => "Физика сломалась...\nОни даже не пытаются это чинить.\nПросто оставили как есть.",
                9 => "Монетки стали платформами...\nОни переиспользуют все подряд.\nНовые ассеты... закончились.",
                10 => "Ура, теперь эта игра проживет еще пару месяцев.",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(text))
                return null;

            return new SpeechBubble(text, levelStartUtc, TimeSpan.FromSeconds(5));
        }

        private readonly struct PlayerSnapshot
        {
            public float X { get; }
            public float Y { get; }
            public DateTime TimestampUtc { get; }

            public PlayerSnapshot(float x, float y, DateTime timestampUtc)
            {
                X = x;
                Y = y;
                TimestampUtc = timestampUtc;
            }
        }
    }
}
