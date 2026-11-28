using PlasticGui;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class ObjectDisplayResolver
    {
        internal static string ResolveDisplayName(UnityEngine.Object obj)
        {
            if (obj == null)
                return PlasticLocalization.Name.DiffUnknownObject.GetString();
            if (obj is Component)
                return obj.GetType().Name;

            if (!string.IsNullOrEmpty(obj.name))
                return obj.name;

            // Newer Editor versions wrap native-only types (e.g. PrefabInstance) as FailedToLoadScriptObject instead of UnityEngine.Object.
            // In those cases, we continue resolving the type as Object
            if (obj.GetType().FullName == FAILED_TO_LOAD_SCRIPT_OBJECT_TYPE_NAME)
                return nameof(UnityEngine.Object);

            return obj.GetType().Name;
        }

        internal static GameObject ResolveGameObject(UnityEngine.Object obj)
        {
            if (obj is GameObject go)
                return go;
            if (obj is Component c)
                return c.gameObject;

            return null;
        }

        const string FAILED_TO_LOAD_SCRIPT_OBJECT_TYPE_NAME = "UnityEngine.FailedToLoadScriptObject";
    }
}
