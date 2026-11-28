using System;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal static class TreeOperations
    {
        internal static Func<IPlasticTree, IPlasticTreeNode, bool> BuildExpandAllFunction()
        {
            return (IPlasticTree tree, IPlasticTreeNode node) =>
            {
                return true;
            };
        }
    }
}
