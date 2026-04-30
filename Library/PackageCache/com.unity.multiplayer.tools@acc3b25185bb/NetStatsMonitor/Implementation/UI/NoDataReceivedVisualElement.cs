using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class NoDataReceivedVisualElement : VisualElement
    {
        Label m_Label = new();
        internal NoDataReceivedVisualElement()
        {
            AddToClassList(UssClassNames.k_DisplayElement);
            AddToClassList(UssClassNames.k_NoDataReceived);
            Add(m_Label);
            m_Label.AddToClassList(UssClassNames.k_NoDataReceivedLabel);
        }

        internal void Update(double secondsSinceDataReceived)
        {
            var wholeSecondsSinceLastUpdate = secondsSinceDataReceived.ToString("N0");
            m_Label.text = $"No data received for {wholeSecondsSinceLastUpdate} seconds";
        }
    }
}
