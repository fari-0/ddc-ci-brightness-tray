namespace DdcCi.BrightnessTray.Core
{
    public sealed class MonitorDescriptor
    {
        public string Name { get; private set; }
        public uint Maximum { get; private set; }

        public MonitorDescriptor(string name, uint maximum)
        {
            Name = name;
            Maximum = maximum;
        }

        public static int ToPercent(int value, long maximum)
        {
            long max = maximum < 1 ? 1 : maximum;
            int pct = (int)(value * 100L / max);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            return pct;
        }
    }
}
