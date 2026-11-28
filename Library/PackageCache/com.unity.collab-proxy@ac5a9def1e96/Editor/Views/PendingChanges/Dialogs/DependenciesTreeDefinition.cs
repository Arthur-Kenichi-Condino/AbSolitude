using System;

using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.UI.UIElements.Tree;

using PlasticTableCell = Unity.PlasticSCM.Editor.UI.UIElements.Tree.PlasticTableCell;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs
{
    internal static class DependenciesTreeDefinition
    {
        internal static PlasticTableColumn[] BuildColumns()
        {
            return new PlasticTableColumn[]
            {
                new PlasticTableColumn(
                    PlasticLocalization.Name.ItemColumn.GetString(),
                    ITEM_COLUMN_NAME,
                    PlasticTableColumn.COLUMN_WIDTH_STAR,
                    PlasticTableColumn.Render.Text)
            };
        }

        internal static Func<string, IPlasticTreeNode, PlasticTableCell> BuildCellRenderFunction(
            string wkPath)
        {
            return (string columnName, IPlasticTreeNode node) =>
            {
                return new PlasticTableCell()
                {
                    Text = ChangeInfoView.GetPathDescription(
                        wkPath, ((DependencyNode)node).ChangeInfo),
                    Trimming = PlasticTableCell.TextTrimming.Path
                };
            };
        }

        internal const string ITEM_COLUMN_NAME = "item";
    }
}
