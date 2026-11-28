using UnityEditor;

namespace Unity.PlasticSCM.Editor
{
    internal static class StringSetting
    {
        internal static string Load(string settingName, string defaultValue)
        {
            return EditorPrefs.GetString(GetSettingKey(settingName), defaultValue);
        }

        internal static void Save(string value, string settingName)
        {
            EditorPrefs.SetString(GetSettingKey(settingName), value);
        }

        internal static void Clear(string settingName)
        {
            EditorPrefs.DeleteKey(GetSettingKey(settingName));
        }

        static string GetSettingKey(string settingName)
        {
            return string.Format(settingName, PlayerSettings.productGUID);
        }
    }
}
