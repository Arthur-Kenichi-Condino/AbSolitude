using System.Collections.Generic;
using System.Diagnostics;

using UnityEditor;
using UnityEngine;

using Codice.LogWrapper;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.UnityObject
{
    internal static class UnityObjectContents
    {
        internal static List<ObjectContent> BuildContentData(string file)
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<ObjectContentPair> pairs = new List<ObjectContentPair>();
            List<ObjectContent> objectContents;

            using (SerializedFileObjects serializedFileObjects = SerializedFileObjects.Read(file))
            {
                UnityEngine.Object[] objects = serializedFileObjects.Objects;

                foreach (UnityEngine.Object obj in objects)
                {
                    if (obj == null)
                        continue;

                    pairs.Add(BuildObjectContentPair(obj));
                }

                GroupComponentsUnderGameObjects(pairs);

                RemoveEmptyNonGameObjects(pairs);

                HierarchyOrderSorting.SortByHierarchyOrder(
                    pairs, p => p.Object, objects);

                objectContents = BuildObjectContents(pairs);
            }

            mLog.DebugFormat(
                "{0} object contents calculated in {1} ms",
                objectContents.Count, sw.ElapsedMilliseconds);

            return objectContents;
        }

        static ObjectContentPair BuildObjectContentPair(UnityEngine.Object obj)
        {
            ObjectContentPair pair = new ObjectContentPair { Object = obj };

            using (SerializedObject so = new SerializedObject(obj))
                pair.Content.PropertyTree = PropertyTreeBuilder.Build(so);

            return pair;
        }

        static List<ObjectContent> BuildObjectContents(List<ObjectContentPair> pairs)
        {
            List<ObjectContent> result = new List<ObjectContent>(pairs.Count);

            foreach (ObjectContentPair pair in pairs)
            {
                ContentDisplayProperties.Fill(pair);
                result.Add(pair.Content);

                if (pair.ComponentPairs == null)
                    continue;

                pair.Content.ComponentContents = new List<ObjectContent>(
                    pair.ComponentPairs.Count);

                foreach (ObjectContentPair comp in pair.ComponentPairs)
                {
                    ContentDisplayProperties.Fill(comp);
                    pair.Content.ComponentContents.Add(comp.Content);
                }
            }

            return result;
        }

        static void GroupComponentsUnderGameObjects(List<ObjectContentPair> pairs)
        {
            Dictionary<GameObject, ObjectContentPair> gameObjectMap =
                new Dictionary<GameObject, ObjectContentPair>();

            foreach (ObjectContentPair pair in pairs)
            {
                if (pair.Object is GameObject go)
                    gameObjectMap[go] = pair;
            }

            if (gameObjectMap.Count == 0)
                return;

            List<ObjectContentPair> topLevel = new List<ObjectContentPair>(pairs.Count);

            foreach (ObjectContentPair pair in pairs)
            {
                if (pair.Object is Component c && c.gameObject != null
                    && gameObjectMap.TryGetValue(c.gameObject, out ObjectContentPair parent))
                {
                    if (parent.ComponentPairs == null)
                        parent.ComponentPairs = new List<ObjectContentPair>();

                    parent.ComponentPairs.Add(pair);
                    continue;
                }

                topLevel.Add(pair);
            }

            pairs.Clear();
            pairs.AddRange(topLevel);

            foreach (ObjectContentPair pair in pairs)
                SortComponentsByGameObjectOrder(pair);
        }

        static void SortComponentsByGameObjectOrder(ObjectContentPair gameObjectPair)
        {
            if (gameObjectPair.ComponentPairs == null)
                return;

            if (!(gameObjectPair.Object is GameObject go))
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
                int ia = a.Object is Component ca && orderIndex.TryGetValue(ca, out int va)
                    ? va : int.MaxValue;
                int ib = b.Object is Component cb && orderIndex.TryGetValue(cb, out int vb)
                    ? vb : int.MaxValue;
                return ia.CompareTo(ib);
            });
        }

        // Drops objects whose property tree is empty after IgnoredProperties
        // filtering — e.g. SceneRoots, whose only fields (m_ObjectHideFlags
        // and m_Roots) are both ignored. GameObjects are kept regardless
        // because they own their components.
        static void RemoveEmptyNonGameObjects(List<ObjectContentPair> pairs)
        {
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                ObjectContentPair pair = pairs[i];
                if (pair.Object is GameObject)
                    continue;

                PropertyTreeNode tree = pair.Content.PropertyTree;
                if (tree != null && tree.Children.Count > 0)
                    continue;

                pairs.RemoveAt(i);
            }
        }

        static readonly ILog mLog = PlasticApp.GetLogger("UnityObjectContents");
    }
}
