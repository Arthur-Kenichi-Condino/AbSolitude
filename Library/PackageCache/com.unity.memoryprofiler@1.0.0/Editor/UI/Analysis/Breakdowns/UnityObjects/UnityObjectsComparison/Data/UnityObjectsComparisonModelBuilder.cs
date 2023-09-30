#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    // Builds a UnityObjectsComparisonModel.
    class UnityObjectsComparisonModelBuilder : UnityObjectsModelBuilder
    {
        public UnityObjectsComparisonModel Build(
            CachedSnapshot snapshotA,
            CachedSnapshot snapshotB,
            BuildArgs args)
        {
            if (!CanBuildBreakdownForSnapshot(snapshotA))
                throw new UnsupportedSnapshotVersionException(snapshotA);

            if (!CanBuildBreakdownForSnapshot(snapshotB))
                throw new UnsupportedSnapshotVersionException(snapshotB);

            var typeNameToObjectNameAndObjectsMapA = BuildUnityObjectTypeNameToUnityObjectNameAndObjectsMapForSnapshot(
                snapshotA,
                args);
            var typeNameToObjectNameAndObjectsMapB = BuildUnityObjectTypeNameToUnityObjectNameAndObjectsMapForSnapshot(
                snapshotB,
                args);
            var rootNodes = BuildUnityObjectComparisonTree(
                typeNameToObjectNameAndObjectsMapA,
                typeNameToObjectNameAndObjectsMapB,
                args);

            if (args.FlattenHierarchy)
                rootNodes = TreeModelUtility.RetrieveLeafNodesOfTree(rootNodes);

            var totalSnapshotAMemorySize = snapshotA.MetaData.TargetMemoryStats.Value.TotalVirtualMemory;
            var totalSnapshotBMemorySize = snapshotB.MetaData.TargetMemoryStats.Value.TotalVirtualMemory;
            var model = new UnityObjectsComparisonModel(
                rootNodes,
                totalSnapshotAMemorySize,
                totalSnapshotBMemorySize);
            return model;
        }

        // Build a map of Unity-Object-Type-Name to map of Unity-Object-Name to Unity-Objects.
        Dictionary<string, Dictionary<string, List<TreeViewItemData<UnityObjectsModel.ItemData>>>> BuildUnityObjectTypeNameToUnityObjectNameAndObjectsMapForSnapshot(
            CachedSnapshot snapshot,
            in BuildArgs args)
        {
            var typeNameToTypeObjectsMap = BuildUnityObjectTypeNameToUnityObjectsMapForSnapshot(
                snapshot,
                new UnityObjectsModelBuilder.BuildArgs(
                    args.UnityObjectNameFilter,
                    flattenHierarchy: args.FlattenHierarchy));

            var typeNameToObjectNameAndObjectsMap = new Dictionary<string, Dictionary<string, List<TreeViewItemData<UnityObjectsModel.ItemData>>>>();
            foreach (var typeNameToTypeObjectsKvp in typeNameToTypeObjectsMap)
            {
                var typeName = typeNameToTypeObjectsKvp.Key;
                var typeObjects = typeNameToTypeObjectsKvp.Value;

                // Group type's objects by their object name.
                var objectNameToTypeObjectsMap = new Dictionary<string, List<TreeViewItemData<UnityObjectsModel.ItemData>>>();
                foreach (var typeObject in typeObjects)
                {
                    var objectName = typeObject.data.Name;
                    if (objectNameToTypeObjectsMap.TryGetValue(objectName, out var typeObjectsByName))
                        typeObjectsByName.Add(typeObject);
                    else
                        objectNameToTypeObjectsMap.Add(objectName, new List<TreeViewItemData<UnityObjectsModel.ItemData>> { typeObject });
                }

                typeNameToObjectNameAndObjectsMap.Add(typeName, objectNameToTypeObjectsMap);
            }

            return typeNameToObjectNameAndObjectsMap;
        }

        List<TreeViewItemData<UnityObjectsComparisonModel.ItemData>> BuildUnityObjectComparisonTree(
            Dictionary<string, Dictionary<string, List<TreeViewItemData<UnityObjectsModel.ItemData>>>> typeNameToObjectNameAndObjectsMapA,
            Dictionary<string, Dictionary<string, List<TreeViewItemData<UnityObjectsModel.ItemData>>>> typeNameToObjectNameAndObjectsMapB,
            BuildArgs args)
        {
            var rootNodes = new List<TreeViewItemData<UnityObjectsComparisonModel.ItemData>>();
            foreach (var kvp in typeNameToObjectNameAndObjectsMapA)
            {
                var comparisonNodes = new List<TreeViewItemData<UnityObjectsComparisonModel.ItemData>>();

                // Check if type exists in B.
                var typeName = kvp.Key;
                var objectNameToTypeObjectsMapA = kvp.Value;
                if (typeNameToObjectNameAndObjectsMapB.TryGetValue(
                    typeName,
                    out var objectNameToTypeObjectsMapB))
                {
                    // Type exists in both A and B.
                    foreach (var objectNameToTypeObjectsKvp in objectNameToTypeObjectsMapA)
                    {
                        // Check if object with name exists in B for this type.
                        var objectName = objectNameToTypeObjectsKvp.Key;
                        var typeObjectsA = objectNameToTypeObjectsKvp.Value;
                        if (objectNameToTypeObjectsMapB.TryGetValue(
                            objectName,
                            out var typeObjectsB))
                        {
                            // Object with name exists in B for this type. Create a comparison node for all matched objects.
                            var comparisonNode = CreateComparisonNodeForUnityObjects(
                                typeObjectsA,
                                typeObjectsB,
                                objectName,
                                typeName,
                                args.UnityObjectNameGroupComparisonSelectionProcessor);

                            // Check the current filters include unchanged.
                            if (args.IncludeUnchanged)
                                comparisonNodes.Add(comparisonNode);
                            else
                            {
                                if (comparisonNode.data.HasChanged)
                                    comparisonNodes.Add(comparisonNode);
                            }

                            // Remove from B's objects map (mark as processed).
                            objectNameToTypeObjectsMapB.Remove(objectName);
                        }
                        else
                        {
                            // This object name wasn't found in B for this type, so all this type's Unity Objects with this name are exclusive to A. Create a comparison node for all deleted objects.
                            var comparisonNode = CreateComparisonNodeForDeletedUnityObjects(
                                typeObjectsA,
                                objectName,
                                typeName,
                                args.UnityObjectNameGroupComparisonSelectionProcessor);
                            comparisonNodes.Add(comparisonNode);
                        }
                    }

                    // Any Object Names remaining in B's map are exclusive to B. Create comparison nodes for each group of created objects of this type remaining.
                    foreach (var objectNameToTypeObjectsKvp in objectNameToTypeObjectsMapB)
                    {
                        var objectName = objectNameToTypeObjectsKvp.Key;
                        var typeObjects = objectNameToTypeObjectsKvp.Value;
                        var comparisonNode = CreateComparisonNodeForCreatedUnityObjects(
                            typeObjects,
                            objectName,
                            typeName,
                            args.UnityObjectNameGroupComparisonSelectionProcessor);
                        comparisonNodes.Add(comparisonNode);
                    }

                    // Remove all processed Object Name groups from B's map.
                    objectNameToTypeObjectsMapB.Clear();
                }
                else
                {
                    // This type wasn't found in B, so all this type's Unity Objects are exclusive to A. Create comparison nodes for all deleted objects.
                    foreach (var objectNameToTypeObjectsKvp in objectNameToTypeObjectsMapA)
                    {
                        var objectName = objectNameToTypeObjectsKvp.Key;
                        var typeObjects = objectNameToTypeObjectsKvp.Value;
                        var comparisonNode = CreateComparisonNodeForDeletedUnityObjects(
                            typeObjects,
                            objectName,
                            typeName,
                            args.UnityObjectNameGroupComparisonSelectionProcessor);
                        comparisonNodes.Add(comparisonNode);
                    }
                }

                if (comparisonNodes.Count > 0)
                {
                    // Create node for Unity Object Type.
                    var node = CreateTypeComparisonNodeForUnityObjectComparisonNodes(
                        typeName,
                        comparisonNodes,
                        args.UnityObjectTypeComparisonSelectionProcessor);
                    rootNodes.Add(node);
                }
            }

            // Any Unity Object Types remaining in B's map are exclusive to B. Create comparison nodes for all created objects and a Unity Object Type node to parent them.
            foreach (var kvp in typeNameToObjectNameAndObjectsMapB)
            {
                var typeName = kvp.Key;
                var objectNameToTypeObjectsMapB = kvp.Value;

                var comparisonNodes = new List<TreeViewItemData<UnityObjectsComparisonModel.ItemData>>();
                foreach (var objectNameToTypeObjectsKvp in objectNameToTypeObjectsMapB)
                {
                    var objectName = objectNameToTypeObjectsKvp.Key;
                    var typeObjects = objectNameToTypeObjectsKvp.Value;
                    var comparisonNode = CreateComparisonNodeForCreatedUnityObjects(
                        typeObjects,
                        objectName,
                        typeName,
                        args.UnityObjectNameGroupComparisonSelectionProcessor);
                    comparisonNodes.Add(comparisonNode);
                }

                if (comparisonNodes.Count > 0)
                {
                    // Create node for Unity Object Type exclusive to B.
                    var node = CreateTypeComparisonNodeForUnityObjectComparisonNodes(
                        typeName,
                        comparisonNodes,
                        args.UnityObjectTypeComparisonSelectionProcessor);
                    rootNodes.Add(node);
                }
            }

            return rootNodes;
        }

        TreeViewItemData<UnityObjectsComparisonModel.ItemData> CreateComparisonNodeForCreatedUnityObjects(
            List<TreeViewItemData<UnityObjectsModel.ItemData>> createdObjects,
            string objectName,
            string typeName,
            Action<string, string> unityObjectNameGroupComparisonSelectionProcessor)
        {
            return CreateComparisonNodeForUnityObjects(
                null,
                createdObjects,
                objectName,
                typeName,
                unityObjectNameGroupComparisonSelectionProcessor);
        }

        TreeViewItemData<UnityObjectsComparisonModel.ItemData> CreateComparisonNodeForDeletedUnityObjects(
            List<TreeViewItemData<UnityObjectsModel.ItemData>> deletedObjects,
            string objectName,
            string typeName,
            Action<string, string> unityObjectNameGroupComparisonSelectionProcessor)
        {
            return CreateComparisonNodeForUnityObjects(
                deletedObjects,
                null,
                objectName,
                typeName,
                unityObjectNameGroupComparisonSelectionProcessor);
        }

        TreeViewItemData<UnityObjectsComparisonModel.ItemData> CreateComparisonNodeForUnityObjects(
            List<TreeViewItemData<UnityObjectsModel.ItemData>> unityObjectsA,
            List<TreeViewItemData<UnityObjectsModel.ItemData>> unityObjectsB,
            string objectName,
            string typeName,
            Action<string, string> unityObjectNameGroupComparisonSelectionProcessor)
        {
            var totalSizeInA = 0UL;
            var countInA = 0U;
            if (unityObjectsA != null)
            {
                foreach (var typeObject in unityObjectsA)
                {
                    totalSizeInA += typeObject.data.TotalSize;
                    countInA++;
                }
            }

            var totalSizeInB = 0UL;
            var countInB = 0U;
            if (unityObjectsB != null)
            {
                foreach (var typeObject in unityObjectsB)
                {
                    totalSizeInB += typeObject.data.TotalSize;
                    countInB++;
                }
            }

            var childCount = 0;
            void ProcessUnityObjectNameGroupComparisonSelection()
            {
                unityObjectNameGroupComparisonSelectionProcessor?.Invoke(objectName, typeName);
            }

            return new TreeViewItemData<UnityObjectsComparisonModel.ItemData>(
                m_ItemId++,
                new UnityObjectsComparisonModel.ItemData(
                    objectName,
                    totalSizeInA,
                    totalSizeInB,
                    countInA,
                    countInB,
                    typeName,
                    ProcessUnityObjectNameGroupComparisonSelection,
                    childCount)
            );
        }

        TreeViewItemData<UnityObjectsComparisonModel.ItemData> CreateTypeComparisonNodeForUnityObjectComparisonNodes(
            string typeName,
            List<TreeViewItemData<UnityObjectsComparisonModel.ItemData>> comparisonNodes,
            Action<string> unityObjectTypeComparisonSelectionProcessor)
        {
            var totalSizeInA = 0UL;
            var totalSizeInB = 0UL;
            var countInA = 0U;
            var countInB = 0U;
            foreach (var comparisonNode in comparisonNodes)
            {
                totalSizeInA += comparisonNode.data.TotalSizeInA;
                totalSizeInB += comparisonNode.data.TotalSizeInB;
                countInA += comparisonNode.data.CountInA;
                countInB += comparisonNode.data.CountInB;
            }

            void ProcessUnityObjectTypeComparisonSelection()
            {
                unityObjectTypeComparisonSelectionProcessor?.Invoke(typeName);
            };

            // Create node for Unity Object Type.
            return new TreeViewItemData<UnityObjectsComparisonModel.ItemData>(
                m_ItemId++,
                new UnityObjectsComparisonModel.ItemData(
                    typeName,
                    totalSizeInA,
                    totalSizeInB,
                    countInA,
                    countInB,
                    typeName,
                    ProcessUnityObjectTypeComparisonSelection,
                    comparisonNodes.Count),
                comparisonNodes);
        }

        public new readonly struct BuildArgs
        {
            public BuildArgs(
                ITextFilter unityObjectNameFilter,
                bool flattenHierarchy,
                bool includeUnchanged,
                Action<string, string> unityObjectNameGroupComparisonSelectionProcessor,
                Action<string> unityObjectTypeComparisonSelectionProcessor)
            {
                UnityObjectNameFilter = unityObjectNameFilter;
                IncludeUnchanged = includeUnchanged;
                FlattenHierarchy = flattenHierarchy;
                UnityObjectNameGroupComparisonSelectionProcessor = unityObjectNameGroupComparisonSelectionProcessor;
                UnityObjectTypeComparisonSelectionProcessor = unityObjectTypeComparisonSelectionProcessor;
            }

            // Only include Unity Objects with a name that passes this filter.
            public ITextFilter UnityObjectNameFilter { get; }

            // Include unchanged Unity Objects.
            public bool IncludeUnchanged { get; }

            // Flatten hierarchy to leaf nodes only (remove all categorization).
            public bool FlattenHierarchy { get; }

            // Selection processor for a Unity Object Name Group comparison item. Unity Objects of the same type in each snapshot are grouped by name and then compared as groups. Arguments are the native object's name and the native object's type's name (in both snapshots).
            public Action<string, string> UnityObjectNameGroupComparisonSelectionProcessor { get; }

            // Selection processor for a Unity Object Type comparison item. Argument is the native object's type's name (in both snapshots).
            public Action<string> UnityObjectTypeComparisonSelectionProcessor { get; }
        }
    }
}
#endif
