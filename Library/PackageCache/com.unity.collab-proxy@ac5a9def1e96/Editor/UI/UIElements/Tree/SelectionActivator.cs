using System;
using System.Collections.Generic;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal class SelectionActivator
    {
        internal SelectionActivator(
            Func<List<IPlasticTreeNode>> getSelectedNodesFunc,
            Func<List<IPlasticTreeNode>> getRootNodesFunc,
            Action<IPlasticTreeNode> selectOnlyNodeAction,
            Action refreshAction)
        {
            mGetSelectedNodesFunc = getSelectedNodesFunc;
            mGetRootNodesFunc = getRootNodesFunc;
            mSelectOnlyNodeAction = selectOnlyNodeAction;
            mRefreshAction = refreshAction;
        }

        internal void OnSelectionActivated(IPlasticTreeNode node)
        {
            SelectOnlyNodeIfOutsideSelection(node);

            SetCheckBoxStates(
                mGetSelectedNodesFunc(),
                CheckableItems.GetIsCheckedValue(node) != true);
        }

        internal void SetCheckBoxStates(
            IEnumerable<IPlasticTreeNode> nodes,
            bool isChecked)
        {
            foreach (IPlasticTreeNode node in nodes)
                CheckableItems.SetCheckedValue(node, isChecked);

            mRefreshAction();
        }

        internal void OnCheckUncheckAllStateChanged(bool isChecked)
        {
            SetCheckBoxStates(mGetRootNodesFunc(), isChecked);
        }

        internal bool? GetCheckUncheckAllState()
        {
            List<IPlasticTreeNode> rootNodes = mGetRootNodesFunc();

            if (rootNodes.Count == 0)
                return false;

            bool areAllChecked = true;
            bool isAnyChecked = false;

            foreach (IPlasticTreeNode node in rootNodes)
            {
                bool? checkedValue = CheckableItems.GetIsCheckedValue(node);

                if (checkedValue == null)
                    return null;

                areAllChecked &= checkedValue.Value;
                isAnyChecked |= checkedValue.Value;
            }

            if (areAllChecked)
                return true;

            return isAnyChecked ? (bool?)null : false;
        }

        void SelectOnlyNodeIfOutsideSelection(IPlasticTreeNode node)
        {
            if (mGetSelectedNodesFunc().Contains(node))
                return;

            mSelectOnlyNodeAction(node);
        }

        readonly Func<List<IPlasticTreeNode>> mGetSelectedNodesFunc;
        readonly Func<List<IPlasticTreeNode>> mGetRootNodesFunc;
        readonly Action<IPlasticTreeNode> mSelectOnlyNodeAction;
        readonly Action mRefreshAction;
    }
}
