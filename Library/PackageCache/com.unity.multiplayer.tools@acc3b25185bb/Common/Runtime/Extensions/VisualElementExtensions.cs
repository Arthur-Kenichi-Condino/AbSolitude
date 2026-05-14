using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.Common
{
    static class VisualElementExtensions
    {
        public static void AddEventLifecycle(
            this VisualElement visualElement,
            EventCallback<AttachToPanelEvent> onAttach,
            EventCallback<DetachFromPanelEvent> onDetach)
        {
            // If the element is already attached, fire the onAttach callback immediately
            if (visualElement.hierarchy.parent != null)
            {
                onAttach(default);
            }
            visualElement.RegisterCallback(onAttach);
            visualElement.RegisterCallback(onDetach);
        }

        /// <summary>
        /// This differs from VisualElement.Visible, because a VisualElement that is
        /// not visible still takes up space in the layout, whereas a VisualElement
        /// that is not included is skipped and does not take up space.
        /// </summary>
        /// <remarks>
        /// This is syntactic sugar for setting the display style to either Flex or None
        /// </remarks>
        public static void SetInclude(this VisualElement visualElement, bool includeInLayout)
        {
            visualElement.style.display = includeInLayout
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        /// <summary>
        /// This differs from VisualElement.Visible, because a VisualElement that is
        /// not visible still takes up space in the layout, whereas a VisualElement
        /// that is not included is skipped and does not take up space.
        /// </summary>
        /// <remarks>
        /// This is syntactic sugar for getting the display style (either Flex or None)
        /// </remarks>
        public static bool GetInclude(this VisualElement visualElement) =>
            visualElement.style.display.value == DisplayStyle.Flex;

        /// <summary>
        /// Registers the specified callback for every element matched in the query and automatically Unregisters the callback when
        /// the element is detached.
        /// </summary>
        /// <param name="visualElement">Root VisualElement on which the selector will be applied.</param>
        /// <param name="callback">The callback that will be registered for each child matching the query.</param>
        /// /// <param name="name">If specified, will select elements with this name.</param>
        /// <param name="className">If specified, will select elements with the given class (not to be confused with Type).</param>
        /// <typeparam name="TEventType">Type of the event to register the callback.</typeparam>
        public static void QueryRegisterCallback<TEventType>(
            this VisualElement visualElement,
            EventCallback<TEventType> callback,
            string name = null,
            string className = null)
            where TEventType : EventBase<TEventType>, new()
        {
            visualElement.Query<VisualElement>(name, className).Build().ForEach(AddLifecycleEvents);

            void AddLifecycleEvents(VisualElement element)
            {
                element.AddEventLifecycle(OnAttach, OnDetach);
                void OnAttach(AttachToPanelEvent evt) => element.RegisterCallback(callback);
                void OnDetach(DetachFromPanelEvent evt) => element.UnregisterCallback(callback);
            }
        }
    }
}
