#if UNITY_2023_2_OR_NEWER && UNITY_EDITOR
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation.Analytics
{
    [Serializable]
    internal class ConfigUpdatedData : IAnalytic.IData
    {
        public bool isUsingCSS;
        public bool isUsingSettingsOverride;
        public bool isUsingPositionOverride;
    }
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsNetworkStatsMonitorConfigUpdated_v1
    // Taxonomy: editor.analytics.mpToolsNetStatsMonitorConfigUpdated.v1
    [AnalyticInfo(eventName: "mpToolsNetStatsMonitorConfigUpdated", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 100)]
    internal class ConfigUpdatedAnalytic : IAnalytic, IEquatable<ConfigUpdatedAnalytic>
    {
        public ConfigUpdatedAnalytic(bool isUsingCSS, bool isUsingSettingsOverride, bool isUsingPositionOverride)
        {
            m_IsUsingCSS = isUsingCSS;
            m_IsUsingSettingsOverride = isUsingSettingsOverride;
            m_IsUsingPositionOverride = isUsingPositionOverride;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data = new ConfigUpdatedData
            {
                isUsingCSS = m_IsUsingCSS,
                isUsingSettingsOverride = m_IsUsingSettingsOverride,
                isUsingPositionOverride = m_IsUsingPositionOverride
            };
            return true;
        }
        
        public bool Equals(ConfigUpdatedAnalytic other)
        {
            if (other == null)
            {
                return false;
            }

            return m_IsUsingCSS == other.m_IsUsingCSS &&
                   m_IsUsingSettingsOverride == other.m_IsUsingSettingsOverride &&
                   m_IsUsingPositionOverride == other.m_IsUsingPositionOverride;
        }
        
        private readonly bool m_IsUsingCSS;
        private readonly bool m_IsUsingSettingsOverride;
        private readonly bool m_IsUsingPositionOverride;
    }
}
#endif