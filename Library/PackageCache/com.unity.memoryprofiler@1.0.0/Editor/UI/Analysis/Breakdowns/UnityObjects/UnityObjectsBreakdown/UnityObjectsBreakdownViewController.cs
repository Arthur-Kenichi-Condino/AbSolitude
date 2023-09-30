#if UNITY_2022_1_OR_NEWER
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class UnityObjectsBreakdownViewController : BreakdownViewController, UnityObjectsTableViewController.IResponder
    {
        public UnityObjectsBreakdownViewController(CachedSnapshot snapshot, string description)
            : base(snapshot, description)
        {
        }

        protected override void ViewLoaded()
        {
            base.ViewLoaded();

            // Initialize All Tracked Memory table as a child view controller.
            var tableViewController = new UnityObjectsTableViewController(Snapshot, responder: this);
            AddChild(tableViewController);
            TableContainer.Add(tableViewController.View);

            SearchField.RegisterValueChangedCallback((evt) =>
            {
                var searchText = evt.newValue;
                // ToolbarSearchField provides an empty (non-null) string when the search bar is empty. In that case we don't want a valid filter for "", i.e. "any text".
                if (string.IsNullOrEmpty(searchText))
                    searchText = null;
                var unityObjectNameFilter = ContainsTextFilter.Create(searchText);
                tableViewController.SetFilters(unityObjectNameFilter);
            });
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerReloaded(
            UnityObjectsTableViewController viewController,
            bool success)
        {
            if (!success)
                return;

            var model = viewController.Model;
            RefreshTableSizeBar(model.TotalMemorySize, model.TotalSnapshotMemorySize);
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerSelectedUnityObjectItem(
            UnityObjectsTableViewController viewController,
            int nativeObjectInstanceId,
            UnityObjectsModel.ItemData itemData)
        {
            // Temporary integration with legacy history/selection API to show UI for some selected items in the Details view.
            // Convert to a unified object to get Unified Object UI in Details view.
            var nativeObjectIndex = Snapshot.NativeObjects.instanceId2Index[nativeObjectInstanceId];
            var unifiedObjectIndex = ObjectData.FromNativeObjectIndex(
                    Snapshot,
                Convert.ToInt32(nativeObjectIndex))
                .GetUnifiedObjectIndex(Snapshot);
            var selection = MemorySampleSelection.FromUnifiedObjectIndex(unifiedObjectIndex);
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            window.UIState.RegisterSelectionChangeEvent(selection);
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerSelectedUnityObjectTypeItem(
            UnityObjectsTableViewController viewController,
            int nativeTypeIndex,
            UnityObjectsModel.ItemData itemData)
        {
            // Temporary integration with legacy history/selection API to show UI for some selected items in the Details view.
            var selection = MemorySampleSelection.FromNativeTypeIndex(nativeTypeIndex);
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            window.UIState.RegisterSelectionChangeEvent(selection);
        }
    }
}
#endif
