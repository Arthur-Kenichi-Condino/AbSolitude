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
    partial class OwnershipConfigurationView : InjectedVisualElement<OwnershipConfigurationView>
    {
        [UxmlQuery] OwnershipServerClientConfigurationView OwnershipServerClientConfigurationView;
        [UxmlQuery] Toggle MeshShadingEnabled;
        [UxmlQuery] Toggle TextOverlayEnabled;

        [Inject] NetVisConfigurationWithEvents Configuration;
        OwnershipSettings OwnershipSettings => Configuration.Configuration.Settings.Ownership;

        public OwnershipConfigurationView()
        {
            MeshShadingEnabled.Bind(
                OwnershipSettings.MeshShadingEnabled,
                value =>
                {
                    OwnershipSettings.MeshShadingEnabled = value;
                    Configuration.NotifySettingsChanged();
                });

            TextOverlayEnabled.Bind(
                OwnershipSettings.TextOverlayEnabled,
                value =>
                {
                    OwnershipSettings.TextOverlayEnabled = value;
                    Configuration.NotifySettingsChanged();
                });
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<OwnershipConfigurationView, UxmlTraits> { }
#endif
    }
}
