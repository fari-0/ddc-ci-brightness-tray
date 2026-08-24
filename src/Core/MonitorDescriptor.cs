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
    }
}
