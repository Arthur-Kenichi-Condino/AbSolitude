using UnityEngine.UIElements;

using PlasticGui;
using PlasticGui.WorkspaceWindow.BranchExplorer;

namespace Unity.PlasticSCM.Editor.Views.BranchExplorer.Menu
{
    internal class LinkMenu
    {
        internal LinkMenu(ILinkMenuOperations linkOperations)
        {
            mLinkMenuOperations = linkOperations;
        }

        internal void Populate(DropdownMenu menu)
        {
            if (mLinkMenuOperations.GetSelectedLinksCount() != 1)
                return;

            menu.AppendAction(
                PlasticLocalization.Name.GoToSourceChangesetMenuItem.GetString(),
                action => mLinkMenuOperations.GoToSourcechangeset(),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendAction(
                PlasticLocalization.Name.GoToDestinationChangesetMenuItem.GetString(),
                action => mLinkMenuOperations.GoToDestinationChangeset(),
                DropdownMenuAction.AlwaysEnabled);
        }

        readonly ILinkMenuOperations mLinkMenuOperations;
    }
}
