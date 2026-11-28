using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal static class DiffDisplayProperties
    {
        internal static void Fill(ObjectDiffPair pair)
        {
            UnityEngine.Object src = pair.SrcObject;
            UnityEngine.Object dst = pair.DstObject;
            UnityEngine.Object any = src ?? dst;

            ObjectDiff diff = pair.Diff;

            // The dst object identifies the row so expansion state survives a
            // refresh; a removed object only exists on the src side.
            UnityEngine.Object idObject = dst ?? src;
            diff.ObjectId = idObject != null ? idObject.GetObjectId() : 0;

            diff.SrcDisplayName = ObjectDisplayResolver.ResolveDisplayName(src ?? dst);
            diff.DstDisplayName = ObjectDisplayResolver.ResolveDisplayName(dst ?? src);
            diff.TypeName = any == null ? null : any.GetType().Name;
            diff.ObjectName = any == null ? null : any.name;
            diff.IsGameObjectRow = any is GameObject;

            diff.Icon = ObjectIconResolver.ResolveObjectIcon(any);
            diff.GameObjectIcon = ObjectIconResolver.ResolveGameObjectIcon(
                ObjectDisplayResolver.ResolveGameObject(any));

            diff.ParentChanged = pair.SrcParent != null || pair.DstParent != null;
            diff.SrcParentName = pair.SrcParent == null ? null : pair.SrcParent.name;
            diff.DstParentName = pair.DstParent == null ? null : pair.DstParent.name;
        }
    }
}
