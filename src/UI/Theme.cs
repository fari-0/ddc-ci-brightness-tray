using System.Drawing;

namespace DdcCi.BrightnessTray.UI
{
    internal static class Theme
    {
        public static readonly Color DefaultAccent = Color.FromArgb(255, 179, 0);

        public static readonly Color[] Presets = new Color[]
        {
            Color.FromArgb(255, 179, 0),
            Color.FromArgb(0, 120, 215),
            Color.FromArgb(16, 124, 16),
            Color.FromArgb(136, 86, 207),
        };

        private static Color _accent = DefaultAccent;

        public static Color Accent
        {
            get { return _accent; }
            set { _accent = value; }
        }

        public static Color Darken(Color color, float factor)
        {
            if (factor < 0f) factor = 0f;
            if (factor > 1f) factor = 1f;
            return Color.FromArgb(
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor));
        }

        public static Color IconCore(Color accent, float intensity)
        {
            if (intensity < 0f) intensity = 0f;
            if (intensity > 1f) intensity = 1f;
            Color dark = Color.FromArgb(105, 105, 110);
            Color light = Mix(accent, Color.White, 0.2f);
            return Color.FromArgb(
                dark.R + (int)((light.R - dark.R) * intensity),
                dark.G + (int)((light.G - dark.G) * intensity),
                dark.B + (int)((light.B - dark.B) * intensity));
        }

        private static Color Mix(Color first, Color second, float amount)
        {
            if (amount < 0f) amount = 0f;
            if (amount > 1f) amount = 1f;
            return Color.FromArgb(
                first.R + (int)((second.R - first.R) * amount),
                first.G + (int)((second.G - first.G) * amount),
                first.B + (int)((second.B - first.B) * amount));
        }
    }
}
