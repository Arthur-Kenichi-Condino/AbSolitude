using System;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    class MeshShadingGradientPicker : InjectedVisualElement<MeshShadingGradientPicker>
    {
        public event Action<MeshShadingFillGradientField> OnGradientSelected;

        [UxmlQuery] VisualElement GradientContainer;

        public MeshShadingGradientPicker()
        {
            foreach (var preset in EnumUtil.GetValuesAndNames(skip: MeshShadingGradientPreset.None))
            {
                var gradient = new MeshShadingGradient
                {
                    Preset = preset.value,
                    Gradient = preset.value.ToGradient(),
                };

                var field = new MeshShadingFillGradientField
                {
                    name = preset.name,
                    MeshShadingGradient = gradient,
                    value = gradient.Gradient,
                };

                field.Selected += _ => OnGradientSelected?.Invoke(field);

                GradientContainer.Add(field);
            }
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<MeshShadingGradientPicker, UxmlTraits> { }
#endif
    }
}
