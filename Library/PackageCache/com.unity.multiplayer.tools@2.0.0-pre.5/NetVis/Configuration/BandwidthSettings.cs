using System.Linq;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    class BandwidthSettings
    {
        static readonly MeshShadingGradientPreset k_FirstGradientPreset =
            EnumUtil.GetValuesAndNames(skip: MeshShadingGradientPreset.None).First().value;

        public bool MeshShadingEnabled { get; set; } = true;
        public bool TextOverlayEnabled { get; set; } = true;

        // Smoothing
        // --------------------------------------------------------------------
        public float SmoothingHalfLife = 1;

        // Filtering
        // --------------------------------------------------------------------
        public BandwidthTypes BandwidthType = BandwidthTypes.All;
        public NetworkDirection NetworkDirection = NetworkDirection.SentAndReceived;

        // Mesh shading
        // --------------------------------------------------------------------
        public MeshShadingGradient MeshShadingFill = new MeshShadingGradient
        {
            Preset = k_FirstGradientPreset,
            Gradient = k_FirstGradientPreset.ToGradient(),
        };
        public bool BandwidthAutoscaling = true;

        public int BandwidthMin = 0;
        public int BandwidthMax = 512;

        /// <summary>
        /// BandwidthMax modified to avoid division by zero
        /// </summary>
        /// <remarks>
        /// This behaviour is documented for the user in the help box text in BandwidthConfigurationView.cs
        /// </remarks>
        public int BandwidthMaxSafe => BandwidthMin == BandwidthMax
            ? BandwidthMin + 1
            : BandwidthMax;
    }
}
