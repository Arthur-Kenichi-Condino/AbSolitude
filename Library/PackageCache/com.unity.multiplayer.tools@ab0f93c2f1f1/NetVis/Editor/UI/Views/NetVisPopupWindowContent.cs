using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    class NetVisPopupWindowContent<TView> : PopupWindowContent
        where TView : VisualElement, new()
    {
        readonly TView m_View;
        Vector2 m_WindowContentSize;
        bool m_HasMeasuredContent;

        public NetVisPopupWindowContent(int width)
        {
            m_View = new TView
            {
                style =
                {
                    // The window is sized from the view's resolved height, which must not be clamped by the window.
                    flexShrink = 0,
                    paddingBottom = new StyleLength(new Length(4, LengthUnit.Pixel)),
                    paddingLeft = new StyleLength(new Length(4, LengthUnit.Pixel)),
                    paddingRight = new StyleLength(new Length(4, LengthUnit.Pixel)),
                    paddingTop = new StyleLength(new Length(4, LengthUnit.Pixel)),
                },
            };

            // Set initial content height large enough to fit all content, it will be updated dynamically in OnGUI.
            m_WindowContentSize = new Vector2(width, 4096);
        }

        public override Vector2 GetWindowSize()
        {
            return m_WindowContentSize;
        }

        public override void OnGUI(Rect rect)
        {
            // Whole pixels, so that a fractional size doesn't resize the window every repaint.
            var contentSize = new Vector2(
                Mathf.Ceil(m_View.resolvedStyle.width),
                Mathf.Ceil(m_View.resolvedStyle.height));

            // Not laid out yet.
            if (contentSize.x <= 0 || contentSize.y <= 0)
            {
                return;
            }

            // Only grow: shrinking makes the next expansion resize the window, one frame after it is drawn.
            var wantedSize = m_HasMeasuredContent
                ? Vector2.Max(m_WindowContentSize, contentSize)
                : contentSize;
            m_HasMeasuredContent = true;

            if (wantedSize == m_WindowContentSize)
            {
                return;
            }

            m_WindowContentSize = wantedSize;
            editorWindow.minSize = m_WindowContentSize;
            editorWindow.maxSize = m_WindowContentSize;
        }

        public override void OnOpen()
        {
            editorWindow.rootVisualElement.Add(m_View);
        }
    }
}
