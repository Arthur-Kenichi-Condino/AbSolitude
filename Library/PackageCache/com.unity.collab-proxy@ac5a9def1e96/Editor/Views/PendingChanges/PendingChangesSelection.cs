using System.Collections.Generic;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
using Unity.PlasticSCM.Editor.UI.UIElements.Tree;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges
{
    internal static class PendingChangesSelection
    {
        internal static void SelectChanges(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree,
            List<ChangeInfo> changesToSelect)
        {
            if (changesToSelect == null || changesToSelect.Count == 0)
            {
                SelectFirstPendingChangeOnTree(treeControl, pendingChangesTree);
                return;
            }

            List<IPlasticTreeNode> nodesToSelect =
                FindNodesToSelect(pendingChangesTree, changesToSelect);

            if (nodesToSelect.Count == 0)
            {
                SelectFirstPendingChangeOnTree(treeControl, pendingChangesTree);
                return;
            }

            treeControl.SelectNodes(nodesToSelect);
        }

        internal static List<string> GetSelectedPathsWithoutMeta(
            PlasticTree treeControl)
        {
            List<string> result = new List<string>();

            foreach (ChangeInfo change in PendingChangesSelectionOperations.BuildChangeInfos(
                treeControl.GetSelectedNodes<PendingChangeInfo>(),
                bExcludePrivates: false))
            {
                result.Add(change.GetFullPath());
            }

            return result;
        }

        internal static List<string> GetSelectedPaths(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree)
        {
            List<string> result = new List<string>();

            foreach (ChangeInfo change in GetSelectedChanges(
                treeControl, pendingChangesTree))
            {
                result.Add(change.GetFullPath());
            }

            return result;
        }

        internal static List<string> GetSelectedMetaPaths(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree)
        {
            List<string> result = new List<string>();

            foreach (ChangeInfo change in GetSelectedChanges(
                treeControl, pendingChangesTree))
            {
                string path = change.GetFullPath();

                if (!MetaPath.IsMetaPath(path))
                    continue;

                result.Add(path);
            }

            return result;
        }

        internal static List<ChangeInfo> GetChangesToFocus(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree)
        {
            List<ChangeInfo> selectedChanges = GetSelectedChanges(
                treeControl, pendingChangesTree);

            if (selectedChanges.Count == 0)
                return selectedChanges;

            List<ChangeInfo> changesToFocus = new List<ChangeInfo>();

            foreach (ChangeInfo change in selectedChanges)
            {
                if (PendingChangesSelectionOperations.IsAddedFile(change))
                    continue;

                changesToFocus.Add(change);
            }

            if (changesToFocus.Count == 0)
            {
                ChangeInfo nearestAddedChange = GetNearestAddedChange(treeControl);

                if (nearestAddedChange != null)
                    changesToFocus.Add(nearestAddedChange);
            }

            return changesToFocus;
        }

        internal static SelectedChangesGroupInfo GetSelectedChangesGroupInfo(
            string wkPath, PlasticTree treeControl)
        {
            List<PendingChangeInfo> selectedPendingChanges =
                treeControl.GetSelectedNodes<PendingChangeInfo>();

            return SelectedChangesGroupInfo.BuildFromChangeInfos(
                wkPath,
                PendingChangesSelectionOperations.BuildChangeInfos(
                    selectedPendingChanges, bExcludePrivates: false),
                PendingChangesSelectionOperations.GetInvolvedChangelists(
                    selectedPendingChanges));
        }

        internal static List<ChangeInfo> GetSelectedChanges(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree)
        {
            return GetSelectedChanges(
                treeControl, pendingChangesTree, bExcludePrivates: false);
        }

        internal static List<ChangeInfo> GetSelectedChanges(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree,
            bool bExcludePrivates)
        {
            List<ChangeInfo> result = PendingChangesSelectionOperations.BuildChangeInfos(
                treeControl.GetSelectedNodes<PendingChangeInfo>(),
                bExcludePrivates);

            pendingChangesTree.FillWithMeta(result);

            return result;
        }

        internal static List<ChangeListInfo> GetSelectedChangeListInfos(
            PlasticTree treeControl)
        {
            List<ChangeListInfo> result = new List<ChangeListInfo>();

            foreach (ChangelistNode node in GetSelectedChangelistNodes(treeControl))
                result.Add(node.ChangelistInfo);

            return result;
        }

        internal static ChangeListInfo GetSelectedChangeListInfo(
            PlasticTree treeControl)
        {
            List<ChangeListInfo> changeListInfos = GetSelectedChangeListInfos(treeControl);

            if (changeListInfos.Count == 0)
                return null;

            return changeListInfos[0];
        }

        internal static List<ChangelistNode> GetSelectedChangelistNodes(
            PlasticTree treeControl)
        {
            return treeControl.GetSelectedNodes<ChangelistNode>();
        }

        internal static ChangeInfo GetSelectedChange(
            PlasticTree treeControl)
        {
            List<IPlasticTreeNode> selection = treeControl.GetAllSelection();

            if (selection.Count == 0)
                return null;

            PendingChangeInfo change = selection[0] as PendingChangeInfo;

            if (change == null || !change.IsChangedNode())
                return null;

            return change.ChangeInfo;
        }

        internal static bool GetSelectedPathsToDelete(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree,
            out List<string> privateDirectories,
            out List<string> privateFiles)
        {
            privateDirectories = new List<string>();
            privateFiles = new List<string>();

            if (!treeControl.HasSelectedRows())
                return false;

            List<PendingChangeInfo> selectedPendingChanges =
                treeControl.GetSelectedNodes<PendingChangeInfo>();

            foreach (PendingChangeInfo pendingChangeInfo in selectedPendingChanges)
            {
                if (!pendingChangeInfo.IsChangedNode())
                    continue;

                ChangeInfo changeInfo = pendingChangeInfo.ChangeInfo;

                if (ChangeInfoType.IsControlled(changeInfo))
                    continue;

                if (changeInfo.IsDirectory)
                    privateDirectories.Add(changeInfo.GetFullPath());
                else
                    privateFiles.Add(changeInfo.GetFullPath());

                ChangeInfo metaChangeInfo = pendingChangesTree.GetMetaChange(changeInfo);

                if (metaChangeInfo != null)
                    privateFiles.Add(metaChangeInfo.GetFullPath());
            }

            return true;
        }

        static void SelectFirstPendingChangeOnTree(
            PlasticTree treeControl,
            UnityPendingChangesTree pendingChangesTree)
        {
            PendingChangeInfo firstChange = pendingChangesTree.GetFirstPendingChange();

            if (firstChange == null)
                return;

            treeControl.SelectNodes(
                new List<IPlasticTreeNode>() { firstChange });
        }

        static List<IPlasticTreeNode> FindNodesToSelect(
            UnityPendingChangesTree pendingChangesTree,
            List<ChangeInfo> changesToSelect)
        {
            List<IPlasticTreeNode> result = new List<IPlasticTreeNode>();

            List<PendingChangeInfo> allPendingChanges =
                pendingChangesTree.GetAllPendingChangeInfos();

            foreach (ChangeInfo change in changesToSelect)
            {
                foreach (PendingChangeInfo pendingChange in allPendingChanges)
                {
                    ChangeInfo changeInfo = pendingChange.ChangeInfo;

                    if (changeInfo.ChangeTypes != change.ChangeTypes)
                        continue;

                    if (changeInfo.GetFullPath() != change.GetFullPath())
                        continue;

                    result.Add(pendingChange);
                    break;
                }
            }

            return result;
        }

        static ChangeInfo GetNearestAddedChange(PlasticTree treeControl)
        {
            PendingChangeInfo changeInfo =
                treeControl.GetSelectedNode<PendingChangeInfo>();

            if (changeInfo == null)
                return null;

            PendingChangeCategory category =
                ((IPlasticTreeNode)changeInfo).GetParent() as PendingChangeCategory;

            if (category == null)
                return null;

            int itemIndex = category.GetChildPosition(changeInfo);

            ChangeInfo result = PendingChangesSelectionOperations.GetNextExistingAddedItem(
                category, itemIndex);

            if (result != null)
                return result;

            return PendingChangesSelectionOperations.GetPreviousExistingAddedItem(
                category, itemIndex);
        }
    }
}
