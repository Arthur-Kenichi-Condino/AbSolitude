#if UNITY_2023_2_OR_NEWER
using System;
using UnityEngine.Analytics;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics
{
    [Serializable]
    internal class ConnectionPresetChangedData : IAnalytic.IData
    {
        public bool usedEditorGUI;
        public string presetName;
        public bool isPartOfScenario;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsNetworkSimulatorConnectionPresetChanged_v1
    // Taxonomy: editor.analytics.mpToolsNetSimConnectionPresetChanged.v1
    [AnalyticInfo(eventName: "mpToolsNetSimConnectionPresetChanged", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 1000)]
    internal class ConnectionPresetChangedAnalytic : IAnalytic
    {
        public ConnectionPresetChangedAnalytic(bool usedEditorGUI, string presetName, bool isPartOfScenario)
        {
            m_UsedEditorGUI = usedEditorGUI;
            m_PresetName = presetName;
            m_IsPartOfScenario = isPartOfScenario;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data= new ConnectionPresetChangedData
            {
                usedEditorGUI = m_UsedEditorGUI,
                presetName = m_PresetName,
                isPartOfScenario = m_IsPartOfScenario
            };
            return true;
        }

        private readonly bool m_UsedEditorGUI;
        private readonly string m_PresetName;
        private readonly bool m_IsPartOfScenario;
    }
}
#endif
