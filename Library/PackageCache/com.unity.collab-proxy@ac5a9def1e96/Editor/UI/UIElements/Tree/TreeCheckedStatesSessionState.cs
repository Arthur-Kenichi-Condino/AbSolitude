using System.Collections.Generic;

using UnityEditor;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal static class TreeCheckedStatesSessionState
    {
        internal static void Save(
            List<IPlasticTreeNode> allNodes,
            string checkedKey,
            string uncheckedKey)
        {
            if (allNodes.Count == 0)
                return;

            List<string> checkedNodeKeys = new List<string>();
            List<string> uncheckedNodeKeys = new List<string>();

            foreach (IPlasticTreeNode node in allNodes)
            {
                if (!IsCheckableChange(node))
                    continue;

                bool? isChecked = CheckableItems.GetIsCheckedValue(node);

                if (!isChecked.HasValue)
                    continue;

                string nodeKey = ((IPlasticTreeNodeKey)node).GetKey();

                if (isChecked.Value)
                {
                    checkedNodeKeys.Add(nodeKey);
                    continue;
                }

                uncheckedNodeKeys.Add(nodeKey);
            }

            SessionState.SetString(
                checkedKey, ConvertNodeKeysSetting.ToString(checkedNodeKeys));
            SessionState.SetString(
                uncheckedKey, ConvertNodeKeysSetting.ToString(uncheckedNodeKeys));
        }

        internal static void Restore(
            List<IPlasticTreeNode> allNodes,
            string checkedKey,
            string uncheckedKey)
        {
            if (allNodes.Count == 0)
                return;

            List<KeyValuePair<IPlasticTreeNode, bool>> checkedValues = CaptureTargetCheckedValues(
                allNodes, LoadNodeKeys(checkedKey), LoadNodeKeys(uncheckedKey));

            foreach (KeyValuePair<IPlasticTreeNode, bool> checkedValue in checkedValues)
            {
                if (CheckableItems.GetIsCheckedValue(checkedValue.Key) == checkedValue.Value)
                    continue;

                CheckableItems.SetCheckedValue(checkedValue.Key, checkedValue.Value);
            }

            SessionState.EraseString(checkedKey);
            SessionState.EraseString(uncheckedKey);
        }

        static List<KeyValuePair<IPlasticTreeNode, bool>> CaptureTargetCheckedValues(
            List<IPlasticTreeNode> allNodes,
            HashSet<string> checkedNodeKeys,
            HashSet<string> uncheckedNodeKeys)
        {
            List<KeyValuePair<IPlasticTreeNode, bool>> result =
                new List<KeyValuePair<IPlasticTreeNode, bool>>();

            foreach (IPlasticTreeNode node in allNodes)
            {
                if (!IsCheckableChange(node))
                    continue;

                string nodeKey = ((IPlasticTreeNodeKey)node).GetKey();

                if (checkedNodeKeys.Contains(nodeKey))
                {
                    result.Add(new KeyValuePair<IPlasticTreeNode, bool>(node, true));
                    continue;
                }

                if (uncheckedNodeKeys.Contains(nodeKey))
                {
                    result.Add(new KeyValuePair<IPlasticTreeNode, bool>(node, false));
                    continue;
                }

                bool? currentValue = CheckableItems.GetIsCheckedValue(node);

                if (currentValue.HasValue)
                    result.Add(new KeyValuePair<IPlasticTreeNode, bool>(node, currentValue.Value));
            }

            return result;
        }

        static HashSet<string> LoadNodeKeys(string sessionStateKey)
        {
            return new HashSet<string>(ConvertNodeKeysSetting.ToList(
                SessionState.GetString(sessionStateKey, string.Empty)));
        }

        static bool IsCheckableChange(IPlasticTreeNode node)
        {
            ICheckablePlasticTreeNode checkableNode = node as ICheckablePlasticTreeNode;

            return checkableNode != null && checkableNode.IsChangedNode();
        }
    }
}
