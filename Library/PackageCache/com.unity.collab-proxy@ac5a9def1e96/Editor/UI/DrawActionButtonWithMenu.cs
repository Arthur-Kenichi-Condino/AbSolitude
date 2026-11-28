using System;

using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class DrawActionButtonWithMenu
    {
        internal static void For(
            string text,
            string tooltip,
            Action buttonAction,
            GenericMenu actionMenu)
        {
            float width = MeasureMaxWidth.ForTexts(UnityStyles.PendingChangesTab.ActionButtonLeft, text);

            For(
                text,
                tooltip,
                width,
                buttonAction,
                actionMenu,
                UnityStyles.PendingChangesTab.ActionButtonLeft,
                UnityStyles.PendingChangesTab.DropDownButton);
        }

        internal static void ForTopbar(
            string text,
            string tooltip,
            Action buttonAction,
            GenericMenu actionMenu)
        {
            float width = MeasureMaxWidth.ForTexts(UnityStyles.Topbar.Button, text);

            For(
                text,
                tooltip,
                width,
                buttonAction,
                actionMenu,
                UnityStyles.Topbar.ButtonLeft,
                UnityStyles.Topbar.ButtonRight);
        }

        static void For(
            string text,
            string tooltip,
            float width,
            Action buttonAction,
            GenericMenu actionMenu,
            GUIStyle buttonStyle,
            GUIStyle dropDownStyle)
        {
            // Action button
            GUIContent buttonContent = new GUIContent(text, tooltip);

            Rect rt = GUILayoutUtility.GetRect(
                buttonContent,
                buttonStyle,
                GUILayout.MinWidth(width),
                GUILayout.MaxWidth(width));

            if (GUI.Button(rt, buttonContent, buttonStyle))
            {
                buttonAction();
            }

            // Menu dropdown
            GUIContent dropDownContent = new GUIContent(
                string.Empty, Images.GetDropDownIcon());

            Rect dropDownRect = GUILayoutUtility.GetRect(
                dropDownContent,
                dropDownStyle,
                GUILayout.MinWidth(DROPDOWN_BUTTON_WIDTH),
                GUILayout.MaxWidth(DROPDOWN_BUTTON_WIDTH));

            if (EditorGUI.DropdownButton(
                    dropDownRect, dropDownContent, FocusType.Passive, dropDownStyle))
            {
                actionMenu.DropDown(dropDownRect);
            }
        }

        const int DROPDOWN_BUTTON_WIDTH = 16;
    }
}
