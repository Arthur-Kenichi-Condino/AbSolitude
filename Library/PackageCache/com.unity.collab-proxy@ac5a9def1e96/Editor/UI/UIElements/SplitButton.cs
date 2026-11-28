using System;

using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal class SplitButton : VisualElement
    {
        internal Button MainButton { get { return mMainButton; } }
        internal Button DropdownButton { get { return mDropdownButton; } }

        internal SplitButton(
            string text,
            Action mainAction,
            Action<DropdownMenu> populateMenuAction)
        {
            mPopulateMenuAction = populateMenuAction;

            style.flexDirection = FlexDirection.Row;

            mMainButton = ControlBuilder.Button.CreateButton(text, mainAction);
            mMainButton.style.flexGrow = 1;
            Styles.SplitButton.ApplyNoMarginStyle(mMainButton);
            Add(mMainButton);

            if (populateMenuAction == null)
                return;

            Styles.SplitButton.ApplyMainButtonStyle(mMainButton);

            mDropdownButton = BuildDropdownButton();
            Add(mDropdownButton);

            RegisterDropdownButtonEvents();
        }

        internal void OnEnable()
        {
            RegisterDropdownButtonEvents();
        }

        internal void OnDisable()
        {
            UnRegisterDropdownButtonEvents();
        }

        void RegisterDropdownButtonEvents()
        {
            if (mDropdownButton == null)
                return;

            mDropdownButton.RegisterCallback<ClickEvent>(DropdownButton_OnClick);
            mDropdownButton.RegisterCallback<ContextualMenuPopulateEvent>(
                DropdownButton_OnPopulateMenu);
        }

        void UnRegisterDropdownButtonEvents()
        {
            if (mDropdownButton == null)
                return;

            mDropdownButton.UnregisterCallback<ClickEvent>(DropdownButton_OnClick);
            mDropdownButton.UnregisterCallback<ContextualMenuPopulateEvent>(
                DropdownButton_OnPopulateMenu);
        }

        void DropdownButton_OnClick(ClickEvent evt)
        {
            mDropdownButton.panel?.contextualMenuManager?.DisplayMenu(evt, mDropdownButton);
        }

        void DropdownButton_OnPopulateMenu(ContextualMenuPopulateEvent evt)
        {
            mPopulateMenuAction(evt.menu);
        }

        static Button BuildDropdownButton()
        {
            Button result = new Button();
            result.style.width = DROPDOWN_BUTTON_WIDTH;
            result.style.paddingLeft = 0;
            result.style.paddingRight = 0;
            result.style.justifyContent = Justify.Center;
            result.style.alignItems = Align.Center;
            result.focusable = false;

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList(Styles.DROPDOWN_ARROW_CLASS_NAME);
            result.Add(arrow);

            Styles.SplitButton.ApplyNoMarginStyle(result);
            Styles.SplitButton.ApplyDropdownButtonStyle(result);

            return result;
        }

        Button mDropdownButton;

        readonly Button mMainButton;
        readonly Action<DropdownMenu> mPopulateMenuAction;

        const int DROPDOWN_BUTTON_WIDTH = 16;
    }
}
