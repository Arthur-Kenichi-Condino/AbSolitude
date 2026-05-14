#if UNITY_2023_2_OR_NEWER
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics
{
    [Serializable]
    internal class ConnectionStateChangedData : IAnalytic.IData
    {
        public bool usedEditorGUI;
        public bool isPartOfLagSpike;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsNetworkSimulatorConnectionStateChanged_v1
    // Taxonomy: editor.analytics.mpToolsNetSimConnectionStateChanged.v1
    [AnalyticInfo(eventName: "mpToolsNetSimConnectionStateChanged", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 1000)]
    internal class ConnectionStateChangedAnalytic : IAnalytic
    {
        public ConnectionStateChangedAnalytic(bool usedEditorGUI, bool isPartOfLagSpike)
        {
            m_UsedEditorGUI = usedEditorGUI;
            m_IsPartOfLagSpike = isPartOfLagSpike;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data = new ConnectionStateChangedData
            {
                usedEditorGUI = m_UsedEditorGUI,
                isPartOfLagSpike = m_IsPartOfLagSpike
            };
            return true;
        }

        private readonly bool m_UsedEditorGUI;
        private readonly bool m_IsPartOfLagSpike;
    }
}
#endif
