using System;
using UnityEditor;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow.Analytics
{
#if UNITY_2023_2_OR_NEWER
    using UnityEngine.Analytics;

    [Serializable]
    internal class OpenedData : IAnalytic.IData
    {
        public string[] installedDependencies;
    }

    // Schema: com.unity3d.data.schemas.editor.analytics.multiplayerToolsToolsWindowOpened_v1
    // Taxonomy: editor.analytics.mpToolsToolsWindowOpened.v1
    [AnalyticInfo(eventName: "mpToolsToolsWindowOpened", vendorKey: "unity.multiplayer.tools", version: 1,
        maxEventsPerHour: 100)]
    internal class OpenedAnalytic : IAnalytic
    {
        public OpenedAnalytic(string[] installedDependencies)
        {
            m_InstalledDependencies = installedDependencies;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data= new OpenedData
            {
                installedDependencies = m_InstalledDependencies
            };
            return true;
        }

        private readonly string[] m_InstalledDependencies;
    }
#endif
    internal static class OpenedAnalyticHelper
    {
        public static void Send(string[] installedDependencies)
        {
#if UNITY_2023_2_OR_NEWER
            var analytic = new OpenedAnalytic(installedDependencies);
            EditorAnalytics.SendAnalytic(analytic);
#endif
        }
    }
}
