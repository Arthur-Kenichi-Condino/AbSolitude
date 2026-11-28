using UnityEditor;
using UnityEngine;

using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UnityInternals.UnityEditor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal static class RepaintEditor
    {
        internal static void InspectorWindow()
        {
            UnityEditor.Editor[] editors =
                Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();

            foreach (UnityEditor.Editor editor in editors)
                editor.Repaint();
        }

        internal static void ProjectWindow()
        {
            UnityEditor.EditorWindow projectWindow = FindEditorWindow.ProjectWindow();

            if (projectWindow == null)
                return;

            projectWindow.Repaint();
        }

        internal static void HierarchyWindow()
        {
            EditorApplication.RepaintHierarchyWindow();

            // We use an extension method to refresh the hierarchy V2 view via reflection
            // while there isn't a public method to do it
            RefreshHierarchyV2Windows();
        }

        static void RefreshHierarchyV2Windows()
        {
#if UNITY_6000_5_OR_NEWER
            Unity.Hierarchy.Editor.HierarchyWindow[] hierarchyV2Windows =
                Resources.FindObjectsOfTypeAll<Unity.Hierarchy.Editor.HierarchyWindow>();

            foreach (Unity.Hierarchy.Editor.HierarchyWindow hierarchyV2Window in hierarchyV2Windows)
            {
                hierarchyV2Window.View.RefreshItems();
            }
#endif
        }

        internal static void AllWindowsWithOverlays()
        {
            InspectorWindow();
            ProjectWindow();
            HierarchyWindow();
        }
    }
}
