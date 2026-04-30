using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    /// Combined history storage requirements for multiple stats across all RNSM display elements.
    internal class MultiStatHistoryRequirements
    {
        [NotNull]
        internal Dictionary<MetricId, StatHistoryRequirements> Data { get; } = new();

        internal static MultiStatHistoryRequirements FromConfiguration(NetStatsMonitorConfiguration configuration)
        {
            MultiStatHistoryRequirements multiStatHistoryRequirements = new();
            if (configuration == null)
            {
                return multiStatHistoryRequirements;
            }

            var allStatRequirements = multiStatHistoryRequirements.Data;
            foreach (var displayElement in configuration.DisplayElements)
            {
                var sampleCount = displayElement.SampleCount;
                var sampleRate = displayElement.SampleRate;
                var decayConstant = displayElement.DecayConstant;
                foreach (var metricId in displayElement.Stats)
                {
                    if (!allStatRequirements.ContainsKey(metricId))
                    {
                        allStatRequirements[metricId] = new StatHistoryRequirements(
                            decayConstants: new HashSet<double>(),
                            sampleCounts: new());
                    }
                    var requirements = multiStatHistoryRequirements.Data[metricId];

                    requirements.SampleCounts[sampleRate] = Math.Max(
                        requirements.SampleCounts[sampleRate],
                        sampleCount);

                    if (decayConstant.HasValue)
                    {
                        requirements.DecayConstants.Add(decayConstant.Value);
                    }
                }
            }
            return multiStatHistoryRequirements;
        }
    }
}
