using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UndoRedo
{
    internal class UndoRedoTextField : UndoRedoHelper.IUndoRedoHost
    {
        UndoRedoState UndoRedoHelper.IUndoRedoHost.UndoRedoState
        {
            get { return new UndoRedoState(mTextField.value, mTextField.cursorIndex); }
            set { SetTextFieldStateWithoutSnapshot(value); }
        }

        internal UndoRedoTextField(TextField textField)
        {
            mTextField = textField;
            mUndoRedoHelper = new UndoRedoHelper(this);

            mUndoRedoHelper.Snapshot();
        }

        internal void RegisterEvents()
        {
            mTextField.RegisterValueChangedCallback(TextField_OnValueChanged);
            mTextField.RegisterCallback<KeyDownEvent>(
                TextField_OnKeyDown, TrickleDown.TrickleDown);
        }

        internal void UnRegisterEvents()
        {
            mTextField.UnregisterValueChangedCallback(TextField_OnValueChanged);
            mTextField.UnregisterCallback<KeyDownEvent>(
                TextField_OnKeyDown, TrickleDown.TrickleDown);
        }

        internal void SetValueWithoutNotify(string value)
        {
            mTextField.SetValueWithoutNotify(value);
            mTextField.SelectRange(value.Length, value.Length);

            mUndoRedoHelper.Snapshot();
        }

        void SetTextFieldStateWithoutSnapshot(UndoRedoState state)
        {
            mbIsSettingStateWithoutSnapshot = true;

            try
            {
                mTextField.value = state.Text;
                mTextField.SelectRange(state.CaretPosition, state.CaretPosition);
            }
            finally
            {
                mbIsSettingStateWithoutSnapshot = false;
            }
        }

        void TextField_OnValueChanged(ChangeEvent<string> evt)
        {
            if (mbIsSettingStateWithoutSnapshot)
                return;

            mUndoRedoHelper.Snapshot();
        }

        void TextField_OnKeyDown(KeyDownEvent evt)
        {
            Event e = evt.ToIMGUIEvent();

            if (Keyboard.IsUndoShortcutPressed(e))
            {
                evt.StopPropagation();

                if (mUndoRedoHelper.CanUndo)
                    mUndoRedoHelper.Undo();

                return;
            }

            if (Keyboard.IsRedoShortcutPressed(e))
            {
                evt.StopPropagation();

                if (mUndoRedoHelper.CanRedo)
                    mUndoRedoHelper.Redo();
            }
        }

        bool mbIsSettingStateWithoutSnapshot;

        readonly TextField mTextField;
        readonly UndoRedoHelper mUndoRedoHelper;
    }
}
