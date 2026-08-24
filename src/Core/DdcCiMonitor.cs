using System;
using System.Threading;
using DdcCi.BrightnessTray.Infrastructure;

namespace DdcCi.BrightnessTray.Core
{
    public sealed class DdcCiMonitor : IMonitorBrightness
    {
        private readonly object _gate = new object();
        private IntPtr _handle;

        public MonitorDescriptor Descriptor { get; private set; }

        public DdcCiMonitor(IntPtr physicalHandle, string name, uint maximum)
        {
            _handle = physicalHandle;
            Descriptor = new MonitorDescriptor(name, maximum);
        }

        public bool TryGetBrightness(out uint current)
        {
            lock (_gate)
            {
                current = 0;
                if (_handle == IntPtr.Zero) return false;
                uint type = 0;
                uint maximum;
                return NativeMethods.GetVCPFeatureAndVCPFeatureReply(
                    _handle, NativeMethods.VcpLuminance, ref type, out current, out maximum);
            }
        }

        public bool TrySetBrightness(uint value)
        {
            lock (_gate)
            {
                if (_handle == IntPtr.Zero) return false;
                if (value > Descriptor.Maximum) value = Descriptor.Maximum;
                return NativeMethods.SetVCPFeature(_handle, NativeMethods.VcpLuminance, value);
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_handle == IntPtr.Zero) return;
                NativeMethods.PHYSICAL_MONITOR[] arr = new NativeMethods.PHYSICAL_MONITOR[1];
                arr[0].Handle = _handle;
                arr[0].Description = Descriptor.Name;
                NativeMethods.DestroyPhysicalMonitors(1, arr);
                _handle = IntPtr.Zero;
            }
        }
    }
}
