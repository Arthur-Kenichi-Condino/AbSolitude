#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.MemoryProfiler.Editor.UI
{
    // Displays comparison between two All Tracked Memory trees.
    class AllTrackedMemoryComparisonViewController : ComparisonViewController, AllTrackedMemoryTableViewController.IResponder
    {
        // Children.
        AllTrackedMemoryTableViewController m_BaseViewController;
        AllTrackedMemoryTableViewController m_ComparedViewController;

        public AllTrackedMemoryComparisonViewController(CachedSnapshot snapshotA, CachedSnapshot snapshotB, string description)
            : base(snapshotA, snapshotB, description, AllTrackedMemoryComparisonTableModelBuilder.Build)
        {
        }

        protected override void ViewLoaded()
        {
            base.ViewLoaded();

            // Configure 'Base (A)' table.
            m_BaseViewController = new AllTrackedMemoryTableViewController(
                SnapshotA,
                false,
                this);
            BaseViewContainer.Add(m_BaseViewController.View);
            AddChild(m_BaseViewController);

            // Configure 'Compared (B)' table.
            m_ComparedViewController = new AllTrackedMemoryTableViewController(
                SnapshotB,
                false,
                this);
            ComparedViewContainer.Add(m_ComparedViewController.View);
            AddChild(m_ComparedViewController);
        }

        protected override void OnComparisonTreeItemSelected(List<string> itemPath)
        {
            m_BaseViewController.ClearSelection();
            m_ComparedViewController.ClearSelection();
            SelectInDetailsView(MemorySampleSelection.InvalidMainSelection);

            var selectedItems = TreeView.GetSelectedItems<ComparisonTableModel.ComparisonData>();
            var selectedItem = selectedItems.First();
            if (!selectedItem.hasChildren)
            {
                // Filter the base/compared tables to the current comparison table selection and search text filter.
                var itemNameFilter = BuildTextFilterFromSearchText();
                var itemPathFilter = new ITextFilter[itemPath.Count];
                for (var i = 0; i < itemPath.Count; i++)
                {
                    var pathComponent = itemPath[i];
                    itemPathFilter[i] = MatchesTextFilter.Create(pathComponent);
                }
                m_BaseViewController.SetFilters(itemNameFilter, itemPathFilter);
                m_ComparedViewController.SetFilters(itemNameFilter, itemPathFilter);
            }
            else
            {
                // Show an empty table if a non-leaf (group) node is selected in the comparison table.
                m_BaseViewController.SetFilters(excludeAll: true);
                m_ComparedViewController.SetFilters(excludeAll: true);
            }
        }

        void AllTrackedMemoryTableViewController.IResponder.Reloaded(
            AllTrackedMemoryTableViewController viewController,
            bool success)
        {
            var isBaseView = viewController == m_BaseViewController;
            var descriptionLabel = (isBaseView) ? BaseDescriptionLabel : ComparedDescriptionLabel;
            var model = viewController.Model;
            var itemCount = model.RootNodes.Count;
            descriptionLabel.text = $"{itemCount:N0} item{(itemCount != 1 ? "s" : string.Empty)} | Size: {EditorUtility.FormatBytes(Convert.ToInt64(model.TotalMemorySize))}";
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedNativeObjectItem(
            AllTrackedMemoryTableViewController viewController,
            long nativeObjectIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var isBaseView = viewController == m_BaseViewController;
            var snapshotAge = GetSnapshotAgeForBaseOrCompared(isBaseView);
            var selection = MemorySampleSelection.FromNativeObjectIndex(nativeObjectIndex, snapshotAge);
            SelectInDetailsView(selection);

            var otherViewController = (isBaseView) ? m_ComparedViewController : m_BaseViewController;
            otherViewController.ClearSelection();
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedNativeTypeItem(
            AllTrackedMemoryTableViewController viewController,
            int nativeTypeIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var isBaseView = viewController == m_BaseViewController;
            var snapshotAge = GetSnapshotAgeForBaseOrCompared(isBaseView);
            var selection = MemorySampleSelection.FromNativeTypeIndex(nativeTypeIndex, snapshotAge);
            SelectInDetailsView(selection);

            var otherViewController = (isBaseView) ? m_ComparedViewController : m_BaseViewController;
            otherViewController.ClearSelection();
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedManagedObjectItem(
            AllTrackedMemoryTableViewController viewController,
            long managedObjectIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var isBaseView = viewController == m_BaseViewController;
            var snapshotAge = GetSnapshotAgeForBaseOrCompared(isBaseView);
            var selection = MemorySampleSelection.FromManagedObjectIndex(managedObjectIndex, snapshotAge);
            SelectInDetailsView(selection);

            var otherViewController = (isBaseView) ? m_ComparedViewController : m_BaseViewController;
            otherViewController.ClearSelection();
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedManagedTypeItem(
            AllTrackedMemoryTableViewController viewController,
            int managedTypeIndex,
            AllTrackedMemoryModel.ItemData itemData)
        {
            var isBaseView = viewController == m_BaseViewController;
            var snapshotAge = GetSnapshotAgeForBaseOrCompared(isBaseView);
            var selection = MemorySampleSelection.FromManagedTypeIndex(managedTypeIndex, snapshotAge);
            SelectInDetailsView(selection);

            var otherViewController = (isBaseView) ? m_ComparedViewController : m_BaseViewController;
            otherViewController.ClearSelection();
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedUnhandledItem(
            AllTrackedMemoryTableViewController viewController,
            AllTrackedMemoryModel.ItemData itemData)
        {
            SelectInDetailsView(MemorySampleSelection.InvalidMainSelection);
        }

        // Temporary integration with legacy history/selection API to show UI for some selected items in the Details view.
        void SelectInDetailsView(MemorySampleSelection selection)
        {
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            if (!selection.Equals(MemorySampleSelection.InvalidMainSelection))
                window.UIState.RegisterSelectionChangeEvent(selection);
            else
                window.UIState.ClearSelection(MemorySampleSelectionRank.MainSelection);
        }

        // When a user switches the snapshots, it seems the existing implementation makes the second snapshot the 'older' snapshot and the first snapshot the 'newer' snapshot. Then, in MemorySampleSelection.GetSnapshotItemIsPresentIn, if the first snapshot is 'newer', SnapshotAge.Older will return the second snapshot instead of the first. Therefore, which SnapshotAge we want to pass is not 'Older' for Base/A and 'Newer' for Compared/B, but instead changes depending whether the snapshots have been switched or not. As an aside, this appears to have no relation to the actual time of the snapshots either. Therefore 'Older' and 'Newer' is extremely confusing terminology that we should remove.
        static SnapshotAge GetSnapshotAgeForBaseOrCompared(bool isBase)
        {
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            if (window.UIState.FirstSnapshotAge == SnapshotAge.Newer)
                return (isBase) ? SnapshotAge.Newer : SnapshotAge.Older;
            else
                return (isBase) ? SnapshotAge.Older : SnapshotAge.Newer;
        }

        public void SelectedGroupItem(AllTrackedMemoryTableViewController viewController, string description, AllTrackedMemoryModel.ItemData itemData)
        {
            var isBaseView = viewController == m_BaseViewController;
            var selection = new MemorySampleSelection(itemData.Name, description);
            SelectInDetailsView(selection);

            var otherViewController = (isBaseView) ? m_ComparedViewController : m_BaseViewController;
            otherViewController.ClearSelection();
        }
    }
}
#endif
