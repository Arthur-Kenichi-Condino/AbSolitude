#if UNITY_MP_TOOLS_DEV
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Unity.Multiplayer.Tools.Common
{
    class MultiSampleTimer
    {
        const double k_Milli = 1000;
        static readonly double k_SecondsPerTick = 1d / Stopwatch.Frequency;
        static readonly double k_MilliSecondsPerTick = k_Milli * k_SecondsPerTick;

        const long k_SampleSize = 1000;

        readonly List<long> m_TickCounts = new();
        readonly Stopwatch m_Stopwatch = new();
        readonly string m_ScopeName;

        long SampleCount => m_TickCounts.Count;

        public MultiSampleTimer(string scopeName)
        {
            m_ScopeName = scopeName;
        }

        void Reset()
        {
            m_TickCounts.Clear();
            m_Stopwatch.Reset();
        }

        public void BeginSample()
        {
            m_Stopwatch.Start();
        }

        public void EndSample()
        {
            m_Stopwatch.Stop();
            var sample = m_Stopwatch.ElapsedTicks;
            m_TickCounts.Add(sample);
            m_Stopwatch.Reset();

            if (SampleCount >= k_SampleSize)
            {
                LogStatsToConsole();
                Reset();
            }
        }

        public void LogStatsToConsole()
        {
            var totalTicks = 0L;
            var minTicks = long.MaxValue;
            var maxTicks = long.MinValue;
            var sampleCount = m_TickCounts.Count;
            for (var i = 0; i < sampleCount; ++i)
            {
                var sample = m_TickCounts[i];
                totalTicks += m_TickCounts[i];
                minTicks = Math.Min(minTicks, sample);
                maxTicks = Math.Max(maxTicks, sample);
            }
            var totalSeconds = totalTicks * k_SecondsPerTick;

            m_TickCounts.Sort();
            var medianTicks = m_TickCounts[sampleCount / 2];

            var average_ms = k_Milli * totalSeconds / sampleCount;
            var median_ms = medianTicks * k_MilliSecondsPerTick;
            var min_ms = minTicks * k_MilliSecondsPerTick;
            var max_ms = maxTicks * k_MilliSecondsPerTick;

            UnityEngine.Debug.Log(
                $"{m_ScopeName} executed {sampleCount} times in {totalSeconds} seconds. " +
                $"Average: {average_ms} ms, median: {median_ms} ms, min: {min_ms}, max: {max_ms}");
        }

        public void LogSamplesToFile(string filepath)
        {
            File.WriteAllText(filepath, string.Join(",\n", m_TickCounts));
        }
    }
}
#endif // UNITY_MP_TOOLS_DEV
