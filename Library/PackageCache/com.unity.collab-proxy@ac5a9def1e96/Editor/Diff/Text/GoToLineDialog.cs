using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class GoToLineDialog : PlasticDialog
    {
        internal TextField LineNumberField { get { return mLineNumberField; } }
        internal Button OkButton { get { return mOkButton; } }
        internal Button CancelButton { get { return mCancelButton; } }

        internal static void Show(
            int currentLine,
            int totalLines,
            Action<int> onGoToLine)
        {
            GoToLineDialog dialog = Create(currentLine, totalLines, onGoToLine);
            dialog.Show(focusedWindow);
        }

        internal static GoToLineDialog Create(
            int currentLine,
            int totalLines,
            Action<int> onGoToLine)
        {
            GoToLineDialog dialog = CreateInstance<GoToLineDialog>();
            dialog.mCurrentLine = currentLine;
            dialog.mTotalLines = totalLines;
            dialog.mOnGoToLine = onGoToLine;
            return dialog;
        }

        internal static bool IsValidInput(string text, int totalLines)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            int line;
            if (!int.TryParse(text, out line))
                return false;

            return line >= 1 && line <= totalLines;
        }

        internal override void OkButtonAction()
        {
            if (!IsValidInput(mLineNumberField.value, mTotalLines))
                return;

            mOnGoToLine?.Invoke(int.Parse(mLineNumberField.value));

            base.OkButtonAction();
        }

        protected override Vector2 DefaultSize
        {
            get { return new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT); }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.GetString(PlasticLocalization.Name.GoToLine);
        }

        protected override void BuildComponents()
        {
            VisualElement inputArea = new VisualElement();

            Label label = new Label(
                string.Format(
                    PlasticLocalization.GetString(PlasticLocalization.Name.LineNumberRange),
                    1,
                    mTotalLines));
            label.style.marginBottom = 4;

            mLineNumberField = new TextField();
            mLineNumberField.value = mCurrentLine.ToString();
            mLineNumberField.RegisterValueChangedCallback(OnLineNumberChanged);

            inputArea.Add(label);
            inputArea.Add(mLineNumberField);

            AddComponents(inputArea);

            mOkButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.GetString(PlasticLocalization.Name.OkButton),
                OkButtonAction);
            mCancelButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.GetString(PlasticLocalization.Name.CancelButton),
                CancelButtonAction);

            AddButtons(mOkButton, mCancelButton);

            mLineNumberField.schedule.Execute(() =>
            {
                mLineNumberField.Focus();
                mLineNumberField.SelectAll();
            });
        }

        void OnLineNumberChanged(ChangeEvent<string> e)
        {
            mOkButton.SetEnabled(IsValidInput(e.newValue, mTotalLines));
        }

        Action<int> mOnGoToLine;
        int mCurrentLine;
        int mTotalLines;
        TextField mLineNumberField;
        Button mOkButton;
        Button mCancelButton;

        const float DIALOG_WIDTH = 300f;
        const float DIALOG_HEIGHT = 105f;
    }
}
