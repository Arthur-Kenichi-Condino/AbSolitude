using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class LegendKey : VisualElement
    {
        Label m_KeyLabel;
        VisualElement m_Swatch;

        internal LegendKey()
        {
            AddToClassList(UssClassNames.k_GraphLegendKey);

            AddToClassList(UssClassNames.k_GraphLegendKey);
            m_Swatch = new VisualElement();
            m_Swatch.AddToClassList(UssClassNames.k_GraphLegendKeySwatch);
            Add(m_Swatch);

            m_KeyLabel = new Label();
            m_KeyLabel.AddToClassList(UssClassNames.k_GraphLegendKeyLabel);
            Add(m_KeyLabel);
        }

        internal LegendKey(string name, Color32 color)
            : this()
        {
            UpdateName(name);
            UpdateColor(color);
        }

        internal void Update(string name, Color color)
        {
            UpdateName(name);
            UpdateColor(color);
        }

        internal void UpdateName(string name)
        {
            this.name = name;
            m_KeyLabel.text = name;
        }

        internal void UpdateColor(Color color)
        {
            m_Swatch.style.backgroundColor = color;
        }
    }
}
