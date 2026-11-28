using System;
using UnityEditor;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow.Analytics
{
#if UNITY_2023_2_OR_NEWER
    using UnityEngine.Analytics;
    [Serializable]
    internal class InteractedData : IAnalytic.IData
    {
        public string toolName;
    }
    
    [Serializable]
    internal class InteractedWithEnabledData : IAnalytic.IData
    {
        public string toolName;
        public bool isToolEnabled;
    }
    
    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsToolsWindowInteracted_v1
    // Taxonomy: editor.analytics.mpToolsToolsWindowInteracted.v1
    [AnalyticInfo(eventName: "mpToolsToolsWindowInteracted", vendorKey: "unity.multiplayer.tools", version:1, maxEventsPerHour: 1000)]
    internal class InteractedAnalytic : IAnalytic
    {
        public InteractedAnalytic(string toolName, bool? isToolEnabled = null)
        {
            m_ToolName = toolName;
            m_IsToolEnabled = isToolEnabled;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            if (m_IsToolEnabled.HasValue)
            {
                data = new InteractedWithEnabledData
                {
                    toolName = m_ToolName,
                    isToolEnabled = m_IsToolEnabled.Value
                };
                return true;
            }
            data = new InteractedData
            {
                toolName = m_ToolName,
            };
            
            return true;
        }

        private readonly string m_ToolName;
        private readonly bool? m_IsToolEnabled;
    }
#endif

    internal static class InteractedAnalyticHelper
    {
        internal static void Send(string name, bool? isToolEnabled = null)
        {
#if UNITY_2023_2_OR_NEWER
            var analytic = new InteractedAnalytic(name, isToolEnabled);
            EditorAnalytics.SendAnalytic(analytic);
#endif
        }
    }
}
