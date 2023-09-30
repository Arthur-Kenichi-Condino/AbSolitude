#if UNITY_2022_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class AllTrackedMemoryBreakdownViewController : BreakdownViewController,
        AllTrackedMemoryTableViewController.IResponder
    {
        public AllTrackedMemoryBreakdownViewController(CachedSnapshot snapshot, string description)
            : base(snapshot, description)
        {
        }

        protected override void ViewLoaded()
        {
            base.ViewLoaded();

            // Initialize All Tracked Memory table as a child view controller.
            var tableViewController = new AllTrackedMemoryTableViewController(Snapshot, responder: this);
            AddChild(tableViewController);
            TableContainer.Add(tableViewController.View);

            SearchField.RegisterValueChangedCallback((evt) =>
            {
                var searchText = evt.newValue;
                // ToolbarSearchField provides an empty (non-null) string when the search bar is empty. In that case we don't want a valid filter for "", i.e. "any text".
                if (string.IsNullOrEmpty(searchText))
                    searchText = null;
                var nameFilter = ContainsTextFilter.Create(searchText);
                tableViewController.SetFilters(nameFilter);
            });
        }

        void AllTrackedMemoryTableViewController.IResponder.Reloaded(
            AllTrackedMemoryTableViewController viewController,
            bool success)
        {
            if (!success)
                return;

            var model = viewController.Model;
            RefreshTableSizeBar(model.TotalMemorySize, model.TotalSnapshotMemorySize);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedNativeObjectItem(
            AllTrackedMemoryTableViewController viewController,
            long nativeObjectIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var selection = MemorySampleSelection.FromNativeObjectIndex(nativeObjectIndex);
            RegisterSelectionInDetailsView(selection);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedNativeTypeItem(
            AllTrackedMemoryTableViewController viewController,
            int nativeTypeIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var selection = MemorySampleSelection.FromNativeTypeIndex(nativeTypeIndex);
            RegisterSelectionInDetailsView(selection);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedManagedObjectItem(
            AllTrackedMemoryTableViewController viewController,
            long managedObjectIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var selection = MemorySampleSelection.FromManagedObjectIndex(managedObjectIndex);
            RegisterSelectionInDetailsView(selection);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedManagedTypeItem(
            AllTrackedMemoryTableViewController viewController,
            int managedTypeIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var selection = MemorySampleSelection.FromManagedTypeIndex(managedTypeIndex);
            RegisterSelectionInDetailsView(selection);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedUnhandledItem(
            AllTrackedMemoryTableViewController viewController, AllTrackedMemoryModel.ItemData itemData)
        {
            RegisterSelectionInDetailsView(MemorySampleSelection.InvalidMainSelection);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedGroupItem(
            AllTrackedMemoryTableViewController viewController, string description, AllTrackedMemoryModel.ItemData itemData)
        {
            var selection = new MemorySampleSelection(itemData.Name, description);
            RegisterSelectionInDetailsView(selection);
        }

        // Temporary integration with legacy history/selection API to show UI for some selected items in the Details view.
        void RegisterSelectionInDetailsView(MemorySampleSelection selection)
        {
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            if (!selection.Equals(MemorySampleSelection.InvalidMainSelection))
                window.UIState.RegisterSelectionChangeEvent(selection);
            else
                window.UIState.ClearSelection(MemorySampleSelectionRank.MainSelection);
        }
    }
}
#endif
