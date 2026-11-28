#if UNITY_2022_1_OR_NEWER
using System.Linq;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.NetworkProfiler.Editor.NoDataView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    class TreeViewNetwork : VisualElement
    {
        public enum DisplayType
        {
            Messages,
            Activity,
        }

        readonly TreeModel m_TreeModel;
        TreeView m_InnerTreeView;
        VisualElement m_TreeViewContainer;
        SortDirection m_SortDirection;
        MultiColumnTreeView m_MultiColumnTreeView = new MultiColumnTreeView();

        public TreeViewNetwork(TreeModel treeModel)
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPaths.Column);
            var root = tree.CloneTree();
            InitializeMcTreeView(root);

            m_TreeModel = treeModel;

            style.fontSize = 12;
            style.flexDirection = FlexDirection.Row;

            this.StretchToParentSize();
        }

        /// <summary>
        /// This method is used to initialize the MultiColumnTreeView
        /// </summary>
        /// <param name="root"></param>
        void InitializeMcTreeView(TemplateContainer root)
        {
            m_MultiColumnTreeView = root.Q<MultiColumnTreeView>();
            m_MultiColumnTreeView.columns["NameColumn"].makeCell = () => new Label();
            m_MultiColumnTreeView.columns["TypeColumn"].makeCell = () => new Label();
            m_MultiColumnTreeView.columns["BytesSentColumn"].makeCell = () => new Label();
            m_MultiColumnTreeView.columns["BytesReceivedColumn"].makeCell = () => new Label();

            // For each column, set Column.bindCell to bind an initialized node to a data item.
            m_MultiColumnTreeView.columns["NameColumn"].bindCell = (VisualElement element, int index) =>
                (element as Label).text = m_MultiColumnTreeView.GetItemDataForIndex<IRowData>(index).Name;
            m_MultiColumnTreeView.columns["TypeColumn"].bindCell = (VisualElement element, int index) =>
                (element as Label).text = m_MultiColumnTreeView.GetItemDataForIndex<IRowData>(index).TypeName;
            m_MultiColumnTreeView.columns["BytesSentColumn"].bindCell = (VisualElement element, int index) =>
                (element as Label).text = m_MultiColumnTreeView.GetItemDataForIndex<IRowData>(index).Bytes.Sent.ToString();
            m_MultiColumnTreeView.columns["BytesReceivedColumn"].bindCell = (VisualElement element, int index) =>
                (element as Label).text = m_MultiColumnTreeView.GetItemDataForIndex<IRowData>(index).Bytes.Received.ToString();
#if UNITY_2023_2_OR_NEWER
            m_MultiColumnTreeView.sortingMode = ColumnSortingMode.Default;
#else
            m_MultiColumnTreeView.sortingEnabled = true;
#endif
            m_MultiColumnTreeView.CollapseAll();
            m_MultiColumnTreeView.columnSortingChanged += OnColumnSortingChanged;
            m_MultiColumnTreeView.itemsChosen += OnItemsChosen;
            m_MultiColumnTreeView.selectionType = SelectionType.Multiple;
        }

        /// <summary>
        /// Double-click on the MCView Element will highlight the obj in the hireacrhy
        /// </summary>
        /// <param name="obj"></param>
        void OnItemsChosen(IEnumerable<object> obj)
        {
            var item = m_MultiColumnTreeView.selectedItem as IRowData;
            item?.OnSelectedCallback?.Invoke();
        }

        /// <summary>
        /// Show DetailsView if there is data to show on the frame, otherwise show no info message
        /// </summary>
        public void Show()
        {
            if (m_TreeModel.HasData)
            {
                BuildTreeView(m_SortDirection);
            }
            else
            {
                var profilerNoDataElement = new ProfilerNoData();
                Add(profilerNoDataElement);
            }
        }

        /// <summary>
        /// Rebuild MultiColumnTreeView after sorting changes according to TreeVIew 
        /// </summary>
        void BuildMultiColumnTreeView()
        {
            var rootItems = SortAndStructureData(m_SortDirection, m_TreeModel);

            m_InnerTreeView?.RemoveFromHierarchy();
            m_InnerTreeView = new TreeView(rootItems, 20, MakeItem, BindItem);

            foreach (var item in rootItems)
            {
                SetExpandedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
                SetSelectedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
            }

            UpdateMultiColumnTreeView(m_InnerTreeView);
        }

        void BuildTreeView(SortDirection sort)
        {
            var rootItems = SortAndStructureData(sort, m_TreeModel);
            UpdateTreeView(rootItems);
        }

        static List<ITreeViewItem> SortAndStructureData(SortDirection sort, TreeModel tree)
        {
            tree.SortChildren(sort);
            return CreateTreeViewItemsFromTreeData(tree);
        }

        void UpdateTreeView(IList<ITreeViewItem> rootItems)
        {
            m_InnerTreeView?.RemoveFromHierarchy();
            m_InnerTreeView = new TreeView(rootItems, 20, MakeItem, BindItem);

            foreach (var item in rootItems)
            {
                SetExpandedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
                SetSelectedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
            }

            AddTreeView(m_InnerTreeView);
        }

        static void SetExpandedStateRecursive(TreeView treeView, TreeViewItem<IRowData> item)
        {
            var uniquePath = item.data.TreeViewPath + item.data.Id;
            var expandedState = DetailsViewPersistentState.IsFoldedOut(uniquePath);
            if (expandedState)
            {
                treeView.ExpandItem(item.id);
            }
            else
            {
                treeView.CollapseItem(item.id);
            }

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    SetExpandedStateRecursive(treeView, (TreeViewItem<IRowData>) child);
                }
            }
        }

        static void SetSelectedStateRecursive(TreeView treeView, TreeViewItem<IRowData> item)
        {
            // Check if the item is selected by using both path and id to avoid conflicts
            var uniquePath = item.data.TreeViewPath + item.data.Id;
            var selectedState = DetailsViewPersistentState.IsSelected(uniquePath);
            if (selectedState)
            {
                treeView.AddToSelection(item.id);
            }
            else
            {
                treeView.RemoveFromSelection(item.id);
            }

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    SetSelectedStateRecursive(treeView, (TreeViewItem<IRowData>) child);
                }
            }
        }

        static TreeViewItemData<IRowData> ConvertToTreeViewItemData(TreeViewItem<IRowData> node)
        {
            if (!node.hasChildren)
            {
                return new TreeViewItemData<IRowData>(node.id, node.data);
            }

            var childDataList = new List<TreeViewItemData<IRowData>>();
            foreach (var treeViewItemChild in node.children)
            {
                var rowDataChild = (TreeViewItem<IRowData>) treeViewItemChild;
                var childData = ConvertToTreeViewItemData(rowDataChild);

                childDataList.Add(childData);
            }

            return new TreeViewItemData<IRowData>(node.id, node.data, childDataList);
        }

        void UpdateMultiColumnTreeView(TreeView treeView)
        {
            // Update data in TreeView
            var treeViewItemsList = treeView.rootItems.Select(rootItem => rootItem as TreeViewItem<IRowData>).Select(ConvertToTreeViewItemData).ToList();
            m_MultiColumnTreeView.SetRootItems(treeViewItemsList);

            // Update Expanded-FoldoutState 
            m_MultiColumnTreeView.CollapseAll();
            foreach (var expandedItemId in treeView.expandedItemIds)
            {
                m_MultiColumnTreeView.ExpandItem(expandedItemId);
            }

            // Update SelectedState
            m_MultiColumnTreeView.ClearSelection();
            foreach (var selectedItem in treeView.selectedItems)
            {
                m_MultiColumnTreeView.AddToSelectionById(selectedItem.id);
            }

            m_MultiColumnTreeView.Rebuild();
        }

        void AddTreeView(TreeView treeView)
        {
            var treeViewItemDataList = new List<TreeViewItemData<IRowData>>();
            foreach (var item in treeView.rootItems)
            {
                var rootItem = item as TreeViewItem<IRowData>;
                treeViewItemDataList.Add(ConvertToTreeViewItemData(rootItem));
            }

            m_MultiColumnTreeView.SetRootItems<IRowData>(treeViewItemDataList);
            m_TreeViewContainer?.RemoveFromHierarchy();

            m_TreeViewContainer = new VisualElement
            {
                name = "TreeView Container",
                style =
                {
                    flexGrow = 1f,
                    flexShrink = 0f,
                    flexBasis = 0f
                }
            };

            m_TreeViewContainer.Add(m_MultiColumnTreeView);

            Add(m_TreeViewContainer);
        }

        void SaveMultiColumnTreeViewState()
        {
            for (var index = 0; index < m_InnerTreeView.items.Count(); index++)
            {
                var isMultiColumnExpanded = m_MultiColumnTreeView.IsExpanded(index);
                if (m_InnerTreeView.FindItem(index) is TreeViewItem<IRowData> item)
                {
                    // The unique path is used to identify the item in the persistent state
                    var treeViewId = item.data.Id;
                    var treeViewPath = item.data.TreeViewPath;
                    var locator = treeViewPath + treeViewId;
                    DetailsViewPersistentState.SetFoldout(locator, isMultiColumnExpanded);
                }
            }

            var pathList = new List<string>(m_MultiColumnTreeView.selectedIndices.Count());
            var networkIdList = new List<ulong>(m_MultiColumnTreeView.selectedIndices.Count());
            foreach (var index in m_MultiColumnTreeView.selectedIndices)
            {
                var itemData = m_MultiColumnTreeView.GetItemDataForIndex<IRowData>(index);
                pathList.Add(itemData.TreeViewPath);
                networkIdList.Add(itemData.Id);
            }

            DetailsViewPersistentState.SetSelected(pathList, networkIdList);
        }

        void OnColumnSortingChanged()
        {
            SaveMultiColumnTreeViewState();

            if (m_MultiColumnTreeView.sortColumnDescriptions.Count == 0)
            {
                return;
            }

            var whichColumn = m_MultiColumnTreeView.sortColumnDescriptions[0].columnName;

            m_SortDirection = whichColumn switch
            {
                "NameColumn" => m_SortDirection switch
                {
                    SortDirection.NameAscending => SortDirection.NameDescending,
                    SortDirection.NameDescending => SortDirection.NameAscending,
                    _ => SortDirection.NameAscending
                },
                "TypeColumn" => m_SortDirection switch
                {
                    SortDirection.TypeAscending => SortDirection.TypeDescending,
                    SortDirection.TypeDescending => SortDirection.TypeAscending,
                    _ => SortDirection.TypeAscending
                },
                "BytesSentColumn" => m_SortDirection switch
                {
                    SortDirection.BytesSentAscending => SortDirection.BytesSentDescending,
                    SortDirection.BytesSentDescending => SortDirection.BytesSentAscending,
                    _ => SortDirection.BytesSentAscending
                },
                "BytesReceivedColumn" => m_SortDirection switch
                {
                    SortDirection.BytesReceivedAscending => SortDirection.BytesReceivedDescending,
                    SortDirection.BytesReceivedDescending => SortDirection.BytesReceivedAscending,
                    _ => SortDirection.BytesReceivedAscending
                },
                _ => m_SortDirection
            };

            BuildMultiColumnTreeView();
        }

        static VisualElement MakeItem()
        {
            return new DetailsViewRow();
        }

        static void BindItem(VisualElement element, ITreeViewItem item)
        {
            (element as DetailsViewRow)?.BindItem(item);
        }

        static List<ITreeViewItem> CreateTreeViewItemsFromTreeData(TreeModel tree)
        {
            var nextId = 0;
            return tree.Children.Select(child => CreateTreeViewItemsRecursive(child, ref nextId)).ToList();
        }

        static ITreeViewItem CreateTreeViewItemsRecursive(TreeModelNode node, ref int incrementalId)
        {
            var item = new TreeViewItem<IRowData>(incrementalId++, node.RowData);
            foreach (var child in node.Children)
            {
                item.AddChild(CreateTreeViewItemsRecursive(child, ref incrementalId));
            }

            return item;
        }

        public void RefreshSelected()
        {
            if (m_InnerTreeView != null)
            {
                BuildMultiColumnTreeView();
            }
        }

        /// <summary>
        /// Path to MultiColumnView uxml
        /// </summary>
        static class VisualTreeAssetPaths
        {
            public const string Column =
                "Packages/com.unity.multiplayer.tools/NetworkProfiler/Editor/Windows/TreeView/MultiColumnTreeViewNetwork.uxml";
        }
    }
}
#endif
