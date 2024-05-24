using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.Adapters;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    class NetworkTransportApi : INetworkTransportApi, IDisposable
    {
        readonly IList<INetworkAvailability> m_NetworkAvailabilityComponents = new List<INetworkAvailability>();
        readonly IList<ISimulateDisconnectAndReconnect> m_DisconnectAndReconnectComponents = new List<ISimulateDisconnectAndReconnect>();
        readonly IList<IHandleNetworkParameters> m_HandleNetworkParametersComponents = new List<IHandleNetworkParameters>();

        public NetworkTransportApi()
        {
            SubscribeToAllAdapters();
        }

        public void Dispose()
        {
            UnsubscribeFromAllAdapters();
        }

        public bool IsAvailable => m_NetworkAvailabilityComponents.Any() &&
                                   m_DisconnectAndReconnectComponents.Any() &&
                                   m_HandleNetworkParametersComponents.Any();

        public bool IsConnected => IsAvailable && m_NetworkAvailabilityComponents.All(x => x.IsConnected);

        public void SimulateDisconnect()
        {
            foreach (var component in m_DisconnectAndReconnectComponents)
            {
                component.SimulateDisconnect();
            }
        }

        public void SimulateReconnect()
        {
            foreach (var component in m_DisconnectAndReconnectComponents)
            {
                component.SimulateReconnect();
            }
        }

        public void UpdateNetworkParameters(NetworkParameters networkParameters)
        {
            foreach (var component in m_HandleNetworkParametersComponents)
            {
                component.NetworkParameters = networkParameters;
            }
        }

        void SubscribeToAllAdapters()
        {
            foreach (var adapter in NetworkAdapters.Adapters)
            {
                SubscribeToAdapter(adapter);
            }

            NetworkAdapters.OnAdapterAdded += SubscribeToAdapter;
            NetworkAdapters.OnAdapterRemoved += UnsubscribeFromAdapter;
        }

        void UnsubscribeFromAllAdapters()
        {
            foreach (var adapter in NetworkAdapters.Adapters)
            {
                UnsubscribeFromAdapter(adapter);
            }

            NetworkAdapters.OnAdapterAdded -= SubscribeToAdapter;
            NetworkAdapters.OnAdapterRemoved -= UnsubscribeFromAdapter;
        }

        void SubscribeToAdapter(INetworkAdapter adapter)
        {
            var networkAvailability = adapter.GetComponent<INetworkAvailability>();
            if (networkAvailability != null)
            {
                m_NetworkAvailabilityComponents.Add(networkAvailability);
            }

            var disconnectAndReconnect = adapter.GetComponent<ISimulateDisconnectAndReconnect>();
            if (disconnectAndReconnect != null)
            {
                m_DisconnectAndReconnectComponents.Add(disconnectAndReconnect);
            }

            var handleNetworkParameters = adapter.GetComponent<IHandleNetworkParameters>();
            if (handleNetworkParameters != null)
            {
                m_HandleNetworkParametersComponents.Add(handleNetworkParameters);
            }
        }

        void UnsubscribeFromAdapter(INetworkAdapter adapter)
        {
            var networkAvailability = adapter.GetComponent<INetworkAvailability>();
            if (networkAvailability != null)
            {
                m_NetworkAvailabilityComponents.Remove(networkAvailability);
            }

            var disconnectAndReconnect = adapter.GetComponent<ISimulateDisconnectAndReconnect>();
            if (disconnectAndReconnect != null)
            {
                m_DisconnectAndReconnectComponents.Remove(disconnectAndReconnect);
            }

            var handleNetworkParameters = adapter.GetComponent<IHandleNetworkParameters>();
            if (handleNetworkParameters != null)
            {
                m_HandleNetworkParametersComponents.Remove(handleNetworkParameters);
            }
        }
    }
}