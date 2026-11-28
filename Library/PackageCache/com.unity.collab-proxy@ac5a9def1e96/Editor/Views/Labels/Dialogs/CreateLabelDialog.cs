using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.QueryViews.Labels;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.Views.Changesets;

using ImguiProgressControls = Unity.PlasticSCM.Editor.UI.Progress.ProgressControlsForDialogs;
using PlasticDialog = Unity.PlasticSCM.Editor.UI.UIElements.PlasticDialog;

namespace Unity.PlasticSCM.Editor.Views.Labels.Dialogs
{
    internal class CreateLabelDialog : PlasticDialog
    {
        internal const string CHANGESET_SELECT_BUTTON = "create-label-changeset-select-button";
        internal const string CHANGESET_BACK_BUTTON = "create-label-changeset-back-button";

        internal TextField NameField { get { return mNameField; } }
        internal TextField CommentField { get { return mCommentField; } }
        internal Label ChangesetIdLabel { get { return mChangesetIdLabel; } }
        internal Button ChooseButton { get { return mChooseButton; } }
        internal Toggle LabelAllXlinksToggle { get { return mLabelAllXlinksToggle; } }
        internal Toggle SwitchToLabelToggle { get { return mSwitchToLabelToggle; } }
        internal Button CreateButton { get { return mCreateButton; } }
        internal Button CancelButton { get { return mCancelButton; } }

        internal long ChangesetId { get { return mChangesetId; } }

        internal static LabelCreationData CreateLabel(
            EditorWindow parentWindow,
            WorkspaceInfo wkInfo)
        {
            RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
            long currentChangesetId =
                PlasticGui.Plastic.API.GetCurrentChangesetOnWorkspace(wkInfo);

            CreateLabelDialog dialog = Create(wkInfo, repSpec, currentChangesetId);

            ResponseType response = dialog.ShowModal(parentWindow);

            LabelCreationData result = dialog.GetResultData();
            result.Result = response == ResponseType.Ok;
            return result;
        }

        internal static CreateLabelDialog Create(
            WorkspaceInfo wkInfo,
            RepositorySpec repSpec,
            long changesetId)
        {
            CreateLabelDialog dialog = CreateInstance<CreateLabelDialog>();
            dialog.mWkInfo = wkInfo;
            dialog.mRepSpec = repSpec;
            dialog.mChangesetId = changesetId;
            return dialog;
        }

        internal void SetChangesetId(long changesetId)
        {
            mChangesetId = changesetId;
            mChangesetIdLabel.text = changesetId.ToString();
        }

        internal void SelectChangesetAction()
        {
            ChangesetInfo changesetInfo =
                ChangesetsSelection.GetSelectedChangeset(mChangesetExplorerView.Table);

            if (changesetInfo == null)
                return;

            SetChangesetId(changesetInfo.ChangesetId);

            ShowForm();
        }

        protected override Vector2 DefaultSize
        {
            get { return new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT); }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.CreateLabelTitle.GetString();
        }

