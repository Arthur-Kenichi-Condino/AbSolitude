using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    class CommonSettingsView : InjectedVisualElement<CommonSettingsView>
    {
        [UxmlQuery] Toggle Outline;
        [UxmlQuery] SliderInt Saturation;

        [Inject] NetVisConfigurationWithEvents Configuration;
        NetVisCommonSettings CommonSettings => Configuration.Configuration.Settings.Common;

        public CommonSettingsView()
        {
            Outline.Bind(CommonSettings.Outline, value =>
            {
                CommonSettings.Outline = value;
                Configuration.NotifySettingsChanged();
            });

            Saturation.Bind(Mathf.RoundToInt(CommonSettings.SceneSaturation * 100f), value =>
            {
                CommonSettings.SceneSaturation = value / 100f;
                Configuration.NotifySettingsChanged();
            });
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public new class UxmlFactory : UxmlFactory<CommonSettingsView, UxmlTraits> { }
#endif
    }
}
