using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DdcCi.BrightnessTray.UI
{
    public sealed class BrightnessSlider : Control
    {
        private const int TrackThickness = 4;
        private const int ThumbDiameter = 16;
        private const int HorizontalPad = 14;

        private static readonly Color AmberFill = Color.FromArgb(255, 179, 0);
        private static readonly Color TrackRest = Color.FromArgb(122, 122, 130);
        private static readonly Color DisabledFill = Color.FromArgb(178, 178, 182);
        private static readonly Color DisabledTrack = Color.FromArgb(206, 206, 211);
        private static readonly Color ThumbBorder = Color.FromArgb(158, 158, 166);

        private bool _dragging;
        private int _value;

        public event EventHandler<int> ValueChanged;

        public int Maximum { get; set; }
        public int SmallStep { get; set; }

        public int Value
        {
            get { return _value; }
            set { SetCore(Clamp(value), false); }
        }

        public BrightnessSlider()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            Maximum = 100;
            SmallStep = 1;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool active = Enabled && Maximum > 0;
            Color fillColor = active ? AmberFill : DisabledFill;
            Color restColor = active ? TrackRest : DisabledTrack;

            float cy = Height / 2f;
            RectangleF track = new RectangleF(
                HorizontalPad,
                cy - TrackThickness / 2f,
                Width - HorizontalPad * 2f,
                TrackThickness);

            using (GraphicsPath path = Rounded(track, TrackThickness / 2f))
            using (SolidBrush restBrush = new SolidBrush(restColor))
                g.FillPath(restBrush, path);

            if (_value > 0 && Maximum > 0)
            {
                float ratio = (float)_value / Maximum;
                float fillWidth = Math.Max(track.Width * ratio, track.Height);
                g.SetClip(new RectangleF(track.X, track.Y - 4, fillWidth, track.Height + 8));
                using (GraphicsPath path = Rounded(track, TrackThickness / 2f))
                using (SolidBrush fillBrush = new SolidBrush(fillColor))
                    g.FillPath(fillBrush, path);
                g.ResetClip();
            }

            if (Maximum > 0)
            {
                float ratio = (float)_value / Maximum;
                float thumbX = track.X + ThumbDiameter / 2f + (track.Width - ThumbDiameter) * ratio;
                RectangleF thumb = new RectangleF(
                    thumbX - ThumbDiameter / 2f,
                    cy - ThumbDiameter / 2f,
                    ThumbDiameter,
                    ThumbDiameter);
                using (SolidBrush thumbBrush = new SolidBrush(Color.White))
                    g.FillEllipse(thumbBrush, thumb);
                using (Pen borderPen = new Pen(active ? ThumbBorder : DisabledTrack))
                    g.DrawEllipse(borderPen, thumb);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left) return;
            _dragging = true;
            Capture = true;
            ApplyMouse(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_dragging || !Enabled) return;
            ApplyMouse(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!Enabled || Maximum <= 0) return;

            HandledMouseEventArgs handled = e as HandledMouseEventArgs;
            if (handled != null) handled.Handled = true;

            SetCore(Clamp(Value + Math.Sign(e.Delta) * SmallStep), true);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private void ApplyMouse(int mouseX)
        {
            if (Maximum <= 0) return;
            float trackLeft = HorizontalPad + ThumbDiameter / 2f;
            float span = Width - HorizontalPad * 2f - ThumbDiameter;
            if (span <= 0) return;
            float ratio = (mouseX - trackLeft) / span;
            if (ratio < 0f) ratio = 0f;
            if (ratio > 1f) ratio = 1f;
            SetCore((int)Math.Round(ratio * Maximum), true);
        }

        private int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > Maximum) return Maximum;
            return v;
        }

        private void SetCore(int value, bool raise)
        {
            if (value == _value) return;
            _value = value;
            Invalidate();
            EventHandler<int> handler = ValueChanged;
            if (raise && handler != null) handler(this, value);
        }

        private static GraphicsPath Rounded(RectangleF r, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
