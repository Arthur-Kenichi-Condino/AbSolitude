using System;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    class MeshShadingFillDropdown : MeshShadingFillGradientField
    {
        Action<MeshShadingGradient> m_ValueChangedCallback;

        public MeshShadingFillDropdown()
        {
            Selected += OnDropdownPointerDown;
        }

        public void Bind(MeshShadingGradient meshShadingGradient, Action<MeshShadingGradient> onValueChanged)
        {
            MeshShadingGradient = meshShadingGradient;
            m_ValueChangedCallback = onValueChanged;
            value = meshShadingGradient.ToGradient();
        }

        void OnDropdownPointerDown(MeshShadingFillGradientField obj)
        {
            var popup = new MeshShadingGradientPickerPopup();
            popup.GradientSelected += OnGradientSelected;

            PopupWindow.Show(worldBound, popup);
        }

        void OnGradientSelected(MeshShadingFillGradientField field)
        {
            MeshShadingGradient.Preset = field.MeshShadingGradient.Preset;
            MeshShadingGradient.Gradient = field.MeshShadingGradient.Gradient;
            value = field.MeshShadingGradient.Gradient;
            m_ValueChangedCallback?.Invoke(MeshShadingGradient);
        }

        public new class UxmlFactory : UxmlFactory<MeshShadingFillDropdown, UxmlTraits> { }
    }
}
