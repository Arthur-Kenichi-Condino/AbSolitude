using Unity.Multiplayer.Tools.Common;
using UnityEditor;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    static class SaveLoadEditorPrefs
    {
        public static void Save<T>(T t, string key = nameof(T))
        {
            var json = JsonSerialization.ToJson(t);
            EditorPrefs.SetString(key, json);
            DebugUtil.Trace($"NetVis settings saved:\n{json}");
        }

        public static T Load<T>(string key = nameof(T))
        {
            var json = EditorPrefs.GetString(key);
            DebugUtil.Trace($"NetVis settings loaded:\n{json}");
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            var deserialized = JsonSerialization.FromJson<T>(json);
            return deserialized;
        }
    }
}