        protected override void BuildComponents()
        {
            SetExplanation(PlasticLocalization.Name.CreateLabelExplanation.GetString());

            AddComponents(
                CreateLabelNameArea(),
                CreateCommentsArea(),
                CreateChangesetArea(),
                CreateCheckButtonsArea());

            mCreateButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.CreateButton.GetString(), CreateLabelButtonAction);
            mCancelButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.CancelButton.GetString(), CancelButtonAction);

            AddButtons(mCreateButton, mCancelButton);

            mProgressControls = new ProgressControlsForDialogs(
                new VisualElement[] { mCreateButton, mCancelButton, mChooseButton });
            DialogPanel.Add(mProgressControls);

            AddControlConsumingEnterKey(mCommentField);

            mNameField.schedule.Execute(() =>
            {
                mNameField.Focus();
                mNameField.SelectAll();
            });
        }

        VisualElement CreateLabelNameArea()
        {
            mNameField = new TextField();

            return EntriesPacker.CreateEntriesHPanel(
                new Label(PlasticLocalization.Name.LabelNameEntry.GetString()),
                LABEL_WIDTH,
                mNameField);
        }

        VisualElement CreateCommentsArea()
        {
            mCommentField = new TextField();
            mCommentField.multiline = true;
            mCommentField.style.height = COMMENT_HEIGHT;

            return EntriesPacker.CreateEntriesHPanel(
                new Label(PlasticLocalization.Name.CommentsEntry.GetString()),
                LABEL_WIDTH,
                mCommentField);
        }

        VisualElement CreateChangesetArea()
        {
            mChangesetIdLabel = new Label(mChangesetId.ToString());

            mChooseButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.ChooseMessage.GetString(), ShowChangesetExplorer);

            return EntriesPacker.CreateEntriesHPanel(
                new Label(PlasticLocalization.Name.ChangesetToLabelEntry.GetString()),
                LABEL_WIDTH,
                mChangesetIdLabel,
                mChooseButton);
        }

        VisualElement CreateCheckButtonsArea()
        {
            mLabelAllXlinksToggle = new Toggle(
                PlasticLocalization.Name.LabelAllXlinksCheckButton.GetString());
            mLabelAllXlinksToggle.style.marginLeft = 0;
            mLabelAllXlinksToggle.focusable = false;

            mSwitchToLabelToggle = new Toggle(
                PlasticLocalization.Name.SwitchToLabelCheckButton.GetString());
            mSwitchToLabelToggle.style.marginLeft = 0;
            mSwitchToLabelToggle.focusable = false;

            VisualElement panel = new VisualElement();
            panel.Add(mLabelAllXlinksToggle);
            panel.Add(mSwitchToLabelToggle);

            return panel;
        }

        LabelCreationData GetResultData()
        {
            mResultData = new LabelCreationData(
                mRepSpec,
                mChangesetId,
                mNameField.value,
                mCommentField.value,
                mSwitchToLabelToggle.value,
                mLabelAllXlinksToggle.value,
                (mResultData != null) ? mResultData.XlinksToLabel : null);

            return mResultData;
        }

        void CreateLabelButtonAction()
        {
            LabelCreationValidation.AsyncValidation(
                GetResultData(), this, mProgressControls);
        }

        void ShowChangesetExplorer()
        {
            if (mExplorerContainer == null)
                BuildExplorer();

            DialogPanel.style.display = DisplayStyle.None;
            mExplorerContainer.style.display = DisplayStyle.Flex;

            SetWindowFixedSize(new Vector2(DIALOG_WIDTH, EXPLORER_HEIGHT));

            SetTitle(PlasticLocalization.Name.AvailableChangesets.GetString());
        }

        void ShowForm()
        {
            mExplorerContainer.style.display = DisplayStyle.None;
            DialogPanel.style.display = DisplayStyle.Flex;

            SetWindowFixedSize(DefaultSize);

            SetTitle(PlasticLocalization.Name.CreateLabelTitle.GetString());
        }

        void BuildExplorer()
        {
            mExplorerProgressControls = new ImguiProgressControls();
            mChangesetExplorerView = new ChangesetExplorerView(
                this, mWkInfo, mExplorerProgressControls);

            mExplorerContainer = BuildExplorerContainer();
            rootVisualElement.Add(mExplorerContainer);
        }

        VisualElement BuildExplorerContainer()
        {
            VisualElement container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = ControlBuilder.Margin.DIALOG_PADDING;
            container.style.paddingBottom = ControlBuilder.Margin.DIALOG_PADDING;
            container.style.paddingLeft = ControlBuilder.Margin.DIALOG_PADDING;
            container.style.paddingRight = ControlBuilder.Margin.DIALOG_PADDING;

            Label explanation = new Label(
                PlasticLocalization.Name.SelectChangesetBelow.GetString());
            explanation.style.marginBottom = ControlBuilder.Margin.DIALOG_PADDING;
            explanation.style.whiteSpace = WhiteSpace.Normal;

            IMGUIContainer explorer = new IMGUIContainer(OnExplorerGUI);
            explorer.style.flexGrow = 1;

            Button selectButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.OkButton.GetString(), SelectChangesetAction);
            selectButton.name = CHANGESET_SELECT_BUTTON;

            Button backButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.BackButton.GetString(), ShowForm);
            backButton.name = CHANGESET_BACK_BUTTON;

            container.Add(explanation);
            container.Add(explorer);
            AddButtons(container, selectButton, backButton);

            container.RegisterCallback<KeyDownEvent>(
                OnExplorerKeyDown, TrickleDown.TrickleDown);

            return container;
        }

        void OnExplorerKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                SelectChangesetAction();
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                ShowForm();
                e.StopPropagation();
            }
        }

        void OnExplorerGUI()
        {
            mChangesetExplorerView.OnGUI();
            mExplorerProgressControls.UpdateProgress(this);
        }

        TextField mNameField;
        TextField mCommentField;
        Label mChangesetIdLabel;
        Button mChooseButton;
        Toggle mLabelAllXlinksToggle;
        Toggle mSwitchToLabelToggle;
        Button mCreateButton;
        Button mCancelButton;
        ProgressControlsForDialogs mProgressControls;

        ChangesetExplorerView mChangesetExplorerView;
        ImguiProgressControls mExplorerProgressControls;
        VisualElement mExplorerContainer;

        LabelCreationData mResultData;
        long mChangesetId;

        WorkspaceInfo mWkInfo;
        RepositorySpec mRepSpec;

        const float DIALOG_WIDTH = 710f;
        const float DIALOG_HEIGHT = 340f;
        const float EXPLORER_HEIGHT = 480f;
        const float LABEL_WIDTH = 150f;
        const float COMMENT_HEIGHT = 100f;
    }
}
