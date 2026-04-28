using System;

namespace GlitchGame_WF.Models
{
    public class SpeechBubble
    {
        public string Text { get; }
        public DateTime ShowFromUtc { get; }
        public TimeSpan Duration { get; }

        public SpeechBubble(string text, DateTime showFromUtc, TimeSpan duration)
        {
            Text = text;
            ShowFromUtc = showFromUtc;
            Duration = duration;
        }

        public bool IsVisible(DateTime utcNow)
        {
            return utcNow >= ShowFromUtc && utcNow <= ShowFromUtc + Duration;
        }
    }
}
