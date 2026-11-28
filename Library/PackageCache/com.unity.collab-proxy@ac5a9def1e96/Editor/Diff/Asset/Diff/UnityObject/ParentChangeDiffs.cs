using System.Collections.Generic;
using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Detects re-parenting (a GameObject's transform.parent fileId differs
    // between sides). Sibling reorder and parent change are mutually
    // exclusive: a reparented GO is never in the common-children set of
    // either old or new parent, so SiblingReorderDiffs won't fire on it.
    internal static class ParentChangeDiffs
    {
        internal static void Append(
            Dictionary<long, UnityEngine.Object> srcById,
            Dictionary<long, UnityEngine.Object> dstById,
            List<ObjectDiffPair> pairs)
        {
            Dictionary<GameObject, ObjectDiffPair> bySrcGameObject =
                new Dictionary<GameObject, ObjectDiffPair>();
            Dictionary<GameObject, ObjectDiffPair> byDstGameObject =
                new Dictionary<GameObject, ObjectDiffPair>();

            foreach (ObjectDiffPair pair in pairs)
            {
                if (pair.SrcObject is GameObject sg) bySrcGameObject[sg] = pair;
                if (pair.DstObject is GameObject dg) byDstGameObject[dg] = pair;
            }

            foreach (KeyValuePair<long, UnityEngine.Object> kvp in srcById)
            {
                if (!(kvp.Value is GameObject srcGo))
                    continue;
                if (!dstById.TryGetValue(kvp.Key, out UnityEngine.Object dstObj))
                    continue;
                if (!(dstObj is GameObject dstGo))
                    continue;

                Transform srcParentT = srcGo.transform.parent;
                Transform dstParentT = dstGo.transform.parent;

                long srcParentId = srcParentT == null ? 0 : srcParentT.GetLocalFileId();
                long dstParentId = dstParentT == null ? 0 : dstParentT.GetLocalFileId();

                if (srcParentId == dstParentId)
                    continue;

                ObjectDiffPair existing = null;
                if (!bySrcGameObject.TryGetValue(srcGo, out existing))
                    byDstGameObject.TryGetValue(dstGo, out existing);

                if (existing == null)
                {
                    // GameObject reparented but otherwise unchanged — emit a
                    // Modified diff with no PropertyDiffs so the row still appears.
                    existing = new ObjectDiffPair
                    {
                        SrcObject = srcGo,
                        DstObject = dstGo,
                        Diff = { DiffType = DiffType.Modified }
                    };
                    pairs.Add(existing);
                    bySrcGameObject[srcGo] = existing;
                    byDstGameObject[dstGo] = existing;
                }

                existing.SrcParent = srcParentT == null ? null : srcParentT.gameObject;
                existing.DstParent = dstParentT == null ? null : dstParentT.gameObject;
            }
        }
    }
}
