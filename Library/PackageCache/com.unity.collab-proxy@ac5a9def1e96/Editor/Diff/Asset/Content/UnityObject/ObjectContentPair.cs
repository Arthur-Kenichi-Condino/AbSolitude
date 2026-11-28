using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.UnityObject
{
    internal class ObjectContentPair
    {
        internal UnityEngine.Object Object;
        internal List<ObjectContentPair> ComponentPairs;
        internal ObjectContent Content = new ObjectContent();
    }
}
