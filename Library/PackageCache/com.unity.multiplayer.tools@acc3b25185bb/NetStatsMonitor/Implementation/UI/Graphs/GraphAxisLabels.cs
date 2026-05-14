using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class GraphAxisLabels : VisualElement
    {
        readonly Label m_MinLabel = new();
        readonly Label m_MaxLabel = new();

        public string MinLabel
        {
            get => m_MinLabel.text;
            set => m_MinLabel.text = value;
        }
        public string MaxLabel
        {
            get => m_MaxLabel.text;
            set => m_MaxLabel.text = value;
        }

        internal GraphAxisLabels()
        {
            AddToClassList(UssClassNames.k_GraphAxis);
            m_MinLabel.AddToClassList(UssClassNames.k_GraphAxisMinValueLabel);
            m_MaxLabel.AddToClassList(UssClassNames.k_GraphAxisMaxValueLabel);

            Add(m_MinLabel);
            Add(m_MaxLabel);
        }

        public void SetLabels(string minLabel, string maxLabel)
        {
            MinLabel = minLabel;
            MaxLabel = maxLabel;
        }

        public StyleLength MaxLabelMarginRight
        {
            get => m_MaxLabel.style.marginRight;
            set => m_MaxLabel.style.marginRight = value;
        }
    }
}
