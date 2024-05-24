using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.Common.Visualization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    class GraphLegend : VisualElement
    {
        List<LegendKey> m_LegendKeys = new List<LegendKey>();
        public GraphLegend()
        {
            AddToClassList(UssClassNames.k_GraphLegend);
        }

        public void UpdateConfiguration(DisplayElementConfiguration configuration)
        {
            var stats = configuration.Stats;

            if (stats.Count < m_LegendKeys.Count)
            {
                //Delete the extra legends
                var diff = m_LegendKeys.Count - stats.Count;
                for (var i = m_LegendKeys.Count-1; m_LegendKeys.Count != stats.Count; --i)
                {
                    RemoveAt(i);
                    m_LegendKeys.RemoveAt(i);
                }
            }

            m_LegendKeys.Resize(stats.Count, () => new LegendKey());
            var childrenCount = Children().Count();
            var variableColors = configuration.GraphConfiguration.VariableColors;
            for (var i = 0; i < stats.Count; ++i)
            {
                var legendKey = m_LegendKeys[i];
                var stat = stats[i];
                Color color = (variableColors != null && i < variableColors.Count)
                    ? variableColors[i]
                    : CategoricalColorPalette.GetColor(i);
                legendKey.Update(stat.ToString(), color);

                if (i >= childrenCount)
                {
                    Add(legendKey);
                }
            }
        }
    }
}
