using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DdcCi.BrightnessTray.Core;

namespace DdcCi.BrightnessTray.UI
{
    public sealed class SliderChangedEventArgs : EventArgs
    {
        public MonitorDescriptor Monitor { get; private set; }
        public int Value { get; private set; }

        public SliderChangedEventArgs(MonitorDescriptor monitor, int value)
        {
            Monitor = monitor;
            Value = value;
        }
    }

    internal sealed class FlyoutForm : Form
    {
        private const int PanelWidth = 320;
        private const int Pad = 14;
        private const int NameHeight = 22;
        private const int SliderHeight = 32;
        private const int RowGap = 6;
        private const int FooterHeight = 30;

        private readonly LinkLabel _startupLink;
        private readonly LinkLabel _exitLink;
        private readonly List<RowBinding> _rows = new List<RowBinding>();

        public event EventHandler<SliderChangedEventArgs> SliderChanged;
        public event EventHandler ExitRequested;
        public event EventHandler StartupToggleRequested;

        private sealed class RowBinding
        {
            public MonitorDescriptor Descriptor;
            public Label NameLabel;
            public Label PercentLabel;
            public BrightnessSlider Slider;
        }

        public FlyoutForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(243, 243, 246);
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);

            _startupLink = CreateLink("Başlangıçta çalıştır: -", 176);
            _startupLink.Click += delegate
            {
                EventHandler handler = StartupToggleRequested;
                if (handler != null) handler(this, EventArgs.Empty);
            };

            _exitLink = CreateLink("Çıkış", 44);
            _exitLink.Click += delegate
            {
                EventHandler handler = ExitRequested;
                if (handler != null) handler(this, EventArgs.Empty);
            };

            Controls.Add(_startupLink);
            Controls.Add(_exitLink);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        public void Bind(IList<MonitorDescriptor> descriptors)
        {
            SuspendLayout();

            foreach (RowBinding row in _rows)
            {
                Controls.Remove(row.NameLabel);
                Controls.Remove(row.PercentLabel);
                Controls.Remove(row.Slider);
                row.NameLabel.Dispose();
                row.PercentLabel.Dispose();
                row.Slider.Dispose();
            }
            _rows.Clear();

            int innerWidth = PanelWidth - Pad * 2;
            int y = Pad;

            foreach (MonitorDescriptor descriptor in descriptors)
            {
                RowBinding row = new RowBinding();
                row.Descriptor = descriptor;

                row.NameLabel = new Label
                {
                    Text = descriptor.Name,
                    AutoSize = false,
                    Size = new Size(innerWidth - 48, NameHeight),
                    Location = new Point(Pad, y),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(30, 30, 34)
                };
                row.NameLabel.Font = new Font(Font, FontStyle.Bold);

                row.PercentLabel = new Label
                {
                    Text = "—",
                    AutoSize = false,
                    Size = new Size(46, NameHeight),
                    Location = new Point(Pad + innerWidth - 46, y),
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.FromArgb(90, 90, 98)
                };

                row.Slider = new BrightnessSlider
                {
                    Maximum = (int)Math.Max(1, descriptor.Maximum),
                    Location = new Point(Pad - 6, y + NameHeight - 4),
                    Size = new Size(innerWidth + 12, SliderHeight),
                    BackColor = BackColor,
                    Enabled = false
                };
                row.Slider.ValueChanged += delegate(object s, int v)
                {
                    UpdatePercentText(row);
                    EventHandler<SliderChangedEventArgs> handler = SliderChanged;
                    if (handler != null) handler(this, new SliderChangedEventArgs(row.Descriptor, v));
                };

                Controls.Add(row.NameLabel);
                Controls.Add(row.PercentLabel);
                Controls.Add(row.Slider);
                _rows.Add(row);

                y += NameHeight - 4 + SliderHeight + RowGap;
            }

            ClientSize = new Size(PanelWidth, y + FooterHeight + 4);

            _startupLink.Location = new Point(
                ClientSize.Width - Pad - _exitLink.Width - 10 - _startupLink.Width,
                ClientSize.Height - FooterHeight + (FooterHeight - _startupLink.Height) / 2);
            _exitLink.Location = new Point(
                ClientSize.Width - Pad - _exitLink.Width,
                ClientSize.Height - FooterHeight + (FooterHeight - _exitLink.Height) / 2);

            ResumeLayout(true);
            Invalidate();
        }

        public void UpdateValue(MonitorDescriptor descriptor, uint rawValue)
        {
            foreach (RowBinding row in _rows)
            {
                if (!ReferenceEquals(row.Descriptor, descriptor)) continue;
                BrightnessSlider slider = row.Slider;
                int clamped = Math.Min(slider.Maximum, (int)Math.Max(0, rawValue));
                slider.Value = clamped;
                UpdatePercentText(row);
                return;
            }
        }

        public void SetInteractive(bool interactive)
        {
            foreach (RowBinding row in _rows)
                row.Slider.Enabled = interactive;
        }

        public void SetStartupState(bool enabled)
        {
            _startupLink.Text = enabled ? "Başlangıçta çalıştır: Açık" : "Başlangıçta çalıştır: Kapalı";
        }

        public void PositionNearCursor()
        {
            Rectangle workingArea = Screen.GetWorkingArea(Cursor.Position);
            int x = Cursor.Position.X - Width / 2;
            if (x < workingArea.Left + 4) x = workingArea.Left + 4;
            if (x + Width > workingArea.Right - 4) x = workingArea.Right - Width - 4;

            int y = Cursor.Position.Y - Height - 12;
            if (y < workingArea.Top) y = Cursor.Position.Y + 12;

            Location = new Point(x, y);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Hide();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Hide();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen borderPen = new Pen(Color.FromArgb(196, 196, 204)))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                e.Graphics.DrawLine(
                    borderPen,
                    8, ClientSize.Height - FooterHeight,
                    ClientSize.Width - 9, ClientSize.Height - FooterHeight);
            }
        }

        private static void UpdatePercentText(RowBinding row)
        {
            int max = Math.Max(1, row.Slider.Maximum);
            int pct = row.Slider.Value * 100 / max;
            row.PercentLabel.Text = pct + "%";
        }

        private static LinkLabel CreateLink(string text, int width)
        {
            return new LinkLabel
            {
                AutoSize = false,
                Size = new Size(width, 18),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = Color.FromArgb(176, 116, 0),
                ActiveLinkColor = Color.FromArgb(130, 84, 0),
                ForeColor = Color.FromArgb(90, 90, 98)
            };
        }
    }
}
