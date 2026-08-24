using System;

namespace DdcCi.BrightnessTray.Core
{
    public interface IMonitorBrightness : IDisposable
    {
        MonitorDescriptor Descriptor { get; }
        bool TryGetBrightness(out uint current);
        bool TrySetBrightness(uint value);
    }
}
