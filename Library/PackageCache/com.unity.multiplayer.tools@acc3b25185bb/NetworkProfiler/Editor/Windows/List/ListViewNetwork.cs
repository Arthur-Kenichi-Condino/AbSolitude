using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    class ListViewNetwork : VisualElement
    {
        readonly ToolbarBreadcrumbs m_SelectedItemPath = new ToolbarBreadcrumbs
        {
            style =
            {
                flexGrow = 1,
                position = Position.Absolute,
                bottom = 0,
                left = 0,
            }
        };

        internal ListViewNetwork(IReadOnlyCollection<IRowData> connections)
        {
            connections ??= new List<IRowData>();

            style.fontSize = 14;
            style.flexDirection = FlexDirection.Row;

            this.StretchToParentSize();

            if (connections.Count > 0)
            {
                BuildListView(connections);
            }
        }

        void BuildListView(IReadOnlyCollection<IRowData> connections)
        {
            var nextId = 0;
            var rootItems = GenerateRowList(connections, ref nextId);

            var inlineTreeView = new TreeView(
                rootItems,
                20,
                () => new DetailsViewRow(),
                (element, item) => (element as DetailsViewRow)?.BindItem(item));
#if !UNITY_2022_1_OR_NEWER
            var mostRecentlySelectedItem = DetailsViewPersistentState.s_mostRecentlySelectedItem;
#endif
            foreach (var item in rootItems.OfType<TreeViewItem<IRowData>>())
            {
                SetSelectedState(inlineTreeView, item);
#if !UNITY_2022_1_OR_NEWER
                if (item.data.TreeViewPath == mostRecentlySelectedItem.path && item.data.Id == mostRecentlySelectedItem.id)
                {
                    UpdateSelectionPath(item);
                }
#endif
            }

            inlineTreeView.onSelectionChange += OnSelectionChange;
            InitTreeView(inlineTreeView);
            AddTreeView(inlineTreeView);

            Add(m_SelectedItemPath);
        }

        static List<IRowData> GetAncestors(TreeViewItem<IRowData> item)
        {
            var ancestors = new List<IRowData>();
            for (var current = item.data; current != null; current = current.Parent)
            {
                ancestors.Add(current);
            }

            ancestors.Reverse();
            return ancestors;
        }

        void UpdateSelectionPath(TreeViewItem<IRowData> mostRecentlySelected)
        {
            m_SelectedItemPath.Clear();
            var ancestors = GetAncestors(mostRecentlySelected);
            foreach (var ancestor in ancestors)
            {
                m_SelectedItemPath.PushItem(ancestor.Name);
            }
        }

        void OnSelectionChange(IReadOnlyList<ITreeViewItem> items)
        {
            var treeViewItems = items.OfType<TreeViewItem<IRowData>>().ToList();
            var treeViewIDs = treeViewItems.Select(t => t.data.Id).ToList();
            var locators = treeViewItems.Select(t => t.data.TreeViewPath).ToList();

            // Here we need both the path and ID because Server and Client might share the same ID for the same shared items 
            DetailsViewPersistentState.SetSelected(locators, treeViewIDs);

            if (treeViewItems.Count > 0)
            {
                UpdateSelectionPath(treeViewItems[^1]);
            }
        }

        static void SetSelectedState(TreeView treeView, TreeViewItem<IRowData> item)
        {
            var uniquePath = item.data.TreeViewPath + item.id;
            if (DetailsViewPersistentState.IsSelected(uniquePath))
            {
                treeView.AddToSelection(item.id);
            }
            else
            {
                treeView.RemoveFromSelection(item.id);
            }
        }

        static void InitTreeView(TreeView treeView)
        {
            treeView.selectionType = SelectionType.Multiple;

            treeView.style.flexGrow = 1f;
            treeView.style.flexShrink = 0f;
            treeView.style.flexBasis = 0f;
        }

        void AddTreeView(TreeView treeView)
        {
            var col = new VisualElement();
            col.style.flexGrow = 1f;
            col.style.flexShrink = 0f;
            col.style.flexBasis = 0f;

            col.Add(treeView);

            Add(col);
        }

        static IList<ITreeViewItem> GenerateRowList(IReadOnlyCollection<IRowData> rows, ref int nextId)
        {
            var items = new List<ITreeViewItem>(rows.Count);
            foreach (var row in rows)
            {
                nextId++;
                var currentId = nextId;
                var newItem = new TreeViewItem<IRowData>(currentId, row);
                items.Add(newItem);
            }

            return items;
        }
    }
}
