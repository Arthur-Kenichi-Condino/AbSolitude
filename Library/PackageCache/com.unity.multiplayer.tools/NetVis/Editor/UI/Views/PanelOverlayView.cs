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
        [UxmlQuery] BandwidthConfigurationView BandwidthConfigurationView;
        [UxmlQuery] OwnershipConfigurationView OwnershipConfigurationView;
        [UxmlQuery] ToolbarToggle BandwidthToggle;
        [UxmlQuery] VisualElement BandwidthIcon;
        [UxmlQuery] ToolbarToggle OwnershipToggle;
        [UxmlQuery] VisualElement OwnershipIcon;
        [UxmlQuery] ToolbarButton SettingsButton;
        [UxmlQuery] VisualElement SettingsIcon;

        [Inject] NetVisConfigurationWithEvents Configuration;

        protected override void Initialized()
        {
            LoadIcons().Forget();
            SetupBindings();
            OnMetricChanged(Configuration.Metric);
            this.AddEventLifecycle(OnAttach, OnDetach);
        }

        async Task LoadIcons()
        {
            var editorTheme = EditorGUIUtility.isProSkin ? EditorTheme.Dark : EditorTheme.Light;
            var bandwidthIconTask = NetVisIcon.Bandwidth.LoadAsync(editorTheme);
            var ownershipIconTask = NetVisIcon.Ownership.LoadAsync(editorTheme);
            var settingsIconTask = NetVisIcon.Settings.LoadAsync(editorTheme);

            await Task.WhenAll(bandwidthIconTask, ownershipIconTask, settingsIconTask);
            
            BandwidthIcon.style.backgroundImage = bandwidthIconTask.Result;
            OwnershipIcon.style.backgroundImage = ownershipIconTask.Result;
            SettingsIcon.style.backgroundImage = settingsIconTask.Result;
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
            SettingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            Configuration.MetricChanged -= OnMetricChanged;

            SettingsButton.UnregisterCallback<ClickEvent>(OnSettingsClicked);
        }

        void OnSettingsClicked(ClickEvent _)
        {
            PopupWindow.Show(worldBound, new NetVisPopupWindowContent<CommonSettingsView>(320, 200));
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
