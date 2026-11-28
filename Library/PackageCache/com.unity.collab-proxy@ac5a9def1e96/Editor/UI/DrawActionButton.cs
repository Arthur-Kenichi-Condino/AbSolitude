using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class DrawActionButton
    {
        internal static bool For(string buttonText)
        {
            GUIContent buttonContent = new GUIContent(buttonText);

            return ForRegularButton(buttonContent);
        }

        static bool ForRegularButton(GUIContent buttonContent)
        {
            GUIStyle style = UnityStyles.PendingChangesTab.ActionButton;

            Rect rt = GUILayoutUtility.GetRect(
                buttonContent,
                style,
                GUILayout.MinWidth(UnityConstants.REGULAR_BUTTON_WIDTH));

            return GUI.Button(rt, buttonContent, style);
        }
    }
}
