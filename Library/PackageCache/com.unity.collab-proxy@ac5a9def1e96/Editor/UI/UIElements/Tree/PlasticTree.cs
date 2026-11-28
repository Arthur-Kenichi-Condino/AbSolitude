using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI.Tree;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal class PlasticTree : VisualElement, IPlasticTree<IPlasticTreeNode>
    {
        internal const string CHECK_TOGGLE_NAME = "plastic-tree-check-toggle";
        internal const string MAIN_LABEL_NAME = "plastic-tree-main-label";
        internal const string ICON_IMAGE_NAME = "plastic-tree-icon-image";
        internal const string OVERLAY_ICON_IMAGE_NAME = "plastic-tree-overlay-icon-image";

        internal MultiColumnTreeView TreeView { get { return mTreeView; } }
        internal Toggle HeaderCheckToggle { get { return mHeaderCheckToggle; } }
        internal Label HeaderCheckLabel { get { return mHeaderCheckLabel; } }
        internal SelectionActivator SelectionActivator { get { return mSelectionActivator; } }

        internal PlasticTree(
            PlasticTableColumn[] columns,
            Func<string, IPlasticTreeNode, PlasticTableCell> cellFunc,
            Func<IPlasticTree, IPlasticTreeNode, bool> expandFunc,
            SelectionType selectionType,
            string serializationName,
            string legacySettingsName,
            IList<string> legacySettingsColumnNamesByIndex = null,
            Func<IPlasticTreeNode, bool> defaultSelectionFunc = null,
            string defaultSortColumnName = null,
            Action checkedStatesChangedAction = null,
            Action<Filter> afterFilterAppliedAction = null,
            Action<IPlasticTreeNode, bool> afterExpandChangedAction = null)
        {
            mColumns = columns;
            mCellFunc = cellFunc;
            mExpandFunc = expandFunc;
            mDefaultSelectionFunc = defaultSelectionFunc;
            mCheckedStatesChangedAction = checkedStatesChangedAction;
            mAfterExpandChangedAction = afterExpandChangedAction;
            mDefaultSortColumnName = defaultSortColumnName;
            mAfterFilterAppliedAction = afterFilterAppliedAction;
            mSerializationName = serializationName;
            mColumnDisplayNames = PlasticTableColumn.GetColumnNames(columns);
            mNodeIds = new PlasticTreeNodeIds();

            IList<string> legacyColumnNames =
                legacySettingsColumnNamesByIndex ?? new string[0];
            string configFile = PlasticGuiConfig.GetConfigFile();
            List<string> columnNames = GetColumnNameKeys(columns);

            RestoreColumnVisibilities(
                columns,
                configFile,
                serializationName,
                columnNames,
                legacySettingsName,
                legacyColumnNames);

            mTreeView = new MultiColumnTreeView(BuildColumns(columns))
            {
                fixedItemHeight = ROW_HEIGHT,
                selectionType = selectionType,
                sortingMode = ColumnSortingMode.Custom
            };
            mTreeView.style.flexGrow = 1;
            mTreeView.style.backgroundColor = UnityStyles.Colors.TreeViewBackground;

            mColumnWeights = BuildColumnWeights(
                columns,
                LoadColumnWidths(
                    configFile,
                    serializationName,
                    columnNames,
                    legacySettingsName,
                    legacyColumnNames));
            mViewport = mTreeView.Q<ScrollView>().contentViewport;

            RestoreSortColumn(
                mTreeView,
                configFile,
                serializationName,
                columnNames,
                legacySettingsName,
                legacyColumnNames,
                defaultSortColumnName);

            RegisterTreeViewEvents();

            if (HasCheckableColumn(columns))
            {
                mSelectionActivator = new SelectionActivator(
                    GetAllSelection,
                    GetRootNodes,
                    SelectOnlyNode,
                    RefreshCheckedStatesAndNotify);
            }

            style.flexGrow = 1;
            Add(mTreeView);
        }

        internal void OnEnable()
        {
            RegisterTreeViewEvents();
        }

        internal void OnDisable()
        {
            EndHeaderPointerPress();
            UnRegisterTreeViewEvents();

            string sortColumnName;
            bool bSortAscending;

            if (!TryGetSortColumn(out sortColumnName, out bSortAscending))
                sortColumnName = null;

            TreeColumnsSettings.Save(
                PlasticGuiConfig.GetConfigFile(),
                mSerializationName,
                ToSavedColumnWidths(mColumnWeights),
                GetColumnVisibilities(mTreeView.columns),
                sortColumnName,
                bSortAscending);
        }

        internal void FillItems(IPlasticTree treeModel)
        {
            FillItemsAndSelectRows(treeModel, null);
        }

        public void FillItemsAndSelectRows(
            IPlasticTree treeModel,
            List<IPlasticTreeNode> itemsToSelect,
            Filter currentFilter)
        {
            if (currentFilter != null)
            {
                treeModel.Filter(currentFilter, mColumnDisplayNames);

                if (mAfterFilterAppliedAction != null)
                    mAfterFilterAppliedAction(currentFilter);
            }

            SortModel(treeModel);

            FillItemsAndSelectRows(treeModel, itemsToSelect);
        }

        internal void FillItemsAndSelectRows(
            IPlasticTree treeModel,
            List<IPlasticTreeNode> itemsToSelect)
        {
            if (itemsToSelect == null || itemsToSelect.Count == 0)
                itemsToSelect = GetAllSelection();

            int previousFirstSelectedIndex = GetFirstSelectedIndex();

            mTreeModel = treeModel;

            mTreeView.SetRootItems(mNodeIds.BuildTreeViewItems(treeModel));
            mTreeView.Rebuild();

            ApplyExpandFunc();
            RefreshHeaderToggle();

            SetSelectedRows(itemsToSelect, previousFirstSelectedIndex);
        }

        internal void SelectNodes(List<IPlasticTreeNode> nodesToSelect)
        {
            SelectRows(nodesToSelect, -1);

            int firstSelectedIndex = GetFirstSelectedIndex();

            if (firstSelectedIndex == -1)
                return;

            mTreeView.ScrollToItem(firstSelectedIndex);
        }

        internal bool HasSelectedRows()
        {
            return GetSelectedRowsCount() > 0;
        }

        internal int GetSelectedRowsCount()
        {
            int result = 0;

            foreach (int index in mTreeView.selectedIndices)
                result++;

            return result;
        }

        internal List<IPlasticTreeNode> GetAllSelection()
        {
            return GetSelectedNodes<IPlasticTreeNode>();
        }

        internal T GetSelectedNode<T>() where T : class
        {
            foreach (object selectedItem in mTreeView.selectedItems)
            {
                T node = selectedItem as T;

                if (node != null)
                    return node;
            }

            return null;
        }

        internal List<T> GetSelectedNodes<T>() where T : class
        {
            List<T> result = new List<T>();

            foreach (object selectedItem in mTreeView.selectedItems)
            {
                T node = selectedItem as T;

                if (node != null)
                    result.Add(node);
            }

            return result;
        }

        internal void ExpandAll()
        {
            mTreeView.ExpandAll();
        }

        internal void CollapseAll()
        {
            mTreeView.CollapseAll();
        }

        internal void ExpandNode(IPlasticTreeNode node)
        {
            int id;

            if (!mNodeIds.TryGetId(node, out id))
                return;

            mTreeView.ExpandItem(id);
        }

        internal void CollapseNode(IPlasticTreeNode node)
        {
            int id;

            if (!mNodeIds.TryGetId(node, out id))
                return;

            mTreeView.CollapseItem(id);
        }

        internal bool IsExpanded(IPlasticTreeNode node)
        {
            int id;

            if (!mNodeIds.TryGetId(node, out id))
                return false;

            return mTreeView.viewController.IsExpanded(id);
        }

        internal void RefreshCheckedStates()
        {
            mTreeView.RefreshItems();
            RefreshHeaderToggle();
        }

        internal void FocusTree()
        {
            mTreeView.Q<ScrollView>().contentContainer.Focus();
        }

        internal IEnumerable<string> GetExpandedNodeKeys()
        {
            List<string> result = new List<string>();

            foreach (int id in mNodeIds.GetAllIds())
            {
                if (mTreeView.viewController.IsExpanded(id))
                    result.Add(mNodeIds.GetKey(id));
            }

            return result;
        }

        internal void ExpandNodeKeys(IEnumerable<string> nodeKeys)
        {
            foreach (string nodeKey in nodeKeys)
            {
                int id;

                if (mNodeIds.TryGetId(nodeKey, out id))
                    mTreeView.ExpandItem(id);
            }
        }

        internal int GetIdForNodeKey(string nodeKey)
        {
            int id;

            if (mNodeIds.TryGetId(nodeKey, out id))
                return id;

            return -1;
        }

        internal void SetColumnTitle(string columnName, string title)
        {
            Column column = mTreeView.columns[columnName];

            if (column == null)
                return;

            column.title = title;

            if (columnName == mCheckableColumnName && mHeaderCheckLabel != null)
                mHeaderCheckLabel.text = title;
        }

        internal static string GetCellLabelName(string columnName)
        {
            return string.Concat("plastic-tree-cell-", columnName);
        }

        internal static bool IsClickedOnHeader(EventBase evt)
        {
            VisualElement element = evt.target as VisualElement;

            while (element != null)
            {
                if (element.ClassListContains(
                        MultiColumnController.headerContainerUssClassName))
                    return true;

                element = element.parent;
            }

            return false;
        }

        Columns BuildColumns(PlasticTableColumn[] columns)
        {
            Columns result = new Columns();

            foreach (PlasticTableColumn column in columns)
            {
                string columnKey = column.NameKey;

                Column uiColumn = new Column()
                {
                    name = columnKey,
                    title = column.Name,
                    visible = column.IsVisible,
                    sortable = true,
                    optional = columns.Length > 1
                };

                if (column.DefaultWidth == PlasticTableColumn.COLUMN_WIDTH_STAR)
                    uiColumn.stretchable = true;
                else
                    uiColumn.width = column.DefaultWidth;

                if (column.MinWidth > 0)
                    uiColumn.minWidth = column.MinWidth;

                if (IsCheckable(column))
                {
                    mCheckableColumnName = columnKey;
                    mbCheckableColumnHasIcon = HasIconRender(column);
                    uiColumn.optional = false;
                    uiColumn.makeCell = MakeCheckableCell;
                    uiColumn.bindCell = BindCheckableCell;
                    uiColumn.makeHeader = MakeCheckableHeader;
                    uiColumn.bindHeader = header => BindCheckableHeader(header, uiColumn);
                }
                else if (HasIconRender(column))
                {
                    uiColumn.makeCell = () => MakeIconAndTextCell(columnKey);
                    uiColumn.bindCell = (cell, index) =>
                        BindIconAndTextCell(cell, index, columnKey);
                }
                else
                {
                    uiColumn.makeCell = () => MakeTextCell(columnKey);
                    uiColumn.bindCell = (cell, index) =>
                    {
                        BindCellLabel(
                            cell.Q<Label>(GetCellLabelName(columnKey)),
                            mCellFunc(columnKey, GetNodeForIndex(index)));
                    };
                }

                result.Add(uiColumn);
            }

            return result;
        }

        VisualElement MakeCheckableCell()
        {
            VisualElement cell = MakeCellContainer();

            Toggle toggle = new Toggle() { name = CHECK_TOGGLE_NAME };
            toggle.style.flexShrink = 0;
            toggle.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            toggle.RegisterValueChangedCallback(evt =>
            {
                mSelectionActivator.OnSelectionActivated(
                    (IPlasticTreeNode)cell.userData);
            });

            cell.Add(toggle);

            if (mbCheckableColumnHasIcon)
                cell.Add(MakeCellIcon());

            cell.Add(MakeCellLabel(MAIN_LABEL_NAME));

            return cell;
        }

        void BindCheckableCell(VisualElement cell, int index)
        {
            IPlasticTreeNode node = GetNodeForIndex(index);
            cell.userData = node;

            bool? isChecked = CheckableItems.GetIsCheckedValue(node);

            Toggle toggle = cell.Q<Toggle>(CHECK_TOGGLE_NAME);
            toggle.SetValueWithoutNotify(isChecked == true);
            toggle.showMixedValue = !isChecked.HasValue;

            PlasticTableCell cellData = mCellFunc(mCheckableColumnName, node);

            if (mbCheckableColumnHasIcon)
                BindCellIcon(cell, cellData);

            BindCellLabel(cell.Q<Label>(MAIN_LABEL_NAME), cellData);
        }

        VisualElement MakeIconAndTextCell(string columnName)
        {
            VisualElement cell = MakeHeaderAlignedCellContainer();

            cell.Add(MakeCellIcon());
            cell.Add(MakeCellLabel(GetCellLabelName(columnName)));

            return cell;
        }

        static VisualElement MakeTextCell(string columnName)
        {
            VisualElement cell = MakeHeaderAlignedCellContainer();

            cell.Add(MakeCellLabel(GetCellLabelName(columnName)));

            return cell;
        }

        static VisualElement MakeHeaderAlignedCellContainer()
        {
            VisualElement cell = MakeCellContainer();
            cell.style.paddingLeft = HEADER_TITLE_PADDING_LEFT;

            return cell;
        }

        static VisualElement MakeCellContainer()
        {
            VisualElement cell = new VisualElement();
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.flexGrow = 1;
            cell.style.flexShrink = 1;
            cell.style.minWidth = 0;
            cell.style.paddingRight = CELL_PADDING_RIGHT;
            cell.style.overflow = Overflow.Hidden;

            return cell;
        }

        static Label MakeCellLabel(string name)
        {
            Label label = new Label() { name = name };
            label.RegisterCallback<GeometryChangedEvent>(OnCellLabelGeometryChanged);
            return label;
        }

        void BindIconAndTextCell(VisualElement cell, int index, string columnName)
        {
            PlasticTableCell cellData = mCellFunc(columnName, GetNodeForIndex(index));

            BindCellIcon(cell, cellData);

            BindCellLabel(cell.Q<Label>(GetCellLabelName(columnName)), cellData);
        }

        static void BindCellIcon(VisualElement cell, PlasticTableCell cellData)
        {
            Image icon = cell.Q<Image>(ICON_IMAGE_NAME);
            icon.image = cellData.Icon;
            icon.parent.SetVisible(cellData.Icon != null);

            Image overlayIcon = cell.Q<Image>(OVERLAY_ICON_IMAGE_NAME);
            overlayIcon.image = cellData.OverlayIcon;
            overlayIcon.SetVisible(cellData.OverlayIcon != null);
        }

        static void BindCellLabel(Label label, PlasticTableCell cellData)
        {
            label.style.unityTextAlign = GetTextAnchor(cellData.TextAlignment);
            label.style.color = GetLabelColor(cellData.Style);

            if (cellData.Trimming != PlasticTableCell.TextTrimming.Path)
            {
                ResetPathTrimmingStyles(label);

                label.userData = null;
                label.text = cellData.Text;
                label.tooltip = cellData.Tooltip;
                return;
            }

            ApplyPathTrimmingStyles(label);

            label.userData = cellData;
            ApplyPathTrimming(label, cellData);
        }

        static void ApplyPathTrimmingStyles(Label label)
        {
            label.style.flexGrow = 1;
            label.style.flexShrink = 1;
            label.style.flexBasis = 0;
            label.style.minWidth = 0;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.overflow = Overflow.Hidden;
        }

        static void ResetPathTrimmingStyles(Label label)
        {
            label.style.flexGrow = StyleKeyword.Null;
            label.style.flexShrink = StyleKeyword.Null;
            label.style.flexBasis = StyleKeyword.Null;
            label.style.minWidth = StyleKeyword.Null;
            label.style.whiteSpace = StyleKeyword.Null;
            label.style.overflow = StyleKeyword.Null;
        }

        static void OnCellLabelGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.oldRect.width == evt.newRect.width)
                return;

            Label label = (Label)evt.target;

            if (label.userData is PlasticTableCell cellData)
                ApplyPathTrimming(label, cellData);
        }

        static void ApplyPathTrimming(Label label, PlasticTableCell cellData)
        {
            float availableWidth = label.contentRect.width;

            if (float.IsNaN(availableWidth) || availableWidth <= 0)
            {
                label.text = cellData.Text;
                label.tooltip = cellData.Tooltip;
                return;
            }

            bool bWasTrimmed;
            label.text = PathTrimming.TruncatePath(
                cellData.Text,
                availableWidth,
                new LabelCalcTextSize(label),
                out bWasTrimmed);

            label.tooltip = bWasTrimmed ? cellData.Text : cellData.Tooltip;
        }

        class LabelCalcTextSize : ICalcTextSize
        {
            internal LabelCalcTextSize(Label label)
            {
                mLabel = label;
            }

            float ICalcTextSize.GetTextWidth(string text)
            {
                return mLabel.MeasureTextSize(
                    text,
                    0, MeasureMode.Undefined,
                    0, MeasureMode.Undefined).x;
            }

            readonly Label mLabel;
        }

        static StyleColor GetLabelColor(PlasticTableCell.StyleType style)
        {
            if (style == PlasticTableCell.StyleType.Lighter)
                return UnityStyles.Colors.SecondaryLabel;

            return UnityStyles.Colors.TreeLabel;
        }

        static TextAnchor GetTextAnchor(PlasticTableCell.Alignment alignment)
        {
            switch (alignment)
            {
                case PlasticTableCell.Alignment.Center:
                    return TextAnchor.MiddleCenter;
                case PlasticTableCell.Alignment.Right:
                    return TextAnchor.MiddleRight;
                default:
                    return TextAnchor.MiddleLeft;
            }
        }

        VisualElement MakeCheckableHeader()
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;

            Toggle toggle = new Toggle();
            toggle.RegisterValueChangedCallback(evt =>
            {
                mSelectionActivator.OnCheckUncheckAllStateChanged(evt.newValue);
            });

            header.Add(toggle);
            header.Add(new Label());
            header.userData = toggle;

            return header;
        }

        void BindCheckableHeader(VisualElement header, Column column)
        {
            mHeaderCheckToggle = (Toggle)header.userData;
            mHeaderCheckLabel = header.Q<Label>();
            mHeaderCheckLabel.text = column.title;
            RefreshHeaderToggle();
        }

        void RefreshHeaderToggle()
        {
            if (mHeaderCheckToggle == null || mSelectionActivator == null)
                return;

            bool? state = mSelectionActivator.GetCheckUncheckAllState();

            mHeaderCheckToggle.SetValueWithoutNotify(state == true);
            mHeaderCheckToggle.showMixedValue = state == null;
        }

        void RefreshCheckedStatesAndNotify()
        {
            RefreshCheckedStates();

            if (mCheckedStatesChangedAction != null)
                mCheckedStatesChangedAction();
        }

        void RegisterTreeViewEvents()
        {
            mTreeView.columnSortingChanged += OnColumnSortingChanged;
            mViewport.RegisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
            mTreeView.RegisterCallback<PointerDownEvent>(
                OnTreeViewPointerDown, TrickleDown.TrickleDown);
            mTreeView.RegisterCallback<PointerUpEvent>(OnTreeViewPointerUp);
            mTreeView.RegisterCallback<PointerCaptureOutEvent>(
                OnTreeViewPointerCaptureOut, TrickleDown.TrickleDown);

            foreach (Column column in mTreeView.columns)
                column.propertyChanged += OnColumnPropertyChanged;

            if (mAfterExpandChangedAction != null)
                mTreeView.itemExpandedChanged += OnItemExpandedChanged;
        }

        void UnRegisterTreeViewEvents()
        {
            mTreeView.columnSortingChanged -= OnColumnSortingChanged;
            mViewport.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
            mTreeView.UnregisterCallback<PointerDownEvent>(
                OnTreeViewPointerDown, TrickleDown.TrickleDown);
            mTreeView.UnregisterCallback<PointerUpEvent>(OnTreeViewPointerUp);
            mTreeView.UnregisterCallback<PointerCaptureOutEvent>(
                OnTreeViewPointerCaptureOut, TrickleDown.TrickleDown);

            foreach (Column column in mTreeView.columns)
                column.propertyChanged -= OnColumnPropertyChanged;

            if (mAfterExpandChangedAction != null)
                mTreeView.itemExpandedChanged -= OnItemExpandedChanged;
        }

        static string FindColumnDisplayName(
            PlasticTableColumn[] columns, string columnKey)
        {
            foreach (PlasticTableColumn column in columns)
            {
                if (column.NameKey == columnKey)
                    return column.Name;
            }

            return columnKey;
        }

        bool TryGetSortColumn(out string sortColumnName, out bool bSortAscending)
        {
            sortColumnName = null;
            bSortAscending = true;

            if (mDefaultSortColumnName == null)
                return false;

            foreach (SortColumnDescription description in mTreeView.sortedColumns)
            {
                sortColumnName = description.column.name;
                bSortAscending = description.direction == SortDirection.Ascending;
                return true;
            }

            return false;
        }

        static void RestoreSortColumn(
            MultiColumnTreeView treeView,
            string configFile,
            string serializationName,
            List<string> columnNames,
            string legacySettingsName,
            IList<string> legacyColumnNames,
            string defaultSortColumnName)
        {
            if (defaultSortColumnName == null)
                return;

            string sortColumnName;
            bool bSortAscending;

            bool bIsSortColumnLoaded = TreeColumnsSettings.TryLoadSortColumn(
                configFile,
                serializationName,
                columnNames,
                out sortColumnName,
                out bSortAscending);

            if (!bIsSortColumnLoaded && legacySettingsName != null)
            {
                bIsSortColumnLoaded = TreeHeaderSettings.TryLoadLegacySortColumn(
                    legacySettingsName,
                    legacyColumnNames,
                    out sortColumnName,
                    out bSortAscending);
            }

            if (!bIsSortColumnLoaded || !IsVisibleColumn(treeView.columns, sortColumnName))
            {
                sortColumnName = defaultSortColumnName;
                bSortAscending = true;
            }

            treeView.sortColumnDescriptions.Add(new SortColumnDescription()
            {
                columnName = sortColumnName,
                direction = bSortAscending ? SortDirection.Ascending : SortDirection.Descending
            });
        }

        static bool IsVisibleColumn(Columns columns, string columnName)
        {
            Column column = columns[columnName];

            return column != null && column.visible;
        }

        static void RestoreColumnVisibilities(
            PlasticTableColumn[] columns,
            string configFile,
            string serializationName,
            List<string> columnNames,
            string legacySettingsName,
            IList<string> legacyColumnNames)
        {
            Dictionary<string, bool> savedVisibilities =
                TreeColumnsSettings.LoadColumnVisibilities(
                    configFile, serializationName, columnNames);

            if (savedVisibilities.Count == 0)
            {
                savedVisibilities = LoadLegacyColumnVisibilities(
                    columnNames, legacySettingsName, legacyColumnNames);
            }

            for (int i = 1; i < columns.Length; i++)
            {
                bool bIsVisible;

                if (savedVisibilities.TryGetValue(columns[i].NameKey, out bIsVisible))
                    columns[i].IsVisible = bIsVisible;
            }
        }

        static Dictionary<string, bool> LoadLegacyColumnVisibilities(
            List<string> columnNames,
            string legacySettingsName,
            IList<string> legacyColumnNames)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            if (legacySettingsName == null)
                return result;

            List<string> legacyVisibleColumns = TreeHeaderSettings.LoadLegacyVisibleColumnNames(
                legacySettingsName, legacyColumnNames);

            if (legacyVisibleColumns == null)
                return result;

            foreach (string columnName in columnNames)
                result[columnName] = legacyVisibleColumns.Contains(columnName);

            return result;
        }

        static Dictionary<string, float> LoadColumnWidths(
            string configFile,
            string serializationName,
            List<string> columnNames,
            string legacySettingsName,
            IList<string> legacyColumnNames)
        {
            Dictionary<string, int> savedWidths = TreeColumnsSettings.LoadColumnWidths(
                configFile, serializationName, columnNames);

            if (savedWidths.Count > 0)
            {
                Dictionary<string, float> result = new Dictionary<string, float>();

                foreach (KeyValuePair<string, int> savedWidth in savedWidths)
                    result[savedWidth.Key] = savedWidth.Value;

                return result;
            }

            if (legacySettingsName == null)
                return null;

            return TreeHeaderSettings.LoadLegacyColumnWidths(
                legacySettingsName, legacyColumnNames);
        }

        static Dictionary<string, int> ToSavedColumnWidths(Dictionary<string, float> weights)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (KeyValuePair<string, float> weight in weights)
                result[weight.Key] = Math.Max(1, (int)Math.Round(weight.Value));

            return result;
        }

        static Dictionary<string, bool> GetColumnVisibilities(Columns columns)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (Column column in columns)
            {
                if (!column.optional)
                    continue;

                result[column.name] = column.visible;
            }

            return result;
        }

        static List<string> GetColumnNameKeys(PlasticTableColumn[] columns)
        {
            List<string> result = new List<string>();

            foreach (PlasticTableColumn column in columns)
                result.Add(column.NameKey);

            return result;
        }

        void ApplyExpandFunc()
        {
            if (mExpandFunc == null)
                return;

            foreach (int id in mNodeIds.GetAllIds())
            {
                if (mExpandFunc(mTreeModel, mNodeIds.GetNode(id)))
                    mTreeView.ExpandItem(id);
            }
        }

        void SetSelectedRows(List<IPlasticTreeNode> itemsToSelect, int defaultRow)
        {
            if (itemsToSelect.Count == 0)
            {
                if (mDefaultSelectionFunc != null)
                    SelectFirstRowThatMatches(mDefaultSelectionFunc, bNotify: false);

                return;
            }

            if (SelectRows(itemsToSelect, -1, bNotify: false))
                return;

            if (defaultRow == -1)
                return;

            int visibleRowsCount = mTreeView.viewController.GetItemsCount();

            if (visibleRowsCount == 0)
                return;

            mTreeView.SetSelectionWithoutNotify(
                new int[] { Math.Min(defaultRow, visibleRowsCount - 1) });
        }

        bool SelectRows(
            List<IPlasticTreeNode> itemsToSelect,
            int previousFirstSelectedIndex,
            bool bNotify = true)
        {
            List<int> idsToSelect = new List<int>();

            foreach (IPlasticTreeNode node in itemsToSelect)
            {
                int id;

                if (!mNodeIds.TryGetId(node, out id))
                    continue;

                idsToSelect.Add(id);
                ExpandAncestors(node);
            }

            if (idsToSelect.Count > 0)
            {
                if (bNotify)
                    mTreeView.SetSelectionById(idsToSelect);
                else
                    mTreeView.SetSelectionByIdWithoutNotify(idsToSelect);

                return true;
            }

            if (previousFirstSelectedIndex == -1)
                return false;

            int visibleRowsCount = mTreeView.viewController.GetItemsCount();

            if (visibleRowsCount == 0)
                return false;

            int indexToSelect =
                Math.Min(previousFirstSelectedIndex, visibleRowsCount - 1);

            if (bNotify)
                mTreeView.SetSelection(indexToSelect);
            else
                mTreeView.SetSelectionWithoutNotify(new int[] { indexToSelect });

            return true;
        }

        bool SelectFirstRowThatMatches(
            Func<IPlasticTreeNode, bool> matchFunc,
            bool bNotify = true)
        {
            int visibleRowsCount = mTreeView.viewController.GetItemsCount();

            for (int i = 0; i < visibleRowsCount; i++)
            {
                if (!matchFunc(GetNodeForIndex(i)))
                    continue;

                if (bNotify)
                    mTreeView.SetSelection(i);
                else
                    mTreeView.SetSelectionWithoutNotify(new int[] { i });

                return true;
            }

            return false;
        }

        void ExpandAncestors(IPlasticTreeNode node)
        {
            IPlasticTreeNode current = node.GetParent();

            while (current != null)
            {
                ExpandNode(current);
                current = current.GetParent();
            }
        }

        void SelectOnlyNode(IPlasticTreeNode node)
        {
            int id;

            if (!mNodeIds.TryGetId(node, out id))
                return;

            mTreeView.SetSelectionById(id);
        }

        void OnColumnSortingChanged()
        {
            if (!(mTreeModel is ITreeSortable))
                return;

            SortModel(mTreeModel);
            FillItemsAndSelectRows(mTreeModel, null);
        }

        void SortModel(IPlasticTree treeModel)
        {
            ITreeSortable sortableModel = treeModel as ITreeSortable;

            if (sortableModel == null)
                return;

            string sortedColumnName = mDefaultSortColumnName;
            bool bSortAscending = true;

            foreach (SortColumnDescription description in mTreeView.sortedColumns)
            {
                sortedColumnName = description.column.name;
                bSortAscending = description.direction == SortDirection.Ascending;
                break;
            }

            if (sortedColumnName == null)
                return;

            sortableModel.Sort(
                FindColumnDisplayName(mColumns, sortedColumnName), bSortAscending);
        }

        void OnViewportGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.oldRect.width == evt.newRect.width)
                return;

            if (mHeaderDraggedColumn != null)
                return;

            FitColumnsToWidth(evt.newRect.width, null);
        }

        void OnColumnPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
        {
            if (mbIsSettingColumnWidthWithoutNotify)
                return;

            Column column = (Column)sender;

            if (!mColumnWeights.ContainsKey(column.name))
                return;

            if (args.propertyName == nameof(Column.width))
            {
                if (mbIsHeaderPointerPressed)
                {
                    if (mHeaderDraggedColumn == null)
                        mHeaderDraggedColumn = column;

                    return;
                }

                FitDraggedColumnAndUpdateWeights(column);
                return;
            }

            if (args.propertyName == nameof(Column.visible))
                FitColumnsToWidth(GetViewportWidth(), null);
        }

        void OnTreeViewPointerDown(PointerDownEvent evt)
        {
            EndHeaderPointerPress();

            mbIsHeaderPointerPressed =
                evt.button == (int)MouseButton.LeftMouse &&
                IsClickedOnHeader(evt);
        }

        void OnTreeViewPointerUp(PointerUpEvent evt)
        {
            EndHeaderPointerPress();
        }

        void OnTreeViewPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            EndHeaderPointerPress();
        }

        void EndHeaderPointerPress()
        {
            if (!mbIsHeaderPointerPressed)
                return;

            mbIsHeaderPointerPressed = false;

            if (mHeaderDraggedColumn == null)
                return;

            Column draggedColumn = mHeaderDraggedColumn;
            mHeaderDraggedColumn = null;

            FitDraggedColumnAndUpdateWeights(draggedColumn);
        }

        float GetViewportWidth()
        {
            return mViewport.resolvedStyle.width;
        }

        void FitColumnsToWidth(float width, Column pinnedColumn)
        {
            foreach (KeyValuePair<Column, float> columnWidth in ComputeColumnWidths(
                mTreeView.columns, mColumnWeights, width, pinnedColumn))
            {
                SetColumnWidthWithoutNotify(columnWidth.Key, columnWidth.Value);
            }
        }

        void FitDraggedColumnAndUpdateWeights(Column draggedColumn)
        {
            Dictionary<Column, float> widthsBeforeFit = GetColumnWidths(mTreeView.columns);

            FitColumnsToWidth(GetViewportWidth(), draggedColumn);

            float scale = GetWidthToWeightScale(
                mTreeView.columns,
                mColumnWeights,
                draggedColumn,
                GetColumnWidths(mTreeView.columns));

            if (scale <= 0)
            {
                scale = GetWidthToWeightScale(
                    mTreeView.columns, mColumnWeights, draggedColumn, widthsBeforeFit);
            }

            foreach (Column column in mTreeView.columns)
            {
                if (!mColumnWeights.ContainsKey(column.name))
                    continue;

                if (column == draggedColumn ||
                    (column.visible && !IsAtMinWidth(column)))
                {
                    mColumnWeights[column.name] = column.width.value;
                    continue;
                }

                if (scale > 0)
                    mColumnWeights[column.name] *= scale;
            }
        }

        void SetColumnWidthWithoutNotify(Column column, float width)
        {
            mbIsSettingColumnWidthWithoutNotify = true;

            try
            {
                column.width = width;
            }
            finally
            {
                mbIsSettingColumnWidthWithoutNotify = false;
            }
        }

        static Dictionary<Column, float> GetColumnWidths(Columns columns)
        {
            Dictionary<Column, float> result = new Dictionary<Column, float>();

            foreach (Column column in columns)
                result[column] = column.width.value;

            return result;
        }

        static float GetWidthToWeightScale(
            Columns columns,
            Dictionary<string, float> weights,
            Column draggedColumn,
            Dictionary<Column, float> columnWidths)
        {
            float totalWidth = 0;
            float totalWeight = 0;

            foreach (Column column in columns)
            {
                if (!column.visible || column == draggedColumn ||
                    !weights.ContainsKey(column.name))
                    continue;

                float width = columnWidths[column];

                if (width <= column.minWidth.value)
                    continue;

                totalWidth += width;
                totalWeight += weights[column.name];
            }

            if (totalWeight <= 0)
                return 0;

            return totalWidth / totalWeight;
        }

        static Dictionary<Column, float> ComputeColumnWidths(
            Columns columns,
            Dictionary<string, float> weights,
            float width,
            Column pinnedColumn)
        {
            if (float.IsNaN(width) || width <= 0)
                return new Dictionary<Column, float>();

            List<Column> columnsToFit = new List<Column>();

            foreach (Column column in columns)
            {
                if (column.visible && column != pinnedColumn &&
                    weights.ContainsKey(column.name))
                    columnsToFit.Add(column);
            }

            float availableWidth = width;

            if (pinnedColumn != null)
                availableWidth -= pinnedColumn.width.value;

            return DistributeWidth(columnsToFit, weights, availableWidth);
        }

        static Dictionary<Column, float> DistributeWidth(
            List<Column> columns,
            Dictionary<string, float> weights,
            float availableWidth)
        {
            Dictionary<Column, float> result = new Dictionary<Column, float>();
            List<Column> remainingColumns = new List<Column>(columns);

            while (remainingColumns.Count > 0)
            {
                float totalWeight = 0;

                foreach (Column column in remainingColumns)
                    totalWeight += weights[column.name];

                if (totalWeight <= 0)
                    return result;

                Column clampedColumn = FindColumnBelowMinWidth(
                    remainingColumns, weights, availableWidth, totalWeight);

                if (clampedColumn == null)
                {
                    float fitScale = availableWidth / totalWeight;

                    foreach (Column column in remainingColumns)
                        result[column] = fitScale * weights[column.name];

                    return result;
                }

                result[clampedColumn] = clampedColumn.minWidth.value;
                availableWidth -= clampedColumn.minWidth.value;
                remainingColumns.Remove(clampedColumn);
            }

            return result;
        }

        static Column FindColumnBelowMinWidth(
            List<Column> columns,
            Dictionary<string, float> weights,
            float availableWidth,
            float totalWeight)
        {
            foreach (Column column in columns)
            {
                if (availableWidth * weights[column.name] / totalWeight < column.minWidth.value)
                    return column;
            }

            return null;
        }

        void OnItemExpandedChanged(TreeViewExpansionChangedArgs args)
        {
            IPlasticTreeNode node;

            if (!mNodeIds.TryGetNode(args.id, out node))
                return;

            mAfterExpandChangedAction(node, args.isExpanded);
        }

        int GetFirstSelectedIndex()
        {
            foreach (int index in mTreeView.selectedIndices)
                return index;

            return -1;
        }

        IPlasticTreeNode GetNodeForIndex(int index)
        {
            return mTreeView.GetItemDataForIndex<IPlasticTreeNode>(index);
        }

        List<IPlasticTreeNode> GetRootNodes()
        {
            List<IPlasticTreeNode> result = new List<IPlasticTreeNode>();

            if (mTreeModel == null)
                return result;

            foreach (IPlasticTreeNode node in mTreeModel.GetNodes())
                result.Add(node);

            return result;
        }

        static Dictionary<string, float> BuildColumnWeights(
            PlasticTableColumn[] columns,
            Dictionary<string, float> savedColumnWeights)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();

            foreach (PlasticTableColumn column in columns)
            {
                if (column.DefaultWidth == PlasticTableColumn.COLUMN_WIDTH_STAR)
                    continue;

                float savedWeight;

                if (savedColumnWeights != null &&
                    savedColumnWeights.TryGetValue(column.NameKey, out savedWeight) &&
                    savedWeight > 0 &&
                    !float.IsInfinity(savedWeight))
                {
                    result.Add(column.NameKey, savedWeight);
                    continue;
                }

                result.Add(column.NameKey, column.DefaultWidth);
            }

            return result;
        }

        static bool IsAtMinWidth(Column column)
        {
            return column.width.value <= column.minWidth.value;
        }

        static bool HasCheckableColumn(PlasticTableColumn[] columns)
        {
            foreach (PlasticTableColumn column in columns)
            {
                if (IsCheckable(column))
                    return true;
            }

            return false;
        }

        static bool IsCheckable(PlasticTableColumn column)
        {
            foreach (PlasticTableColumn.Render renderType in column.RenderTypes)
            {
                if (renderType == PlasticTableColumn.Render.CheckBoxAndText ||
                    renderType == PlasticTableColumn.Render.CheckBoxIconAndText)
                    return true;
            }

            return false;
        }

        static bool HasIconRender(PlasticTableColumn column)
        {
            foreach (PlasticTableColumn.Render renderType in column.RenderTypes)
            {
                if (renderType == PlasticTableColumn.Render.IconAndText ||
                    renderType == PlasticTableColumn.Render.CheckBoxIconAndText)
                    return true;
            }

            return false;
        }

        static VisualElement MakeCellIcon()
        {
            VisualElement iconContainer = new VisualElement();
            iconContainer.style.width = CELL_ICON_SIZE;
            iconContainer.style.height = CELL_ICON_SIZE;
            iconContainer.style.flexShrink = 0;

            Image icon = new Image()
            {
                name = ICON_IMAGE_NAME,
                scaleMode = ScaleMode.ScaleToFit
            };
            icon.style.width = CELL_ICON_SIZE;
            icon.style.height = CELL_ICON_SIZE;

            Image overlayIcon = new Image()
            {
                name = OVERLAY_ICON_IMAGE_NAME,
                scaleMode = ScaleMode.ScaleToFit
            };
            overlayIcon.style.position = Position.Absolute;
            overlayIcon.style.left = OVERLAY_ICON_OFFSET_X;
            overlayIcon.style.top = OVERLAY_ICON_OFFSET_Y;
            overlayIcon.style.width = OVERLAY_ICON_SIZE;
            overlayIcon.style.height = OVERLAY_ICON_SIZE;

            iconContainer.Add(icon);
            iconContainer.Add(overlayIcon);

            return iconContainer;
        }

        bool mbCheckableColumnHasIcon;
        bool mbIsSettingColumnWidthWithoutNotify;
        bool mbIsHeaderPointerPressed;
        Column mHeaderDraggedColumn;
        IPlasticTree mTreeModel;
        Toggle mHeaderCheckToggle;
        Label mHeaderCheckLabel;
        string mCheckableColumnName;

        readonly MultiColumnTreeView mTreeView;
        readonly VisualElement mViewport;
        readonly PlasticTreeNodeIds mNodeIds;
        readonly SelectionActivator mSelectionActivator;
        readonly Func<string, IPlasticTreeNode, PlasticTableCell> mCellFunc;
        readonly Func<IPlasticTree, IPlasticTreeNode, bool> mExpandFunc;
        readonly Func<IPlasticTreeNode, bool> mDefaultSelectionFunc;
        readonly Action mCheckedStatesChangedAction;
        readonly Action<IPlasticTreeNode, bool> mAfterExpandChangedAction;
        readonly string mDefaultSortColumnName;
        readonly Action<Filter> mAfterFilterAppliedAction;
        readonly string mSerializationName;
        readonly PlasticTableColumn[] mColumns;
        readonly List<string> mColumnDisplayNames;
        readonly Dictionary<string, float> mColumnWeights;

        const int ROW_HEIGHT = 20;
        const int CELL_ICON_SIZE = 20;
        const int CELL_PADDING_RIGHT = 2;
        const int HEADER_TITLE_PADDING_LEFT = 6;
        const int OVERLAY_ICON_SIZE = 16;
        const int OVERLAY_ICON_OFFSET_X = 6;
        const int OVERLAY_ICON_OFFSET_Y = 8;
    }
}
