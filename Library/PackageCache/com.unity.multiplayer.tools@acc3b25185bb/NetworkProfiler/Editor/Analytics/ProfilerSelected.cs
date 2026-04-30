#if UNITY_2023_2_OR_NEWER
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor.Analytics
{
    [Serializable]
    internal class ProfilerSelectedData : IAnalytic.IData
    {
        public string profiler;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsProfilerSelected_v1
    // Taxonomy: editor.analytics.mpToolsProfilerSelected.v1
    [AnalyticInfo(eventName: "mpToolsProfilerSelected", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 1000)]
    internal class ProfilerSelectedAnalytic : IAnalytic
    {
        public ProfilerSelectedAnalytic(string profiler)
        {
            m_Profiler = profiler;
        }
        
        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data = new ProfilerSelectedData()
            {
                profiler = m_Profiler
            };
            return true;
        }
        
        private string m_Profiler;
    }
}
#endif
