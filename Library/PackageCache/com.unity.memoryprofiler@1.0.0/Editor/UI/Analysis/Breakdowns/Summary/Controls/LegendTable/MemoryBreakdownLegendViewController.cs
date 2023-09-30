using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    internal class MemoryBreakdownLegendViewController : ViewController
    {
        const string k_UxmlAssetGuid = "3bf369f02dcfe494284c624593e24cfe";
        const string k_UxmlNameCellGuid = "c30ef0628d7c0d446a7befff07cae5b7";
        const string k_UxmlSizeCellGuid = "223694cb87b669347baf1a3e5aec5ddb";

        // Legend table columns
        const string k_UxmlColumnName = "memory-usage-breakdown__legend__name-column";
        const string k_UxmlColumnA = "memory-usage-breakdown__legend__snapshot-a-column";
        const string k_UxmlColumnB = "memory-usage-breakdown__legend__snapshot-b-column";
        const string k_UxmlColumnDiff = "memory-usage-breakdown__legend__diff-column";
        // Legend table column elements
        const string k_UxmlColumnHeader = "memory-usage-breakdown__legend__column-controls";
        const string k_UxmlColumnCells = "memory-usage-breakdown__legend__cells";
        const string k_UxmlFirstRow = "memory-usage-breakdown__legend__first-row";
        const string k_UxmlLastRow = "memory-usage-breakdown__legend__last-row";
        // Legend table cell elements
        const string k_UxmlCellNameLabel = "memory-usage-breakdown__legend__name";
        const string k_UxmlCellValueLabel = "memory-usage-breakdown__legend__size-column";
        const string k_UxmlCellColorBox = "memory-usage-breakdown__legend__color-box";
        const string k_UxmlCellColorBoxFree = "memory-usage-breakdown__legend__color-box__unused";
        const string k_UxmlCellColorBoxReserved = "memory-usage-breakdown__legend__used-reserved";
        //  Element states
        const string k_UxmlCellHoverStateClass = "memory-usage-breakdown__element-hovered";
        const string k_UxmlCellSelectedStateClass = "memory-usage-breakdown__element-selected";
        // Category color style templates
        const string k_UxmlElementSolidColor = "memory-usage-breakdown-color-category-";

        static readonly string k_LegendTableSizeFormatString = L10n.Tr("{0}");
        static readonly string k_LegendTableSizeWithUsedFormatString = L10n.Tr("{0} / {1}");

        static readonly string k_TooltipRowUsedFormat = L10n.Tr("Used: {0} ({1:0.0}% of reserved)");
        static readonly string k_TooltipRowReservedFormat = L10n.Tr("Reserved: {0} ({1:0.0}% of total)");
        static readonly string k_TooltipRowTotalFormat = L10n.Tr("{0} ({1:0.0}% of total)");

        // Model
        readonly MemoryBreakdownModel m_Model;

        // View
        struct Column
        {
            public VisualElement Root;
            public VisualElement Header;
            public VisualElement CellsContainer;
        }

        // View
        Column m_ColumnName;
        Column m_ColumnSnapshotA;
        Column m_ColumnSnapshotB;
        Column m_ColumnDifference;

        public event Action<int, bool> OnRowHovered = delegate { };
        public event Action<int> OnRowClicked = delegate { };

        public MemoryBreakdownLegendViewController(MemoryBreakdownModel model)
        {
            m_Model = model;
        }

        public void SetRowHovered(int index, bool state)
        {
            SetCellHoverState(m_ColumnName.CellsContainer, index, state);
            SetCellHoverState(m_ColumnSnapshotA.CellsContainer, index, state);
            SetCellHoverState(m_ColumnSnapshotB.CellsContainer, index, state);
            SetCellHoverState(m_ColumnDifference.CellsContainer, index, state);
        }

        public void SetRowSelected(int index, bool state)
        {
            SetCellSelectedState(m_ColumnName.CellsContainer, index, state);
            SetCellSelectedState(m_ColumnSnapshotA.CellsContainer, index, state);
            SetCellSelectedState(m_ColumnSnapshotB.CellsContainer, index, state);
            SetCellSelectedState(m_ColumnDifference.CellsContainer, index, state);
        }

        protected override VisualElement LoadView()
        {
            var view = ViewControllerUtility.LoadVisualTreeFromUxml(k_UxmlAssetGuid);
            if (view == null)
                throw new InvalidOperationException("Unable to create view from Uxml. Uxml must contain at least one child element.");

            return view;
        }

        protected override void ViewLoaded()
        {
            GatherViewReferences();
            RefreshView();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        void GatherViewReferences()
        {
            GatherColumnReferences(k_UxmlColumnName, ref m_ColumnName);
            GatherColumnReferences(k_UxmlColumnA, ref m_ColumnSnapshotA);
            GatherColumnReferences(k_UxmlColumnB, ref m_ColumnSnapshotB);
            GatherColumnReferences(k_UxmlColumnDiff, ref m_ColumnDifference);
        }

        void GatherColumnReferences(string id, ref Column ret)
        {
            ret = new Column();
            ret.Root = View.Q<VisualElement>(id);
            ret.Header = ret.Root.Q<VisualElement>(k_UxmlColumnHeader);
            ret.CellsContainer = ret.Root.Q<VisualElement>(k_UxmlColumnCells);
        }

        void RefreshView()
        {
            // Update legend table header snapshot id icons state
            UIElementsHelper.SetVisibility(m_ColumnName.Header, m_Model.CompareMode);
            UIElementsHelper.SetVisibility(m_ColumnSnapshotA.Header, m_Model.CompareMode);
            UIElementsHelper.SetVisibility(m_ColumnSnapshotB.Header, m_Model.CompareMode);
            UIElementsHelper.SetVisibility(m_ColumnDifference.Header, m_Model.CompareMode);

            // Build names column for the Legend Table
            RefreshColumnCells(ref m_ColumnName, (rowId, row) =>
            {
                return MakeNameCell(rowId);
            });

            // Build Snapshot *A* column for the Legend Table
            RefreshColumnCells(ref m_ColumnSnapshotA, (rowId, row) =>
            {
                return MakeSizeCell(rowId, (long)row.TotalA, (long)row.UsedA);
            });

            // Build Snapshot *B* and *diff* column for the Legend Table
            UIElementsHelper.SetVisibility(m_ColumnSnapshotB.Root, m_Model.CompareMode);
            UIElementsHelper.SetVisibility(m_ColumnDifference.Root, m_Model.CompareMode);
            if (m_Model.CompareMode)
            {
                UIElementsHelper.SetVisibility(m_ColumnSnapshotB.Root, true);
                RefreshColumnCells(ref m_ColumnSnapshotB, (rowId, row) =>
                {
                    return MakeSizeCell(rowId, (long)row.TotalB, (long)row.UsedB);
                });

                UIElementsHelper.SetVisibility(m_ColumnDifference.Root, true);
                RefreshColumnCells(ref m_ColumnDifference, (rowId, row) =>
                {
                    var diffUsed = Math.Abs((long)row.UsedA - (long)row.UsedB);
                    var diffTotal = Math.Abs((long)row.TotalA - (long)row.TotalB);
                    return MakeSizeCell(rowId, diffTotal, diffUsed);
                });
            }
        }

        void RefreshColumnCells(ref Column column, Func<int, MemoryBreakdownModel.Row, VisualElement> makeCell)
        {
            // Remove all old cells
            column.CellsContainer.Clear();

            // Create cells for column from model data
            for (var i = 0; i < m_Model.Rows.Count; i++)
            {
                var elem = makeCell(i, m_Model.Rows[i]);
                if (i == 0)
                    elem.AddToClassList(k_UxmlFirstRow);
                if (i == m_Model.Rows.Count - 1)
                    elem.AddToClassList(k_UxmlLastRow);
                column.CellsContainer.Add(elem);
            }
        }

        VisualElement MakeNameCell(int rowId)
        {
            var row = m_Model.Rows[rowId];

            var item = ViewControllerUtility.LoadVisualTreeFromUxml(k_UxmlNameCellGuid);
            item.tooltip = MakeTooltipText(rowId);
            RegisterCellCallbacks(item, rowId);

            var colorBox = item.Q<VisualElement>(k_UxmlCellColorBox);
            colorBox.AddToClassList(k_UxmlElementSolidColor + row.StyleId);
            var colorBoxUnused = item.Q<VisualElement>(k_UxmlCellColorBoxFree);
            UIElementsHelper.SetVisibility(colorBoxUnused, row.UsedA > 0);

            var reservedLabel = item.Q<VisualElement>(k_UxmlCellColorBoxReserved);
            UIElementsHelper.SetVisibility(reservedLabel, row.UsedA > 0);

            var text = item.Q<Label>(k_UxmlCellNameLabel);
            text.text = row.Name;
            return item;
        }

        VisualElement MakeSizeCell(int rowId, long rowTotal, long rowUsed)
        {
            string sizeText;
            if (rowUsed > 0)
                sizeText = string.Format(k_LegendTableSizeWithUsedFormatString, EditorUtility.FormatBytes(rowUsed), EditorUtility.FormatBytes(rowTotal));
            else
                sizeText = string.Format(k_LegendTableSizeFormatString, EditorUtility.FormatBytes(rowTotal));

            var item = ViewControllerUtility.LoadVisualTreeFromUxml(k_UxmlSizeCellGuid);
            item.Q<Label>(k_UxmlCellValueLabel).text = sizeText;
            item.tooltip = MakeTooltipText(rowId);
            RegisterCellCallbacks(item, rowId);
            return item;
        }

        string MakeTooltipText(int rowId)
        {
            var row = m_Model.Rows[rowId];
            string toolTipText = row.Name + "\n";

            if (m_Model.CompareMode)
            {
                toolTipText += "\nA:\n";
                toolTipText += MakeToolTipTextForSnapshot(row.TotalA, row.UsedA, m_Model.TotalA, " - ");

                toolTipText += "\n\nB:\n";
                toolTipText += MakeToolTipTextForSnapshot(row.TotalB, row.UsedB, m_Model.TotalB, " - ");
            }
            else
                toolTipText += MakeToolTipTextForSnapshot(row.TotalA, row.UsedA, m_Model.TotalA, "");

            return toolTipText;
        }

        string MakeToolTipTextForSnapshot(ulong rowTotal, ulong rowUsed, ulong columnTotal, string linePrefix)
        {
            var reservedPercent = ((float)rowTotal) / columnTotal * 100;

            if (rowUsed == 0)
                return linePrefix + string.Format(k_TooltipRowTotalFormat, EditorUtility.FormatBytes((long)rowTotal), reservedPercent);

            var usedPercent = ((float)rowUsed) / rowTotal * 100;
            return linePrefix + string.Format(k_TooltipRowUsedFormat, EditorUtility.FormatBytes((long)rowUsed), usedPercent) +
                "\n" + linePrefix + string.Format(k_TooltipRowReservedFormat, EditorUtility.FormatBytes((long)rowTotal), reservedPercent);
        }

        void RegisterCellCallbacks(VisualElement element, int rowId)
        {
            element.RegisterCallback<MouseEnterEvent>((e) => { OnRowHovered(rowId, true); });
            element.RegisterCallback<MouseLeaveEvent>((e) => { OnRowHovered(rowId, false); });
            element.RegisterCallback<PointerUpEvent>((e) => { OnRowClicked(rowId); });
        }

        void SetCellHoverState(VisualElement root, int index, bool state)
        {
            if (root.childCount <= index)
                return;

            var element = root[index];
            if (state)
                element.AddToClassList(k_UxmlCellHoverStateClass);
            else
                element.RemoveFromClassList(k_UxmlCellHoverStateClass);
        }

        void SetCellSelectedState(VisualElement root, int index, bool state)
        {
            if (root.childCount <= index)
                return;

            var element = root[index];
            if (state)
                element.AddToClassList(k_UxmlCellSelectedStateClass);
            else
                element.RemoveFromClassList(k_UxmlCellSelectedStateClass);
        }
    }
}
