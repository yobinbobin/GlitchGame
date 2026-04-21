using System;

namespace GlitchGame_WF.Models
{
    public class Glitch
    {
        public string Name { get; }
        public int ActivationLevel { get; }
        public bool InvertHorizontalInput { get; }
        public int InputLagFrames { get; }
        public float MoveSpeedMultiplier { get; }
        public bool IgnorePlatformCollisions { get; }

        public Glitch(
            string name,
            int activationLevel,
            bool invertHorizontalInput = false,
            int inputLagFrames = 0,
            float moveSpeedMultiplier = 1f,
            bool ignorePlatformCollisions = false)
        {
            Name = name;
            ActivationLevel = activationLevel;
            InvertHorizontalInput = invertHorizontalInput;
            InputLagFrames = Math.Max(0, inputLagFrames);
            MoveSpeedMultiplier = Math.Max(0.1f, moveSpeedMultiplier);
            IgnorePlatformCollisions = ignorePlatformCollisions;
        }

        public bool IsActive(int currentLevel) => currentLevel == ActivationLevel;
    }
}