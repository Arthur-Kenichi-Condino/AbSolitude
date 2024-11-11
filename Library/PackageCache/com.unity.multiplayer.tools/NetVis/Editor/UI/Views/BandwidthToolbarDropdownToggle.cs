using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [EditorToolbarElement(k_Id, typeof(SceneView))]
    class BandwidthToolbarDropdownToggle : NetVisToolbarDropdownToggle
    {
        public const string k_Id = "Network Visualization/Bandwidth Visualization Mode";

        public BandwidthToolbarDropdownToggle()
            : base(
                name: "Network Visualization Bandwidth",
                tooltip: "Visualize Networked Objects Bandwidth using colored categories.",
                icon: NetVisIcon.Bandwidth)
        {
        }

        protected override void OnMetricChanged(NetVisMetric metric)
        {
            SetValueWithoutNotify(metric == NetVisMetric.Bandwidth);
        }

        protected override void OnStateChange(ChangeEvent<bool> stateChange)
        {
            ConfigurationWithEvents.Metric = stateChange.newValue
                ? NetVisMetric.Bandwidth
                : NetVisMetric.None;
        }

        protected override void ShowOverlayPopUp()
        {
            PopupWindow.Show(worldBound, new NetVisPopupWindowContent<BandwidthConfigurationView>(320, 300));
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<BandwidthToolbarDropdownToggle, UxmlTraits> { }
#endif
    }
}
