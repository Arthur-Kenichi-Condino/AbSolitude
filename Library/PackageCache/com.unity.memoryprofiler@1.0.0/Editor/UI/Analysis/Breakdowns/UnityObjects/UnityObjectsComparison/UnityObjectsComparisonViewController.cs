#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class UnityObjectsComparisonViewController : ViewController, UnityObjectsTableViewController.IResponder
    {
        const string k_UxmlAssetGuid = "22b678e6f811eec4782c655ff73c2677";
        const string k_UssClass_Dark = "unity-objects-comparison-view__dark";
        const string k_UssClass_Light = "unity-objects-comparison-view__light";
        const string k_UxmlIdentifier_DescriptionLabel = "unity-objects-comparison-view__description-label";
        const string k_UxmlIdentifier_SearchField = "unity-objects-comparison-view__search-field";
        const string k_UxmlIdentifier_SplitView = "unity-objects-comparison-view__split-view";
        const string k_UxmlIdentifier_BaseTotalSizeBar = "unity-objects-comparison-view__base-total-size-bar";
        const string k_UxmlIdentifier_ComparedTotalSizeBar = "unity-objects-comparison-view__compared-total-size-bar";
        const string k_UxmlIdentifier_TreeView = "unity-objects-comparison-view__tree-view";
        const string k_UxmlIdentifier_TreeViewColumn__Description = "unity-objects-comparison-view__tree-view__column__description";
        const string k_UxmlIdentifier_TreeViewColumn__CountDelta = "unity-objects-comparison-view__tree-view__column__count-delta";
        const string k_UxmlIdentifier_TreeViewColumn__SizeDeltaBar = "unity-objects-comparison-view__tree-view__column__size-delta-bar";
        const string k_UxmlIdentifier_TreeViewColumn__SizeDelta = "unity-objects-comparison-view__tree-view__column__size-delta";
        const string k_UxmlIdentifier_TreeViewColumn__TotalSizeInA = "unity-objects-comparison-view__tree-view__column__total-size-in-a";
        const string k_UxmlIdentifier_TreeViewColumn__TotalSizeInB = "unity-objects-comparison-view__tree-view__column__total-size-in-b";
        const string k_UxmlIdentifier_TreeViewColumn__CountInA = "unity-objects-comparison-view__tree-view__column__count-in-a";
        const string k_UxmlIdentifier_TreeViewColumn__CountInB = "unity-objects-comparison-view__tree-view__column__count-in-b";
        const string k_UxmlIdentifier_FlattenToggle = "unity-objects-comparison-view__toolbar__flatten-toggle";
        const string k_UxmlIdentifier_UnchangedToggle = "unity-objects-comparison-view__toolbar__unchanged-toggle";
        const string k_UxmlIdentifier_LoadingOverlay = "unity-objects-comparison-view__loading-overlay";
        const string k_UxmlIdentifier_BaseTitleLabel = "unity-objects-comparison-view__secondary__base-title-label";
        const string k_UxmlIdentifier_BaseDescriptionLabel = "unity-objects-comparison-view__secondary__base-description-label";
        const string k_UxmlIdentifier_BaseViewContainer = "unity-objects-comparison-view__secondary__base-table-container";
        const string k_UxmlIdentifier_ComparedTitleLabel = "unity-objects-comparison-view__secondary__compared-title-label";
        const string k_UxmlIdentifier_ComparedDescriptionLabel = "unity-objects-comparison-view__secondary__compared-description-label";
        const string k_UxmlIdentifier_ComparedViewContainer = "unity-objects-comparison-view__secondary__compared-table-container";
        const string k_UxmlIdentifier_ErrorLabel = "unity-objects-comparison-view__error-label";
        const string k_ErrorMessage = "At least one snapshot is from an outdated Unity version that is not fully supported.";

        // Sort comparisons for each column.
        static readonly Dictionary<string, Comparison<TreeViewItemData<UnityObjectsComparisonModel.ItemData>>> k_SortComparisons = new()
        {
            { k_UxmlIdentifier_TreeViewColumn__Description, (x, y) => string.Compare(x.data.Name, y.data.Name, StringComparison.OrdinalIgnoreCase) },
            { k_UxmlIdentifier_TreeViewColumn__CountDelta, (x, y) => x.data.CountDelta.CompareTo(y.data.CountDelta) },
            { k_UxmlIdentifier_TreeViewColumn__SizeDelta, (x, y) => x.data.SizeDelta.CompareTo(y.data.SizeDelta) },
            { k_UxmlIdentifier_TreeViewColumn__SizeDeltaBar, (x, y) => x.data.SizeDelta.CompareTo(y.data.SizeDelta) },
            { k_UxmlIdentifier_TreeViewColumn__TotalSizeInA, (x, y) => x.data.TotalSizeInA.CompareTo(y.data.TotalSizeInA) },
            { k_UxmlIdentifier_TreeViewColumn__TotalSizeInB, (x, y) => x.data.TotalSizeInB.CompareTo(y.data.TotalSizeInB) },
            { k_UxmlIdentifier_TreeViewColumn__CountInA, (x, y) => x.data.CountInA.CompareTo(y.data.CountInA) },
            { k_UxmlIdentifier_TreeViewColumn__CountInB, (x, y) => x.data.CountInB.CompareTo(y.data.CountInB) },
        };

        // Model.
        readonly CachedSnapshot m_SnapshotA;
        readonly CachedSnapshot m_SnapshotB;
        readonly string m_Description;
        UnityObjectsComparisonModel m_Model;
        AsyncWorker<UnityObjectsComparisonModel> m_BuildModelWorker;

        // View.
        Label m_DescriptionLabel;
        ToolbarSearchField m_SearchField;
        Toggle m_UnchangedToggle;
        UnityEngine.UIElements.TwoPaneSplitView m_SplitView;
        DetailedSizeBar m_TotalSizeBarA;
        DetailedSizeBar m_TotalSizeBarB;
        MultiColumnTreeView m_TreeView;
        Toggle m_FlattenToggle;
        ActivityIndicatorOverlay m_LoadingOverlay;
        Label m_BaseTitleLabel;
        Label m_BaseDescriptionLabel;
        VisualElement m_BaseViewContainer;
        Label m_ComparedTitleLabel;
        Label m_ComparedDescriptionLabel;
        VisualElement m_ComparedViewContainer;
        Label m_ErrorLabel;

        // Child Controllers.
        UnityObjectsTableViewController m_BaseTableViewController;
        UnityObjectsTableViewController m_ComparedTableViewController;

        public UnityObjectsComparisonViewController(CachedSnapshot snapshotA, CachedSnapshot snapshotB, string description)
        {
            m_SnapshotA = snapshotA;
            m_SnapshotB = snapshotB;
            m_Description = description;
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
            m_SplitView.RegisterCallback<GeometryChangedEvent>(ConfigureSplitViewLayout);
            ConfigureComparisonTreeView();

            m_DescriptionLabel.text = m_Description;
            m_SearchField.RegisterValueChangedCallback(SearchBreakdown);
            m_SearchField.RegisterCallback<FocusOutEvent>(OnSearchFocusLost);
            m_FlattenToggle.text = "Flatten Hierarchy";
            m_FlattenToggle.RegisterValueChangedCallback(SetHierarchyFlattened);
            m_UnchangedToggle.text = "Show Unchanged";
            m_UnchangedToggle.RegisterValueChangedCallback(ApplyFilter);

            // Configure 'Base (A)' Unity Objects table.
            m_BaseTitleLabel.text = "Base";
            m_BaseTableViewController = new UnityObjectsTableViewController(
                m_SnapshotA,
                false,
                false,
                this)
            {
                FlattenHierarchy = true
            };
            m_BaseTableViewController.SetFilters(unityObjectInstanceIdFilter: CachedSnapshot.NativeObjectEntriesCache.InstanceIDNone);
            m_BaseViewContainer.Add(m_BaseTableViewController.View);
            AddChild(m_BaseTableViewController);

            // Configure 'Compared (B)' Unity Objects table.
            m_ComparedTitleLabel.text = "Compared";
            m_ComparedTableViewController = new UnityObjectsTableViewController(
                m_SnapshotB,
                false,
                false,
                this)
            {
                FlattenHierarchy = true
            };
            m_ComparedTableViewController.SetFilters(unityObjectInstanceIdFilter: CachedSnapshot.NativeObjectEntriesCache.InstanceIDNone);
            m_ComparedViewContainer.Add(m_ComparedTableViewController.View);
            AddChild(m_ComparedTableViewController);

            BuildModelAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_BuildModelWorker?.Dispose();

            base.Dispose(disposing);
        }

        void GatherReferencesInView(VisualElement view)
        {
            m_DescriptionLabel = view.Q<Label>(k_UxmlIdentifier_DescriptionLabel);
            m_SearchField = view.Q<ToolbarSearchField>(k_UxmlIdentifier_SearchField);
            m_SplitView = view.Q<UnityEngine.UIElements.TwoPaneSplitView>(k_UxmlIdentifier_SplitView);
            m_TotalSizeBarA = view.Q<DetailedSizeBar>(k_UxmlIdentifier_BaseTotalSizeBar);
            m_TotalSizeBarB = view.Q<DetailedSizeBar>(k_UxmlIdentifier_ComparedTotalSizeBar);
            m_TreeView = view.Q<MultiColumnTreeView>(k_UxmlIdentifier_TreeView);
            m_FlattenToggle = view.Q<Toggle>(k_UxmlIdentifier_FlattenToggle);
            m_UnchangedToggle = view.Q<Toggle>(k_UxmlIdentifier_UnchangedToggle);
            m_LoadingOverlay = view.Q<ActivityIndicatorOverlay>(k_UxmlIdentifier_LoadingOverlay);
            m_BaseTitleLabel = view.Q<Label>(k_UxmlIdentifier_BaseTitleLabel);
            m_BaseDescriptionLabel = view.Q<Label>(k_UxmlIdentifier_BaseDescriptionLabel);
            m_BaseViewContainer = view.Q<VisualElement>(k_UxmlIdentifier_BaseViewContainer);
            m_ComparedTitleLabel = view.Q<Label>(k_UxmlIdentifier_ComparedTitleLabel);
            m_ComparedDescriptionLabel = view.Q<Label>(k_UxmlIdentifier_ComparedDescriptionLabel);
            m_ComparedViewContainer = view.Q<VisualElement>(k_UxmlIdentifier_ComparedViewContainer);
            m_ErrorLabel = view.Q<Label>(k_UxmlIdentifier_ErrorLabel);
        }

        void ConfigureSplitViewLayout(GeometryChangedEvent evt)
        {
            // There is currently no way to set a split view's initial dimension as a percentage from UXML/USS, so we must do it manually once on load.
            m_SplitView.fixedPaneInitialDimension = m_SplitView.layout.height * 0.5f;
            m_SplitView.UnregisterCallback<GeometryChangedEvent>(ConfigureSplitViewLayout);
        }

        void ConfigureComparisonTreeView()
        {
            m_TreeView.RegisterCallback<GeometryChangedEvent>(ConfigureInitialComparisonTreeViewLayout);

            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Description, "Description", BindCellForDescriptionColumn(), UnityObjectsDescriptionCell.Instantiate);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__CountDelta, "Count Difference", BindCellForCountDeltaColumn(), CountDeltaCell.Instantiate);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__SizeDeltaBar, "Size Difference Bar", BindCellForSizeDeltaBarColumn(), DeltaBarCell.Instantiate);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__SizeDelta, "Size Difference", BindCellForSizeColumn(SizeType.SizeDelta), MakeSizeDeltaCell);
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__TotalSizeInA, "Size In A", BindCellForSizeColumn(SizeType.TotalSizeInA));
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__TotalSizeInB, "Size In B", BindCellForSizeColumn(SizeType.TotalSizeInB));
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__CountInA, "Count In A", BindCellForCountColumn(CountType.CountInA));
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__CountInB, "Count In B", BindCellForCountColumn(CountType.CountInB));

