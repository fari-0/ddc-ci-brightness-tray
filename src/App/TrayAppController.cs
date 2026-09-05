using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DdcCi.BrightnessTray.Core;
using DdcCi.BrightnessTray.UI;

namespace DdcCi.BrightnessTray.App
{
    internal sealed class TrayAppController : ApplicationContext
    {
        private readonly BrightnessService _service;
        private readonly TrayIconView _tray;
        private readonly FlyoutForm _flyout;

        private readonly EventHandler _displayChangedHandler;
        private bool _enabled = true;
        private int _lastKnownPercent = -1;
        private bool _cleanedUp;
        private int _rescanGeneration;

        public TrayAppController(BrightnessService service)
        {
            _service = service;

            _tray = new TrayIconView();
            _tray.LeftClick += OnTrayLeftClick;
            _tray.RightClick += OnTrayRightClick;

            _flyout = new FlyoutForm();
            _flyout.SliderChanged += OnSliderChanged;
            _flyout.ExitRequested += delegate { ExitApplication(); };
            _flyout.StartupToggleRequested += delegate
            {
                StartupManager.Toggle();
                _flyout.SetStartupState(StartupManager.IsEnabled());
            };

            _displayChangedHandler = delegate { RefreshMonitors(); };
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += _displayChangedHandler;

            RefreshMonitors();
            UpdateTrayVisual();
        }

        private void OnTrayLeftClick(object sender, EventArgs e)
        {
            _enabled = !_enabled;
            _service.Enabled = _enabled;
            if (_flyout.Visible) _flyout.SetInteractive(_enabled);
            UpdateTrayVisual();
        }

        private void OnTrayRightClick(object sender, EventArgs e)
        {
            RefreshMonitors();
            if (_service.Snapshot().Count == 0)
            {
                _tray.ShowBalloon("Brightness Tray", "No external monitor supporting DDC/CI was found.", ToolTipIcon.Warning);
                return;
            }

            _flyout.SetInteractive(_enabled);
            _flyout.SetStartupState(StartupManager.IsEnabled());
            _flyout.PositionNearCursor();
            _flyout.Show();
            _flyout.Activate();
        }

        private void OnSliderChanged(object sender, SliderChangedEventArgs e)
        {
            IMonitorBrightness monitor = FindMonitor(e.Monitor);
            if (monitor != null)
                _service.RequestSet(monitor, (uint)Math.Max(0, e.Value));

            _lastKnownPercent = ScalePercent(e.Monitor, e.Value);
            UpdateTrayVisual();
        }

        private void RefreshMonitors()
        {
            _service.Rescan();
            RebindFlyout();
        }

        private void RebindFlyout()
        {
            IList<IMonitorBrightness> monitors = _service.Snapshot();
            List<MonitorDescriptor> descriptors = monitors.Select(m => m.Descriptor).ToList();
            _flyout.Bind(descriptors);

            int generation = System.Threading.Interlocked.Increment(ref _rescanGeneration);
            List<IMonitorBrightness> copy = new List<IMonitorBrightness>(monitors);
            Task.Factory.StartNew(delegate
            {
                foreach (IMonitorBrightness m in copy)
                {
                    if (generation != _rescanGeneration) return;
                    IMonitorBrightness captured = m;
                    uint value;
                    if (!_service.TryRead(captured, out value)) continue;
                    if (generation != _rescanGeneration) return;
                    uint v = value;
                    RunOnUi(delegate
                    {
                        if (generation != _rescanGeneration) return;
                        _flyout.UpdateValue(captured.Descriptor, v);
                        _lastKnownPercent = ScalePercent(captured.Descriptor, (int)v);
                        UpdateTrayVisual();
                    });
                }
            });
        }

        private IMonitorBrightness FindMonitor(MonitorDescriptor descriptor)
        {
            if (descriptor == null) return null;
            foreach (IMonitorBrightness m in _service.Snapshot())
            {
                MonitorDescriptor d = m.Descriptor;
                if (d != null && d.Name == descriptor.Name && d.Maximum == descriptor.Maximum) return m;
            }
            return null;
        }

        private void RunOnUi(Action action)
        {
            if (_cleanedUp || _flyout.IsDisposed || !_flyout.IsHandleCreated) return;
            try { _flyout.BeginInvoke(action); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void UpdateTrayVisual()
        {
            int? shown = _lastKnownPercent >= 0 ? (int?)_lastKnownPercent : null;
            string tooltip = _enabled
                ? (_lastKnownPercent >= 0 ? "Brightness Tray: " + _lastKnownPercent + "%" : "Brightness Tray")
                : "Brightness Tray (paused)";
            _tray.Show(IconPainter.Paint(shown, _enabled), tooltip);
        }

        private static int ScalePercent(MonitorDescriptor descriptor, int rawValue)
        {
            return MonitorDescriptor.ToPercent(rawValue, descriptor.Maximum);
        }

        private void ExitApplication()
        {
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            Cleanup();
            base.Dispose(disposing);
        }

        private void Cleanup()
        {
            if (_cleanedUp) return;
            _cleanedUp = true;

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= _displayChangedHandler;
            _tray.Dispose();
            _flyout.Dispose();
            _service.Dispose();
        }
    }
}
