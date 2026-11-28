using System;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_6000_5_OR_NEWER
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
#endif

using Codice.Client.Common.Threading;
using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils;

using Object = UnityEngine.Object;

namespace Unity.PlasticSCM.Editor.AssetsOverlays
{
    internal static class DrawHierarchyOverlay
    {
        internal interface IGetAssetPathFromInstance
        {
            bool TryGetAssetPath(UnityObjectInstance instance, out string assetPath);
        }

        internal static IGetAssetPathFromInstance GetSubSceneAssetPath;

        internal static void Enable(
            string wkPath,
            IAssetStatusCache assetStatusCache)
        {
            if (mIsEnabled)
                return;

            mLog.Debug("Enable");

            mWkPath = wkPath;
            mAssetStatusCache = assetStatusCache;

            mIsEnabled = true;

#if UNITY_6000_5_OR_NEWER
            HierarchyWindow.BindViewItem += OnHierarchyBindViewItem;
#endif

#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyGUIByEntityId;
#else
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
#endif

            RepaintEditor.HierarchyWindow();
        }

        internal static void Disable()
        {
            mLog.Debug("Disable");

            mIsEnabled = false;

#if UNITY_6000_5_OR_NEWER
            HierarchyWindow.BindViewItem -= OnHierarchyBindViewItem;
#endif

#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyGUIByEntityId;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
#endif

            RepaintEditor.HierarchyWindow();

            mWkPath = null;
            mAssetStatusCache = null;
        }

#if UNITY_6000_5_OR_NEWER
        static void OnHierarchyBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            try
            {
#if UNITY_6000_6_OR_NEWER
                EntityId entityId = view.Source.GetEntityIdFromNode(in item.Node);
#else
                EntityId entityId = view.Source.GetEntityId(in item.Node);
#endif

                if (entityId == EntityId.None)
                    return;

                UnityObjectInstance instance = UnityObjectInstance.FromEntityId(entityId);

                string assetPath = GetAssetPathFromInstance(instance);

                if (assetPath == null)
                    return;

                DrawOverlayForHierarchyV2Asset(assetPath, item, mAssetStatusCache);
            }
            catch (Exception ex)
            {
                ExceptionsHandler.LogException(nameof(DrawHierarchyOverlay), ex);
            }
        }
#endif

#if UNITY_6000_4_OR_NEWER
        static void OnHierarchyGUIByEntityId(EntityId entityId, Rect selectionRect)
        {
            OnHierarchyGUI(UnityObjectInstance.FromEntityId(entityId), selectionRect);
        }
#else
        static void OnHierarchyGUI(int instanceId, Rect selectionRect)
        {
            OnHierarchyGUI(UnityObjectInstance.FromInstanceId(instanceId), selectionRect);
        }
#endif

        static void OnHierarchyGUI(UnityObjectInstance instance, Rect selectionRect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            try
            {
                string assetPath = GetAssetPathFromInstance(instance);

                if (assetPath == null)
                    return;

                DrawOverlayForHierarchyAsset(assetPath, selectionRect, mAssetStatusCache);
            }
            catch (Exception ex)
            {
                ExceptionsHandler.LogException(typeof(DrawHierarchyOverlay).Name, ex);
            }
        }

        static string GetAssetPathFromInstance(UnityObjectInstance instance)
        {
            string assetPath;

            if (TryGetAssetPathForScene(instance, out assetPath) ||
                TryGetAssetPathForSubScene(instance, out assetPath) ||
                TryGetAssetPathForPrefab(instance, out assetPath))
            {
                return AssetsPath.GetFullPathUnderWorkspace.ForAsset(mWkPath, assetPath);
            }

            return null;
        }

        static bool TryGetAssetPathForScene(UnityObjectInstance instance, out string assetPath)
        {
            assetPath = null;

            if (instance.FindObject() != null)
                return false;

            assetPath = FindScenePathForHandle(instance);

            return assetPath != null;
        }

        static string FindScenePathForHandle(UnityObjectInstance instance)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (instance.MatchesScene(scene) && scene.path != null)
                {
                    return scene.path;
                }
            }

            return null;
        }

        static bool TryGetAssetPathForSubScene(UnityObjectInstance instance, out string assetPath)
        {
            assetPath = null;

            string subSceneAssetPath;
            if (GetSubSceneAssetPath != null &&
                GetSubSceneAssetPath.TryGetAssetPath(instance, out subSceneAssetPath))
            {
                assetPath = subSceneAssetPath;
            }

            return assetPath != null;
        }

        static bool TryGetAssetPathForPrefab(UnityObjectInstance instance, out string assetPath)
        {
            assetPath = null;

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null)
                return false;

            Object hierarchyObject = instance.FindObject();

            if (hierarchyObject == null)
                return false;

            if (prefabStage.prefabContentsRoot == hierarchyObject)
                assetPath = prefabStage.assetPath;

            return assetPath != null;
        }

        static void DrawOverlayForHierarchyAsset(
            string assetFullPath,
            Rect selectionRect,
            IAssetStatusCache assetStatusCache)
        {
            AssetStatus assetStatus = assetStatusCache.GetStatus(assetFullPath);

            string tooltipText = AssetOverlay.GetTooltipText(
                assetStatus,
                assetStatusCache.GetLockStatusData(assetFullPath));

            DrawAssetOverlayIcon.ForStatus(
                selectionRect,
                assetStatus,
                tooltipText);
        }

#if UNITY_6000_5_OR_NEWER
        static void DrawOverlayForHierarchyV2Asset(
            string assetFullPath,
            HierarchyViewItem hierarchyViewItem,
            IAssetStatusCache assetStatusCache)
        {
            AssetStatus assetStatus = assetStatusCache.GetStatus(assetFullPath);

            string tooltipText = AssetOverlay.GetTooltipText(
                assetStatus,
                assetStatusCache.GetLockStatusData(assetFullPath));

            DrawAssetOverlayIcon.ForHierarchyV2(
                hierarchyViewItem.Icon,
                hierarchyViewItem.OverlayIcon,
                assetStatus,
                tooltipText);
        }
#endif

        static bool mIsEnabled;
        static IAssetStatusCache mAssetStatusCache;
        static string mWkPath;

        static readonly ILog mLog = PlasticApp.GetLogger("DrawHierarchyOverlay");
    }
}
