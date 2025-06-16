using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [EditorToolbarElement(k_Id, typeof(SceneView))]
    class SettingsToolbarButton : EditorToolbarButton
    {
        public const string k_Id = "Network Visualization/Settings";

        // We have to cache this because the constructor needs a static Action, but we want a reference
        // to the button instance's worldBound in the click handler. Keeping this hacky thing in this class
        // so it's self-contained. Hopefully we find a way to access the instance-specific activationRect.
        static SettingsToolbarButton s_Instance;

        public SettingsToolbarButton()
            : base(
                string.Empty,
                EditorGUIUtility.FindTexture("_Popup"),
                ShowSettings)
        {
            s_Instance = this;
        }

        static void ShowSettings()
        {
            PopupWindow.Show(s_Instance.worldBound, new NetVisPopupWindowContent<CommonSettingsView>(320, 200));
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<SettingsToolbarButton, UxmlTraits> { }
#endif
    }
}
