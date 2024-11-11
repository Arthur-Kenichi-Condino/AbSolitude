using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [EditorToolbarElement(k_Id, typeof(SceneView))]
    class OwnershipToolbarDropdownToggle : NetVisToolbarDropdownToggle
    {
        public const string k_Id = "Network Visualization/Ownership Visualization Mode";

        public OwnershipToolbarDropdownToggle()
            : base(
                name: "Network Visualization Ownership",
                tooltip: "Visualize Networked Objects Ownership using colored categories.",
                icon: NetVisIcon.Ownership)
        {
        }

        protected override void OnMetricChanged(NetVisMetric metric)
        {
            SetValueWithoutNotify(metric == NetVisMetric.Ownership);
        }

        protected override void OnStateChange(ChangeEvent<bool> stateChange)
        {
            ConfigurationWithEvents.Metric = stateChange.newValue
                ? NetVisMetric.Ownership
                : NetVisMetric.None;
        }

        protected override void ShowOverlayPopUp()
        {
            PopupWindow.Show(worldBound, new NetVisPopupWindowContent<OwnershipConfigurationView>(400, 300));
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<OwnershipToolbarDropdownToggle, UxmlTraits> { }
#endif
    }
}
