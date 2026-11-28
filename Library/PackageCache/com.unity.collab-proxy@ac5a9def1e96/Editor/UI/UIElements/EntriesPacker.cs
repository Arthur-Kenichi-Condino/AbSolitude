using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal static class EntriesPacker
    {
        internal static VisualElement CreateAbsoluteFillPanel(
            params VisualElement[] children)
        {
            VisualElement panel = new VisualElement();
            panel.style.position = Position.Absolute;
            panel.style.left = 0;
            panel.style.top = 0;
            panel.style.right = 0;
            panel.style.bottom = 0;

            AddChildren(panel, children);

            return panel;
        }

        internal static VisualElement CreateVPanel(params VisualElement[] children)
        {
            VisualElement panel = new VisualElement();

            AddChildren(panel, children);

            return panel;
        }

        internal static VisualElement CreateHPanel(params VisualElement[] children)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Row;

            AddChildren(panel, children);

            return panel;
        }

        internal static VisualElement CreateRightAlignedButtonsPanel(params Button[] buttons)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Row;
            panel.style.justifyContent = Justify.FlexEnd;
            panel.style.marginTop = ENTRIES_SEPARATION;

            foreach (Button button in buttons)
                panel.Add(button);

            return panel;
        }

        internal static VisualElement CreateEntriesHPanel(
            VisualElement leftElement,
            float leftElementWidth,
            VisualElement fillElement,
            params VisualElement[] rightElements)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Row;
            panel.style.alignItems = Align.FlexStart;
            panel.style.marginBottom = ControlBuilder.Margin.DEFAULT;

            leftElement.style.width = leftElementWidth;
            leftElement.style.flexShrink = 0;

            fillElement.style.flexGrow = 1;

            panel.Add(leftElement);
            panel.Add(fillElement);

            foreach (VisualElement rightElement in rightElements)
                panel.Add(rightElement);

            return panel;
        }

        static void AddChildren(VisualElement panel, VisualElement[] children)
        {
            foreach (VisualElement child in children)
            {
                if (child == null)
                    continue;

                panel.Add(child);
            }
        }

        const float ENTRIES_SEPARATION = 2 * ControlBuilder.Margin.DEFAULT;
    }
}
