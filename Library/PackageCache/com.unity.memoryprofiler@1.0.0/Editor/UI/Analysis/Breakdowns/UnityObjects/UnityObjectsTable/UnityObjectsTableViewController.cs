#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class UnityObjectsTableViewController : ViewController
    {
        const string k_UxmlAssetGuid = "e43db30ef43ddfd44bea11154f274126";
        const string k_UssClass_Dark = "unity-objects-table-view__dark";
        const string k_UssClass_Light = "unity-objects-table-view__light";
        const string k_UxmlIdentifier_TreeView = "unity-objects-table-view__tree-view";
        const string k_UxmlIdentifier_TreeViewColumn__Description = "unity-objects-table-view__tree-view__column__description";
        const string k_UxmlIdentifier_TreeViewColumn__Size = "unity-objects-table-view__tree-view__column__size";
        const string k_UxmlIdentifier_TreeViewColumn__SizeBar = "unity-objects-table-view__tree-view__column__size-bar";
        const string k_UxmlIdentifier_TreeViewColumn__NativeSize = "unity-objects-table-view__tree-view__column__native-size";
        const string k_UxmlIdentifier_TreeViewColumn__ManagedSize = "unity-objects-table-view__tree-view__column__managed-size";
        const string k_UxmlIdentifier_Toolbar = "unity-objects-table-view__toolbar";
        const string k_UxmlIdentifier_FlattenToggle = "unity-objects-table-view__toolbar__flatten-toggle";
        const string k_UxmlIdentifier_DuplicatesToggle = "unity-objects-table-view__toolbar__duplicates-toggle";
        const string k_UxmlIdentifier_LoadingOverlay = "unity-objects-table-view__loading-overlay";
        const string k_UxmlIdentifier_ErrorLabel = "unity-objects-table-view__error-label";
        const string k_ErrorMessage = "Snapshot is from an outdated Unity version that is not fully supported.";

        // Sort comparisons for each column.
        static readonly Dictionary<string, Comparison<TreeViewItemData<UnityObjectsModel.ItemData>>> k_SortComparisons = new()
        {
            { k_UxmlIdentifier_TreeViewColumn__Description, (x, y) => string.Compare(x.data.Name, y.data.Name, StringComparison.OrdinalIgnoreCase) },
            { k_UxmlIdentifier_TreeViewColumn__Size, (x, y) => x.data.TotalSize.CompareTo(y.data.TotalSize) },
            { k_UxmlIdentifier_TreeViewColumn__SizeBar, (x, y) => x.data.TotalSize.CompareTo(y.data.TotalSize) },
            { k_UxmlIdentifier_TreeViewColumn__NativeSize, (x, y) => x.data.NativeSize.CompareTo(y.data.NativeSize) },
            { k_UxmlIdentifier_TreeViewColumn__ManagedSize, (x, y) => x.data.ManagedSize.CompareTo(y.data.ManagedSize) }
        };

        // Model.
        readonly CachedSnapshot m_Snapshot;
        readonly bool m_BuildOnLoad;
        readonly bool m_ShowAdditionalOptions;
        readonly IResponder m_Responder;
        UnityObjectsModel m_Model;
        AsyncWorker<UnityObjectsModel> m_BuildModelWorker;
        bool m_ShowDuplicatesOnly;
        bool m_FlattenHierarchy;

        // View.
        MultiColumnTreeView m_TreeView;
        Toolbar m_Toolbar;
        Toggle m_FlattenToggle;
        Toggle m_DuplicatesToggle;
        ActivityIndicatorOverlay m_LoadingOverlay;
        Label m_ErrorLabel;

        public UnityObjectsTableViewController(
            CachedSnapshot snapshot,
            bool buildOnLoad = true,
            bool showAdditionalOptions = true,
            IResponder responder = null)
        {
            m_Snapshot = snapshot;
            m_BuildOnLoad = buildOnLoad;
            m_ShowAdditionalOptions = showAdditionalOptions;
            m_Responder = responder;
        }

        public UnityObjectsModel Model => m_Model;

        public bool ShowDuplicatesOnly
        {
            get => m_ShowDuplicatesOnly;
            set
            {
                m_ShowDuplicatesOnly = value;
                if (IsViewLoaded)
                    BuildModelAsync();
            }
        }

        public bool FlattenHierarchy
        {
            get => m_FlattenHierarchy;
            set
            {
                m_FlattenHierarchy = value;
                if (IsViewLoaded)
                    BuildModelAsync();
            }
        }

        public ITextFilter UnityObjectNameFilter { get; private set; }

        public ITextFilter UnityObjectTypeNameFilter { get; private set; }

        public int? UnityObjectInstanceIdFilter { get; private set; }

        public void SetFilters(
            ITextFilter unityObjectNameFilter = null,
            ITextFilter unityObjectTypeNameFilter = null,
            int? unityObjectInstanceIdFilter = null)
        {
            UnityObjectNameFilter = unityObjectNameFilter;
            UnityObjectTypeNameFilter = unityObjectTypeNameFilter;
            UnityObjectInstanceIdFilter = unityObjectInstanceIdFilter;
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

            m_FlattenToggle.text = "Flatten Hierarchy";
            m_FlattenToggle.RegisterValueChangedCallback(OnFlattenHierarchyToggleValueChanged);
            m_DuplicatesToggle.text = "Show Potential Duplicates Only";
            m_DuplicatesToggle.tooltip = "Show potential duplicate Unity Objects only. Potential duplicates, which are Unity Objects of the same type, name, and size, might represent the same asset loaded multiple times in memory.";
            m_DuplicatesToggle.RegisterValueChangedCallback(OnShowDuplicatesOnlyToggleValueChanged);

            if (!m_ShowAdditionalOptions)
                UIElementsHelper.SetElementDisplay(m_Toolbar, false);

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
            m_Toolbar = view.Q<Toolbar>(k_UxmlIdentifier_Toolbar);
            m_FlattenToggle = view.Q<Toggle>(k_UxmlIdentifier_FlattenToggle);
            m_DuplicatesToggle = view.Q<Toggle>(k_UxmlIdentifier_DuplicatesToggle);
            m_LoadingOverlay = view.Q<ActivityIndicatorOverlay>(k_UxmlIdentifier_LoadingOverlay);
            m_ErrorLabel = view.Q<Label>(k_UxmlIdentifier_ErrorLabel);
        }

        void ConfigureTreeView()
        {
            m_TreeView.RegisterCallback<GeometryChangedEvent>(ConfigureInitialTreeViewLayout);

            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Description, "Description", BindCellForDescriptionColumn(), UnityObjectsDescriptionCell.Instantiate);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Size, "Total Size", BindCellForSizeColumn(SizeType.Total));
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__SizeBar, "Total Size % Bar", BindCellForSizeBarColumn(), MakeSizeBarCell);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__NativeSize, "Native Size", BindCellForSizeColumn(SizeType.Native));
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__ManagedSize, "Managed Size", BindCellForSizeColumn(SizeType.Managed));

