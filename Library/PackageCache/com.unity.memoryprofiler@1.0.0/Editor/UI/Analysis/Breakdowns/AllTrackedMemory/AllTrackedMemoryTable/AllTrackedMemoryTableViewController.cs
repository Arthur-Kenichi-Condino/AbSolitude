#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class AllTrackedMemoryTableViewController : ViewController
    {
        const string k_UxmlAssetGuid = "e7ac30fe2b076984e978d41347c5f0e0";
        const string k_UssClass_Dark = "all-tracked-memory-table-view__dark";
        const string k_UssClass_Light = "all-tracked-memory-table-view__light";
        const string k_UxmlIdentifier_TreeView = "all-tracked-memory-table-view__tree-view";
        const string k_UxmlIdentifier_TreeViewColumn__Description = "all-tracked-memory-table-view__tree-view__column__description";
        const string k_UxmlIdentifier_TreeViewColumn__Size = "all-tracked-memory-table-view__tree-view__column__size";
        const string k_UxmlIdentifier_TreeViewColumn__SizeBar = "all-tracked-memory-table-view__tree-view__column__size-bar";
        const string k_UxmlIdentifier_LoadingOverlay = "all-tracked-memory-table-view__loading-overlay";
        const string k_UxmlIdentifier_ErrorLabel = "all-tracked-memory-table-view__error-label";
        const string k_ErrorMessage = "Snapshot is from an outdated Unity version that is not fully supported.";

        // Sort comparisons for each column.
        static readonly Dictionary<string, Comparison<TreeViewItemData<AllTrackedMemoryModel.ItemData>>> k_SortComparisons = new()
        {
            { k_UxmlIdentifier_TreeViewColumn__Description, (x, y) => string.Compare(x.data.Name, y.data.Name, StringComparison.OrdinalIgnoreCase) },
            { k_UxmlIdentifier_TreeViewColumn__Size, (x, y) => x.data.Size.CompareTo(y.data.Size) },
            { k_UxmlIdentifier_TreeViewColumn__SizeBar, (x, y) => x.data.Size.CompareTo(y.data.Size) },
        };

        // Model.
        readonly CachedSnapshot m_Snapshot;
        readonly bool m_BuildOnLoad;
        readonly IResponder m_Responder;
        AllTrackedMemoryModel m_Model;
        AsyncWorker<AllTrackedMemoryModel> m_BuildModelWorker;

        // View.
        MultiColumnTreeView m_TreeView;
        ActivityIndicatorOverlay m_LoadingOverlay;
        Label m_ErrorLabel;

        public AllTrackedMemoryTableViewController(
            CachedSnapshot snapshot,
            bool buildOnLoad = true,
            IResponder responder = null)
        {
            m_Snapshot = snapshot;
            m_BuildOnLoad = buildOnLoad;
            m_Responder = responder;
        }

        public AllTrackedMemoryModel Model => m_Model;

        public ITextFilter ItemNameFilter { get; private set; }

        public IEnumerable<ITextFilter> ItemPathFilter { get; private set; }

        public bool ExcludeAll { get; private set; }

        public void SetFilters(
            ITextFilter itemNameFilter = null,
            IEnumerable<ITextFilter> itemPathFilter = null,
            bool excludeAll = false)
        {
            ItemNameFilter = itemNameFilter;
            ItemPathFilter = itemPathFilter;
            ExcludeAll = excludeAll;
            if (IsViewLoaded)
                BuildModelAsync();
        }

        public void ClearSelection()
        {
            m_TreeView.ClearSelection();
        }

        protected override VisualElement LoadView()
        {
            var view = ViewControllerUtility.LoadVisualTreeFromUxml(k_UxmlAssetGuid);
            if (view == null)
                throw new InvalidOperationException("Unable to create view from Uxml. Uxml must contain at least one child element.");
            view.style.flexGrow = 1;

            var themeUssClass = (EditorGUIUtility.isProSkin) ? k_UssClass_Dark : k_UssClass_Light;
            view.AddToClassList(themeUssClass);

            GatherReferencesInView(view);

            return view;
        }

        protected override void ViewLoaded()
        {
            ConfigureTreeView();

            if (m_BuildOnLoad)
                BuildModelAsync();
            else
                m_LoadingOverlay.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_BuildModelWorker?.Dispose();

            base.Dispose(disposing);
        }

        void GatherReferencesInView(VisualElement view)
        {
            m_TreeView = view.Q<MultiColumnTreeView>(k_UxmlIdentifier_TreeView);
            m_LoadingOverlay = view.Q<ActivityIndicatorOverlay>(k_UxmlIdentifier_LoadingOverlay);
            m_ErrorLabel = view.Q<Label>(k_UxmlIdentifier_ErrorLabel);
        }

        void ConfigureTreeView()
        {
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Description, "Description", BindCellForDescriptionColumn(), AllTrackedMemoryDescriptionCell.Instantiate);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Size, "Total Size", BindCellForSizeColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__SizeBar, "Total Size Bar", BindCellForSizeBarColumn(), MakeSizeBarCell);

