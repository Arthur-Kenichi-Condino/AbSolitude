using System.Collections.Generic;

using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.UnityInternals.UnityEditor;

using UnityGenericMenu = UnityEditor.GenericMenu;

namespace Unity.PlasticSCM.Editor.Views.BranchExplorer.Menu
{
    // Transitional adapter: renders an IMGUI GenericMenu (still used by the
    // Changesets/Branches/Labels menus shared with the IMGUI tabs) into a UI
    // Toolkit DropdownMenu, so the Branch Explorer can show every context menu
    // through the UITK contextual menu manager without calling ShowAsContext.
    // It is removed once the remaining menus migrate to DropdownMenu natively.
    internal static class GenericMenuToDropdownMenuBridge
    {
        internal static void Append(UnityGenericMenu source, DropdownMenu target)
        {
            List<GenericMenu.MenuItem> menuItems = source.menuItems();

            if (menuItems == null)
                return;

            foreach (GenericMenu.MenuItem menuItem in menuItems)
            {
                string text = menuItem.content == null ? string.Empty : menuItem.content.text;

                // A separator's text is its sub-menu path ("" at the root,
                // "name/" inside a sub-menu), so detect it by the flag, not by
                // an empty text — sub-menu separators have a non-empty path.
                if (menuItem.separator)
                {
                    target.AppendSeparator(text);
                    continue;
                }

                if (string.IsNullOrEmpty(text))
                    continue;

                UnityGenericMenu.MenuFunction menuFunction = menuItem.func;

                if (menuFunction == null)
                {
                    target.AppendAction(text, action => { }, DropdownMenuAction.AlwaysDisabled);
                    continue;
                }

                target.AppendAction(text, action => menuFunction(), DropdownMenuAction.AlwaysEnabled);
            }
        }
    }
}
