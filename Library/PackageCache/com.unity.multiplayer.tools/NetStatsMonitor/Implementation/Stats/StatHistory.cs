using System.Linq;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    // TODO, consider: can historical values always be stored as floats for display, or does this need to be generic?

    /// A class for storing an RNSM stat over multiple frames,
    /// to facilitate moving averages and graphs over time
    class StatHistory
    {
        /// Array of exponential moving averages that are being maintained, can be empty
        public ContinuousExponentialMovingAverage[] ContinuousExponentialMovingAverages { get; private set; }

        /// A ring buffer of recent values for each sample rate.
        /// Entries may be null or empty if not required.
        public EnumMap<SampleRate, RingBuffer<float>> SampleBuffers { get; } = new();

        public StatHistory(StatHistoryRequirements requirements)
        {
            ContinuousExponentialMovingAverages = requirements
                .DecayConstants
                .Select(decayConstant => new ContinuousExponentialMovingAverage(decayConstant))
                .ToArray();
            for (var rate = SampleRates.k_First; rate <= SampleRates.k_Last; rate = rate.Next())
            {
                SampleBuffers[rate] = new RingBuffer<float>(requirements.SampleCounts[rate]);
            }
        }

        static EnumMap<SampleRate, int> GetSampleBufferCapacities(
            EnumMap<SampleRate, RingBuffer<float>> sampleBuffers)
        {
            var requirements = new EnumMap<SampleRate, int>();
            for (var rate = SampleRates.k_First; rate <= SampleRates.k_Last; rate = rate.Next())
            {
                requirements[rate] = sampleBuffers[rate].Capacity;
            }
            return requirements;
        }

        StatHistoryRequirements GetRequirements()
        {
            // It's possible to infer the requirements from the kind of data we're storing
            return new StatHistoryRequirements(
                ContinuousExponentialMovingAverages.Select(cema => cema.DecayConstant),
                GetSampleBufferCapacities(SampleBuffers));
        }

        /// Can be called to update the requirements, while preserving all existing
        /// data that is still required. In the event that the new requirements are
        /// the same as the old, this function will preserve all data.
        internal void UpdateRequirements(StatHistoryRequirements requirements)
        {
            // Writing to the capacity resizes the ring buffer if needed,
            // while preserving all values that are still within capacity
            for (var rate = SampleRates.k_First; rate <= SampleRates.k_Last; rate = rate.Next())
            {
                SampleBuffers[rate].Capacity = requirements.SampleCounts[rate];
            }

            var existingAverages = ContinuousExponentialMovingAverages;
            ContinuousExponentialMovingAverages = requirements
                .DecayConstants
                .Select(decayConstant =>
                {
                    var existingCema = existingAverages
                        .FirstOrDefault(existingCema => existingCema.DecayConstant == decayConstant);
                    return existingCema != null
                        ? new ContinuousExponentialMovingAverage(
                            decayConstant: decayConstant,
                            value: existingCema.LastValue,
                            time: existingCema.LastTime)
                        : new ContinuousExponentialMovingAverage(decayConstant);
                })
                .ToArray();
        }

        public void Update(MetricId metric, SampleRate rate, float value, double time)
        {
            SampleBuffers[rate].PushBack(value);
            if (rate == SampleRate.PerFrame)
            {
                // EMA counters only receive per-frame values
                switch (metric.MetricKind)
                {
                    case MetricKind.Counter:
                    {
                        foreach (var cema in ContinuousExponentialMovingAverages)
                        {
                            cema.AddSampleForCounter(value, time);
                        }
                        break;
                    }
                    case MetricKind.Gauge:
                    {
                        foreach (var cema in ContinuousExponentialMovingAverages)
                        {
                            cema.AddSampleForGauge(value, time);
                        }
                        break;
                    }
                }
            }
        }
    }
}
