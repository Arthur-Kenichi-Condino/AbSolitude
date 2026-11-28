using System.Collections.Generic;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Restructures a flat list of pairs into a tree where each Component pair is
    // nested under its owning GameObject pair (creating a synthetic Unchanged
    // container when the GameObject itself has no own changes). Sorts each
    // GameObject's components to match the inspector order.
    internal static class ObjectDiffGrouping
    {
        internal static void GroupComponentsUnderGameObjects(List<ObjectDiffPair> pairs)
        {
            Dictionary<GameObject, ObjectDiffPair> goPairs =
                new Dictionary<GameObject, ObjectDiffPair>();

            foreach (ObjectDiffPair pair in pairs)
            {
                if (pair.SrcObject is GameObject srcGo) goPairs[srcGo] = pair;
                if (pair.DstObject is GameObject dstGo) goPairs[dstGo] = pair;
            }

            List<ObjectDiffPair> topLevel = new List<ObjectDiffPair>(pairs.Count);

            foreach (ObjectDiffPair pair in pairs)
            {
                Component c = (pair.DstObject ?? pair.SrcObject) as Component;
                if (c == null)
                {
                    topLevel.Add(pair);
                    continue;
                }

                ObjectDiffPair parent = GetOrCreateGameObjectPair(pair, goPairs, topLevel);
                if (parent.ComponentPairs == null)
                    parent.ComponentPairs = new List<ObjectDiffPair>();
                parent.ComponentPairs.Add(pair);
            }

            pairs.Clear();
            pairs.AddRange(topLevel);

            foreach (ObjectDiffPair pair in pairs)
                SortComponentsByGameObjectOrder(pair);
        }

        static ObjectDiffPair GetOrCreateGameObjectPair(
            ObjectDiffPair componentPair,
            Dictionary<GameObject, ObjectDiffPair> goPairs,
            List<ObjectDiffPair> topLevel)
        {
            GameObject srcParent = (componentPair.SrcObject as Component)?.gameObject;
            GameObject dstParent = (componentPair.DstObject as Component)?.gameObject;

            if (srcParent != null && goPairs.TryGetValue(srcParent, out ObjectDiffPair existing))
                return existing;
            if (dstParent != null && goPairs.TryGetValue(dstParent, out existing))
                return existing;

            ObjectDiffPair container = new ObjectDiffPair
            {
                SrcObject = srcParent,
                DstObject = dstParent,
                Diff = { DiffType = DiffType.Unchanged }
            };

            if (srcParent != null) goPairs[srcParent] = container;
            if (dstParent != null) goPairs[dstParent] = container;
            topLevel.Add(container);

            return container;
        }

        static void SortComponentsByGameObjectOrder(ObjectDiffPair gameObjectPair)
        {
            if (gameObjectPair.ComponentPairs == null)
                return;

            GameObject go =
                (gameObjectPair.DstObject ?? gameObjectPair.SrcObject) as GameObject;
            if (go == null)
                return;

            Component[] orderedComponents = go.GetComponents<Component>();
            Dictionary<Component, int> orderIndex = new Dictionary<Component, int>(
                orderedComponents.Length);

            for (int i = 0; i < orderedComponents.Length; i++)
            {
                if (orderedComponents[i] != null)
                    orderIndex[orderedComponents[i]] = i;
            }

            gameObjectPair.ComponentPairs.Sort((a, b) =>
            {
                int ia = GetComponentOrder(a, orderIndex);
                int ib = GetComponentOrder(b, orderIndex);
                return ia.CompareTo(ib);
            });
        }

        static int GetComponentOrder(
            ObjectDiffPair pair,
            Dictionary<Component, int> orderIndex)
        {
            if (pair.DstObject is Component dstComp
                && orderIndex.TryGetValue(dstComp, out int dstIdx))
                return dstIdx;

            if (pair.SrcObject is Component srcComp
                && orderIndex.TryGetValue(srcComp, out int srcIdx))
                return srcIdx;

            return int.MaxValue;
        }
    }
}
