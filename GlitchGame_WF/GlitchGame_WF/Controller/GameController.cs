using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

        public int Score => _player.Score;
        public int CurrentLevelNumber => _levelManager.CurrentLevelNumber;
        public float PlayerX => _player.X;
        public float PlayerY => _player.Y;
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
            var effectiveKeys = GetEffectiveInput(keys);
            var invertHorizontalInput = IsHorizontalInputInverted();
            var moveSpeedMultiplier = GetMoveSpeedMultiplier();

            bool moveLeftPressed = effectiveKeys.Contains(Keys.Left) || effectiveKeys.Contains(Keys.A);
            bool moveRightPressed = effectiveKeys.Contains(Keys.Right) || effectiveKeys.Contains(Keys.D);

            if (invertHorizontalInput)
            {
                (moveLeftPressed, moveRightPressed) = (moveRightPressed, moveLeftPressed);
            }

            if (moveRightPressed)
                _player.MoveRight(moveSpeedMultiplier);
            if (moveLeftPressed)
                _player.MoveLeft(moveSpeedMultiplier);
            if (effectiveKeys.Contains(Keys.Space) || effectiveKeys.Contains(Keys.Up) || effectiveKeys.Contains(Keys.W))
                _player.Jump();
        }

        public void Update()
        {
            _player.ApplyGravity();
            _player.ApplyPlatforms(_currentLevel.Platforms, ShouldIgnorePhantomPlatforms());
            _player.CollectCoins(_currentLevel.Coins);
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
        
        public bool CheckWin()
        {
            foreach (var coin in _currentLevel.Coins)
            {
                if (!coin.Collected)
                    return false;
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
        }
    }
}
