using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.Common.EventTracking;
using Codice.CM.Common;
using Codice.Utils;
using CodiceApp;
using PlasticGui;
using PlasticGui.UnityGateway;
using PlasticGui.UnityGateway.Access;
using PlasticGui.WorkspaceWindow.Home;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Avatar;
using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Views.InviteMembers
{
    internal class InviteMembersDialog : EditorWindow
    {
        internal static void Show(string projectId, string projectName, RepositorySpec repSpec)
        {
            if (HasOpenInstances<InviteMembersDialog>())
            {
                GetWindow<InviteMembersDialog>(true);
            }
            else
            {
                InviteMembersDialog dialog = Create(projectId, projectName, repSpec);
                dialog.ShowUtility();

                TrackFeatureUseEvent.For(
                    repSpec, TrackFeatureUseEvent.Features.UnityPackage.OpenInviteMembersDialog);
            }
        }

        void OnEnable()
        {
            // OnEnable will be called during CreateInstance, but also after a domain reload.
            // In order for the dialog to survive a domain reload, components need to be rebuilt.
            // We use the serialized projectId to detect that scenario and reset the form.

            if (string.IsNullOrEmpty(mProjectId))
                return;

            BuildComponents();
        }

        static InviteMembersDialog Create(string projectId, string projectName, RepositorySpec repSpec)
        {
            InviteMembersDialog instance = CreateInstance<InviteMembersDialog>();
            instance.titleContent = new GUIContent(PlasticLocalization.Name.InviteDialogTitle.GetString());
            instance.mProjectId = projectId;
            instance.mProjectName = projectName;
            instance.mRepSpec = repSpec;
            instance.minSize = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT);
            instance.maxSize = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT);

            instance.BuildComponents();

            return instance;
        }

        void BuildComponents()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            InitializeLayoutAndStyles();

            // Components caching
            Image projectIcon = root.Q<Image>(PROJECT_ICON_IMAGE_NAME);
            mProjectNameLabel = root.Q<Label>(PROJECT_NAME_LABEL_NAME);
            mEmailChipsScrollView = root.Q<ScrollView>(EMAIL_CHIPS_SCROLL_VIEW_NAME);
            mEmailsInput = root.Q<TextField>(EMAILS_INPUT_NAME);
            mEmailChipsContainer = root.Q<VisualElement>(EMAIL_CHIPS_CONTAINER_NAME);
            mEmailsHelper = root.Q<Label>(EMAILS_HELPER_LABEL_NAME);
            mEmailsLimitIcon = root.Q<Image>(EMAILS_LIMIT_ICON_IMAGE_NAME);
            mEmailsHelperLimit = root.Q<Label>(EMAILS_HELPER_LIMIT_LABEL_NAME);
            mResponseHelpBox = root.Q<VisualElement>(RESPONSE_HELP_BOX_NAME);
            mSuccessRow = root.Q<VisualElement>(SUCCESS_ROW_NAME);
            mSuccessIcon = root.Q<Image>(SUCCESS_ICON_IMAGE_NAME);
            mAddedMembersLabel = root.Q<Label>(ADDED_MEMBERS_LABEL_NAME);
            mErrorRow = root.Q<VisualElement>(ERROR_ROW_NAME);
            mErrorIcon = root.Q<Image>(ERROR_ICON_IMAGE_NAME);
            mErrorMessageLabel = root.Q<Label>(ERROR_MESSAGE_LABEL_NAME);
            mUserTypeDropdown = root.Q<DropdownField>(USER_TYPE_DROPDOWN_NAME);
            mUserTypeHelp = root.Q<Image>(USER_TYPE_HELP_IMAGE_NAME);
            mUserTypeDescription = root.Q<Label>(USER_TYPE_DESCRIPTION_LABEL_NAME);
            mAddButton = root.Q<Button>(ADD_BUTTON_NAME);
            mCancelButton = root.Q<Button>(CANCEL_BUTTON_NAME);

            // One-off values initialization
            VisualElement projectIconContainer = root.Q<VisualElement>(PROJECT_ICON_CONTAINER_NAME);
            projectIconContainer.style.backgroundColor = GetColorFromProjectId(mProjectId);
            projectIcon.image = Images.GetUnityProjectIcon();
            mProjectNameLabel.text = mProjectName;

            root.Q<Label>(EMAILS_LABEL_NAME).text = PlasticLocalization.Name.InviteDialogUsersAndEmailsLabel.GetString();
            mEmailsInput.textEdition.placeholder = " "; // to position the caret correctly
            mEmailsLimitIcon.image = Images.GetWarnIcon();
            mEmailsHelper.text = PlasticLocalization.Name.InviteDialogEmailHelper.GetString();
            mEmailsHelperLimit.text = PlasticLocalization.Name.InviteDialogEmailLimit.GetString();

            root.Q<Label>(USER_TYPE_LABEL_NAME).text = PlasticLocalization.Name.InviteDialogUserTypeLabel.GetString();
            mUserTypeDropdown.choices = new List<string> { "User", "Manager", "Owner" };
            mUserTypeDropdown.value = mUserTypeDropdown.choices[0];
            mUserTypeHelp.image = Images.GetHelpIcon();
            mUserTypeHelp.tooltip = PlasticLocalization.Name.InviteDialogUserTypeTooltip.GetString();
            UpdateUserTypeDescription();

            mSuccessIcon.image = Images.GetInfoIcon();
            mErrorIcon.image = Images.GetErrorIcon();

            Label tryInUnityCloudLabel = root.Q<Label>(TRY_IN_UNITY_CLOUD_LABEL_NAME);
            tryInUnityCloudLabel.text = PlasticLocalization.Name.InviteDialogTryInUnityCloud.GetString();

            mAddButton.text = PlasticLocalization.Name.InviteDialogAddButton.GetString();
            mAddButton.SetEnabled(false);
            mCancelButton.text = PlasticLocalization.Name.InviteDialogCancelButton.GetString();
            SwapButtonsOrderIfMacOS(root, mAddButton, mCancelButton);

            // UI listeners
            mEmailsInput.RegisterCallback<FocusOutEvent>(OnEmailInputFocusOut);
            mEmailsInput.RegisterCallback<KeyDownEvent>(OnEmailInputKeyDown, TrickleDown.TrickleDown);
            mEmailsInput.RegisterValueChangedCallback(OnEmailInputChanged);
            mEmailChipsScrollView.RegisterCallback<ClickEvent>(OnChipsContainerClicked);
            mUserTypeDropdown.RegisterValueChangedCallback(OnUserTypeChanged);
            mResponseHelpBox.RegisterCallback<GeometryChangedEvent>(OnHelpBoxGeometryChanged);
            tryInUnityCloudLabel.RegisterCallback<ClickEvent>(OnTryInUnityCloudClicked);
            mAddButton.clicked += OnAddButtonClicked;
            mCancelButton.clicked += OnCancelButtonClicked;

            mEmailsInput.schedule.Execute(() => mEmailsInput.Focus());
        }

        void InitializeLayoutAndStyles()
        {
            rootVisualElement.LoadLayout(nameof(InviteMembersDialog));
            rootVisualElement.LoadStyle(nameof(InviteMembersDialog));
        }

        static Color GetColorFromProjectId(string projectId)
        {
            ColorRGB color = ProjectIconColor.FromProjectId(projectId);
            return new Color(color.R, color.G, color.B, color.A);
        }

        void OnEmailInputFocusOut(FocusOutEvent evt)
        {
            ProcessEmailInput();
        }

        void OnEmailInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Backspace && mEmailsInput.text.Length == 0)
            {
                RemoveLastEmailChip();
            }

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                mEmailsInput.schedule.Execute(() =>
                {
                    ProcessEmailInput();
                    mEmailsInput.Focus();
                });
            }
        }

        void OnChipsContainerClicked(ClickEvent evt)
        {
            mEmailsInput.Focus();
        }

        void OnEmailInputChanged(ChangeEvent<string> evt)
        {
            string value = evt.newValue;
            if (value.Contains(",") || value.Contains(";") || value.Contains(" "))
            {
                ProcessEmailInput();
            }

            ScrollToBottom();
        }

        void ProcessEmailInput()
        {
            string input = mEmailsInput.value;

            if (string.IsNullOrEmpty(input))
                return;

            string[] emails = input.Split(
                new[] { ',', ';', ' ', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string email in emails)
            {
                string trimmed = email.Trim();

                if (!string.IsNullOrEmpty(trimmed))
                {
                    AddEmailChip(trimmed);
                }
            }

            mEmailsInput.value = string.Empty;
            UpdateAddButtonState();

            mEmailsInput.Focus();
        }

        void AddEmailChip(string email)
        {
            if (mEmailChips.Count >= MAX_EMAILS)
                return;

            string normalizedEmail = email.ToLowerInvariant();

            if (mEmailChips.Any(e => e.ToLowerInvariant() == normalizedEmail))
                return;

            mEmailChips.Add(email);

            VisualElement chip = CreateEmailChip(email);
            mEmailToChipElement[email] = chip;
            mEmailChipsContainer.Insert(mEmailChipsContainer.childCount - 1, chip);

            if (!IsValidEmail(email))
            {
                mInvalidEmails.Add(email);
                MarkChipAsError(email, PlasticLocalization.Name.InviteDialogInvalidEmailFormat.GetString());
            }

            UpdateAddButtonState();
            ScrollToBottom();
        }

        static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0)
                return false;

            int dotIndex = email.LastIndexOf('.');
            if (dotIndex <= atIndex + 1)
                return false;

            if (dotIndex >= email.Length - 1)
                return false;

            return true;
        }

        void ScrollToBottom()
        {
            mEmailChipsScrollView.schedule.Execute(() =>
            {
                mEmailChipsScrollView.scrollOffset = new Vector2(
                    0, mEmailChipsScrollView.contentContainer.layout.height);
            });
        }

        VisualElement CreateEmailChip(string email)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList(EMAIL_CHIP_CONTAINER_CLASS);

            VisualElement chip = new VisualElement();
            chip.AddToClassList(EMAIL_CHIP_CLASS);

            VisualElement avatarContainer = new VisualElement();
            avatarContainer.AddToClassList(CHIP_AVATAR_CLASS);

            Image avatarImage = new Image();
            avatarImage.AddToClassList(CHIP_AVATAR_IMAGE_CLASS);
            avatarImage.image = GetAvatar.ForEmail(
                email, () => avatarImage.image = AvatarImages.GetAvatar(email));
            avatarImage.scaleMode = ScaleMode.ScaleToFit;

            Image warningIcon = new Image();
            warningIcon.AddToClassList(CHIP_WARNING_ICON_CLASS);
            warningIcon.AddToClassList(COLLAPSE_CLASS);
            warningIcon.image = Images.GetErrorIcon();

            avatarContainer.Add(avatarImage);
            avatarContainer.Add(warningIcon);
            chip.Add(avatarContainer);

            Label emailLabel = new Label(email);
            emailLabel.AddToClassList(CHIP_EMAIL_CLASS);
            chip.Add(emailLabel);

            Button removeButton = new Button(() => RemoveEmailChip(email));
            removeButton.AddToClassList(CHIP_REMOVE_CLASS);
            removeButton.text = "×";
            chip.Add(removeButton);

            container.Add(chip);
            return container;
        }

        void RemoveEmailChip(string email)
        {
            if (!mEmailToChipElement.TryGetValue(email, out VisualElement chip))
                return;

            mEmailChips.Remove(email);
            mEmailToChipElement.Remove(email);
            mInvalidEmails.Remove(email);
            mEmailChipsContainer.Remove(chip);

            UpdateAddButtonState();

            mEmailsInput.schedule.Execute(() => mEmailsInput.Focus());
        }

        void RemoveLastEmailChip()
        {
            if (mEmailChips.Count == 0)
                return;

            RemoveEmailChip(mEmailChips[mEmailChips.Count - 1]);
        }

        void OnUserTypeChanged(ChangeEvent<string> evt)
        {
            UpdateUserTypeDescription();
        }

        void UpdateUserTypeDescription()
        {
            switch (mUserTypeDropdown.value)
            {
                case "User":
                    mUserTypeDescription.text = PlasticLocalization.Name.InviteDialogUserTypeUserDescription.GetString();
                    break;
                case "Manager":
                    mUserTypeDescription.text = PlasticLocalization.Name.InviteDialogUserTypeManagerDescription.GetString();
                    break;
                case "Owner":
                    mUserTypeDescription.text = PlasticLocalization.Name.InviteDialogUserTypeOwnerDescription.GetString();
                    break;
            }
        }

        void UpdateAddButtonState()
        {
            bool hasValidEmails = mEmailChips.Count > 0 && mInvalidEmails.Count == 0;
            mAddButton.SetEnabled(hasValidEmails);

            if (mEmailChips.Count >= MAX_EMAILS)
            {
                mEmailsInput.Collapse();
                mEmailsHelperLimit.AddToClassList(LIMIT_REACHED_CLASS);
                mEmailsLimitIcon.RemoveFromClassList(COLLAPSE_CLASS);
            }
            else
            {
                mEmailsInput.Show();
                mEmailsHelperLimit.RemoveFromClassList(LIMIT_REACHED_CLASS);
                mEmailsLimitIcon.AddToClassList(COLLAPSE_CLASS);
            }
        }

        void OnAddButtonClicked()
        {
            ClearResults();

            mAddButton.SetEnabled(false);
            mAddButton.text = PlasticLocalization.Name.InviteDialogInvitingProgress.GetString();

            UnityGatewayServicesProvider.GetAccessApi().InviteUsers(
                mProjectId,
                mEmailChips.ToArray(),
                mUserTypeDropdown.value.ToLowerInvariant(),
                OnInviteUsersCompleted);

            TrackFeatureUseEvent.For(
                mRepSpec, TrackFeatureUseEvent.Features.UnityPackage.SubmitInviteMembersDialog);
        }

        void ClearResults()
        {
            mSuccessIcon.tooltip = string.Empty;
            mErrorIcon.tooltip = string.Empty;
            mResponseHelpBox.Collapse();
            mSuccessRow.Collapse();
            mErrorRow.Collapse();

            ResetChipsErrorState();
        }

        void OnInviteUsersCompleted(InviteUsersResponse response)
        {
            ParseInviteUsersResponse(response);

            mAddButton.text = PlasticLocalization.Name.InviteDialogAddButton.GetString();
            UpdateAddButtonState();
        }

        void ParseInviteUsersResponse(InviteUsersResponse response)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                mErrorMessageLabel.text = response.ErrorMessage;
                mErrorRow.Show();
                mResponseHelpBox.Show();

                return;
            }

            List<string> invitedEmails = new List<string>();
            List<string> failedEmails = new List<string>();

            if (response.InvitedUsers != null)
            {
                foreach (InviteUsersResponse.InvitedUser invitedUser in response.InvitedUsers)
                {
                    invitedEmails.Add(invitedUser.Email);
                    RemoveEmailChip(invitedUser.Email);
                }
            }

            if (response.ExistingUserEmails != null)
            {
                foreach (string email in response.ExistingUserEmails)
                {
                    failedEmails.Add(email);
                    MarkChipAsError(email, PlasticLocalization.Name.InviteDialogErrorExistingUser.GetString());
                }
            }

            if (response.EmailsWithoutUnityId != null)
            {
                foreach (string email in response.EmailsWithoutUnityId)
                {
                    failedEmails.Add(email);
                    MarkChipAsError(email, PlasticLocalization.Name.InviteDialogErrorNoUnityId.GetString());
                }
            }

            if (response.FailedInviteEmails != null)
            {
                foreach (string email in response.FailedInviteEmails)
                {
                    failedEmails.Add(email);
                    MarkChipAsError(email, PlasticLocalization.Name.InviteDialogErrorFailedInvite.GetString());
                }
            }

            if (invitedEmails.Count > 0)
            {
                mAddedMembersLabel.text = invitedEmails.Count == 1
                    ? PlasticLocalization.Name.InviteDialogSuccessSingular.GetString()
                    : PlasticLocalization.Name.InviteDialogSuccessPlural.GetString(invitedEmails.Count);
                mSuccessIcon.tooltip = string.Join("\n", invitedEmails);
                mSuccessRow.Show();
            }

            if (failedEmails.Count > 0)
            {
                mErrorMessageLabel.text = failedEmails.Count == 1
                    ? PlasticLocalization.Name.InviteDialogErrorSingular.GetString()
                    : PlasticLocalization.Name.InviteDialogErrorPlural.GetString(failedEmails.Count);
                mErrorIcon.tooltip = string.Join("\n", failedEmails);
                mErrorRow.Show();
            }

            if (invitedEmails.Count > 0 || failedEmails.Count > 0)
            {
                mResponseHelpBox.Show();
            }
        }

        void OnTryInUnityCloudClicked(ClickEvent evt)
        {
            OpenInviteUsersPage.Run(mRepSpec, UnityUrl.UnityDashboard.UnityCloudRequestSource.Editor);
        }

        void OnHelpBoxGeometryChanged(GeometryChangedEvent evt)
        {
            maxSize = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT + GetHelpBoxHeight());
            minSize = maxSize;
        }

        float GetHelpBoxHeight()
        {
            return mResponseHelpBox.resolvedStyle.height + mResponseHelpBox.resolvedStyle.marginTop;
        }

        void MarkChipAsError(string email, string tooltip)
        {
            if (!mEmailToChipElement.TryGetValue(email, out VisualElement container))
                return;

            VisualElement chip = container.Q<VisualElement>(className: EMAIL_CHIP_CLASS);
            if (chip == null)
                return;

            chip.tooltip = tooltip;

            Image avatarImage = chip.Q<Image>(className: CHIP_AVATAR_IMAGE_CLASS);
            Image warningIcon = chip.Q<Image>(className: CHIP_WARNING_ICON_CLASS);

            avatarImage?.AddToClassList(COLLAPSE_CLASS);
            warningIcon?.RemoveFromClassList(COLLAPSE_CLASS);
        }

        void ResetChipsErrorState()
        {
            foreach (VisualElement container in mEmailToChipElement.Values)
            {
                VisualElement chip = container.Q<VisualElement>(className: EMAIL_CHIP_CLASS);
                if (chip == null)
                    continue;

                chip.tooltip = string.Empty;

                Image avatarImage = chip.Q<Image>(className: CHIP_AVATAR_IMAGE_CLASS);
                Image warningIcon = chip.Q<Image>(className: CHIP_WARNING_ICON_CLASS);

                avatarImage?.RemoveFromClassList(COLLAPSE_CLASS);
                warningIcon?.AddToClassList(COLLAPSE_CLASS);
            }
        }

        void OnCancelButtonClicked()
        {
            Close();
        }

        static void SwapButtonsOrderIfMacOS(
            VisualElement root,
            Button addButton,
            Button cancelButton)
        {
            if (!PlatformIdentifier.IsMac())
                return;

            VisualElement footer = root.Q<VisualElement>(FOOTER_NAME);

            footer.Remove(addButton);
            footer.Remove(cancelButton);

            footer.Add(cancelButton);
            footer.Add(addButton);
        }

        [SerializeField] string mProjectId;
        [SerializeField] string mProjectName;
        [SerializeField] RepositorySpec mRepSpec;

        Label mProjectNameLabel;
        ScrollView mEmailChipsScrollView;
        TextField mEmailsInput;
        VisualElement mEmailChipsContainer;
        Label mEmailsHelper;
        Image mEmailsLimitIcon;
        Label mEmailsHelperLimit;
        VisualElement mResponseHelpBox;
        VisualElement mSuccessRow;
        Image mSuccessIcon;
        Image mErrorIcon;
        Label mAddedMembersLabel;
        VisualElement mErrorRow;
        Label mErrorMessageLabel;
        DropdownField mUserTypeDropdown;
        Image mUserTypeHelp;
        Label mUserTypeDescription;
        Button mAddButton;
        Button mCancelButton;

        readonly List<string> mEmailChips = new List<string>();
        readonly HashSet<string> mInvalidEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, VisualElement> mEmailToChipElement = new Dictionary<string, VisualElement>(
            StringComparer.OrdinalIgnoreCase);

        const int MAX_EMAILS = 5;
        const float DIALOG_WIDTH = 550;
        const float DIALOG_HEIGHT = 372;

        internal const string PROJECT_ICON_CONTAINER_NAME = "projectIconContainer";
        internal const string PROJECT_ICON_IMAGE_NAME = "projectIcon";
        internal const string PROJECT_NAME_LABEL_NAME = "projectName";
        internal const string EMAILS_LABEL_NAME = "emailsLabel";
        internal const string EMAIL_CHIPS_SCROLL_VIEW_NAME = "emailChipsScrollView";
        internal const string EMAILS_INPUT_NAME = "emailsInput";
        internal const string EMAIL_CHIPS_CONTAINER_NAME = "emailChipsContainer";
        internal const string EMAILS_HELPER_LABEL_NAME = "emailsHelper";
        internal const string EMAILS_LIMIT_ICON_IMAGE_NAME = "emailsLimitIcon";
        internal const string EMAILS_HELPER_LIMIT_LABEL_NAME = "emailsHelperLimit";
        internal const string RESPONSE_HELP_BOX_NAME = "responseHelpBox";
        internal const string SUCCESS_ROW_NAME = "successRow";
        internal const string SUCCESS_ICON_IMAGE_NAME = "successIcon";
        internal const string ADDED_MEMBERS_LABEL_NAME = "addedMembersLabel";
        internal const string ERROR_ROW_NAME = "errorRow";
        internal const string ERROR_ICON_IMAGE_NAME = "errorIcon";
        internal const string ERROR_MESSAGE_LABEL_NAME = "errorMessageLabel";
        internal const string TRY_IN_UNITY_CLOUD_LABEL_NAME = "tryInUnityCloudLabel";
        internal const string USER_TYPE_LABEL_NAME = "userTypeLabel";
        internal const string USER_TYPE_DROPDOWN_NAME = "userTypeDropdown";
        internal const string USER_TYPE_HELP_IMAGE_NAME = "userTypeHelp";
        internal const string USER_TYPE_DESCRIPTION_LABEL_NAME = "userTypeDescription";
        internal const string FOOTER_NAME = "footer";
        internal const string ADD_BUTTON_NAME = "addButton";
        internal const string CANCEL_BUTTON_NAME = "cancelButton";

        internal const string COLLAPSE_CLASS = "collapse";
        internal const string EMAIL_CHIP_CONTAINER_CLASS = "email-chip-container";
        internal const string EMAIL_CHIP_CLASS = "email-chip";
        internal const string CHIP_AVATAR_CLASS = "chip-avatar";
        internal const string CHIP_AVATAR_IMAGE_CLASS = "chip-avatar-image";
        internal const string CHIP_WARNING_ICON_CLASS = "chip-warning-icon";
        internal const string CHIP_EMAIL_CLASS = "chip-email";
        internal const string CHIP_REMOVE_CLASS = "chip-remove";
        internal const string LIMIT_REACHED_CLASS = "limit-reached";
    }
}
