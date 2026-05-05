namespace GlitchGame_WF.Models
{
    public class RenderDegradation
    {
        public int PixelScale { get; }
        public int NoiseDots { get; }
        public int ScanlineAlpha { get; }
        public int JitterPixels { get; }
        public int ChannelShiftPixels { get; }
        public float DesaturationAmount { get; }

        public RenderDegradation(
            int pixelScale,
            int noiseDots,
            int scanlineAlpha,
            int jitterPixels,
            int channelShiftPixels,
            float desaturationAmount)
        {
            PixelScale = pixelScale;
            NoiseDots = noiseDots;
            ScanlineAlpha = scanlineAlpha;
            JitterPixels = jitterPixels;
            ChannelShiftPixels = channelShiftPixels;
            DesaturationAmount = desaturationAmount;
        }
    }
}
