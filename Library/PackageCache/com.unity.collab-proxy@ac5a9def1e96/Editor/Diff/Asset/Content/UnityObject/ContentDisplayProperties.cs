using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.UnityObject
{
    internal static class ContentDisplayProperties
    {
        internal static void Fill(ObjectContentPair pair)
        {
            UnityEngine.Object obj = pair.Object;
            ObjectContent content = pair.Content;

            content.ObjectId = obj != null ? obj.GetObjectId() : 0;
            content.DisplayName = ObjectDisplayResolver.ResolveDisplayName(obj);
            content.TypeName = obj == null ? null : obj.GetType().Name;
            content.ObjectName = obj == null ? null : obj.name;
            content.IsGameObjectRow = obj is GameObject;

            content.Icon = ObjectIconResolver.ResolveObjectIcon(obj);
            content.GameObjectIcon = ObjectIconResolver.ResolveGameObjectIcon(
                ObjectDisplayResolver.ResolveGameObject(obj));
        }
    }
}
