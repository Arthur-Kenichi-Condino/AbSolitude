#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    // Builds an AllSystemMemoryBreakdownModel.
    class AllSystemMemoryBreakdownModelBuilder
    {
        int m_ItemId;

        readonly string k_DataItemTypeFree = "Free";
        readonly string k_DataItemTypeUntracked = "Untracked";
        readonly string k_DataItemTypeReserved = "Reserved";
        readonly string k_DataItemTypeDevice = "Device";
        readonly string k_DataItemTypeMapped = "Mapped";
        readonly string k_DataItemTypeShared = "Shared";
        readonly string k_DataItemTypeNative = "Native";
        readonly string k_DataItemTypeManaged = "Managed";

        public AllSystemMemoryBreakdownModel Build(CachedSnapshot snapshot, in BuildArgs args)
        {
            if (!CanBuildBreakdownForSnapshot(snapshot))
                throw new UnsupportedSnapshotVersionException(snapshot);

            List<TreeViewItemData<AllSystemMemoryBreakdownModel.ItemData>> roots = null;
            ConvertToTreeViewRecursive(snapshot, args, snapshot.MemoryEntriesHierarchy.GetRoots(), ref roots);
            return new AllSystemMemoryBreakdownModel(roots);
        }

        bool CanBuildBreakdownForSnapshot(CachedSnapshot snapshot)
        {
            // TargetAndMemoryInfo is required to obtain the total snapshot memory size and reserved sizes.
            if (!snapshot.HasSystemMemoryRegionsInfo)
                return false;

            return true;
        }

        ulong ConvertToTreeViewRecursive(CachedSnapshot snapshot, in BuildArgs args, IEnumerable<long> items, ref List<TreeViewItemData<AllSystemMemoryBreakdownModel.ItemData>> output)
        {
            ulong total = 0;

            output = new List<TreeViewItemData<AllSystemMemoryBreakdownModel.ItemData>>();
            var data = snapshot.MemoryEntriesHierarchy.Data;
            foreach(var itemIndex in items)
            {
                var item = data[itemIndex];
                var nextIndex = itemIndex + item.ChildrenCount + 1;
                var nextItem = data[nextIndex];
                var size = nextItem.Address - item.Address;

                if (size == 0)
                    continue;

                var name = snapshot.MemoryEntriesHierarchy.GetName(itemIndex);
                if (!NamePassesFilter(name, args.NameFilter))
                    continue;

                List<TreeViewItemData<AllSystemMemoryBreakdownModel.ItemData>> children = null;
                if (item.ChildrenCount > 0)
                    size = ConvertToTreeViewRecursive(snapshot, args, snapshot.MemoryEntriesHierarchy.GetChildren(itemIndex), ref children);

                var treeNode = new AllSystemMemoryBreakdownModel.ItemData(
                    name,
                    item.Address,
                    size,
                    GetDataSourceResidentSize(snapshot, item.Source),
                    GetDataSourceTypeName(snapshot, item.Type),
                    item.Source);
                output.Add(new TreeViewItemData<AllSystemMemoryBreakdownModel.ItemData>(m_ItemId++, treeNode, children));

                total += size;
            }

            return total;
        }

        ulong GetDataSourceResidentSize(CachedSnapshot snapshot, CachedSnapshot.SourceLink source)
        {
            if (source.Id != CachedSnapshot.SourceLink.SourceId.SystemMemoryRegion)
                return 0;

            return snapshot.SystemMemoryRegions.RegionResident[source.Index];
        }

        string GetDataSourceTypeName(CachedSnapshot snapshot, CachedSnapshot.MemoryEntriesHierarchyCache.RegionType type)
        {
            switch(type)
            {
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Free:
                    return k_DataItemTypeFree;
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Untracked:
                    return k_DataItemTypeUntracked;
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Reserved:
                    return k_DataItemTypeReserved;

                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Device:
                    return k_DataItemTypeDevice;
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Mapped:
                    return k_DataItemTypeMapped;
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Shared:
                    return k_DataItemTypeShared;

                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Managed:
                    return k_DataItemTypeManaged;
                case CachedSnapshot.MemoryEntriesHierarchyCache.RegionType.Native:
                    return k_DataItemTypeNative;
                default:
                    return String.Empty;
            }
        }

        bool NamePassesFilter(string name, string nameFilter)
        {
            if (!string.IsNullOrEmpty(nameFilter))
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                if (!name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        internal readonly struct BuildArgs
        {
            public BuildArgs(string nameFilter)
            {
                NameFilter = nameFilter;
            }

            public string NameFilter { get; }
        }
    }
}
#endif