#if UNITY_2022_2_OR_NEWER
            m_TreeView.selectionChanged += OnComparisonTreeViewSelectionChanged;
#else
            m_TreeView.onSelectionChange += OnComparisonTreeViewSelectionChanged;
#endif
            m_TreeView.columnSortingChanged += OnTreeViewSortingChanged;
        }

        void ConfigureInitialComparisonTreeViewLayout(GeometryChangedEvent evt)
        {
            // There is currently no way to set a tree view column's initial width as a percentage from UXML/USS, so we must do it manually once on load.
            var column = m_TreeView.columns[k_UxmlIdentifier_TreeViewColumn__Description];
            column.width = m_TreeView.layout.width * 0.4f;
            m_TreeView.UnregisterCallback<GeometryChangedEvent>(ConfigureInitialComparisonTreeViewLayout);
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
            var snapshotA = m_SnapshotA;
            var snapshotB = m_SnapshotB;
            var unityObjectNameFilter = ContainsTextFilter.Create(m_SearchField.value);
            var flatten = m_FlattenToggle.value;
            var includeUnchanged = m_UnchangedToggle.value;
            var args = new UnityObjectsComparisonModelBuilder.BuildArgs(
                unityObjectNameFilter,
                flatten,
                includeUnchanged,
                OnUnityObjectNameGroupComparisonSelected,
                OnUnityObjectTypeComparisonSelected);
            var sortComparison = BuildSortComparisonFromTreeView();
            m_BuildModelWorker = new AsyncWorker<UnityObjectsComparisonModel>();
            m_BuildModelWorker.Execute(() =>
            {
                try
                {
                    // Build the data model.
                    var modelBuilder = new UnityObjectsComparisonModelBuilder();
                    var model = modelBuilder.Build(snapshotA, snapshotB, args);

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

                    if (model != null)
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

                    // Dispose asynchronous worker.
                    m_BuildModelWorker.Dispose();
                });
        }

        void RefreshView()
        {
            SetDetailedProgressBarValues(m_TotalSizeBarA, m_Model.TotalMemorySizeA, m_Model.TotalSnapshotAMemorySize);
            SetDetailedProgressBarValues(m_TotalSizeBarB, m_Model.TotalMemorySizeB, m_Model.TotalSnapshotBMemorySize);

            // Make size bars relative to each another.
            if (m_Model.TotalSnapshotAMemorySize < m_Model.TotalSnapshotBMemorySize)
            {
                var relativeSize = (float)m_Model.TotalSnapshotAMemorySize / m_Model.TotalSnapshotBMemorySize;
                m_TotalSizeBarA.Bar.style.width = new StyleLength(new Length(relativeSize * 100, LengthUnit.Percent));
            }
            else
            {
                var relativeSize = (float)m_Model.TotalSnapshotBMemorySize / m_Model.TotalSnapshotAMemorySize;
                m_TotalSizeBarB.Bar.style.width = new StyleLength(new Length(relativeSize * 100, LengthUnit.Percent));
            }

            m_TreeView.SetRootItems(m_Model.RootNodes);
            m_TreeView.Rebuild();
        }

        void SetDetailedProgressBarValues(DetailedSizeBar detailedSizeBar, ulong totalMemorySize, ulong totalSnapshotMemorySize)
        {
            var progress = (float)totalMemorySize / totalSnapshotMemorySize;
            detailedSizeBar.SetRelativeSize(progress);

            var totalMemorySizeText = EditorUtility.FormatBytes((long)totalMemorySize);
            detailedSizeBar.SetSizeText($"Total Memory In Table: {totalMemorySizeText}");

            var totalSnapshotMemorySizeText = EditorUtility.FormatBytes((long)totalSnapshotMemorySize);
            detailedSizeBar.SetTotalText($"Total Memory In Snapshot: {totalSnapshotMemorySizeText}");
        }

        Action<VisualElement, int> BindCellForDescriptionColumn()
        {
            const string k_NoName = "<No Name>";
            return (element, rowIndex) =>
            {
                var cell = (UnityObjectsDescriptionCell)element;
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(rowIndex);

                var typeName = itemData.TypeName;
                cell.SetTypeName(typeName);

                var displayText = itemData.Name;
                if (string.IsNullOrEmpty(displayText))
                    displayText = k_NoName;
                cell.SetText(displayText);

                string secondaryDisplayText;
                var childCount = itemData.ChildCount;
                if (childCount > 0)
                {
                    secondaryDisplayText = $"({childCount:N0} group{(childCount != 1 ? "s" : string.Empty)})";
                }
                else
                {
                    if (itemData.CountInA != itemData.CountInB)
                        secondaryDisplayText = $"({itemData.CountInA:N0} → {itemData.CountInB:N0} object{(itemData.CountInB != 1 ? "s" : string.Empty)})";
                    else
                        secondaryDisplayText = $"({itemData.CountInA:N0} object{(itemData.CountInA != 1 ? "s" : string.Empty)})";
                }
                cell.SetSecondaryText(secondaryDisplayText);
            };
        }

        Action<VisualElement, int> BindCellForSizeDeltaBarColumn()
        {
            return (element, rowIndex) =>
            {
                var cell = (DeltaBarCell)element;
                var sizeDelta = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(rowIndex).SizeDelta;
                var proportionalSizeDelta = 0f;
                if (sizeDelta != 0)
                    proportionalSizeDelta = (float)sizeDelta / m_Model.LargestAbsoluteSizeDelta;
                cell.SetDeltaScalar(proportionalSizeDelta);
                cell.tooltip = FormatBytes(sizeDelta);
            };
        }

        Action<VisualElement, int> BindCellForCountDeltaColumn()
        {
            return (element, rowIndex) =>
            {
                var cell = (CountDeltaCell)element;
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(rowIndex);
                var countDelta = itemData.CountDelta;
                cell.SetCountDelta(countDelta);
            };
        }

        Action<VisualElement, int> BindCellForSizeColumn(SizeType sizeType)
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(rowIndex);
                var size = sizeType switch
                {
                    SizeType.SizeDelta => itemData.SizeDelta,
                    SizeType.TotalSizeInA => Convert.ToInt64(itemData.TotalSizeInA),
                    SizeType.TotalSizeInB => Convert.ToInt64(itemData.TotalSizeInB),
                    _ => throw new ArgumentException("Unknown size type."),
                };

                ((Label)element).text = FormatBytes(size);
            };
        }

        Action<VisualElement, int> BindCellForCountColumn(CountType countType)
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(rowIndex);
                var count = countType switch
                {
                    CountType.CountInA => Convert.ToInt32(itemData.CountInA),
                    CountType.CountInB => Convert.ToInt32(itemData.CountInB),
                    _ => throw new ArgumentException("Unknown count type."),
                };

                ((Label)element).text = $"{count:N0}";
            };
        }

        VisualElement MakeSizeDeltaCell()
        {
            var cell = new Label();
            cell.AddToClassList("unity-multi-column-view__cell__label");

            // Make this a cell with a darkened background. This requires quite a bit of styling to be compatible with tree view selection styling, so that is why it is its own class.
            cell.AddToClassList("dark-tree-view-cell");

            return cell;
        }

        void SearchBreakdown(ChangeEvent<string> evt)
        {
            BuildModelAsync();
        }

        void OnSearchFocusLost(FocusOutEvent evt)
        {
            MemoryProfilerAnalytics.AddInteractionCountToEvent<MemoryProfilerAnalytics.InteractionsInPage, MemoryProfilerAnalytics.PageInteractionType>(
                MemoryProfilerAnalytics.PageInteractionType.SearchInPageWasUsed);
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

        void ApplyFilter(ChangeEvent<bool> evt)
        {
            BuildModelAsync();
            MemoryProfilerAnalytics.AddInteractionCountToEvent<MemoryProfilerAnalytics.InteractionsInPage, MemoryProfilerAnalytics.PageInteractionType>(
                evt.newValue ? MemoryProfilerAnalytics.PageInteractionType.UnchangedFilterWasApplied : MemoryProfilerAnalytics.PageInteractionType.UnchangedFilterWasRemoved);
        }

        void SetHierarchyFlattened(ChangeEvent<bool> evt)
        {
            BuildModelAsync();
            MemoryProfilerAnalytics.AddInteractionCountToEvent<MemoryProfilerAnalytics.InteractionsInPage, MemoryProfilerAnalytics.PageInteractionType>(
                evt.newValue ? MemoryProfilerAnalytics.PageInteractionType.TreeViewWasFlattened : MemoryProfilerAnalytics.PageInteractionType.TreeViewWasUnflattened);
        }

        void OnComparisonTreeViewSelectionChanged(IEnumerable<object> items)
        {
            // Invoke the selection processor for the selected item.
            var selectedIndex = m_TreeView.selectedIndex;
            var itemData = m_TreeView.GetItemDataForIndex<UnityObjectsComparisonModel.ItemData>(selectedIndex);
            itemData.SelectionProcessor?.Invoke();
        }

        void OnUnityObjectNameGroupComparisonSelected(string objectName, string typeName)
        {
            m_BaseTableViewController.ClearSelection();
            m_ComparedTableViewController.ClearSelection();
            ClearSelectionInDetailsView();

            var objectNameFilter = MatchesTextFilter.Create(objectName);
            var typeNameFilter = MatchesTextFilter.Create(typeName);
            m_BaseTableViewController.SetFilters(objectNameFilter, typeNameFilter);
            m_ComparedTableViewController.SetFilters(objectNameFilter, typeNameFilter);
        }

        void OnUnityObjectTypeComparisonSelected(string nativeTypeName)
        {
            ClearSelectionInDetailsView();
            m_BaseTableViewController.SetFilters(
                unityObjectInstanceIdFilter: CachedSnapshot.NativeObjectEntriesCache.InstanceIDNone);
            m_ComparedTableViewController.SetFilters(
                unityObjectInstanceIdFilter: CachedSnapshot.NativeObjectEntriesCache.InstanceIDNone);
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerReloaded(
            UnityObjectsTableViewController tableViewController,
            bool success)
        {
            var isBaseTable = tableViewController == m_BaseTableViewController;
            var descriptionLabel = (isBaseTable) ? m_BaseDescriptionLabel : m_ComparedDescriptionLabel;
            var tableIsFilteringExplicitlyForNoObjects =
                tableViewController.UnityObjectInstanceIdFilter.HasValue &&
                tableViewController.UnityObjectInstanceIdFilter == CachedSnapshot.NativeObjectEntriesCache.InstanceIDNone;
            if (!tableIsFilteringExplicitlyForNoObjects)
            {
                var model = tableViewController.Model;
                var objectCount = model.RootNodes.Count;
                descriptionLabel.text = $"{objectCount:N0} object{(objectCount != 1 ? "s" : string.Empty)} with same name | Group size: {EditorUtility.FormatBytes(Convert.ToInt64(model.TotalMemorySize))}";
            }

            // Hide the description if the table is explicitly filtering for 'no objects'.
            UIElementsHelper.SetElementDisplay(descriptionLabel, !tableIsFilteringExplicitlyForNoObjects);
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerSelectedUnityObjectItem(
            UnityObjectsTableViewController viewController,
            int nativeObjectInstanceId,
            UnityObjectsModel.ItemData itemData)
        {
            var isBaseTable = viewController == m_BaseTableViewController;
            var snapshot = (isBaseTable) ? m_SnapshotA : m_SnapshotB;
            var snapshotType = (isBaseTable) ? SnapshotType.Base : SnapshotType.Compared;
            var otherViewController = (isBaseTable) ? m_ComparedTableViewController : m_BaseTableViewController;

            SelectUnityObjectInDetailsView(nativeObjectInstanceId, snapshot, snapshotType);
            otherViewController.ClearSelection();
        }

        void UnityObjectsTableViewController.IResponder.UnityObjectsTableViewControllerSelectedUnityObjectTypeItem(
            UnityObjectsTableViewController viewController,
            int nativeTypeIndex,
            UnityObjectsModel.ItemData itemData)
        {
            // Cannot select Unity Object types.
        }

        // Temporary integration with legacy history/selection API to show UI for the selected item in the Details view.
        static void SelectUnityObjectInDetailsView(int nativeObjectInstanceId, CachedSnapshot snapshot, SnapshotType snapshotType)
        {
            // When a user switches the snapshots, it seems the existing implementation makes the second snapshot the 'older' snapshot and the first snapshot the 'newer' snapshot. Then, in MemorySampleSelection.GetSnapshotItemIsPresentIn, if the first snapshot is 'newer', SnapshotAge.Older will return the second snapshot instead of the first. Therefore, which SnapshotAge we want to pass is not 'Older' for Base/A and 'Newer' for Compared/B, but instead changes depending whether the snapshots have been switched or not. As an aside, this appears to have no relation to the actual time of the snapshots either. Therefore 'Older' and 'Newer' is extremely confusing terminology that we should remove.
            SnapshotAge snapshotAge;
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            if (window.UIState.FirstSnapshotAge == SnapshotAge.Newer)
            {
                snapshotAge = snapshotType switch
                {
                    SnapshotType.Base => SnapshotAge.Newer,
                    SnapshotType.Compared => SnapshotAge.Older,
                    _ => throw new ArgumentException("Unknown snapshot type.")
                };
            }
            else
            {
                snapshotAge = snapshotType switch
                {
                    SnapshotType.Base => SnapshotAge.Older,
                    SnapshotType.Compared => SnapshotAge.Newer,
                    _ => throw new ArgumentException("Unknown snapshot type.")
                };
            }

            // Convert to a unified object to get Unified Object UI in Details view.
            var nativeObjectIndex = snapshot.NativeObjects.instanceId2Index[nativeObjectInstanceId];
            var unifiedObjectIndex = ObjectData.FromNativeObjectIndex(
                snapshot,
                Convert.ToInt32(nativeObjectIndex))
                .GetUnifiedObjectIndex(snapshot);
            var selection = MemorySampleSelection.FromUnifiedObjectIndex(unifiedObjectIndex, snapshotAge);
            window.UIState.RegisterSelectionChangeEvent(selection);
        }

        // Temporary integration with legacy history/selection API to clear UI for the selected item in the Details view.
        static void ClearSelectionInDetailsView()
        {
            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            window.UIState.ClearSelection(MemorySampleSelectionRank.MainSelection);
        }

        Comparison<TreeViewItemData<UnityObjectsComparisonModel.ItemData>> BuildSortComparisonFromTreeView()
        {
            var sortedColumns = m_TreeView.sortedColumns;
            if (sortedColumns == null)
                return null;

            var sortComparisons = new List<Comparison<TreeViewItemData<UnityObjectsComparisonModel.ItemData>>>();
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

        static string FormatBytes(long bytes)
        {
            var sizeText = new System.Text.StringBuilder();

            // Our built-in formatter for bytes doesn't support negative values.
            if (bytes < 0)
                sizeText.Append("-");

            var absoluteBytes = Math.Abs(bytes);
            sizeText.Append(EditorUtility.FormatBytes(absoluteBytes));
            return sizeText.ToString();
        }

        enum SizeType
        {
            SizeDelta,
            TotalSizeInA,
            TotalSizeInB,
        }

        enum CountType
        {
            CountInA,
            CountInB,
        }

        enum SnapshotType
        {
            Base,
            Compared
        }
    }
}
#endif
