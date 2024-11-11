using System;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
#if UNITY_2023_3_OR_NEWER
    [UxmlElement]
#endif
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    partial class BandwidthShadingConfigurationView : InjectedVisualElement<BandwidthShadingConfigurationView>
    {
        [UxmlQuery(Name = nameof(BandwidthSettings.MeshShadingFill))] MeshShadingFillDropdown MeshShadingFillField;
        [UxmlQuery] Toggle AutoScale;
        [UxmlQuery] IntegerField BandwidthMin;
        [UxmlQuery] IntegerField BandwidthMax;
        [UxmlQuery] HelpBox MinAndMaxWarning;

        [Inject] NetVisConfigurationWithEvents Configuration;
        [Inject] IReadonlyBandwidthStats BandwidthStats;

        BandwidthSettings Settings => Configuration.Configuration.Settings.Bandwidth;

        public BandwidthShadingConfigurationView()
        {
            MeshShadingFillField.Bind(Settings.MeshShadingFill, value =>
            {
                Settings.MeshShadingFill = value;
                Configuration.NotifySettingsChanged();
            });

            AutoScale.Bind(Settings.BandwidthAutoscaling, value =>
            {
                Settings.BandwidthAutoscaling = value;
                Configuration.NotifySettingsChanged();
            });

            BandwidthMin.Bind(Settings.BandwidthMin, value =>
            {
                Settings.BandwidthMin = value;
                Configuration.NotifySettingsChanged();
            });

            BandwidthMax.Bind(Settings.BandwidthMax, value =>
            {
                Settings.BandwidthMax = value;
                Configuration.NotifySettingsChanged();
            });

            UpdateMinAndMaxBandwidthFields();

            this.AddEventLifecycle(OnAttach, OnDetach);
        }

        void OnAttach(AttachToPanelEvent _)
        {
            BandwidthStats.OnBandwidthStatsUpdated += UpdateMinAndMaxBandwidthFields;
            Configuration.SettingsChanged += OnSettingsChanged;
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            BandwidthStats.OnBandwidthStatsUpdated -= UpdateMinAndMaxBandwidthFields;
            Configuration.SettingsChanged -= OnSettingsChanged;
        }

        void OnSettingsChanged(NetVisSettings _)
        {
            UpdateMinAndMaxBandwidthFields();
        }

        void UpdateMinAndMaxBandwidthFields()
        {
            var autoScaleEnabled = Settings.BandwidthAutoscaling;
            BandwidthMin.SetEnabled(!autoScaleEnabled);
            BandwidthMax.SetEnabled(!autoScaleEnabled);

            if (!autoScaleEnabled)
            {
                // Prevent users from entering negative values.
                // IntegerField unfortunately does not appear to have any
                // built in functionality for this, but this approach works.
                Settings.BandwidthMin = Math.Abs(Settings.BandwidthMin);
                Settings.BandwidthMax = Math.Abs(Settings.BandwidthMax);
            }

            var minBandwidth = autoScaleEnabled ? BandwidthStats.MinBandwidth : Settings.BandwidthMin;
            var maxBandwidth = autoScaleEnabled ? BandwidthStats.MaxBandwidth : Settings.BandwidthMax;

            BandwidthMin.SetValueWithoutNotify((int)minBandwidth);
            BandwidthMax.SetValueWithoutNotify((int)maxBandwidth);

            UpdateMinAndMaxBandwidthWarning();
        }

        void UpdateMinAndMaxBandwidthWarning()
        {
            var min = Settings.BandwidthMin;
            var max = Settings.BandwidthMax;

            var displayWarning = !Settings.BandwidthAutoscaling && (min >= max);
            MinAndMaxWarning.IncludeInLayout(displayWarning);
            if (displayWarning)
            {
                MinAndMaxWarning.messageType = HelpBoxMessageType.Warning;
                MinAndMaxWarning.text = min > max
                    ?  "Max bandwidth must be larger than min bandwidth"
                    : $"Max bandwidth must be larger than min bandwidth. A value of {min + 1} will be used";
            }
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<BandwidthShadingConfigurationView, UxmlTraits> { }
#endif
    }
}
