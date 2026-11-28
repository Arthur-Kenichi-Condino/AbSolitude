using UnityEngine.UIElements;

using PlasticGui;
using PlasticGui.WorkspaceWindow.Configuration;

namespace Unity.PlasticSCM.Editor.Views.BranchExplorer.Options.ConditionalFormat
{
    internal class FormatRuleMenu
    {
        internal interface IFormatRuleMenuOperations
        {
            void CreateNewRule(Rule.Type type, Rule.FormattedObject formatTarget);
        }

        internal FormatRuleMenu(IFormatRuleMenuOperations operations)
        {
            mOperations = operations;
        }

        internal void Populate(DropdownMenu menu)
        {
            menu.AppendAction(
                PlasticLocalization.Name.BranchesWithPendingIntegrations.GetString(),
                action => mOperations.CreateNewRule(
                    Rule.Type.NonIntegrated, Rule.FormattedObject.Branch),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendAction(
                PlasticLocalization.Name.ActiveBranch.GetString(),
                action => mOperations.CreateNewRule(
                    Rule.Type.CurrentBranchFormat, Rule.FormattedObject.Branch),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendSeparator();

            menu.AppendAction(
                PlasticLocalization.Name.CustomBranchQuery.GetString(),
                action => mOperations.CreateNewRule(
                    Rule.Type.ConditionalFormat, Rule.FormattedObject.Branch),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendAction(
                PlasticLocalization.Name.CustomChangesetQuery.GetString(),
                action => mOperations.CreateNewRule(
                    Rule.Type.ConditionalFormat, Rule.FormattedObject.Changeset),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendAction(
                PlasticLocalization.Name.CustomLabelQuery.GetString(),
                action => mOperations.CreateNewRule(
                    Rule.Type.ConditionalFormat, Rule.FormattedObject.Label),
                DropdownMenuAction.AlwaysEnabled);
        }

        readonly IFormatRuleMenuOperations mOperations;
    }
}
