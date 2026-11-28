using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal abstract class PlasticDialog : EditorWindow, IPlasticDialogCloser
    {
        internal ResponseType Answer { get { return mAnswer; } }

        internal ResponseType ShowModal(EditorWindow parentWindow)
        {
            InitializeVars(parentWindow);

            if (mDialogTestAction != null)
            {
                ShowUtility();
                mDialogTestAction(this);
                return mAnswer;
            }

            ShowModalUtility();
            return mAnswer;
        }

        internal void Show(EditorWindow parentWindow)
        {
            InitializeVars(parentWindow);
            ShowUtility();
        }

        internal virtual void OkButtonAction()
        {
            CompleteModal(ResponseType.Ok);
        }

        internal virtual void CancelButtonAction()
        {
            CompleteModal(ResponseType.Cancel);
        }

        internal static void SetDialogTestActionForTesting(Action<PlasticDialog> action)
        {
            mDialogTestAction = action;
        }

        void IPlasticDialogCloser.CloseDialog()
        {
            OkButtonAction();
        }

        protected abstract string GetTitle();

        protected abstract void BuildComponents();

        protected virtual Vector2 DefaultSize
        {
            get { return new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT); }
        }

        // The standard chrome lives inside this panel; dialogs that swap the
        // whole content (e.g. an embedded explorer) hide it and show their own
        protected VisualElement DialogPanel { get { return mDialogPanel; } }

        protected void SetTitle(string title)
        {
            // Mutate the shown GUIContent and assign it back: the window
            // container caches the instance at show time, so a plain
            // replacement leaves the visible title at the type-name default
            GUIContent content = titleContent;
            content.text = title;
            titleContent = content;
        }

        protected void SetExplanation(string text)
        {
            mExplanationLabel.text = text;
            mExplanationLabel.style.display = DisplayStyle.Flex;
        }

        protected void AddComponents(params VisualElement[] components)
        {
            foreach (VisualElement component in components)
                mComponentsPanel.Add(component);
        }

        protected void AddButtons(Button positiveButton, Button negativeButton)
        {
            mPositiveButton = positiveButton;
            mNegativeButton = negativeButton;

            AddButtons(mButtonsPanel, positiveButton, negativeButton);
        }

        protected void AddButtons(
            VisualElement panel, Button positiveButton, Button negativeButton)
        {
            positiveButton.style.minWidth = ControlBuilder.Button.DIALOG_BUTTON_MIN_WIDTH;
            negativeButton.style.minWidth = ControlBuilder.Button.DIALOG_BUTTON_MIN_WIDTH;

            panel.Add(EntriesPacker.CreateRightAlignedButtonsPanel(
                ButtonsInPlatformOrder(positiveButton, negativeButton)));
        }

        protected void AddControlConsumingEnterKey(VisualElement control)
        {
            mControlsConsumingEnterKey.Add(control);
        }

        protected void SetWindowFixedSize(Vector2 size)
        {
            minSize = size;
            maxSize = size;
        }

        void CreateGUI()
        {
            // A dialog persisted into the Unity layout across a domain reload
            // comes back unconfigured; close it instead of showing an empty
            // window
            if (!mbIsConfigured)
            {
                EditorApplication.delayCall += Close;
                return;
            }

            mDialogPanel = new VisualElement();
            mDialogPanel.style.paddingTop = ControlBuilder.Margin.DIALOG_PADDING;
            mDialogPanel.style.paddingBottom = ControlBuilder.Margin.DIALOG_PADDING;
            mDialogPanel.style.paddingLeft = ControlBuilder.Margin.DIALOG_PADDING;
            mDialogPanel.style.paddingRight = ControlBuilder.Margin.DIALOG_PADDING;
            mDialogPanel.style.flexGrow = 1;

            mExplanationLabel = new Label();
            mExplanationLabel.style.marginBottom = ControlBuilder.Margin.DIALOG_PADDING;
            mExplanationLabel.style.whiteSpace = WhiteSpace.Normal;
            mExplanationLabel.style.display = DisplayStyle.None;

            mComponentsPanel = new VisualElement();
            mComponentsPanel.style.flexGrow = 1;

            mButtonsPanel = new VisualElement();

            mDialogPanel.Add(mExplanationLabel);
            mDialogPanel.Add(mComponentsPanel);
            mDialogPanel.Add(mButtonsPanel);

            rootVisualElement.Add(mDialogPanel);

            BuildComponents();

            rootVisualElement.RegisterCallback<KeyDownEvent>(
                OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnDestroy()
        {
            if (mParentWindow == null)
                return;

            mParentWindow.Focus();
        }

        void InitializeVars(EditorWindow parentWindow)
        {
            mbIsConfigured = true;

            // The window container takes its caption when the window is shown;
            // titleContent changes after that are not rendered, so the title
            // must be set before ShowModalUtility or ShowUtility run
            SetTitle(GetTitle());
            mAnswer = ResponseType.None;
            mParentWindow = parentWindow;

            SetWindowFixedSize(DefaultSize);
            CenterOnMainWindow(DefaultSize);
        }

        void CenterOnMainWindow(Vector2 size)
        {
            Rect mainWindow = EditorGUIUtility.GetMainWindowPosition();
            position = new Rect(
                mainWindow.x + (mainWindow.width - size.x) / 2f,
                mainWindow.y + (mainWindow.height - size.y) / 2f,
                size.x,
                size.y);
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (mDialogPanel == null ||
                mDialogPanel.resolvedStyle.display == DisplayStyle.None)
                return;

            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                if (IsFocusInsideEnterConsumingControl())
                    return;

                ClickButton(mPositiveButton);
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                ClickButton(mNegativeButton);
                e.StopPropagation();
            }
        }

        bool IsFocusInsideEnterConsumingControl()
        {
            if (rootVisualElement.panel == null)
                return false;

            VisualElement focused =
                rootVisualElement.panel.focusController.focusedElement as VisualElement;

            if (focused == null)
                return false;

            foreach (VisualElement control in mControlsConsumingEnterKey)
            {
                if (focused == control || control.Contains(focused))
                    return true;
            }

            return false;
        }

        void CompleteModal(ResponseType answer)
        {
            mAnswer = answer;
            Close();
        }

        static void ClickButton(Button button)
        {
            if (button == null || !button.enabledSelf)
                return;

            using (NavigationSubmitEvent submitEvent = NavigationSubmitEvent.GetPooled())
            {
                submitEvent.target = button;
                button.SendEvent(submitEvent);
            }
        }

        static Button[] ButtonsInPlatformOrder(
            Button positiveButton, Button negativeButton)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return new Button[] { positiveButton, negativeButton };

            return new Button[] { negativeButton, positiveButton };
        }

        VisualElement mDialogPanel;
        Label mExplanationLabel;
        VisualElement mComponentsPanel;
        VisualElement mButtonsPanel;
        Button mPositiveButton;
        Button mNegativeButton;

        EditorWindow mParentWindow;
        ResponseType mAnswer;
        bool mbIsConfigured;

        readonly List<VisualElement> mControlsConsumingEnterKey = new List<VisualElement>();

        const float DEFAULT_WIDTH = 500f;
        const float DEFAULT_HEIGHT = 150f;

        static Action<PlasticDialog> mDialogTestAction;
    }
}
