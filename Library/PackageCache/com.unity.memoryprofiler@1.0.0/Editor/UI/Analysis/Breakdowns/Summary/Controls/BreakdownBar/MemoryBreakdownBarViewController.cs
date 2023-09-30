using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.MemoryProfiler.Editor.UI
{
    internal class MemoryBreakdownBarViewController : ViewController
    {
        const string k_UxmlAssetGuid = "f1260fe1fcaaea242b006822b2aff5a2";

        // Breakdown bar parts
        const string k_UxmlMemoryUsageBarContainerA = "memory-usage-breakdown__bar__container-a";
        const string k_UxmlMemoryUsageBarContainerB = "memory-usage-breakdown__bar__container-b";
        const string k_UxmlMemoryUsageBarHeaderTitle = "memory-usage-breakdown__bar__total-value";
        const string k_UxmlMemoryUsageBarBreakdownBar = "memory-usage-breakdown__bar";
        const string k_UxmlMemoryUsageBarCell = "memory-usage-breakdown__bar__element";
        const string k_UxmlMemoryUsageBarCellReserved = "memory-usage-breakdown__bar__element-reserved";
        const string k_UxmlMemoryUsageBarCellUsed = "memory-usage-breakdown__bar__element-used-portion";
        const string k_UxmlMemoryUsageBarCellRemainder = "memory-usage-breakdown__category__color-remainder";
        // Element states
        const string k_UxmlLegendTableCellHoverStateClass = "memory-usage-breakdown__element-hovered";
        const string k_UxmlLegendTableCellSelectedStateClass = "memory-usage-breakdown__element-selected";
        // Category color style templates
        const string k_UxmlElementSolidColor = "memory-usage-breakdown-color-category-";
        const string k_UxmlElementFrameColor = "memory-usage-breakdown-frame-category-";

        static readonly string k_TooltipRowUsedFormat = L10n.Tr("Used: {0} ({1:0.0}% of reserved)");
        static readonly string k_TooltipRowReservedFormat = L10n.Tr("Reserved: {0} ({1:0.0}% of total)");
        static readonly string k_TooltipRowTotalFormat = L10n.Tr("{0} ({1:0.0}% of total)");

        // Model
        readonly MemoryBreakdownModel m_Model;
        string m_TotalLabelFormat;

        // View
        VisualElement m_ContainerA;
        VisualElement m_BarA;
        Label m_TotalA;

        VisualElement m_ContainerB;
        VisualElement m_BarB;
        Label m_TotalB;

        // State
        bool m_NormalizeBars;

        public event Action<int, bool> OnRowHovered = delegate { };
        public event Action<int> OnRowClicked = delegate { };

        public MemoryBreakdownBarViewController(MemoryBreakdownModel model)
        {
            m_Model = model;

            m_NormalizeBars = false;
        }

        public string TotalLabelFormat
        {
            get => m_TotalLabelFormat;
            set
            {
                m_TotalLabelFormat = value;
                if (IsViewLoaded)
                    RefreshTotalLabels();
            }
        }

        public bool Normalize
        {
            get => m_NormalizeBars;
            set
            {
                m_NormalizeBars = value;
                if (IsViewLoaded)
                    RefreshView();
            }
        }

        public void SetCellHovered(int index, bool state)
        {
            SetCellHoverState(m_BarA, index, state);
            SetCellHoverState(m_BarB, index, state);
        }

        public void SetCellSelected(int index, bool state)
        {
            SetCellSelectedState(m_BarA, index, state);
            SetCellSelectedState(m_BarB, index, state);
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

        protected virtual void GatherViewReferences()
        {
            m_ContainerA = View.Q(k_UxmlMemoryUsageBarContainerA);
            m_BarA = m_ContainerA.Q(k_UxmlMemoryUsageBarBreakdownBar);
            m_TotalA = m_ContainerA.Q<Label>(k_UxmlMemoryUsageBarHeaderTitle);

            m_ContainerB = View.Q(k_UxmlMemoryUsageBarContainerB);
            m_BarB = m_ContainerB.Q(k_UxmlMemoryUsageBarBreakdownBar);
            m_TotalB = m_ContainerB.Q<Label>(k_UxmlMemoryUsageBarHeaderTitle);
        }

        protected virtual void RefreshView()
        {
            var maxValue = Math.Max(m_Model.TotalA, m_Model.TotalB);

            // Build breakdown bar for snapshot *A*
            var maxValueBarA = m_NormalizeBars ? m_Model.TotalA : maxValue;
            var accumulatedTotalA = RefreshBar(m_BarA, m_Model.TotalA, maxValueBarA, (row) => { return (row.TotalA, row.UsedA); });
            MakeRemainderCell(m_BarA, accumulatedTotalA, maxValueBarA);

            // Build breakdown bar for snapshot *B*
            UIElementsHelper.SetVisibility(m_ContainerB, m_Model.CompareMode);
            if (m_Model.CompareMode)
            {
                var maxValueBarB = m_NormalizeBars ? m_Model.TotalB : maxValue;
                var accumulatedTotalB = RefreshBar(m_BarB, m_Model.TotalB, maxValueBarB, (row) => { return (row.TotalB, row.UsedB); });
                MakeRemainderCell(m_BarB, accumulatedTotalB, maxValueBarB);
            }

            RefreshTotalLabels();
        }

        void RefreshTotalLabels()
        {
            UIElementsHelper.SetVisibility(m_TotalA, m_TotalLabelFormat != null);
            UIElementsHelper.SetVisibility(m_TotalB, m_TotalLabelFormat != null);

            if (m_TotalLabelFormat != null)
            {
                m_TotalA.text = string.Format(m_TotalLabelFormat, EditorUtility.FormatBytes((long)m_Model.TotalA));
                m_TotalB.text = string.Format(m_TotalLabelFormat, EditorUtility.FormatBytes((long)m_Model.TotalB));
            }
        }

        ulong RefreshBar(VisualElement root, ulong total, ulong maxValue, Func<MemoryBreakdownModel.Row, (ulong, ulong)> accessor)
        {
            // Remove all old bar parts
            root.Clear();

            // Create cells for column from model data
            ulong accumulatedTotal = 0;
            for (var i = 0; i < m_Model.Rows.Count; i++)
            {
                var row = m_Model.Rows[i];
                (ulong rowTotal, ulong rowUsed) = accessor(row);
                var elem = MakeCell(i, row.StyleId, rowTotal, rowUsed, total, maxValue);
                root.Add(elem);

                accumulatedTotal += rowTotal;
            }

            return accumulatedTotal;
        }

        VisualElement MakeCell(int rowId, string styleId, ulong rowTotal, ulong rowUsed, ulong total, ulong maxValue)
        {
            float widthInPercent = ((float)rowTotal * 100) / maxValue;
            float usedWidthInPercent = ((float)rowUsed * 100) / rowTotal;

            // Bar section which defines size
            var cell = new VisualElement();
            cell.AddToClassList(k_UxmlMemoryUsageBarCell);
            cell.style.flexGrow = (float)Math.Round(widthInPercent, 1);
            cell.tooltip = MakeTooltipText(rowId, rowTotal, rowUsed, total);
            RegisterCellCallbacks(cell, rowId);

            var reserved = new VisualElement();
            reserved.AddToClassList(k_UxmlMemoryUsageBarCellReserved);
            if (rowUsed > 0)
                reserved.AddToClassList(k_UxmlElementFrameColor + styleId);
            else
                reserved.AddToClassList(k_UxmlElementSolidColor + styleId);
            cell.Add(reserved);

            // UIToolkit doesn't support repeating patterns
            // Remove when UIE-851 is completed
            if (styleId == "unknown")
            {
                var stripes = new BackgroundPattern();
                stripes.AddToClassList("background-color__memory-summary-category__unknown__pattern");
                stripes.Scale = 0.5f;
                reserved.Add(stripes);
            }

            // "Used" bar section
            if (rowUsed > 0)
            {
                var used = new VisualElement();
                used.AddToClassList(k_UxmlMemoryUsageBarCellUsed);
                used.AddToClassList(k_UxmlElementSolidColor + styleId);
                used.style.SetBarWidthInPercent(usedWidthInPercent);
                reserved.Add(used);
            }

            return cell;
        }

        void MakeRemainderCell(VisualElement root, ulong value, ulong total)
        {
            if (value >= total)
                return;

            float widthInPercent = ((float)(total - value)) / total * 100;

            var cell = new VisualElement();
            cell.AddToClassList(k_UxmlMemoryUsageBarCell);
            cell.AddToClassList(k_UxmlMemoryUsageBarCellRemainder);
            cell.style.flexGrow = (float)Math.Round(widthInPercent, 1);
            cell.style.marginLeft = cell.style.marginRight = (StyleLength)1.5;
            root.Add(cell);
        }

        string MakeTooltipText(int rowId, ulong rowTotal, ulong rowUsed, ulong total)
        {
            var row = m_Model.Rows[rowId];

            string toolTipText = row.Name + "\n";

            var reservedPercent = ((float)rowTotal) / total * 100;

            if (rowUsed == 0)
                return toolTipText + string.Format(k_TooltipRowTotalFormat, EditorUtility.FormatBytes((long)rowTotal), reservedPercent);

            var usedPercent = ((float)rowUsed) / rowTotal * 100;
            return toolTipText + string.Format(k_TooltipRowUsedFormat, EditorUtility.FormatBytes((long)rowUsed), usedPercent) +
                "\n" + string.Format(k_TooltipRowReservedFormat, EditorUtility.FormatBytes((long)rowTotal), reservedPercent);
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
                element.AddToClassList(k_UxmlLegendTableCellHoverStateClass);
            else
                element.RemoveFromClassList(k_UxmlLegendTableCellHoverStateClass);
        }

        void SetCellSelectedState(VisualElement root, int index, bool state)
        {
            if (root.childCount <= index)
                return;

            var element = root[index];
            if (state)
                element.AddToClassList(k_UxmlLegendTableCellSelectedStateClass);
            else
                element.RemoveFromClassList(k_UxmlLegendTableCellSelectedStateClass);
        }
    }
}
