using System.IO;

using UnityEditor;

using PackageManager = UnityEditor.PackageManager;

namespace Unity.PlasticSCM.Editor
{
    internal static class AIAssistantSkillStatus
    {
        internal static bool IsAssistantInstalled()
        {
            foreach (PackageManager.PackageInfo package in
                PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (package.name == AI_ASSISTANT_PACKAGE_NAME)
                    return true;
            }

            return false;
        }

        internal static bool IsSkillAllowed()
        {
            string skillFilePath = GetSkillFilePath();

            if (string.IsNullOrEmpty(skillFilePath))
                return false;

            return EditorPrefs.GetBool(BuildSkillAllowedPrefsKey(skillFilePath), false);
        }

        internal static void ShowSkillsPreferences()
        {
            SettingsService.OpenUserPreferences(SKILLS_PREFERENCES_PATH);
        }

        internal static string GetSkillFilePath()
        {
            PackageManager.PackageInfo packageInfo = PackageManager.PackageInfo.FindForAssembly(
                typeof(AIAssistantSkillStatus).Assembly);

            if (packageInfo == null)
                return null;

            return Path.Combine(
                packageInfo.resolvedPath, SKILLS_FOLDER_NAME, SKILL_NAME, SKILL_FILE_NAME);
        }

        // The Allow/Deny value the user picks in Preferences, AI, Skills is stored by the AI
        // Assistant package in EditorPrefs, under a key it exposes through no public API. The
        // key format is replicated here. If it ever changes the read fails closed, so the
        // banner shows again and the user can dismiss it.
        static string BuildSkillAllowedPrefsKey(string skillFilePath)
        {
            return SKILL_ALLOWED_PREFS_PREFIX + SKILL_NAME + ":" +
                Path.GetFullPath(skillFilePath).ToLowerInvariant().Replace('\\', '/');
        }

        const string AI_ASSISTANT_PACKAGE_NAME = "com.unity.ai.assistant";
        const string SKILL_NAME = "unity-version-control";
        const string SKILLS_FOLDER_NAME = "AIAssistantSkills";
        const string SKILL_FILE_NAME = "SKILL.md";
        const string SKILL_ALLOWED_PREFS_PREFIX = "AIAssistant.SkillAllowed.";
        const string SKILLS_PREFERENCES_PATH = "Preferences/AI/Skills";
    }
}
