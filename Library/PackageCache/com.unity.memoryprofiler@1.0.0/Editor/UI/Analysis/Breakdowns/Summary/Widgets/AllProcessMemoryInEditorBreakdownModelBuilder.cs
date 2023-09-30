using System;
using System.Collections.Generic;
using Unity.MemoryProfiler.Editor.UIContentData;
using UnityEditor.Profiling;
using UnityEditorInternal;

namespace Unity.MemoryProfiler.Editor.UI
{
    /// <summary>
    /// All process memory usage as seen by OS broken down
    /// into a set of pre-defined high-level categories.
    /// </summary>
    internal class AllProcessMemoryInEditorBreakdownModelBuilder : IMemoryBreakdownModelBuilder<MemoryBreakdownModel>
    {
        const string k_CategoryManaged = "Managed";
        const string k_CategoryDrivers = "Graphics";
        const string k_CategoryAudio = "Audio";
        const string k_CategoryUnityOther = "Native";
        const string k_CategoryUnityProfiler = "Profiler";
        const string k_CategoryUnknown = "Unknown";

        public AllProcessMemoryInEditorBreakdownModelBuilder()
        {
        }

        public long Frame { get; set; }

        public MemoryBreakdownModel Build()
        {
            var total = 0UL;
            var rows = new List<MemoryBreakdownModel.Row>();
            using (var data = ProfilerDriver.GetRawFrameDataView((int)Frame, 0))
            {
                GetCounterValue(data, "Total Reserved Memory", out var totalTrackedReserved);
                GetCounterValue(data, "Total Used Memory", out var totalTrackedUsed);
                GetCounterValue(data, "Gfx Reserved Memory", out var gfxTracked);
                GetCounterValue(data, "GC Reserved Memory", out var managedTrackedReserved);
                GetCounterValue(data, "GC Used Memory", out var managedTrackedUsed);

                GetCounterValue(data, "Audio Used Memory", out var audioTracked);
                GetCounterValue(data, "Video Used Memory", out var videoTracked);
                GetCounterValue(data, "Profiler Reserved Memory", out var profilerTrackedReserved);
                GetCounterValue(data, "Profiler Used Memory", out var profilerTrackedUsed);

                // Older editors might not have the counter, in that case use total tracked
                if (!GetCounterValue(data, "System Used Memory", out total))
                    total = totalTrackedReserved;

                // For platforms which don't report total committed, it might be too small
                if (total < totalTrackedReserved)
                    total = totalTrackedReserved;

                var otherReserved = totalTrackedReserved - Math.Min(managedTrackedReserved + gfxTracked + audioTracked + videoTracked + profilerTrackedReserved, totalTrackedReserved);
                var otherUsed = totalTrackedUsed - Math.Min(managedTrackedUsed + gfxTracked + audioTracked + videoTracked + profilerTrackedUsed, totalTrackedUsed);

                var unknown = total - totalTrackedReserved;

                rows = new List<MemoryBreakdownModel.Row>() {
                        new MemoryBreakdownModel.Row(k_CategoryManaged, managedTrackedReserved, managedTrackedUsed, 0, 0, "managed", TextContent.ManagedDescription, null),
                        new MemoryBreakdownModel.Row(k_CategoryDrivers, gfxTracked, 0, 0, 0, "gfx", TextContent.GraphicsDescription, null),
                        new MemoryBreakdownModel.Row(k_CategoryAudio, audioTracked, 0, 0, 0, "audio", TextContent.AudioDescription, null),
                        new MemoryBreakdownModel.Row(k_CategoryUnityOther, otherReserved, otherUsed, 0, 0, "unity-other", TextContent.NativeDescription, null),
                        new MemoryBreakdownModel.Row(k_CategoryUnityProfiler, profilerTrackedReserved, profilerTrackedUsed, 0, 0, "profiler", TextContent.ProfilerDescription, null),
                        new MemoryBreakdownModel.Row(k_CategoryUnknown, unknown, 0, 0, 0, "unknown", TextContent.UnknownDescription, DocumentationUrls.UntrackedMemoryDocumentation),
                    };
            }

            return new MemoryBreakdownModel(
                "Total Committed Memory",
                false,
                total,
                0,
                rows
            );
        }

        private bool GetCounterValue(RawFrameDataView data, string counterName, out ulong value)
        {
            if (!data.valid)
            {
                value = 0;
                return false;
            }

            var markerId = data.GetMarkerId(counterName);
            value = (ulong)data.GetCounterValueAsLong(markerId);
            return true;
        }
    }
}
