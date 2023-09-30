#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    class AllSystemMemoryBreakdownViewController : ViewController
    {
        const string k_UxmlAssetGuid = "bc16108acf3b6484aa65bf05d6048e8f";
        const string k_UssClass_Dark = "all-system-memory-breakdown-view__dark";
        const string k_UssClass_Light = "all-system-memory-breakdown-view__light";
        const string k_UxmlIdentifier_SearchField = "all-system-memory-breakdown-view__search-field";
        const string k_UxmlIdentifier_TotalBar = "all-system-memory-breakdown-view__total__bar";
        const string k_UxmlIdentifier_TotalInTableLabel = "all-system-memory-breakdown-view__total-footer__table-label";
        const string k_UxmlIdentifier_TotalInSnapshotLabel = "all-system-memory-breakdown-view__total-footer__snapshot-label";
        const string k_UxmlIdentifier_TreeView = "all-system-memory-breakdown-view__tree-view";
        const string k_UxmlIdentifier_TreeViewColumn__AddressBeg = "all-system-memory-breakdown-view__tree-view__column__address-beg";
        const string k_UxmlIdentifier_TreeViewColumn__AddressEnd = "all-system-memory-breakdown-view__tree-view__column__address-end";
        const string k_UxmlIdentifier_TreeViewColumn__Size = "all-system-memory-breakdown-view__tree-view__column__size";
        const string k_UxmlIdentifier_TreeViewColumn__ResidentSize = "all-system-memory-breakdown-view__tree-view__column__residentsize";
        const string k_UxmlIdentifier_TreeViewColumn__Type = "all-system-memory-breakdown-view__tree-view__column__type";
        const string k_UxmlIdentifier_TreeViewColumn__Name = "all-system-memory-breakdown-view__tree-view__column__name";
        const string k_UxmlIdentifier_LoadingOverlay = "all-system-memory-breakdown-view__loading-overlay";
        const string k_UxmlIdentifier_ErrorLabel = "all-system-memory-breakdown-view__error-label";
        const string k_ErrorMessage = "Snapshot is from an outdated Unity version that is not fully supported.";

        // Model.
        readonly CachedSnapshot m_Snapshot;
        AllSystemMemoryBreakdownModel m_Model;
        AsyncWorker<AllSystemMemoryBreakdownModel> m_BuildModelWorker;

        // View.
        ToolbarSearchField m_SearchField;
        ProgressBar m_TotalBar;
        Label m_TotalInTableLabel;
        Label m_TotalInSnapshotLabel;
        MultiColumnTreeView m_TreeView;
        ActivityIndicatorOverlay m_LoadingOverlay;
        Label m_ErrorLabel;

        public AllSystemMemoryBreakdownViewController(CachedSnapshot snapshot)
        {
            m_Snapshot = snapshot;
        }

        protected override VisualElement LoadView()
        {
            var view = ViewControllerUtility.LoadVisualTreeFromUxml(k_UxmlAssetGuid);
            if (view == null)
                throw new InvalidOperationException("Unable to create view from Uxml. Uxml must contain at least one child element.");
            view.style.flexGrow = 1;

            var themeUssClass = (EditorGUIUtility.isProSkin) ? k_UssClass_Dark : k_UssClass_Light;
            view.AddToClassList(themeUssClass);

            return view;
        }

        protected override void ViewLoaded()
        {
            GatherViewReferences();
            ConfigureTreeView();

            m_SearchField.RegisterValueChangedCallback(OnSearchValueChanged);

            // These styles are not supported in Unity 2020 and earlier. They will cause project errors if included in the stylesheet in those Editor versions.
            // Remove when we drop support for <= 2020 and uncomment these styles in the stylesheet.
            var transitionDuration = new StyleList<TimeValue>(new List<TimeValue>() { new TimeValue(0.23f) });
            var transitionTimingFunction = new StyleList<EasingFunction>(new List<EasingFunction>() { new EasingFunction(EasingMode.EaseOut) });
            m_TotalBar.Fill.style.transitionDuration = transitionDuration;
            m_TotalBar.Fill.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName>() { new StylePropertyName("width") });
            m_TotalBar.Fill.style.transitionTimingFunction = transitionTimingFunction;
            m_LoadingOverlay.style.transitionDuration = transitionDuration;
            m_LoadingOverlay.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName>() { new StylePropertyName("opacity") });
            m_LoadingOverlay.style.transitionTimingFunction = transitionTimingFunction;

            BuildModelAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_BuildModelWorker?.Dispose();

            base.Dispose(disposing);
        }

        void GatherViewReferences()
        {
            m_SearchField = View.Q<ToolbarSearchField>(k_UxmlIdentifier_SearchField);
            m_TotalBar = View.Q<ProgressBar>(k_UxmlIdentifier_TotalBar);
            m_TotalInTableLabel = View.Q<Label>(k_UxmlIdentifier_TotalInTableLabel);
            m_TotalInSnapshotLabel = View.Q<Label>(k_UxmlIdentifier_TotalInSnapshotLabel);
            m_TreeView = View.Q<MultiColumnTreeView>(k_UxmlIdentifier_TreeView);
            m_LoadingOverlay = View.Q<ActivityIndicatorOverlay>(k_UxmlIdentifier_LoadingOverlay);
            m_ErrorLabel = View.Q<Label>(k_UxmlIdentifier_ErrorLabel);
        }

        void ConfigureTreeView()
        {
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__AddressBeg, "Start", 180, BindCellForAddressStartColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__AddressEnd, "End", 180, BindCellForAddressEndColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Size, "Size", 80, BindCellForSizeColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__ResidentSize, "Resident Size", 80, BindCellForResidentSizeColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Type, "Type", 180, BindCellForTypeColumn());
            ConfigureTreeViewColumn(k_UxmlIdentifier_TreeViewColumn__Name, "Name", 0, BindCellForNameColumn());

