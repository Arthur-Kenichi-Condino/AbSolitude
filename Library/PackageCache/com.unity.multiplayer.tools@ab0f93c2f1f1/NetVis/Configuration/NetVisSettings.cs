namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    class NetVisSettings
    {
        public NetVisCommonSettings Common { get; } = new();
        public BandwidthSettings Bandwidth { get; } = new();
        public OwnershipSettings Ownership { get; } = new();
    }
}
