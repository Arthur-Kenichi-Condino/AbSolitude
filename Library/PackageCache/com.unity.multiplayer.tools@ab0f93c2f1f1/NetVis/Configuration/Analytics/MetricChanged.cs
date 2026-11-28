#if UNITY_2023_2_OR_NEWER
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetVis.Configuration.Analytics
{
    [Serializable]
    internal class MetricChangedData : IAnalytic.IData
    {
        public string metric;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsNetworkSceneVisualizerMetricChanged_v1
    // Taxonomy: editor.analytics.mpToolsNetSceneVisMetricChanged.v1
    [AnalyticInfo(eventName: "mpToolsNetSceneVisMetricChanged", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 100)]
    internal class MetricChangedAnalytic : IAnalytic
    {
        public MetricChangedAnalytic(string metric)
        {
            m_Metric = metric;
        }
        
        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null; 
            data = new MetricChangedData
            {
                metric = m_Metric
            };
            return true;
        }
        
        private string m_Metric;
    }
}
#endif
