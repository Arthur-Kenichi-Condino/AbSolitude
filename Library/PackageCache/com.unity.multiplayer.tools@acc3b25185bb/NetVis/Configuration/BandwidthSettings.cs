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

        /// <summary>
        /// When activating the bandwidth visualization when the game is paused, there is no data.
        /// In such case, we want to display a warning to the user.
        /// Argument: true if no data warning should be displayed, false otherwise.
        /// </summary>
        public event System.Action<bool> OnNoDataWarningChanged;

        bool m_HasNoData;

        public bool HasNoData
        {
            get => m_HasNoData;
            set
            {
                if (m_HasNoData != value)
                {
                    m_HasNoData = value;
                    OnNoDataWarningChanged?.Invoke(m_HasNoData);
                }
            }
        }

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
