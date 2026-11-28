using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [Overlay(typeof(SceneView), "Network Visualization", true)]
    [Icon(k_IconPath)]
    class NetVisToolbarOverlay : ToolbarOverlay
    {
        const string k_IconPath =
            // This is the same logic as NetVisIcon.GetPath, however for use in an attribute it must be a compile-time constant
            NetVisEditorPaths.k_IconsRoot + nameof(NetVisIcon.NetSceneVis) + ".png";

        public NetVisToolbarOverlay()
            : base(
                BandwidthToolbarDropdownToggle.k_Id,
                OwnershipToolbarDropdownToggle.k_Id,
                SettingsToolbarButton.k_Id)
        {
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                name = nameof(NetVisToolbarOverlay),
                style =
                {
                    width = 300,
                },
            };

            root.Add(layout == Layout.Panel
                ? new PanelOverlayView()
                : new ToolbarOverlayView());

            return root;
        }
    }
}