#if UNITY_2022_2_OR_NEWER
            m_TreeView.selectionChanged += OnTreeViewSelectionChanged;
#else
            m_TreeView.onSelectionChange += OnTreeViewSelectionChanged;
#endif
            m_TreeView.columnSortingChanged += OnTreeViewSortingChanged;
        }

        void ConfigureInitialTreeViewLayout(GeometryChangedEvent evt)
        {
            // There is currently no way to set a tree view column's initial width as a percentage from UXML/USS, so we must do it manually once on load.
            var column = m_TreeView.columns[k_UxmlIdentifier_TreeViewColumn__Description];
            column.width = m_TreeView.layout.width * 0.4f;
            m_TreeView.UnregisterCallback<GeometryChangedEvent>(ConfigureInitialTreeViewLayout);
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
            var args = new UnityObjectsModelBuilder.BuildArgs(
                UnityObjectNameFilter,
                UnityObjectTypeNameFilter,
                UnityObjectInstanceIdFilter,
                FlattenHierarchy,
                ShowDuplicatesOnly,
                ProcessUnityObjectItemSelected,
                ProcessUnityObjectTypeItemSelected);
            var sortComparison = BuildSortComparisonFromTreeView();
            m_BuildModelWorker = new AsyncWorker<UnityObjectsModel>();
            m_BuildModelWorker.Execute(() =>
            {
                try
                {
                    // Build the data model.
                    var modelBuilder = new UnityObjectsModelBuilder();
                    var model = modelBuilder.Build(snapshot, args);

                    // Sort it according to the current sort descriptors.
                    model.Sort(sortComparison);

                    return model;
                }
                catch (Exception)
                {
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
                    m_Responder?.UnityObjectsTableViewControllerReloaded(this, success);

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
                var cell = (UnityObjectsDescriptionCell)element;
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(rowIndex);

                var itemTypeNames = m_Model.ItemTypeNamesMap;
                itemTypeNames.TryGetValue(itemData.TypeNameLookupKey, out var typeName);
                cell.SetTypeName(typeName);

                var displayText = itemData.Name;
                if (string.IsNullOrEmpty(displayText))
                    displayText = k_NoName;
                cell.SetText(displayText);

                var secondaryDisplayText = string.Empty;
                var childCount = itemData.ChildCount;
                if (childCount > 0)
                    secondaryDisplayText = $"({childCount:N0} Object{((childCount > 1) ? "s" : string.Empty)})";
                cell.SetSecondaryText(secondaryDisplayText);
            };
        }

        Action<VisualElement, int> BindCellForSizeBarColumn()
        {
            return (element, rowIndex) =>
            {
                var size = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(rowIndex).TotalSize;
                var progress = (float)size / m_Model.TotalMemorySize; // TODO Compute this in the model for each item. Do not compute it in the controller.

                var cell = (ProgressBar)element;
                cell.SetProgress(progress);
                cell.tooltip = $"{(progress * 100f):F2}%";
            };
        }

        Action<VisualElement, int> BindCellForSizeColumn(SizeType sizeType)
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(rowIndex);
                var size = 0UL;
                size = sizeType switch
                {
                    SizeType.Total => itemData.TotalSize,
                    SizeType.Native => itemData.NativeSize,
                    SizeType.Managed => itemData.ManagedSize,
                    SizeType.Gpu => itemData.GpuSize,
                    _ => throw new ArgumentException("Unknown size type."),
                };
                ((Label)element).text = EditorUtility.FormatBytes((long)size);
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

        void OnFlattenHierarchyToggleValueChanged(ChangeEvent<bool> evt)
        {
            FlattenHierarchy = evt.newValue;
            MemoryProfilerAnalytics.AddInteractionCountToEvent<MemoryProfilerAnalytics.InteractionsInPage, MemoryProfilerAnalytics.PageInteractionType>(
                FlattenHierarchy ? MemoryProfilerAnalytics.PageInteractionType.TreeViewWasFlattened : MemoryProfilerAnalytics.PageInteractionType.TreeViewWasUnflattened);
        }

        void OnShowDuplicatesOnlyToggleValueChanged(ChangeEvent<bool> evt)
        {
            ShowDuplicatesOnly = evt.newValue;
            MemoryProfilerAnalytics.AddInteractionCountToEvent<MemoryProfilerAnalytics.InteractionsInPage, MemoryProfilerAnalytics.PageInteractionType>(
                ShowDuplicatesOnly ? MemoryProfilerAnalytics.PageInteractionType.DuplicateFilterWasApplied : MemoryProfilerAnalytics.PageInteractionType.DuplicateFilterWasRemoved);
        }

        void OnTreeViewSelectionChanged(IEnumerable<object> items)
        {
            // Invoke the selection processor for the selected item.
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(selectedIndex);
            itemData.SelectionProcessor?.Invoke();
        }

        void ProcessUnityObjectItemSelected(int nativeObjectInstanceId)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(selectedIndex);
            m_Responder?.UnityObjectsTableViewControllerSelectedUnityObjectItem(this, nativeObjectInstanceId, itemData);
        }

        void ProcessUnityObjectTypeItemSelected(int nativeTypeIndex)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsModel.ItemData>(selectedIndex);
            m_Responder?.UnityObjectsTableViewControllerSelectedUnityObjectTypeItem(this, nativeTypeIndex, itemData);
        }

        Comparison<TreeViewItemData<UnityObjectsModel.ItemData>> BuildSortComparisonFromTreeView()
        {
            var sortedColumns = m_TreeView.sortedColumns;
            if (sortedColumns == null)
                return null;

            var sortComparisons = new List<Comparison<TreeViewItemData<UnityObjectsModel.ItemData>>>();
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
            // Invoked when a Unity Object item is selected in the table. Arguments are the view controller, the native object's instance id, and the item's data.
            void UnityObjectsTableViewControllerSelectedUnityObjectItem(
                UnityObjectsTableViewController viewController,
                int nativeObjectInstanceId,
                UnityObjectsModel.ItemData itemData);

            // Invoked when a Unity Object Type item is selected in the table. Arguments are the view controller, the native type's index, and the item's data.
            void UnityObjectsTableViewControllerSelectedUnityObjectTypeItem(
                UnityObjectsTableViewController viewController,
                int nativeTypeIndex,
                UnityObjectsModel.ItemData itemData);

            // Invoked after the table has been reloaded. Success argument is true if a model was successfully built or false it there was an error when building the model.
            void UnityObjectsTableViewControllerReloaded(
                UnityObjectsTableViewController viewController,
                bool success);
        }

        enum SizeType
        {
            Total,
            Native,
            Managed,
            Gpu,
        }
    }
}
#endif
