#if UNITY_2023_2_OR_NEWER
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics
{
    enum ScenarioClassType
    {
        NetworkScenario = 0,
        NetworkScenarioTask = 1,
        NetworkScenarioBehaviour = 2,
    }
    
    [Serializable]
    internal class ScenarioInitializedData : IAnalytic.IData
    {
        public bool autoRun;
        public string scenarioClassType;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsNetworkSimulatorScenarioInitialized_v1
    // Taxonomy: editor.analytics.mpToolsNetSimScenarioInitialized.v1
    [AnalyticInfo(eventName: "mpToolsNetSimScenarioInitialized", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 100)]
    internal class ScenarioInitializedAnalytic : IAnalytic
    {
        public ScenarioInitializedAnalytic(bool autoRun, ScenarioClassType scenarioClassType)
        {
            m_AutoRun = autoRun;
            m_ScenarioClassType = scenarioClassType.ToString();
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data = new ScenarioInitializedData
            {
                autoRun = m_AutoRun,
                scenarioClassType = m_ScenarioClassType
            };
            return true;
        }

        private readonly bool m_AutoRun;
        private readonly string m_ScenarioClassType;
    }
}
#endif
