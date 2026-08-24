using System;
using System.Drawing;
using System.Windows.Forms;

namespace DdcCi.BrightnessTray.UI
{
    internal sealed class TrayIconView : IDisposable
    {
        private readonly NotifyIcon _icon;

        public event EventHandler LeftClick;
        public event EventHandler RightClick;

        public TrayIconView()
        {
            _icon = new NotifyIcon
            {
                Visible = true,
                Text = "Brightness"
            };
            _icon.MouseClick += OnMouseClick;
        }

        public void Show(Icon icon, string tooltip)
        {
            _icon.Icon = icon;
            _icon.Text = tooltip != null && tooltip.Length > 63 ? tooltip.Substring(0, 63) : tooltip ?? string.Empty;
        }

        public void ShowBalloon(string title, string message, ToolTipIcon kind)
        {
            _icon.ShowBalloonTip(2000, title, message, kind);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                EventHandler handler = LeftClick;
                if (handler != null) handler(this, EventArgs.Empty);
            }
            else if (e.Button == MouseButtons.Right)
            {
                EventHandler handler = RightClick;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
        }
    }
}
