using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
using UnityEditor;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics;
#endif

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    /// <summary>
    /// An API to trigger network simulation events.
    /// </summary>
    class NetworkEventsApi : INetworkEventsApi
    {
        private bool m_IsLageSpikeRunning;
        readonly NetworkSimulator m_NetworkSimulator;
        readonly INetworkTransportApi m_NetworkTransportApi;

        internal NetworkEventsApi(NetworkSimulator networkSimulator, INetworkTransportApi networkTransportApi)
        {
            m_NetworkSimulator = networkSimulator;
            m_NetworkTransportApi = networkTransportApi;
        }

        public bool IsAvailable => m_NetworkTransportApi.IsAvailable;

        public bool IsConnected => m_NetworkTransportApi.IsConnected;

        public void Disconnect()
        {
            if (m_NetworkSimulator == null || m_NetworkSimulator.enabled == false)
            {
                return;
            }

#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            EditorAnalytics.SendAnalytic(new ConnectionStateChangedAnalytic(m_NetworkSimulator.UsedEditorGUI, m_IsLageSpikeRunning));
#endif
            m_NetworkTransportApi.SimulateDisconnect();
        }

        public void Reconnect()
        {
            if (m_NetworkSimulator == null || m_NetworkSimulator.enabled == false)
            {
                return;
            }
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            EditorAnalytics.SendAnalytic(new ConnectionStateChangedAnalytic(m_NetworkSimulator.UsedEditorGUI, m_IsLageSpikeRunning));
#endif
            m_NetworkTransportApi.SimulateReconnect();
        }

        public void TriggerLagSpike(TimeSpan duration)
        {
            if (m_NetworkSimulator == null || m_NetworkSimulator.enabled == false)
            {
                return;
            }

            RunLagSpikeAsync(duration).Forget();
        }

        public Task TriggerLagSpikeAsync(TimeSpan duration)
        {
            if (m_NetworkSimulator == null || m_NetworkSimulator.enabled == false)
            {
                return Task.CompletedTask;
            }

            return RunLagSpikeAsync(duration);
        }

        public void ChangeConnectionPreset(INetworkSimulatorPreset newNetworkSimulatorPreset)
        {
            if (m_NetworkSimulator == null || m_NetworkSimulator.enabled == false)
            {
                return;
            }

            m_NetworkSimulator.ConnectionPreset = newNetworkSimulatorPreset;
        }

        public INetworkSimulatorPreset CurrentPreset => m_NetworkSimulator.ConnectionPreset;

        async Task RunLagSpikeAsync(TimeSpan duration)
        {
            m_IsLageSpikeRunning = true;
            Disconnect();

            await Task.Delay(duration);

            Reconnect();
            m_IsLageSpikeRunning = false;
        }
    }
}
