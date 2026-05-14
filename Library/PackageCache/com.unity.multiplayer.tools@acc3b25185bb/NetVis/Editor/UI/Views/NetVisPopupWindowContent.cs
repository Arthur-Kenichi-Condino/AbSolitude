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

        public NetVisPopupWindowContent(int width)
        {
            m_View = new TView
            {
                style =
                {
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
            m_WindowContentSize = new Vector2(m_View.resolvedStyle.width, m_View.resolvedStyle.height);
            editorWindow.minSize = m_WindowContentSize;
            editorWindow.maxSize = m_WindowContentSize;
        }

        public override void OnOpen()
        {
            editorWindow.rootVisualElement.Add(m_View);
        }
    }
}
