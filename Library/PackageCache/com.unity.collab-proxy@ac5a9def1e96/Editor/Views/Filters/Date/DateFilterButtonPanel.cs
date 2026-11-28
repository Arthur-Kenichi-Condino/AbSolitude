using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Configuration;
using PlasticGui.WorkspaceWindow.Filters;
using Unity.PlasticSCM.Editor.UI;
using LayoutFilters = Codice.Client.BaseCommands.LayoutFilters;

namespace Unity.PlasticSCM.Editor.Views.Filters.Date
{
    internal class DateFilterButtonPanel : VisualElement
    {
        internal DateFilterButtonPanel(
            RepositorySpec repSpec,
            FiltersPanel.IFilterableView filterableView,
            IFilterConfig<LayoutFilters.DateFilter> config)
        {
            mRepSpec = repSpec;
            mFilterableView = filterableView;
            mConfig = config;

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            BuildComponents();
        }

        internal void UpdateRepositorySpec(RepositorySpec repSpec)
        {
            mRepSpec = repSpec;
        }

        internal void SetWorkspaceUIConfiguration(WorkspaceUIConfiguration config)
        {
            mConfig.SetWorkspaceUIConfiguration(config);
        }

        internal void LoadFilters()
        {
            LayoutFilters.DateFilter currentFilter = mConfig.GetCurrentFilter();

            string customOptionText = GetCustomOptionText(currentFilter);

            mCustomOptionContent = customOptionText != null
                ? new GUIContent(customOptionText)
                : null;

            mDisplayOptions = BuildDisplayOptions(customOptionText);

            mCurrentIndex = customOptionText != null
                ? BUILT_IN_OPTIONS.Length
                : GetIndexFromFilter(currentFilter);
        }

        void BuildComponents()
        {
            var dateFilterContainer = new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();

                float comboWidth = mCustomOptionContent != null
                    ? EditorStyles.toolbarDropDown.CalcSize(
                        mCustomOptionContent).x + DROPDOWN_ARROW_WIDTH
                    : UnityConstants.TOOLBAR_DATE_FILTER_COMBO_WIDTH;

                int newIndex = EditorGUILayout.Popup(
                    mCurrentIndex,
                    mDisplayOptions,
                    EditorStyles.toolbarDropDown,
                    GUILayout.Width(comboWidth),
                    GUILayout.Height(20));

                if (EditorGUI.EndChangeCheck())
                {
                    if (newIndex >= BUILT_IN_OPTIONS.Length)
                        return;

                    ApplyFilter(GetFilterFromIndex(newIndex));
                }
            });

            Add(dateFilterContainer);
        }

        void ApplyFilter(LayoutFilters.DateFilter newFilter)
        {
            LayoutFilters.DateFilter oldFilter = mConfig.GetCurrentFilter();

            mConfig.SetCurrentFilter(newFilter);

            if (!mFilterableView.ApplyDateFilter(oldFilter, newFilter))
            {
                mConfig.SetCurrentFilter(oldFilter);
                return;
            }

            LoadFilters();
        }

        static string[] BuildDisplayOptions(string customOptionText)
        {
            if (customOptionText == null)
                return BUILT_IN_OPTIONS;

            string[] result = new string[BUILT_IN_OPTIONS.Length + 1];

            BUILT_IN_OPTIONS.CopyTo(result, 0);
            result[BUILT_IN_OPTIONS.Length] = customOptionText;

            return result;
        }

        static string GetCustomOptionText(LayoutFilters.DateFilter filter)
        {
            if (filter == null)
                return null;

            if (filter.SinceTimeType != LayoutFilters.SinceTimeType.AGivenDate)
                return null;

            if (!string.IsNullOrEmpty(filter.FilterName))
            {
                // '/' opens a submenu in editor popups, so it cannot be displayed
                return filter.FilterName.Replace('/', '-');
            }

            string dates = filter.StartDate.ToString(CUSTOM_OPTION_DATE_FORMAT) +
                (filter.EndDateEnabled
                    ? " - " + filter.EndDate.ToString(CUSTOM_OPTION_DATE_FORMAT)
                    : string.Empty);

            return PlasticLocalization.Name.SinceDateFilter.GetString(dates);
        }

        static LayoutFilters.DateFilter GetFilterFromIndex(int index)
        {
            LayoutFilters.SinceTimeType newFilterType;
            switch (index)
            {
                case 0:
                    newFilterType = LayoutFilters.SinceTimeType.OneWeekAgo;
                    break;
                case 1:
                    newFilterType = LayoutFilters.SinceTimeType.FifteenDaysAgo;
                    break;
                case 2:
                    newFilterType = LayoutFilters.SinceTimeType.OneMonthAgo;
                    break;
                case 3:
                    newFilterType = LayoutFilters.SinceTimeType.ThreeMonthsAgo;
                    break;
                case 4:
                    newFilterType = LayoutFilters.SinceTimeType.OneYearAgo;
                    break;
                default:
                    newFilterType = LayoutFilters.SinceTimeType.OneMonthAgo;
                    break;
            }
            return LayoutFilters.DateFilter.BuildFromTimeAgo(newFilterType);
        }

        static int GetIndexFromFilter(LayoutFilters.DateFilter filter)
        {
            if (filter == null)
                return 2; // Default to LastMonth

            switch (filter.SinceTimeType)
            {
                case LayoutFilters.SinceTimeType.OneWeekAgo:
                    return 0;
                case LayoutFilters.SinceTimeType.FifteenDaysAgo:
                    return 1;
                case LayoutFilters.SinceTimeType.OneMonthAgo:
                    return 2;
                case LayoutFilters.SinceTimeType.ThreeMonthsAgo:
                    return 3;
                case LayoutFilters.SinceTimeType.OneYearAgo:
                    return 4;
                default:
                    return 2; // Default to LastMonth
            }
        }

        int mCurrentIndex;
        GUIContent mCustomOptionContent;
        string[] mDisplayOptions = BUILT_IN_OPTIONS;

        // Not used yet, it will be used for the SavedFiltersPanel
        RepositorySpec mRepSpec;

        readonly IFilterConfig<LayoutFilters.DateFilter> mConfig;
        readonly FiltersPanel.IFilterableView mFilterableView;

        const string CUSTOM_OPTION_DATE_FORMAT = "yyyy-MM-dd";
        const float DROPDOWN_ARROW_WIDTH = 15f;

        static readonly string[] BUILT_IN_OPTIONS = new string[]
        {
            "Last Week",
            "Last 15 Days",
            "Last Month",
            "Last 3 Months",
            "Last Year"
        };
    }
}