#if UNITY_2022_2_OR_NEWER
            m_TreeView.selectionChanged += OnTreeViewSelectionChanged;
#else
            m_TreeView.onSelectionChange += OnTreeViewSelectionChanged;
#endif
            m_TreeView.columnSortingChanged += OnTreeViewSortingChanged;
        }

        void ConfigureTreeViewColumn(string columnName, string columnTitle, int width, Action<VisualElement, int> bindCell, Func<VisualElement> makeCell = null)
        {
            var column = m_TreeView.columns[columnName];
            column.title = columnTitle;
            column.bindCell = bindCell;
            if (width != 0)
            {
                column.width = width;
                column.minWidth = width;
            }
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
            var nameFilter = m_SearchField.value;
            var args = new AllSystemMemoryBreakdownModelBuilder.BuildArgs(nameFilter);
            var sortDescriptors = BuildSortDescriptorsFromTreeView();
            m_BuildModelWorker = new AsyncWorker<AllSystemMemoryBreakdownModel>();
            m_BuildModelWorker.Execute(() =>
            {
                try
                {
                    // Build the data model.
                    var modelBuilder = new AllSystemMemoryBreakdownModelBuilder();
                    var model = modelBuilder.Build(snapshot, args);

                    // Sort it according to the current sort descriptors.
                    model.Sort(sortDescriptors);

                    return model;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
            });
        }

        void RefreshView()
        {
            //var totalCommittedText = EditorUtility.FormatBytes((long)m_Model.TotalCommitted);
            //var totalResidentText = EditorUtility.FormatBytes((long)m_Model.TotalResident);
            m_TotalInTableLabel.text = ""; //$"Total Committed: {totalCommittedText} | Resident: {totalResidentText}";

            m_TreeView.SetRootItems(m_Model.RootNodes);
            m_TreeView.Rebuild();
        }

        Action<VisualElement, int> BindCellForAddressStartColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                ((Label)element).text = $"{(long)itemData.Address:X16}";
            };
        }

        Action<VisualElement, int> BindCellForAddressEndColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                ((Label)element).text = $"{(long)(itemData.Address + itemData.Size):X16}";
            };
        }
        Action<VisualElement, int> BindCellForSizeColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                ((Label)element).text = EditorUtility.FormatBytes((long)itemData.Size);
            };
        }

        Action<VisualElement, int> BindCellForResidentSizeColumn()
        {
            return (element, rowIndex) =>
            {
                if (m_TreeView.GetParentIdForIndex(rowIndex) == -1)
                {
                    var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                    ((Label)element).text = EditorUtility.FormatBytes((long)itemData.ResidentSize);
                }
                else
                    ((Label)element).text = "";
            };
        }

        Action<VisualElement, int> BindCellForTypeColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                string labelText = itemData.ItemType;
                ((Label)element).text = labelText;
            };
        }

        Action<VisualElement, int> BindCellForNameColumn()
        {
            return (element, rowIndex) =>
            {
                var itemData = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(rowIndex);
                ((Label)element).text = itemData.Name;
            };
        }

        void OnSearchValueChanged(ChangeEvent<string> evt)
        {
            BuildModelAsync();
        }

        void OnTreeViewSortingChanged()
        {
            BuildModelAsync();
        }

        void OnTreeViewSelectionChanged(IEnumerable<object> items)
        {
            var selectedIndex = m_TreeView.selectedIndex;
            var data = m_TreeView.GetItemDataForIndex<AllSystemMemoryBreakdownModel.ItemData>(selectedIndex);
            var dataIndex = data.DataSource;

            // Temporary integration with legacy history/selection API to show UI for some selected items in the Details view.
            var selection = MemorySampleSelection.InvalidMainSelection;
            switch (dataIndex.Id)
            {
                case CachedSnapshot.SourceLink.SourceId.NativeMemoryRegion:
                {
                    var nativeTypeIndex = dataIndex.Index;
                    selection = MemorySampleSelection.FromNativeTypeIndex(nativeTypeIndex);
                    break;
                }

                case CachedSnapshot.SourceLink.SourceId.ManagedHeapSection:
                {
                    var managedObjectIndex = dataIndex.Index;
                    selection = MemorySampleSelection.FromManagedObjectIndex(managedObjectIndex);
                    break;
                }

                default:
                    break;
            }

            var window = EditorWindow.GetWindow<MemoryProfilerWindow>();
            if (!selection.Equals(MemorySampleSelection.InvalidMainSelection))
                window.UIState.RegisterSelectionChangeEvent(selection);
            else
                window.UIState.ClearSelection(MemorySampleSelectionRank.MainSelection);
        }

        IEnumerable<AllSystemMemoryBreakdownModel.SortDescriptor> BuildSortDescriptorsFromTreeView()
        {
            var sortDescriptors = new List<AllSystemMemoryBreakdownModel.SortDescriptor>();

            var sortedColumns = m_TreeView.sortedColumns;
            using (var enumerator = sortedColumns.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var sortDescription = enumerator.Current;
                    var sortProperty = ColumnNameToSortbleItemDataProperty(sortDescription.columnName);
                    var sortDirection = (sortDescription.direction == SortDirection.Ascending) ?
                        AllSystemMemoryBreakdownModel.SortDirection.Ascending : AllSystemMemoryBreakdownModel.SortDirection.Descending;
                    var sortDescriptor = new AllSystemMemoryBreakdownModel.SortDescriptor(sortProperty, sortDirection);
                    sortDescriptors.Add(sortDescriptor);
                }
            }

            return sortDescriptors;
        }

        AllSystemMemoryBreakdownModel.SortableItemDataProperty ColumnNameToSortbleItemDataProperty(string columnName)
        {
            switch (columnName)
            {
                case k_UxmlIdentifier_TreeViewColumn__AddressBeg:
                    return AllSystemMemoryBreakdownModel.SortableItemDataProperty.Address;

                case k_UxmlIdentifier_TreeViewColumn__Size:
                    return AllSystemMemoryBreakdownModel.SortableItemDataProperty.Size;

                case k_UxmlIdentifier_TreeViewColumn__ResidentSize:
                    return AllSystemMemoryBreakdownModel.SortableItemDataProperty.ResidentSize;

                case k_UxmlIdentifier_TreeViewColumn__Name:
                    return AllSystemMemoryBreakdownModel.SortableItemDataProperty.Name;

                case k_UxmlIdentifier_TreeViewColumn__Type:
                    return AllSystemMemoryBreakdownModel.SortableItemDataProperty.Type;

                default:
                    throw new ArgumentException("Unable to sort. Unknown column name.");
            }
        }
    }
}
#endif
