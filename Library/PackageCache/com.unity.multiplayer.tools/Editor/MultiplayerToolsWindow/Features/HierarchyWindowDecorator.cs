#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    /// <summary>
    /// Adds info next to all objects in the hierarchy that are NetworkObjects.
    /// In play mode, the network objects are marked with a purple square. If the current client owns the object, the
    /// square is filled with a crown icon. The id of the current client is also displayed.
    /// In edit mode only the square is shown.
    /// </summary>
    [InitializeOnLoad]
    static class HierarchyWindowDecorator
    {
        const string k_EditorPrefsKey = "MptWindow.HierarchyWindowDecorator.Enabled";
        const string k_IconsPath = "Packages/com.unity.multiplayer.tools/Editor/MultiplayerToolsWindow/UI/Icons/";
        const string k_NonOwnerLightIconPath = k_IconsPath + "Network@2x.png";
        const string k_NonOwnerDarkIconPath = k_IconsPath + "d_Network@2x.png";
        const string k_OwnerLightIconPath = k_IconsPath + "OwnedNetwork@2x.png";
        const string k_OwnerDarkIconPath = k_IconsPath + "d_OwnedNetwork@2x.png";

        static bool s_Enabled = false;

        static GUIStyle s_TextStyle;
        static Texture2D s_OwnershipActiveIcon;
        static Texture2D s_OwnershipInActiveIcon;

        public static bool Enabled
        {
            get => s_Enabled;
            set
            {
                s_Enabled = value;
                EditorPrefs.SetBool(k_EditorPrefsKey, s_Enabled);
            }
        }

        static HierarchyWindowDecorator()
        {
            s_Enabled = EditorPrefs.GetBool(k_EditorPrefsKey, false);
            EditorApplication.hierarchyWindowItemOnGUI -= DecoratorHandler;
            EditorApplication.hierarchyWindowItemOnGUI += DecoratorHandler;

            // Load icons based on dark/light theme
            if (EditorGUIUtility.isProSkin)
            {
                s_OwnershipActiveIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(k_OwnerDarkIconPath);
                s_OwnershipInActiveIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(k_NonOwnerDarkIconPath);
            }
            else
            {
                s_OwnershipActiveIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(k_OwnerLightIconPath);
                s_OwnershipInActiveIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(k_NonOwnerLightIconPath);
            }
        }

        static void DecoratorHandler(int instanceID, Rect rect)
        {
            if (!s_Enabled) return;
            s_TextStyle ??= new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, 0, 0)};

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
                return;

            RenderNetworkObjectIcon(rect, gameObject);
        }

        static void RenderNetworkObjectIcon(Rect rect, GameObject go)
        {
            if (!go.TryGetComponent<NetworkObject>(out var no)) return;

            var iconRect = rect;
            iconRect.x += rect.width - 20;
            iconRect.height = rect.height;
            iconRect.width = iconRect.height;

            GUI.DrawTexture(iconRect, no.IsOwner ? s_OwnershipActiveIcon : s_OwnershipInActiveIcon);

            var textRect = rect;
            textRect.x += rect.width - iconRect.width - 30;
            textRect.width = 20;
            textRect.height = rect.height;

            if (Application.isPlaying)
                GUI.Box(textRect, no.OwnerClientId.ToString(), s_TextStyle);
        }
    }
}
#endif
