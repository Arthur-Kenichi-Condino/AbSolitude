using System;
using System.Collections.Generic;

using UnityEngine;

using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.AssetsOverlays;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.UI.UIElements.Tree;

using PlasticTableCell = Unity.PlasticSCM.Editor.UI.UIElements.Tree.PlasticTableCell;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges
{
    internal static class PendingChangesTreeDefinition
    {
        internal static PlasticTableColumn[] BuildColumns(bool isGluonMode)
        {
            List<PlasticTableColumn> result = new List<PlasticTableColumn>();

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(ITEM_COLUMN_NAME),
                ITEM_COLUMN_NAME, 800,
                PlasticTableColumn.Render.CheckBoxIconAndText)
            {
                MinWidth = 200
            });

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(STATUS_COLUMN_NAME),
                STATUS_COLUMN_NAME, 200,
                PlasticTableColumn.Render.Text)
            {
                MinWidth = 80
            });

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(SIZE_COLUMN_NAME),
                SIZE_COLUMN_NAME, 80,
                PlasticTableColumn.Render.Text)
            {
                MinWidth = 45,
                IsVisible = false
            });

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(EXTENSION_COLUMN_NAME),
                EXTENSION_COLUMN_NAME, 70,
                PlasticTableColumn.Render.Text)
            {
                MinWidth = 50,
                IsVisible = false
            });

            if (!isGluonMode)
            {
                result.Add(new PlasticTableColumn(
                    GetLocalizedColumnName(TYPE_COLUMN_NAME),
                    TYPE_COLUMN_NAME, 60,
                    PlasticTableColumn.Render.Text)
                {
                    MinWidth = 45,
                    IsVisible = false
                });
            }

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(DATE_MODIFIED_COLUMN_NAME),
                DATE_MODIFIED_COLUMN_NAME, 330,
                PlasticTableColumn.Render.Text)
            {
                MinWidth = 100
            });

            result.Add(new PlasticTableColumn(
                GetLocalizedColumnName(REPOSITORY_COLUMN_NAME),
                REPOSITORY_COLUMN_NAME, 210,
                PlasticTableColumn.Render.Text)
            {
                MinWidth = 90,
                IsVisible = false
            });

            return result.ToArray();
        }

        internal static string GetLocalizedColumnName(string columnName)
        {
            switch (columnName)
            {
                case ITEM_COLUMN_NAME:
                    return PlasticLocalization.Name.ItemColumn.GetString();
                case STATUS_COLUMN_NAME:
                    return PlasticLocalization.Name.StatusColumn.GetString();
                case SIZE_COLUMN_NAME:
                    return PlasticLocalization.Name.SizeColumn.GetString();
                case EXTENSION_COLUMN_NAME:
                    return PlasticLocalization.Name.ExtensionColumn.GetString();
                case TYPE_COLUMN_NAME:
                    return PlasticLocalization.Name.TypeColumn.GetString();
                case DATE_MODIFIED_COLUMN_NAME:
                    return PlasticLocalization.Name.DateModifiedColumn.GetString();
                case REPOSITORY_COLUMN_NAME:
                    return PlasticLocalization.Name.RepositoryColumn.GetString();
                default:
                    return null;
            }
        }

        internal static string BuildItemColumnTitle(int checkedItemCount, int totalItemCount)
        {
            string columnName = PlasticLocalization.Name.ItemColumn.GetString();

            if (totalItemCount == 0)
                return columnName;

            string columnStatus = string.Format(
                PlasticLocalization.Name.ItemsSelected.GetString(),
                checkedItemCount,
                totalItemCount);

            return string.Format("{0}  {1}", columnName, columnStatus);
        }

        internal static Func<string, IPlasticTreeNode, PlasticTableCell> BuildCellRenderFunction(
            UnityPendingChangesTree pendingChangesTree,
            bool isGluonMode,
            IAssetStatusCache assetStatusCache)
        {
            return (string columnName, IPlasticTreeNode node) =>
            {
                return new PlasticTableCell()
                {
                    Text = GetCellText(pendingChangesTree, node, columnName),
                    Icon = GetCellIcon(node, columnName),
                    OverlayIcon = GetCellOverlayIcon(
                        isGluonMode, assetStatusCache, node, columnName),
                    TextAlignment = columnName == SIZE_COLUMN_NAME ?
                        PlasticTableCell.Alignment.Right :
                        PlasticTableCell.Alignment.Left,
                    Style = columnName == ITEM_COLUMN_NAME ?
                        PlasticTableCell.StyleType.Normal :
                        PlasticTableCell.StyleType.Lighter,
                    Trimming = columnName == ITEM_COLUMN_NAME && node is PendingChangeInfo ?
                        PlasticTableCell.TextTrimming.Path :
                        PlasticTableCell.TextTrimming.None
                };
            };
        }

        internal static Func<IPlasticTreeNode, bool> BuildDefaultSelectionFunction()
        {
            return (IPlasticTreeNode node) =>
            {
                return node is PendingChangeInfo;
            };
        }

        internal static Func<IPlasticTree, IPlasticTreeNode, bool> BuildExpandFunction(
            ExpandedNodesCache expandedNodesCache)
        {
            return (IPlasticTree tree, IPlasticTreeNode node) =>
            {
                return expandedNodesCache.IsNodeExpanded(node);
            };
        }

        static string GetCellText(
            UnityPendingChangesTree pendingChangesTree,
            IPlasticTreeNode node,
            string columnName)
        {
            bool bIsItemColumn = columnName == ITEM_COLUMN_NAME;

            if (node is PendingChangeInfo &&
                (bIsItemColumn || ((PendingChangeInfo)node).IsChangedNode()))
            {
                PendingChangeInfo changeInfo = (PendingChangeInfo)node;

                string result = changeInfo.GetColumnText(
                    GetLocalizedColumnName(columnName));

                if (bIsItemColumn && pendingChangesTree.HasMeta(changeInfo.ChangeInfo))
                    return string.Concat(result, UnityConstants.TREEVIEW_META_LABEL);

                return result;
            }

            if (!bIsItemColumn)
                return string.Empty;

            if (node is ICheckablePlasticTreeCategory)
                return ((ICheckablePlasticTreeCategory)node).GetHeaderText();

            return ((ICheckablePlasticTreeCategoryGroup)node).GetHeaderText();
        }

        static Texture GetCellIcon(IPlasticTreeNode node, string columnName)
        {
            if (columnName != ITEM_COLUMN_NAME)
                return null;

            PendingChangeInfo changeInfo = node as PendingChangeInfo;

            if (changeInfo == null || !changeInfo.IsChangedNode())
                return null;

            if (changeInfo.ChangeInfo.IsDirectory)
                return Images.GetFolderIcon();

            return Images.GetFileIcon(changeInfo.ChangeInfo.GetFullPath());
        }

        static Texture GetCellOverlayIcon(
            bool isGluonMode,
            IAssetStatusCache assetStatusCache,
            IPlasticTreeNode node,
            string columnName)
        {
            if (columnName != ITEM_COLUMN_NAME)
                return null;

            PendingChangeInfo changeInfo = node as PendingChangeInfo;

            if (changeInfo == null || !changeInfo.IsChangedNode())
                return null;

            bool bIsConflicted = isGluonMode &&
                ClassifyAssetStatus.IsConflicted(assetStatusCache.GetStatus(
                    changeInfo.ChangeInfo.GetFullPath()));

            return GetChangesOverlayIcon.ForPendingChange(
                changeInfo.ChangeInfo, bIsConflicted);
        }

        internal const string ITEM_COLUMN_NAME = "item";
        internal const string STATUS_COLUMN_NAME = "status";
        internal const string SIZE_COLUMN_NAME = "size";
        internal const string EXTENSION_COLUMN_NAME = "extension";
        internal const string TYPE_COLUMN_NAME = "type";
        internal const string DATE_MODIFIED_COLUMN_NAME = "dateModified";
        internal const string REPOSITORY_COLUMN_NAME = "repository";

        // column order of the removed IMGUI header, used to translate the
        // legacy index-based visibility, width and sort settings into column names
        internal static readonly string[] LEGACY_COLUMN_NAMES_BY_INDEX = new string[]
        {
            ITEM_COLUMN_NAME,
            STATUS_COLUMN_NAME,
            SIZE_COLUMN_NAME,
            EXTENSION_COLUMN_NAME,
            TYPE_COLUMN_NAME,
            DATE_MODIFIED_COLUMN_NAME,
            REPOSITORY_COLUMN_NAME
        };
    }
}
