using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DdcCi.BrightnessTray.Core
{
    public sealed class BrightnessService : IDisposable
    {
        private const int DebounceMs = 120;

        private readonly object _gate = new object();
        private readonly Func<IList<IMonitorBrightness>> _scannerFactory;
        private readonly Dictionary<IMonitorBrightness, CancellationTokenSource> _pendingSets =
            new Dictionary<IMonitorBrightness, CancellationTokenSource>();
        private IList<IMonitorBrightness> _monitors;
        private bool _enabled = true;

        public BrightnessService(Func<IList<IMonitorBrightness>> scannerFactory)
        {
            _scannerFactory = scannerFactory;
            try { _monitors = scannerFactory(); }
            catch { _monitors = new List<IMonitorBrightness>(); }
        }

        public bool Enabled
        {
            get { lock (_gate) return _enabled; }
            set
            {
                List<CancellationTokenSource> toCancel = null;
                lock (_gate)
                {
                    if (_enabled == value) return;
                    _enabled = value;
                    if (!value)
                    {
                        toCancel = new List<CancellationTokenSource>(_pendingSets.Values);
                        _pendingSets.Clear();
                    }
                }
                if (toCancel != null)
                    foreach (CancellationTokenSource cts in toCancel) cts.Cancel();
            }
        }

        public IList<IMonitorBrightness> Snapshot()
        {
            lock (_gate) return new List<IMonitorBrightness>(_monitors);
        }

        public void Rescan()
        {
            IList<IMonitorBrightness> fresh;
            try { fresh = _scannerFactory(); }
            catch { fresh = new List<IMonitorBrightness>(); }

            IList<IMonitorBrightness> old;
            lock (_gate)
            {
                old = _monitors;
                _monitors = fresh;
                foreach (CancellationTokenSource cts in _pendingSets.Values) cts.Cancel();
                _pendingSets.Clear();
            }
            DisposeAll(old);
        }

        public bool TryRead(IMonitorBrightness monitor, out uint current)
        {
            return monitor.TryGetBrightness(out current);
        }

        public void RequestSet(IMonitorBrightness monitor, uint value)
        {
            CancellationTokenSource cts;
            lock (_gate)
            {
                if (!_enabled)
                {
                    return;
                }
                CancellationTokenSource previous;
                if (_pendingSets.TryGetValue(monitor, out previous)) previous.Cancel();
                cts = new CancellationTokenSource();
                _pendingSets[monitor] = cts;
            }

            CancellationToken token = cts.Token;
            Task.Delay(DebounceMs, token).ContinueWith(delegate(Task t)
            {
                try
                {
                    if (t.IsCanceled || token.IsCancellationRequested) return;
                    monitor.TrySetBrightness(value);
                }
                finally
                {
                    lock (_gate)
                    {
                        CancellationTokenSource current;
                        if (_pendingSets.TryGetValue(monitor, out current) && ReferenceEquals(current, cts))
                            _pendingSets.Remove(monitor);
                    }
                    cts.Dispose();
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public void Dispose()
        {
            IList<IMonitorBrightness> all;
            lock (_gate)
            {
                all = _monitors;
                _monitors = new List<IMonitorBrightness>();
                foreach (CancellationTokenSource cts in _pendingSets.Values) cts.Cancel();
                _pendingSets.Clear();
            }
            DisposeAll(all);
        }

        private static void DisposeAll(IList<IMonitorBrightness> list)
        {
            foreach (IMonitorBrightness m in list) m.Dispose();
        }
    }
}
