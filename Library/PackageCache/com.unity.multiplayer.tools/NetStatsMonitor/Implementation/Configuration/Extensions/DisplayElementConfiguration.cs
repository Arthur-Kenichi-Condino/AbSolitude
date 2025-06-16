using System;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    static class DisplayElementConfigurationExtensions
    {
        /// Returns a hash of display element configuration fields related to history requirements,
        /// for the purpose of determining if history requirements have changed and need to be updated
        internal static int GetHistoryRequirementsHash(this DisplayElementConfiguration config)
        {
            var hashCode = 0;
            foreach (var metricId in config.Stats)
            {
                hashCode = HashCode.Combine(hashCode, metricId.GetHashCode());
            }
            hashCode = HashCode.Combine(hashCode, config.SampleCount, config.SampleRate, config.HalfLife);
            return hashCode;
        }
    }
}
