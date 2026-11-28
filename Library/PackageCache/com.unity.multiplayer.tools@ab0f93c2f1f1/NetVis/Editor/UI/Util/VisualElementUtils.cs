using System;
using Unity.Multiplayer.Tools.Common;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor
{
    static class VisualElementUtils
    {
        public static void IncludeInLayout(this VisualElement element, bool includeInLayout)
        {
            element.style.display = includeInLayout ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void Bind<TValue>(
            this BaseField<TValue> element,
            TValue value,
            Action<TValue> onValueChanged)
        {
            element.value = value;

            void OnValueChangedWithNotify(ChangeEvent<TValue> evt)
            {
                onValueChanged(evt.newValue);
            }

            void OnAttach(AttachToPanelEvent _) => element.RegisterValueChangedCallback(OnValueChangedWithNotify);
            void OnDetach(DetachFromPanelEvent _) => element.UnregisterValueChangedCallback(OnValueChangedWithNotify);
            element.AddEventLifecycle(OnAttach, OnDetach);
        }
    }
}
