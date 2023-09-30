#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;

namespace Unity.MemoryProfiler.Editor.UI
{
    static class AllTrackedMemoryComparisonTableModelBuilder
    {
        public static ComparisonTableModel Build(
            CachedSnapshot snapshotA,
            CachedSnapshot snapshotB,
            ITextFilter itemNameFilter,
            bool includeUnchanged,
            Action<List<string>> selectionProcessor)
        {
            var builderA = new AllTrackedMemoryModelBuilder();
            var modelA = builderA.Build(snapshotA, new AllTrackedMemoryModelBuilder.BuildArgs(itemNameFilter));

            var builderB = new AllTrackedMemoryModelBuilder();
            var modelB = builderB.Build(snapshotB, new AllTrackedMemoryModelBuilder.BuildArgs(itemNameFilter));

            var treeComparisonBuilder = new TreeComparisonBuilder();
            var comparisonTree = treeComparisonBuilder.Build(
                modelA.RootNodes,
                modelB.RootNodes,
                new TreeComparisonBuilder.BuildArgs(includeUnchanged, selectionProcessor),
                out var largestAbsoluteSizeDelta);

            var totalSnapshotSizeA = snapshotA.MetaData.TargetMemoryStats.Value.TotalVirtualMemory;
            var totalSnapshotSizeB = snapshotB.MetaData.TargetMemoryStats.Value.TotalVirtualMemory;
            var comparisonModel = new ComparisonTableModel(
                comparisonTree,
                totalSnapshotSizeA,
                totalSnapshotSizeB,
                largestAbsoluteSizeDelta);

            return comparisonModel;
        }
    }
}
#endif
