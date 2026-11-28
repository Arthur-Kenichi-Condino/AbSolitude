using System.Collections.Generic;

using UnityEngine.UIElements;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal class PlasticTreeNodeIds
    {
        internal List<TreeViewItemData<IPlasticTreeNode>> BuildTreeViewItems(
            IPlasticTree treeModel)
        {
            mNodeById.Clear();
            mIdByKey.Clear();
            mKeyById.Clear();

            List<TreeViewItemData<IPlasticTreeNode>> result =
                new List<TreeViewItemData<IPlasticTreeNode>>();

            int nextId = 0;

            foreach (IPlasticTreeNode node in treeModel.GetNodes())
                result.Add(BuildItem(node, ref nextId));

            return result;
        }

        internal IEnumerable<int> GetAllIds()
        {
            return mNodeById.Keys;
        }

        internal IPlasticTreeNode GetNode(int id)
        {
            return mNodeById[id];
        }

        internal bool TryGetNode(int id, out IPlasticTreeNode node)
        {
            return mNodeById.TryGetValue(id, out node);
        }

        internal string GetKey(int id)
        {
            return mKeyById[id];
        }

        internal bool TryGetId(string nodeKey, out int id)
        {
            return mIdByKey.TryGetValue(nodeKey, out id);
        }

        internal bool TryGetId(IPlasticTreeNode node, out int id)
        {
            return mIdByKey.TryGetValue(((IPlasticTreeNodeKey)node).GetKey(), out id);
        }

        TreeViewItemData<IPlasticTreeNode> BuildItem(
            IPlasticTreeNode node,
            ref int nextId)
        {
            int id = nextId++;

            mNodeById[id] = node;
            string nodeKey = ((IPlasticTreeNodeKey)node).GetKey();
            mIdByKey[nodeKey] = id;
            mKeyById[id] = nodeKey;

            List<TreeViewItemData<IPlasticTreeNode>> children =
                new List<TreeViewItemData<IPlasticTreeNode>>();

            IEnumerable<IPlasticTreeNode> nodeChildren = node.GetChildren();

            if (nodeChildren != null)
            {
                foreach (IPlasticTreeNode child in nodeChildren)
                    children.Add(BuildItem(child, ref nextId));
            }

            return new TreeViewItemData<IPlasticTreeNode>(id, node, children);
        }

        readonly Dictionary<int, IPlasticTreeNode> mNodeById =
            new Dictionary<int, IPlasticTreeNode>();
        readonly Dictionary<string, int> mIdByKey =
            new Dictionary<string, int>();
        readonly Dictionary<int, string> mKeyById =
            new Dictionary<int, string>();
    }
}