#if UNITY_2022_2_OR_NEWER
            m_TreeView.selectionChanged += OnTreeViewSelectionChanged;
#else
            m_TreeView.onSelectionChange += OnTreeViewSelectionChanged;
#endif
            m_TreeView.columnSortingChanged += OnTreeViewSortingChanged;
        }

        void ConfigureTreeViewColumn(string columnName, string columnTitle, Action<VisualElement, int> bindCell, Func<VisualElement> makeCell = null)
        {
            var column = m_TreeView.columns[columnName];
            column.title = columnTitle;
            column.bindCell = bindCell;
            if (makeCell != null)
                column.makeCell = makeCell;
        }

        void BuildModelAsync()
        {
            // Cancel existing build if necessary.
            m_BuildModelWorker?.Dispose();

            // Show loading UI.
            m_LoadingOverlay.Show();

            // Dispatch asynchronous build.
            var snapshot = m_Snapshot;
            var args = new AllTrackedMemoryModelBuilder.BuildArgs(
                ItemNameFilter,
                ItemPathFilter,
                ExcludeAll,
                ProcessNativeObjectSelected,
                ProcessNativeTypeSelected,
                ProcessManagedObjectSelected,
                ProcessManagedTypeSelected,
                ProcessGroupSelected);
            var sortComparison = BuildSortComparisonFromTreeView();
            m_BuildModelWorker = new AsyncWorker<AllTrackedMemoryModel>();
            m_BuildModelWorker.Execute(() =>
            {
                try
                {
                    // Build the data model.
                    var modelBuilder = new AllTrackedMemoryModelBuilder();
                    var model = modelBuilder.Build(snapshot, args);

                    // Sort it according to the current sort descriptors.
                    model.Sort(sortComparison);

                    return model;
                }
                catch (UnsupportedSnapshotVersionException)
                {
                    return null;
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // We expect a ThreadAbortException to be thrown when cancelling an in-progress builder. Do not log an error to the console.
                    return null;
                }
                catch (Exception _e)
                {
                    Debug.LogError($"{_e.Message}\n{_e.StackTrace}");
                    return null;
                }
            }, (model) =>
                {
                    // Update model.
                    m_Model = model;

                    var success = model != null;
                    if (success)
                    {
                        // Refresh UI with new data model.
                        RefreshView();
                    }
                    else
                    {
                        // Display error message.
                        m_ErrorLabel.text = k_ErrorMessage;
                        UIElementsHelper.SetElementDisplay(m_ErrorLabel, true);
                    }

                    // Hide loading UI.
                    m_LoadingOverlay.Hide();

                    // Notify responder.
                    m_Responder?.Reloaded(this, success);

                    // Dispose asynchronous worker.
                    m_BuildModelWorker.Dispose();
                });
        }

        void RefreshView()
        {
            m_TreeView.SetRootItems(m_Model.RootNodes);
            m_TreeView.Rebuild();
        }

        Action<VisualElement, int> BindCellForDescriptionColumn()
        {
            const string k_NoName = "<No Name>";
            return (element, rowIndex) =>
            {
                var cell = (AllTrackedMemoryDescriptionCell)element;
                var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(rowIndex);

                var displayText = itemData.Name;
                if (string.IsNullOrEmpty(displayText))
                    displayText = k_NoName;
                cell.SetText(displayText);

                var secondaryDisplayText = string.Empty;
                var childCount = itemData.ChildCount;
                if (childCount > 0)
                    secondaryDisplayText = $"({childCount:N0} Item{((childCount > 1) ? "s" : string.Empty)})";
                cell.SetSecondaryText(secondaryDisplayText);
            };
        }

        Action<VisualElement, int> BindCellForSizeBarColumn()
        {
            return (element, rowIndex) =>
            {
                var size = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(rowIndex).Size;
                var progress = (float)size / m_Model.TotalMemorySize;

                var cell = (ProgressBar)element;
                cell.SetProgress(progress);
                cell.tooltip = $"{(progress * 100f):F2}%";
            };
        }

        Action<VisualElement, int> BindCellForSizeColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(rowIndex);
                var size = itemData.Size;

                var cell = (Label)element;
                cell.text = EditorUtility.FormatBytes((long)size);
            };
        }

        VisualElement MakeSizeBarCell()
        {
            var cell = new ProgressBar();
            cell.AddToClassList("size-bar-cell");
            return cell;
        }

        void OnTreeViewSortingChanged()
        {
            var sortedColumns = m_TreeView.sortedColumns;
            if (sortedColumns == null)
                return;

            BuildModelAsync();

            // Analytics
            {
                using var enumerator = sortedColumns.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var sortDescription = enumerator.Current;
                    if (sortDescription == null)
                        continue;

                    MemoryProfilerAnalytics.StartEvent<MemoryProfilerAnalytics.SortedColumnEvent>();
                    MemoryProfilerAnalytics.EndEvent(new MemoryProfilerAnalytics.SortedColumnEvent()
                    {
                        viewName = MemoryProfilerAnalytics.GetAnalyticsViewNameForOpenPage(),
                        Ascending = sortDescription.direction == SortDirection.Ascending,
                        shown = sortDescription.columnIndex,
                        fileName = sortDescription.columnName
                    });
                    MemoryProfilerAnalytics.AddInteractionCountToEvent<
                        MemoryProfilerAnalytics.InteractionsInPage,
                        MemoryProfilerAnalytics.PageInteractionType>(
                        MemoryProfilerAnalytics.PageInteractionType.TableSortingWasChanged);
                }
            }
        }

        void OnTreeViewSelectionChanged(IEnumerable<object> items)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            if (selectedIndex == -1)
                return;

            // Invoke the selection processor for the selected item.
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            if (itemData.SelectionProcessor != null)
                itemData.SelectionProcessor.Invoke();
            else
                OnItemWithNoSelectionProcessorSelected();
        }

        void ProcessNativeObjectSelected(long nativeObjectIndex)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedNativeObjectItem(this, nativeObjectIndex, itemData);
        }

        void ProcessNativeTypeSelected(int nativeTypeIndex)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedNativeTypeItem(this, nativeTypeIndex, itemData);
        }

        void ProcessManagedObjectSelected(long managedObjectIndex)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedManagedObjectItem(this, managedObjectIndex, itemData);
        }

        void ProcessManagedTypeSelected(int managedTypeIndex)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedManagedTypeItem(this, managedTypeIndex, itemData);
        }

        void ProcessGroupSelected(string description)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedGroupItem(this, description, itemData);
        }

        void OnItemWithNoSelectionProcessorSelected()
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<AllTrackedMemoryModel.ItemData>(selectedIndex);
            m_Responder?.SelectedUnhandledItem(this, itemData);
        }

        Comparison<TreeViewItemData<AllTrackedMemoryModel.ItemData>> BuildSortComparisonFromTreeView()
        {
            var sortedColumns = m_TreeView.sortedColumns;
            if (sortedColumns == null)
                return null;

            var sortComparisons = new List<Comparison<TreeViewItemData<AllTrackedMemoryModel.ItemData>>>();
            foreach (var sortedColumnDescription in sortedColumns)
            {
                if (sortedColumnDescription == null)
                    continue;

                var sortComparison = k_SortComparisons[sortedColumnDescription.columnName];

                // Invert the comparison's input arguments depending on the sort direction.
                var sortComparisonWithDirection = (sortedColumnDescription.direction == SortDirection.Ascending) ? sortComparison : (x, y) => sortComparison(y, x);
                sortComparisons.Add(sortComparisonWithDirection);
            }

            return (x, y) =>
            {
                var result = 0;
                foreach (var sortComparison in sortComparisons)
                {
                    result = sortComparison.Invoke(x, y);
                    if (result != 0)
                        break;
                }

                return result;
            };
        }

        public interface IResponder
        {
            // Invoked when a Native Object item is selected in the table. Arguments are the view controller, the native object's index, and the item's data.
            void SelectedNativeObjectItem(
                AllTrackedMemoryTableViewController viewController,
                long nativeObjectIndex,
                AllTrackedMemoryModel.ItemData itemData);

            // Invoked when a Native Type item is selected in the table. Arguments are the view controller, the native type's index, and the item's data.
            void SelectedNativeTypeItem(
                AllTrackedMemoryTableViewController viewController,
                int nativeTypeIndex,
                AllTrackedMemoryModel.ItemData itemData);

            // Invoked when a Managed Object item is selected in the table. Arguments are the view controller, the managed object's index, and the item's data.
            void SelectedManagedObjectItem(
                AllTrackedMemoryTableViewController viewController,
                long managedObjectIndex,
                AllTrackedMemoryModel.ItemData itemData);

            // Invoked when a Managed Type item is selected in the table. Arguments are the view controller, the managed type's index, and the item's data.
            void SelectedManagedTypeItem(
                AllTrackedMemoryTableViewController viewController,
                int managedTypeIndex,
                AllTrackedMemoryModel.ItemData itemData);

            //  Invoked when a Group item with no specific type is selected in the table.
            void SelectedGroupItem(
                AllTrackedMemoryTableViewController viewController,
                string description,
                AllTrackedMemoryModel.ItemData itemData);

            // Invoked when an item with no selection processor is selected in the table. Arguments are the view controller and the item's data.
            void SelectedUnhandledItem(
                AllTrackedMemoryTableViewController viewController,
                AllTrackedMemoryModel.ItemData itemData);

            // Invoked after the table has been reloaded. Success argument is true if a model was successfully built or false it there was an error when building the model.
            void Reloaded(AllTrackedMemoryTableViewController viewController, bool success);
        }
    }
}
#endif
