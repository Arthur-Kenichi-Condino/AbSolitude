using System;
using System.Collections.Generic;
using Unity.PerformanceTesting.Runtime;
using Unity.Profiling;
using UnityEngine;

namespace Unity.PerformanceTesting.Measurements
{
    class ProfilerMarkerMeasurement : IDisposable
    {
        private readonly bool m_AverageSampleMeasurement;
        bool m_DisposedValue;

        protected struct RecordedSampleGroup
        {
            public SampleGroup SampleGroup;
            public ProfilerRecorder ProfilerRecorder;
        }

        protected readonly List<RecordedSampleGroup> m_SampleGroups = new List<RecordedSampleGroup>();

        public ProfilerMarkerMeasurement(bool averageSampleMeasurement)
        {
            m_AverageSampleMeasurement = averageSampleMeasurement;
        }

        public void AddAndEnableProfilerSampleGroup(IEnumerable<SampleGroup> sampleGroups)
        {
            foreach (var sampleGroup in sampleGroups)
            {
                AddAndEnableProfilerSample(sampleGroup);
            }
        }

        public void AddAndEnableProfilerSample(SampleGroup sampleGroup)
        {
            var recorder = new ProfilerRecorder(sampleGroup.Name, 1, ProfilerRecorderOptions.WrapAroundWhenCapacityReached | ProfilerRecorderOptions.SumAllSamplesInFrame);
            // Start recorder immediately
            recorder.Start();
            m_SampleGroups.Add(new RecordedSampleGroup { SampleGroup = sampleGroup, ProfilerRecorder = recorder });
        }

        public void SampleProfilerSamples(bool stopRecorders = false)
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                // Validate that the recorder is attached to a valid marker
                if (!sampleGroup.ProfilerRecorder.Valid)
                {
                    Debug.LogError($"ProfilerMarker measurement is attached to invalid marker \"{sampleGroup.SampleGroup.Name}\"! Ensure the marker is created at the time of the measurement");
                    continue;
                }

                if (stopRecorders)
                    sampleGroup.ProfilerRecorder.Stop();

                var delta = 0.0;

                // Record the last recorded value if present, otherwise use 0.
                if (sampleGroup.ProfilerRecorder.Count > 0 && sampleGroup.ProfilerRecorder.GetSample(0).Count > 0)
                {
                    var sampleCount = m_AverageSampleMeasurement ? sampleGroup.ProfilerRecorder.GetSample(0).Count : 1;
                    if (sampleCount > 0)
                    {
                        var totalTimeNs = sampleGroup.ProfilerRecorder.GetSample(0).Value / sampleCount;
                        delta = Utils.ConvertSample(SampleUnit.Nanosecond, sampleGroup.SampleGroup.Unit, totalTimeNs);
                    }
                }

                Measure.Custom(sampleGroup.SampleGroup, delta);
            }
        }

        public void StopAndSampleRecorders() => SampleProfilerSamples(true);

        protected virtual void Dispose(bool disposing)
        {
            if (m_DisposedValue)
                return;

            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.ProfilerRecorder.Dispose();
            }

            m_DisposedValue = true;
        }

        ~ProfilerMarkerMeasurement()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
