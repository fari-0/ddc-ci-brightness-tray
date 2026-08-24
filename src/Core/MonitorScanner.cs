using System;
using System.Collections.Generic;
using DdcCi.BrightnessTray.Infrastructure;

namespace DdcCi.BrightnessTray.Core
{
    public sealed class MonitorScanner
    {
        public IList<IMonitorBrightness> Scan()
        {
            List<IMonitorBrightness> found = new List<IMonitorBrightness>();
            NativeMethods.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                delegate(IntPtr hMonitor, IntPtr hdc, ref NativeMethods.RECT rect, IntPtr data)
                {
                    CollectFrom(hMonitor, found);
                    return true;
                },
                IntPtr.Zero);
            return found;
        }

        private static void CollectFrom(IntPtr hMonitor, ICollection<IMonitorBrightness> sink)
        {
            uint count = 0;
            if (!NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) || count == 0) return;

            NativeMethods.PHYSICAL_MONITOR[] monitors = new NativeMethods.PHYSICAL_MONITOR[count];
            if (!NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, monitors)) return;

            foreach (NativeMethods.PHYSICAL_MONITOR pm in monitors)
            {
                if (pm.Handle == IntPtr.Zero) continue;

                uint type = 0;
                uint current;
                uint maximum;
                bool supportsLuminance = NativeMethods.GetVCPFeatureAndVCPFeatureReply(
                    pm.Handle, NativeMethods.VcpLuminance, ref type, out current, out maximum);

                if (!supportsLuminance || maximum == 0)
                {
                    ReleaseUnsupported(pm);
                    continue;
                }

                sink.Add(new DdcCiMonitor(pm.Handle, Normalize(pm.Description), maximum));
            }
        }

        private static void ReleaseUnsupported(NativeMethods.PHYSICAL_MONITOR pm)
        {
            NativeMethods.PHYSICAL_MONITOR[] arr = new NativeMethods.PHYSICAL_MONITOR[1];
            arr[0] = pm;
            NativeMethods.DestroyPhysicalMonitors(1, arr);
        }

        private static string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Display";
            string trimmed = raw.Trim('\0').Trim();
            return trimmed.Length == 0 ? "Display" : trimmed;
        }
    }
}
