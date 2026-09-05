using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using DdcCi.BrightnessTray.Infrastructure;

namespace DdcCi.BrightnessTray.UI
{
    internal static class IconPainter
    {
        private const int Size = 16;

        public static Icon Paint(int? percent, bool enabled)
        {
            int pct = Math.Max(0, Math.Min(100, percent ?? 0));
            float intensity = pct / 100f;

            using (Bitmap bmp = new Bitmap(Size, Size))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color rim = enabled ? Color.FromArgb(45, 45, 48) : Color.FromArgb(152, 152, 158);
                Color core = enabled
                    ? Color.FromArgb(
                        105 + (int)(150 * intensity),
                        105 + (int)(100 * intensity),
                        110 - (int)(50 * intensity))
                    : Color.FromArgb(172, 172, 178);

                DrawSun(g, rim, core, intensity);
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    using (Icon tmp = Icon.FromHandle(hIcon))
                        return (Icon)tmp.Clone();
                }
                finally
                {
                    NativeMethods.DestroyIcon(hIcon);
                }
            }
        }

        private static void DrawSun(Graphics g, Color rim, Color core, float intensity)
        {
            float cx = Size / 2f;
            float cy = Size / 2f;
            float coreRadius = 3.2f;
            float rayStart = 4.2f;
            float rayLength = 1.2f + 2.0f * intensity;

            using (Pen pen = new Pen(rim, 1.4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                for (int i = 0; i < 8; i++)
                {
                    double angle = i * Math.PI / 4.0;
                    float x1 = cx + (float)Math.Cos(angle) * rayStart;
                    float y1 = cy + (float)Math.Sin(angle) * rayStart;
                    float x2 = cx + (float)Math.Cos(angle) * (rayStart + rayLength);
                    float y2 = cy + (float)Math.Sin(angle) * (rayStart + rayLength);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }

                using (SolidBrush brush = new SolidBrush(core))
                    g.FillEllipse(brush, cx - coreRadius, cy - coreRadius, coreRadius * 2, coreRadius * 2);
                g.DrawEllipse(pen, cx - coreRadius, cy - coreRadius, coreRadius * 2, coreRadius * 2);
            }
        }
    }
}
