#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEngine.UIElements;
#endif

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    /// <summary>
    /// Adds info next to all objects in the hierarchy that are NetworkObjects.
    /// In play mode, the network objects are marked with a purple square. If the current client owns the object, the
    /// square is filled with a crown icon. The id of the current client is also displayed.
    /// In edit mode only the square is shown.
    /// </summary>
    /// <remarks>
    /// Renders through the legacy IMGUI callback and, on 6000.5+, the UI Toolkit HierarchyWindow.BindViewItem event.
    /// Only the path matching the active hierarchy window draws, so the two never overlap.
    /// </remarks>
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
                RepaintAllHierarchyWindows();
            }
        }

        static HierarchyWindowDecorator()
        {
            s_Enabled = EditorPrefs.GetBool(k_EditorPrefsKey, false);
#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= EntityDecoratorHandler;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += EntityDecoratorHandler;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= InstanceDecoratorHandler;
            EditorApplication.hierarchyWindowItemOnGUI += InstanceDecoratorHandler;
#endif
#if UNITY_6000_5_OR_NEWER
            HierarchyWindow.BindView -= OnBindView;
            HierarchyWindow.BindView += OnBindView;
            HierarchyWindow.BindViewItem -= OnBindViewItem;
            HierarchyWindow.BindViewItem += OnBindViewItem;
#endif

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

        static void RepaintAllHierarchyWindows()
        {
            // Legacy IMGUI hierarchy.
            EditorApplication.RepaintHierarchyWindow();
#if UNITY_6000_5_OR_NEWER
            // New UI Toolkit hierarchy has no public rebind, so re-apply state to every realized row directly.
            // Off-screen (virtualized) rows are handled by OnBindViewItem when scrolled into view.
            foreach (var window in Resources.FindObjectsOfTypeAll<HierarchyWindow>())
                window.View?.Query<HierarchyViewItem>().ForEach(UpdateNetworkObjectInfo);
#endif
        }

#if UNITY_6000_4_OR_NEWER
        static void EntityDecoratorHandler(EntityId entityId, Rect rect)
        {
            if (!s_Enabled) return;
            s_TextStyle ??= new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, 0, 0)};

            var gameObject = EditorUtility.EntityIdToObject(entityId) as GameObject;
            if (gameObject == null)
                return;

            RenderNetworkObjectIcon(rect, gameObject);
        }
#else
        static void InstanceDecoratorHandler(int instanceID, Rect rect)
        {
            if (!s_Enabled) return;
            s_TextStyle ??= new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, 0, 0)};

            // InstanceIDToObject(int) is obsolete from 6000.3 (EntityIdToObject added in 6000.3.0).
#if UNITY_6000_3_OR_NEWER
            var gameObject = EditorUtility.EntityIdToObject((EntityId)instanceID) as GameObject;
#else
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#endif
            if (gameObject == null)
                return;

            RenderNetworkObjectIcon(rect, gameObject);
        }
#endif
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

#if UNITY_6000_5_OR_NEWER
        // Package-scoped element names so we only touch our own elements and never collide with other decorators.
        const string k_RootElementName = "MultiplayerToolsNetworkObjectInfo";
        const string k_IconElementName = "MultiplayerToolsNetworkObjectIcon";
        const string k_OwnerIdLabelName = "MultiplayerToolsNetworkObjectOwnerId";

        const string k_StyleSheetPath = "Packages/com.unity.multiplayer.tools/Editor/MultiplayerToolsWindow/UI/HierarchyWindowDecorator.uss";
        const string k_RootClass = "multiplayer-tools-network-info";
        const string k_OwnerIdClass = "multiplayer-tools-network-info__owner-id";
        const string k_IconClass = "multiplayer-tools-network-info__icon";

        static StyleSheet s_StyleSheet;

        static void OnBindView(HierarchyWindow window, HierarchyView view)
        {
            // Add our stylesheet once per hierarchy view so the info elements pick up their static layout from USS.
            s_StyleSheet ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            if (s_StyleSheet != null && !view.StyleContainer.styleSheets.Contains(s_StyleSheet))
                view.StyleContainer.styleSheets.Add(s_StyleSheet);
        }

        static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
            => UpdateNetworkObjectInfo(item);

        static void UpdateNetworkObjectInfo(HierarchyViewItem item)
        {
            // Rows are pooled, so re-apply full state every call. Only touch our own element (never Clear() the
            // container or restyle shared elements) so other decorators on the row are left intact.
            if (!s_Enabled || item.Handler is not HierarchyGameObjectHandler goHandler)
            {
                HideNetworkObjectInfo(item);
                return;
            }

            var go = goHandler.GetGameObject(item.Node);
            if (go == null || !go.TryGetComponent<NetworkObject>(out var no))
            {
                HideNetworkObjectInfo(item);
                return;
            }

            var root = GetOrCreateNetworkObjectInfo(item);
            root.style.display = DisplayStyle.Flex;

            var icon = root.Q<VisualElement>(k_IconElementName);
            icon.style.backgroundImage = new StyleBackground(no.IsOwner ? s_OwnershipActiveIcon : s_OwnershipInActiveIcon);

            var label = root.Q<Label>(k_OwnerIdLabelName);
            if (Application.isPlaying)
            {
                label.text = no.OwnerClientId.ToString();
                label.style.display = DisplayStyle.Flex;
            }
            else
            {
                label.style.display = DisplayStyle.None;
            }
        }

        static VisualElement GetOrCreateNetworkObjectInfo(HierarchyViewItem item)
        {
            var root = item.RightCustomContainer.Q<VisualElement>(k_RootElementName);
            if (root != null)
                return root;

            root = new VisualElement { name = k_RootElementName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(k_RootClass);

            var label = new Label { name = k_OwnerIdLabelName, pickingMode = PickingMode.Ignore };
            label.AddToClassList(k_OwnerIdClass);
            root.Add(label);

            var icon = new VisualElement { name = k_IconElementName, pickingMode = PickingMode.Ignore };
            icon.AddToClassList(k_IconClass);
            root.Add(icon);

            item.RightCustomContainer.Add(root);
            return root;
        }

        static void HideNetworkObjectInfo(HierarchyViewItem item)
        {
            var root = item.RightCustomContainer.Q<VisualElement>(k_RootElementName);
            if (root != null)
                root.style.display = DisplayStyle.None;
        }
#endif
    }
}
#endif
