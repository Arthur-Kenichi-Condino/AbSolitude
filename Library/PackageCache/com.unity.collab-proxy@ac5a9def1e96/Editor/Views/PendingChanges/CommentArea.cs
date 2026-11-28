using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Avatar;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.UI.UndoRedo;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges
{
    internal class CommentArea : VisualElement
    {
        internal interface IPendingChangesTabOperations
        {
            void CheckinAction(bool isGluonMode);
            void ShelveAction(bool isGluonMode);
            void ShowShelvesView();
            void UndoChangesAction(bool isGluonMode);
            void UndoUnchanged();
            void UndoCheckoutsKeepingLocalChanges();
        }

        internal TextField SummaryField { get { return mSummaryField; } }
        internal TextField CommentField { get { return mCommentField; } }
        internal Image AvatarImage { get { return mAvatarImage; } }
        internal Button CheckinButton { get { return mCheckinButton; } }
        internal SplitButton ShelveButton { get { return mShelveButton; } }
        internal SplitButton UndoButton { get { return mUndoButton; } }
        internal Toggle KeepLockedToggle { get { return mKeepLockedToggle; } }
        internal VisualElement ButtonsPanel { get { return mButtonsPanel; } }

        internal bool KeepItemsLocked
        {
            get { return mKeepLockedToggle != null && mKeepLockedToggle.value; }
        }

        internal CommentArea(
            IPendingChangesTabOperations pendingChangesTabOperations,
            bool isGluonMode,
            Action afterCommentChangedAction)
        {
            mOperations = pendingChangesTabOperations;
            mIsGluonMode = isGluonMode;
            mAfterCommentChangedAction = afterCommentChangedAction;

            this.LoadStyle(typeof(CommentArea).Name);

            BuildComponents();

            mSummaryUndoRedo.SetValueWithoutNotify(SessionState.GetString(
                UnityConstants.PENDING_CHANGES_CI_SUMMARY_KEY_NAME,
                string.Empty));
            mCommentUndoRedo.SetValueWithoutNotify(SessionState.GetString(
                UnityConstants.PENDING_CHANGES_CI_COMMENTS_KEY_NAME,
                string.Empty));

            RegisterCommentAreaEvents();
        }

        internal void OnEnable()
        {
            RegisterCommentAreaEvents();

            mShelveButton.OnEnable();
            mUndoButton.OnEnable();
        }

        internal void OnDisable()
        {
            SessionState.SetString(
                UnityConstants.PENDING_CHANGES_CI_SUMMARY_KEY_NAME,
                mSummaryField.value);
            SessionState.SetString(
                UnityConstants.PENDING_CHANGES_CI_COMMENTS_KEY_NAME,
                mCommentField.value);

            UnRegisterCommentAreaEvents();

            mShelveButton.OnDisable();
            mUndoButton.OnDisable();
        }

        internal string GetComment()
        {
            if (string.IsNullOrEmpty(mSummaryField.value))
                return mCommentField.value;

            if (string.IsNullOrEmpty(mCommentField.value))
                return mSummaryField.value;

            return mSummaryField.value + "\n" + mCommentField.value;
        }

        internal void SetCommentForTesting(string comment)
        {
            mCommentUndoRedo.SetValueWithoutNotify(comment);
        }

        internal void ClearComments()
        {
            mCommentUndoRedo.SetValueWithoutNotify(string.Empty);
            mSummaryUndoRedo.SetValueWithoutNotify(string.Empty);

            SessionState.EraseString(UnityConstants.PENDING_CHANGES_CI_COMMENTS_KEY_NAME);
            SessionState.EraseString(UnityConstants.PENDING_CHANGES_CI_SUMMARY_KEY_NAME);
        }

        internal void SetCurrentUser(ResolvedUser currentUser)
        {
            if (ReferenceEquals(mCurrentUser, currentUser))
                return;

            mCurrentUser = currentUser;

            UpdateAvatarImage();
        }

        internal void UpdateOperationRunningState(bool isOperationRunning)
        {
            bool bIsEnabled = !isOperationRunning;

            mSummaryField.SetEnabled(bIsEnabled);
            mCommentField.SetEnabled(bIsEnabled);
            mCheckinButton.SetEnabled(bIsEnabled);
            mShelveButton.SetEnabled(bIsEnabled);
            mUndoButton.SetEnabled(bIsEnabled);
        }

        void RegisterCommentAreaEvents()
        {
            mSummaryField.RegisterValueChangedCallback(CommentFields_OnValueChanged);
            mCommentField.RegisterValueChangedCallback(CommentFields_OnValueChanged);
            mSummaryField.RegisterCallback<KeyDownEvent>(
                SummaryField_OnKeyDown, TrickleDown.TrickleDown);
            mCommentField.RegisterCallback<KeyDownEvent>(
                CommentField_OnKeyDown, TrickleDown.TrickleDown);
            mButtonsPanel.RegisterCallback<GeometryChangedEvent>(
                ButtonsPanel_OnGeometryChanged);

            mSummaryUndoRedo.RegisterEvents();
            mCommentUndoRedo.RegisterEvents();
        }

        void UnRegisterCommentAreaEvents()
        {
            mSummaryField.UnregisterValueChangedCallback(CommentFields_OnValueChanged);
            mCommentField.UnregisterValueChangedCallback(CommentFields_OnValueChanged);
            mSummaryField.UnregisterCallback<KeyDownEvent>(
                SummaryField_OnKeyDown, TrickleDown.TrickleDown);
            mCommentField.UnregisterCallback<KeyDownEvent>(
                CommentField_OnKeyDown, TrickleDown.TrickleDown);
            mButtonsPanel.UnregisterCallback<GeometryChangedEvent>(
                ButtonsPanel_OnGeometryChanged);

            mSummaryUndoRedo.UnRegisterEvents();
            mCommentUndoRedo.UnRegisterEvents();
        }

        void CommentFields_OnValueChanged(ChangeEvent<string> evt)
        {
            mAfterCommentChangedAction();
        }

        void SummaryField_OnKeyDown(KeyDownEvent evt)
        {
            if (!ShouldFocusComment(evt.ToIMGUIEvent()))
                return;

            evt.StopPropagation();

            mCommentField.schedule.Execute(FocusCommentField);
        }

        void CommentField_OnKeyDown(KeyDownEvent evt)
        {
            if (!ShouldFocusSummary(evt.ToIMGUIEvent()))
                return;

            evt.StopPropagation();

            mSummaryField.schedule.Execute(FocusSummaryFieldAtEnd);
        }

        void FocusCommentField()
        {
            mCommentField.Focus();
        }

        void FocusSummaryFieldAtEnd()
        {
            mSummaryField.Focus();
            mSummaryField.SelectRange(
                mSummaryField.value.Length, mSummaryField.value.Length);
        }

        void ButtonsPanel_OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateButtonsLayout(evt.newRect.width < MIN_WIDTH_FOR_HORIZONTAL_LAYOUT);
        }

        bool ShouldFocusComment(Event e)
        {
            if (Keyboard.IsKeyPressed(e, KeyCode.DownArrow) && !Keyboard.HasShiftModifier(e))
                return true;

            if (Keyboard.IsKeyPressed(e, KeyCode.RightArrow)
                && !Keyboard.HasShiftModifier(e)
                && mSummaryField.cursorIndex == mSummaryField.value.Length)
                return true;

            if (Keyboard.IsTabPressed(e))
                return true;

            return Keyboard.IsReturnOrEnterKeyPressed(e);
        }

        bool ShouldFocusSummary(Event e)
        {
            if (Keyboard.IsKeyPressed(e, KeyCode.UpArrow)
                && !Keyboard.HasShiftModifier(e)
                && IsCommentCursorOnFirstLine())
                return true;

            if (Keyboard.IsKeyPressed(e, KeyCode.Backspace)
                && mCommentField.cursorIndex == 0
                && mCommentField.selectIndex == 0)
                return true;

            return Keyboard.IsKeyPressed(e, KeyCode.LeftArrow)
                && !Keyboard.HasShiftModifier(e)
                && mCommentField.cursorIndex == 0;
        }

        bool IsCommentCursorOnFirstLine()
        {
            TextElement textElement = mCommentField.Q<TextElement>();

            float lineHeight = textElement.MeasureTextSize(
                LINE_HEIGHT_SAMPLE_TEXT,
                0, MeasureMode.Undefined,
                0, MeasureMode.Undefined).y;

            return mCommentField.textSelection.cursorPosition.y < lineHeight;
        }

        void UpdateButtonsLayout(bool bStackVertically)
        {
            mButtonsPanel.style.flexDirection = bStackVertically ?
                FlexDirection.Column : FlexDirection.Row;

            foreach (VisualElement button in mActionButtons)
            {
                button.style.flexGrow = bStackVertically ? 0 : 1;
                button.style.flexBasis = bStackVertically ?
                    new StyleLength(StyleKeyword.Auto) : new StyleLength(0f);
            }
        }

        void UpdateAvatarImage()
        {
            if (mCurrentUser == null)
            {
                mAvatarImage.image = Images.GetEmptyGravatar();
                mAvatarImage.tooltip = string.Empty;
                return;
            }

            mAvatarImage.image = GetAvatar.ForEmail(mCurrentUser.Name, UpdateAvatarImage);
            mAvatarImage.tooltip = mCurrentUser.Name;
        }

        void PopulateShelveMenu(DropdownMenu menu)
        {
            menu.AppendAction(
                PlasticLocalization.Name.ShowShelvesButton.GetString(),
                action => mOperations.ShowShelvesView(),
                DropdownMenuAction.AlwaysEnabled);
        }

        void PopulateUndoMenu(DropdownMenu menu)
        {
            menu.AppendAction(
                PlasticLocalization.Name.UndoUnchangedButton.GetString(),
                action => mOperations.UndoUnchanged(),
                DropdownMenuAction.AlwaysEnabled);

            menu.AppendAction(
                PlasticLocalization.Name.UndoCheckoutsKeepingChanges.GetString(),
                action => mOperations.UndoCheckoutsKeepingLocalChanges(),
                DropdownMenuAction.AlwaysEnabled);
        }

        void BuildComponents()
        {
            style.backgroundColor = UnityStyles.Colors.ToolbarBackground;
            style.paddingTop = VERTICAL_PADDING;
            style.paddingBottom = VERTICAL_PADDING;

            mAvatarImage = new Image();
            mAvatarImage.style.width = AVATAR_SIZE;
            mAvatarImage.style.height = AVATAR_SIZE;
            mAvatarImage.style.marginLeft = AVATAR_HORIZONTAL_MARGIN;
            mAvatarImage.style.marginRight = AVATAR_HORIZONTAL_MARGIN;
            UpdateAvatarImage();

            mSummaryField = BuildTextField(SUMMARY_WATERMARK, bIsMultiline: false);
            mSummaryField.style.height = SUMMARY_FIELD_HEIGHT;
            mSummaryField.style.flexGrow = 1;
            mSummaryField.style.marginRight = TEXT_FIELD_HORIZONTAL_MARGIN;

            VisualElement summaryRow = EntriesPacker.CreateHPanel(mAvatarImage, mSummaryField);

            mCommentField = BuildTextField(
                PlasticLocalization.Name.CheckinComment.GetString(), bIsMultiline: true);
            mCommentField.verticalScrollerVisibility = ScrollerVisibility.Auto;
            mCommentField.style.flexGrow = 1;
            mCommentField.style.marginLeft = TEXT_FIELD_HORIZONTAL_MARGIN;
            mCommentField.style.marginRight = TEXT_FIELD_HORIZONTAL_MARGIN;
            mCommentField.style.marginTop = COMMENT_FIELD_TOP_MARGIN;
            mCommentField.style.marginBottom = COMMENT_FIELD_BOTTOM_MARGIN;
            mCommentField.Q(TextField.textInputUssName).style.paddingTop = COMMENT_FIELD_TOP_PADDING;

            mSummaryUndoRedo = new UndoRedoTextField(mSummaryField);
            mCommentUndoRedo = new UndoRedoTextField(mCommentField);

            mCheckinButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.Checkin.GetString(),
                () => mOperations.CheckinAction(mIsGluonMode));

            mShelveButton = ControlBuilder.Button.CreateSplitButton(
                PlasticLocalization.Name.Shelve.GetString(),
                () => mOperations.ShelveAction(mIsGluonMode),
                PopulateShelveMenu);

            mUndoButton = ControlBuilder.Button.CreateSplitButton(
                PlasticLocalization.Name.UndoChanges.GetString(),
                () => mOperations.UndoChangesAction(mIsGluonMode),
                mIsGluonMode ? null : (Action<DropdownMenu>)PopulateUndoMenu);

            mActionButtons = new VisualElement[] { mCheckinButton, mShelveButton, mUndoButton };

            Styles.Button.ApplyActionButtonStyle(mCheckinButton);
            Styles.Button.ApplyActionButtonStyle(mShelveButton.MainButton);
            Styles.Button.ApplyActionButtonStyle(mShelveButton.DropdownButton);
            Styles.Button.ApplyActionButtonStyle(mUndoButton.MainButton);

            if (mUndoButton.DropdownButton != null)
                Styles.Button.ApplyActionButtonStyle(mUndoButton.DropdownButton);

            foreach (VisualElement button in mActionButtons)
            {
                button.style.height = ACTION_BUTTON_HEIGHT;
                button.style.flexShrink = 0;
                button.style.marginLeft = 0;
                button.style.marginRight = ACTION_BUTTON_SPACING;
                button.style.marginTop = ACTION_BUTTON_VERTICAL_MARGIN;
                button.style.marginBottom = ACTION_BUTTON_VERTICAL_MARGIN;
            }

            mButtonsPanel = EntriesPacker.CreateHPanel(mActionButtons);
            mButtonsPanel.style.paddingLeft = BUTTONS_PANEL_LEFT_PADDING;
            mButtonsPanel.style.flexShrink = 0;
            UpdateButtonsLayout(false);

            Add(summaryRow);
            Add(mCommentField);
            Add(mButtonsPanel);

            if (!mIsGluonMode)
                return;

            mKeepLockedToggle = new Toggle();
            mKeepLockedToggle.text = PlasticLocalization.Name.KeepLocked.GetString();
            mKeepLockedToggle.style.width = UnityConstants.EXTRA_LARGE_BUTTON_WIDTH;
            mKeepLockedToggle.style.marginLeft = KEEP_LOCKED_TOGGLE_LEFT_MARGIN;
            mKeepLockedToggle.style.flexShrink = 0;
            Add(mKeepLockedToggle);
        }

        static TextField BuildTextField(string placeholder, bool bIsMultiline)
        {
            TextField result = new TextField();
            result.multiline = bIsMultiline;
            result.textEdition.placeholder = placeholder;
            result.textEdition.hidePlaceholderOnFocus = false;
            result.textSelection.selectAllOnFocus = false;
            result.textSelection.selectAllOnMouseUp = false;
            result.Q(TextField.textInputUssName).style.paddingLeft = TEXT_FIELD_LEFT_PADDING;

            return result;
        }

        ResolvedUser mCurrentUser;

        Image mAvatarImage;
        TextField mSummaryField;
        TextField mCommentField;
        UndoRedoTextField mSummaryUndoRedo;
        UndoRedoTextField mCommentUndoRedo;
        Button mCheckinButton;
        SplitButton mShelveButton;
        SplitButton mUndoButton;
        VisualElement[] mActionButtons;
        VisualElement mButtonsPanel;
        Toggle mKeepLockedToggle;

        readonly IPendingChangesTabOperations mOperations;
        readonly bool mIsGluonMode;
        readonly Action mAfterCommentChangedAction;

        const string SUMMARY_WATERMARK = "Summary (optional)";
        const string LINE_HEIGHT_SAMPLE_TEXT = "X";
        const int MIN_WIDTH_FOR_HORIZONTAL_LAYOUT = 250;
        const int VERTICAL_PADDING = 10;
        const int AVATAR_SIZE = 28;
        const int AVATAR_HORIZONTAL_MARGIN = 2;
        const int SUMMARY_FIELD_HEIGHT = 28;
        const int TEXT_FIELD_HORIZONTAL_MARGIN = 3;
        const int TEXT_FIELD_LEFT_PADDING = 7;
        const int COMMENT_FIELD_TOP_MARGIN = 4;
        const int COMMENT_FIELD_BOTTOM_MARGIN = 2;
        const int COMMENT_FIELD_TOP_PADDING = 5;
        const int ACTION_BUTTON_HEIGHT = 20;
        const int ACTION_BUTTON_SPACING = 3;
        const int ACTION_BUTTON_VERTICAL_MARGIN = 1;
        const int BUTTONS_PANEL_LEFT_PADDING = 6;
        const int KEEP_LOCKED_TOGGLE_LEFT_MARGIN = 4;
    }
}
