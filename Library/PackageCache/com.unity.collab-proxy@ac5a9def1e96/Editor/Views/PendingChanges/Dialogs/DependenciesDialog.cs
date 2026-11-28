using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.BaseCommands;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.UI.UIElements.Tree;

using PlasticDialog = Unity.PlasticSCM.Editor.UI.UIElements.PlasticDialog;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs
{
    internal class DependenciesDialog : PlasticDialog
    {
        internal PlasticTree DependenciesTree { get { return mDependenciesTree; } }
        internal Button OkButton { get { return mOkButton; } }
        internal Button CancelButton { get { return mCancelButton; } }

        internal static bool IncludeDependencies(
            WorkspaceInfo wkInfo,
            IList<ChangeDependencies> changesDependencies,
            string operation,
            EditorWindow parentWindow)
        {
            DependenciesDialog dialog = Create(wkInfo, changesDependencies, operation);
            return dialog.ShowModal(parentWindow) == ResponseType.Ok;
        }

        internal static DependenciesDialog Create(
            WorkspaceInfo wkInfo,
            IList<ChangeDependencies> changesDependencies,
            string operation)
        {
            DependenciesDialog dialog = CreateInstance<DependenciesDialog>();
            dialog.mWkInfo = wkInfo;
            dialog.mChangesDependencies = changesDependencies;
            dialog.mOperation = operation;
            return dialog;
        }

        protected override Vector2 DefaultSize
        {
            get { return new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT); }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.DependenciesDialogTitle.GetString();
        }

        protected override void BuildComponents()
        {
            SetExplanation(
                PlasticLocalization.Name.DependenciesExplanation.GetString(mOperation));

            AddComponents(CreateDependenciesTree());

            mOkButton = ControlBuilder.Button.CreateButton(mOperation, OkButtonAction);
            mCancelButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.CancelButton.GetString(), CancelButtonAction);

            AddButtons(mOkButton, mCancelButton);

            mDependenciesTree.schedule.Execute(mDependenciesTree.FocusTree);
        }

        VisualElement CreateDependenciesTree()
        {
            mDependenciesTree = new PlasticTree(
                DependenciesTreeDefinition.BuildColumns(),
                DependenciesTreeDefinition.BuildCellRenderFunction(mWkInfo.ClientPath),
                TreeOperations.BuildExpandAllFunction(),
                SelectionType.Single,
                UnityConstants.DEPENDENCIES_TREE_NAME,
                legacySettingsName: null);
            mDependenciesTree.style.flexGrow = 0;
            mDependenciesTree.style.height = TREE_AREA_HEIGHT;

            mDependenciesTree.FillItems(DependencyTree.Build(mChangesDependencies));

            return mDependenciesTree;
        }

        void OnDisable()
        {
            if (mDependenciesTree == null)
                return;

            mDependenciesTree.OnDisable();
        }

        PlasticTree mDependenciesTree;
        Button mOkButton;
        Button mCancelButton;

        IList<ChangeDependencies> mChangesDependencies;
        string mOperation;
        WorkspaceInfo mWkInfo;

        const float DIALOG_WIDTH = 650f;
        const float DIALOG_HEIGHT = 320f;
        const float TREE_AREA_HEIGHT = 200f;
    }
}
