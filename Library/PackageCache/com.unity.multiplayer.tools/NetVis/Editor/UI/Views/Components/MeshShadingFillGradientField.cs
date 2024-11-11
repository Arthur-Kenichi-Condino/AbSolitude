using System;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    class MeshShadingFillGradientField : GradientField
    {
        public event Action<MeshShadingFillGradientField> Selected;

        public MeshShadingFillGradientField()
        {
            RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
        }

        public MeshShadingGradient MeshShadingGradient { get; set; }

        [EventInterest(typeof(KeyDownEvent), typeof(MouseDownEvent))]
#if UNITY_2023_2_OR_NEWER
        protected override void HandleEventBubbleUp(EventBase _)
#else
        protected override void ExecuteDefaultActionAtTarget(EventBase _)
#endif
        {
            // This prevents calling the gradient picker.
        }

        void OnPointerDownEvent(PointerDownEvent evt)
        {
            Selected?.Invoke(this);
        }
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<MeshShadingFillGradientField, UxmlTraits>{}
#endif
    }
}
