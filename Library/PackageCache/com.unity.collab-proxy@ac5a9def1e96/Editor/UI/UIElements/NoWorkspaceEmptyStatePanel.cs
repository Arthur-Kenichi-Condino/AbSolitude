using System;

using UnityEngine.UIElements;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal class NoWorkspaceEmptyStatePanel : VisualElement
    {
        internal Button CreateWorkspaceButton => mCreateWorkspaceButton;

        internal NoWorkspaceEmptyStatePanel(Action onCreateWorkspaceClicked)
        {
            CreateGUI(onCreateWorkspaceClicked);
        }

        void CreateGUI(Action onCreateWorkspaceClicked)
        {
            style.flexGrow = 1;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            Label emptyStateLabel = new Label(
                PlasticLocalization.Name.BranchExplorerNoWorkspaceMessage.GetString());
            emptyStateLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            emptyStateLabel.style.whiteSpace = WhiteSpace.Normal;

            mCreateWorkspaceButton = new Button(onCreateWorkspaceClicked)
            {
                text = PlasticLocalization.Name.CreateWorkspace.GetString()
            };
            mCreateWorkspaceButton.style.marginTop = 10;

            Add(emptyStateLabel);
            Add(mCreateWorkspaceButton);
        }

        Button mCreateWorkspaceButton;
    }
}
