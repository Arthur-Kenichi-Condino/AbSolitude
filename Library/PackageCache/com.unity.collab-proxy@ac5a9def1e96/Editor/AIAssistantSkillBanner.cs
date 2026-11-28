using UnityEditor;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor
{
    internal class AIAssistantSkillBanner
    {
        internal bool IsVisible { get; private set; }

        internal void Refresh()
        {
            IsVisible = ShouldBeVisible();
        }

        internal void OnGUI()
        {
            if (!IsVisible)
                return;

            GUILayout.BeginHorizontal(UnityStyles.StatusBar.NotificationPanel);

            DrawLabel(PlasticLocalization.Name.AllowAIAssistantSkillExplanation.GetString());

            DrawOpenSkillsPreferencesButton();

            GUILayout.FlexibleSpace();

            DrawCloseButton(this);

            GUILayout.EndHorizontal();
        }

        void Dismiss()
        {
            StringSetting.Save(
                AIAssistantSkillStatus.GetSkillFilePath(),
                UnityConstants.AI_ASSISTANT_SKILL_BANNER_DISMISSED_KEY_NAME);

            IsVisible = false;
        }

        static void DrawLabel(string text)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Label(text, UnityStyles.StatusBar.NotificationLabel);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        static void DrawOpenSkillsPreferencesButton()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    PlasticLocalization.Name.OpenAIAssistantSkillsSettingsButton.GetString(),
                    UnityStyles.StatusBar.NotificationPanelLink))
            {
                AIAssistantSkillStatus.ShowSkillsPreferences();
            }

            EditorGUIUtility.AddCursorRect(
                GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        static void DrawCloseButton(AIAssistantSkillBanner banner)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    new GUIContent(Images.GetCloseIcon()),
                    UnityStyles.StatusBar.NotificationPanelCloseButton))
            {
                banner.Dismiss();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        // The dismissal stores the skill path instead of a flag. Assistant keys its allow state
        // on the path of SKILL.md, which changes when the package is updated, so it drops the
        // skill back to Deny. Comparing paths brings the banner back once after an update
        // instead of leaving the integration silently switched off.
        static bool ShouldBeVisible()
        {
            if (!AIAssistantSkillStatus.IsAssistantInstalled())
                return false;

            if (AIAssistantSkillStatus.IsSkillAllowed())
                return false;

            string skillFilePath = AIAssistantSkillStatus.GetSkillFilePath();

            if (string.IsNullOrEmpty(skillFilePath))
                return false;

            return StringSetting.Load(
                UnityConstants.AI_ASSISTANT_SKILL_BANNER_DISMISSED_KEY_NAME,
                string.Empty) != skillFilePath;
        }
    }
}
