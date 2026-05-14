using System;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    interface IReadonlyBandwidthStats
    {
        float MinBandwidth { get; }
        float MaxBandwidth { get; }

        event Action OnBandwidthStatsUpdated;
    }

    class BandwidthStats : IReadonlyBandwidthStats
    {
        /// <summary>
        /// The minimum bandwidth value used for the purpose of auto-scaling
        /// </summary>
        public const int k_MinBandwidth = 0;

        /// <summary>
        /// Yes, the name is a bit confusing -- this is the minimum value that the
        /// max value can be.
        /// </summary>
        public const int k_MinimumMaxValue = 1;

        public float MinBandwidth => k_MinBandwidth;

        float m_MaxBandwidth = k_MinimumMaxValue;
        public float MaxBandwidth
        {
            get => Math.Max(m_MaxBandwidth, k_MinimumMaxValue);
            set
            {
                var valueChanged = m_MaxBandwidth != value;
                m_MaxBandwidth = value;
                if (valueChanged)
                {
                    OnBandwidthStatsUpdated?.Invoke();
                }
            }
        }

        public event Action OnBandwidthStatsUpdated;
    }
}
