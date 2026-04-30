using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    class PanelOverlayView : InjectedVisualElement<PanelOverlayView>
    {
        const string k_SettingsPanelOpenKey = "NetVis.SettingsPanelOpen";
        
        [UxmlQuery] BandwidthConfigurationView BandwidthConfigurationView;
        [UxmlQuery] OwnershipConfigurationView OwnershipConfigurationView;
        [UxmlQuery] CommonSettingsView CommonSettingsView;
        [UxmlQuery] ToolbarToggle BandwidthToggle;
        [UxmlQuery] VisualElement BandwidthIcon;
        [UxmlQuery] ToolbarToggle OwnershipToggle;
        [UxmlQuery] VisualElement OwnershipIcon;
        [UxmlQuery] ToolbarToggle SettingsToggle;
        [UxmlQuery] VisualElement SettingsIcon;

        [Inject] NetVisConfigurationWithEvents Configuration;

        protected override void Initialized()
        {
            LoadIcons();
            SetupBindings();
            OnMetricChanged(Configuration.Metric);
            bool settingsPanelWasOpen = EditorPrefs.GetBool(k_SettingsPanelOpenKey, false);
            SettingsToggle.SetValueWithoutNotify(settingsPanelWasOpen);
            CommonSettingsView.SetInclude(settingsPanelWasOpen);
            this.AddEventLifecycle(OnAttach, OnDetach);
        }

        void LoadIcons()
        {
            var editorTheme = EditorGUIUtility.isProSkin ? EditorTheme.Dark : EditorTheme.Light;
            BandwidthIcon.style.backgroundImage = NetVisIcon.Bandwidth.LoadIcon(editorTheme);
            OwnershipIcon.style.backgroundImage = NetVisIcon.Ownership.LoadIcon(editorTheme);
            SettingsIcon.style.backgroundImage = NetVisIcon.Settings.LoadIcon(editorTheme);
        }

        void SetupBindings()
        {
            BandwidthToggle.Bind(
                Configuration.Metric == NetVisMetric.Bandwidth,
                value =>
                {
                    Configuration.Metric = value ? NetVisMetric.Bandwidth : NetVisMetric.None;
                });

            OwnershipToggle.Bind(
                Configuration.Metric == NetVisMetric.Ownership,
                value =>
                {
                    Configuration.Metric = value ? NetVisMetric.Ownership : NetVisMetric.None;
                });
        }

        void OnAttach(AttachToPanelEvent _)
        {
            Configuration.MetricChanged += OnMetricChanged;
            SettingsToggle.RegisterValueChangedCallback(OnSettingsToggled);
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            Configuration.MetricChanged -= OnMetricChanged;
            SettingsToggle.UnregisterValueChangedCallback(OnSettingsToggled);
        }

        void OnSettingsToggled(ChangeEvent<bool> evt)
        {
            CommonSettingsView.SetInclude(evt.newValue);
            EditorPrefs.SetBool(k_SettingsPanelOpenKey, evt.newValue);
        }

        void OnMetricChanged(NetVisMetric metric)
        {
            BandwidthToggle.SetValueWithoutNotify(metric == NetVisMetric.Bandwidth);
            OwnershipToggle.SetValueWithoutNotify(metric == NetVisMetric.Ownership);
            BandwidthConfigurationView.SetInclude(metric == NetVisMetric.Bandwidth);
            OwnershipConfigurationView.SetInclude(metric == NetVisMetric.Ownership);
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<PanelOverlayView, UxmlTraits> { }
#endif
    }
}
