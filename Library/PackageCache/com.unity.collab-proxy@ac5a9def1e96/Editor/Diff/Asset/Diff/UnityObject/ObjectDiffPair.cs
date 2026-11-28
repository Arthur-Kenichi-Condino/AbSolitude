using System.Collections.Generic;

using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal class ObjectDiffPair
    {
        internal UnityEngine.Object SrcObject;
        internal UnityEngine.Object DstObject;
        internal GameObject SrcParent;
        internal GameObject DstParent;
        internal List<ObjectDiffPair> ComponentPairs;
        internal ObjectDiff Diff = new ObjectDiff();
    }
}
