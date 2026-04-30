using System;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    static class NetStatsMonitorConfigurationExtensions
    {

        /// Returns a hash of display element fields in the configuration related to history requirements,
        /// for the purpose of determining if the history requirements have changed and need to be updated
        internal static int GetHistoryRequirementsHash(this NetStatsMonitorConfiguration config)
        {
            int hashCode = 0;
            foreach (var displayElement in config.DisplayElements)
            {
                hashCode = HashCode.Combine(hashCode, displayElement.GetHistoryRequirementsHash());
            }
            return hashCode;
        }
    }
}
