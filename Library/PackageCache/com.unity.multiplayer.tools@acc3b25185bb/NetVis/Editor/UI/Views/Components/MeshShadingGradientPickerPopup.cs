using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    class MeshShadingGradientPickerPopup : PopupWindowContent
    {
        public event Action<MeshShadingFillGradientField> GradientSelected;

        public override Vector2 GetWindowSize() => new Vector2(260, 116);

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        public override void OnOpen()
        {
            var picker = new MeshShadingGradientPicker();
            picker.OnGradientSelected += field =>
            {
                GradientSelected?.Invoke(field);

                editorWindow.Close();
            };
            editorWindow.rootVisualElement.Add(picker);
        }
    }
}
