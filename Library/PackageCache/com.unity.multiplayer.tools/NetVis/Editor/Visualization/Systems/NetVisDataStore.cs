using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    /// <summary>
    /// Class that contains the logic to handle Adapters.
    /// </summary>
    class NetVisDataStore : IGetConnectedClients
    {
        internal bool IsConnectedServerOrClient { get; private set; }
        
        const float k_InvalidBandwidth = -1f;

        NetVisConfiguration m_Configuration;
        readonly BandwidthStats m_BandwidthStats;

        BandwidthSettings BandwidthSettings => m_Configuration.Settings.Bandwidth;

        // Adapters:
        IGetBandwidth m_GetBandwidth;
        IGetConnectedClients m_GetConnectedClients;
        IGetConnectionStatus m_GetConnectionStatus;
        IGetObjectIds m_GetObjectIds;
        IGetGameObject m_GetGameObject;
        IGetOwnership m_GetOwnership;

        bool m_SubscribedToBandwidth = false;

        /// <summary>
        /// A cache for storing bandwidth smoothed using Exponential Moving Average.
        /// </summary>
        /// <remarks>
        /// Null if no smoothing is needed, non-null if smoothing is required
        /// </remarks>
        [MaybeNull]
        BandwidthSmoothingCache m_BandwidthSmoothingCache;

        float m_MaxBandwidth = k_InvalidBandwidth;

        public bool IsBandwidthCacheEmpty => m_GetBandwidth is null || m_GetBandwidth.IsCacheEmpty ||
                            m_BandwidthSmoothingCache is null || m_BandwidthSmoothingCache.NeedsResetToImmediateValue;
        
        public NetVisDataStore(
            NetVisConfiguration configuration,
            BandwidthStats bandwidthStats)
        {
            m_Configuration = configuration;
            m_BandwidthStats = bandwidthStats;

            SetupAdapters();
            UpdateBandwidthBackend();
        }

        public void Dispose()
        {
            UnsubscribeFromAllAdapters();
        }

        public void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            m_Configuration = configuration;
            UpdateNoDataState();
            
            // We do not want to lose the data if we switch back and forth between the modes when paused.
            if (EditorApplication.isPaused && m_BandwidthSmoothingCache != null) return;
            
            UpdateBandwidthBackend();
            m_BandwidthSmoothingCache?.OnConfigurationChanged(configuration.Settings.Bandwidth);
            OnBandwidthUpdated();
        }

        public void OnPauseStateChanged(PauseState pauseState)
        {
            UpdateNoDataState();
            UpdateBandwidthBackend();
        }

        void SetupAdapters()
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
            m_GetBandwidth ??= adapter.GetComponent<IGetBandwidth>();
            m_GetConnectedClients ??= adapter.GetComponent<IGetConnectedClients>();
            m_GetConnectionStatus ??= adapter.GetComponent<IGetConnectionStatus>();
            m_GetObjectIds ??= adapter.GetComponent<IGetObjectIds>();
            m_GetGameObject ??= adapter.GetComponent<IGetGameObject>();
            m_GetOwnership ??= adapter.GetComponent<IGetOwnership>();

            if (m_GetConnectedClients is not null)
            {
                m_GetConnectedClients.ClientConnectionEvent -= OnClientConnected;
                m_GetConnectedClients.ClientConnectionEvent += OnClientConnected;

                m_GetConnectedClients.ClientDisconnectionEvent -= OnClientDisconnected;
                m_GetConnectedClients.ClientDisconnectionEvent += OnClientDisconnected;
            }

            if (m_GetConnectionStatus is not null)
            {
                m_GetConnectionStatus.ServerOrClientStarted -= OnServerOrClientStarted;
                m_GetConnectionStatus.ServerOrClientStarted += OnServerOrClientStarted;
                
                m_GetConnectionStatus.ServerOrClientStopped -= OnServerOrClientStopped;
                m_GetConnectionStatus.ServerOrClientStopped += OnServerOrClientStopped;

                OnServerOrClientStarted();
            }

            UpdateBandwidthBackend();
        }

        void OnServerOrClientStopped()
        {
            IsConnectedServerOrClient = false;
        }

        void OnServerOrClientStarted()
        {
            IsConnectedServerOrClient = true;
        }

        void UnsubscribeFromAdapter(INetworkAdapter adapter)
        {
            var getBandwidth = adapter?.GetComponent<IGetBandwidth>();
            if (getBandwidth != null)
            {
                getBandwidth.OnBandwidthUpdated -= OnBandwidthUpdated;
            }

            var connectionEvents = adapter?.GetComponent<IGetConnectedClients>();
            if (connectionEvents != null)
            {
                connectionEvents.ClientConnectionEvent -= OnClientConnected;
                connectionEvents.ClientDisconnectionEvent -= OnClientDisconnected;
            }
            UpdateBandwidthBackend();
        }

        /// <summary>
        /// This method must be run when one of the factors affecting the bandwidth backend changes. These factors include:
        /// <list type="number">
        /// <item> Whether bandwidth is available via the adapters </item>
        /// <item> Whether bandwidth is configured to be displayed </item>
        /// <item> Whether the player is playing or paused, as bandwidth is only smoothed while playing </item>
        /// </list>
        /// </summary>
        void UpdateBandwidthBackend()
        {
            var bandwidthAvailable = m_GetBandwidth != null;
            var needsBandwidth = m_Configuration.Metric == NetVisMetric.Bandwidth;
            var hasBandwidthSmoothingCache = m_BandwidthSmoothingCache != null;

            if (!bandwidthAvailable)
            {
                // Bandwidth is not available, so we are no longer meaningfully subscribed to it
                m_SubscribedToBandwidth = false;
                // Bandwidth is not available, no point storing a cache for it
                m_BandwidthSmoothingCache = null;
                return;
            }

            if (needsBandwidth && !m_SubscribedToBandwidth)
            {
                m_GetBandwidth.OnBandwidthUpdated += OnBandwidthUpdated;
                m_SubscribedToBandwidth = true;
            }
            else if (m_SubscribedToBandwidth && !needsBandwidth)
            {
                m_GetBandwidth.OnBandwidthUpdated -= OnBandwidthUpdated;
                m_SubscribedToBandwidth = false;
            }

            if (EditorApplication.isPaused)
            {
                return; // Do not clear/delete the cache when paused
            }

            if (needsBandwidth && !hasBandwidthSmoothingCache)
            {
                m_BandwidthSmoothingCache = new(BandwidthSettings);
            }
            else if (!needsBandwidth && hasBandwidthSmoothingCache)
            {
                m_BandwidthSmoothingCache = null;
            }
        }

        void OnBandwidthUpdated()
        {
            if (m_BandwidthSmoothingCache is not null)
            {
                m_BandwidthSmoothingCache.Update(m_GetObjectIds, m_GetBandwidth, Time.timeAsDouble);
                m_BandwidthStats.MaxBandwidth = m_BandwidthSmoothingCache.MaxBandwidth;
            }

            // New bandwidth information is available, the max bandwidth is now invalid and needs to be recomputed
            m_MaxBandwidth = k_InvalidBandwidth;
        }

        public IReadOnlyList<ObjectId> GetObjectIds()
        {
            return m_GetObjectIds?.ObjectIds ?? Array.Empty<ObjectId>();
        }

        public GameObject GetGameObject(ObjectId objectId)
        {
            return m_GetGameObject?.GetGameObject(objectId);
        }

        public ClientId GetOwner(ObjectId objectId)
        {
            return m_GetOwnership.GetOwner(objectId);
        }

        void OnClientConnected(ClientId clientId)
        {
            ClientConnectionEvent?.Invoke(clientId);
        }

        void OnClientDisconnected(ClientId clientId)
        {
            ClientDisconnectionEvent?.Invoke(clientId);
        }

        public float GetBandwidth(ObjectId objectId)
        {
            if (m_GetBandwidth == null)
            {
                return 0f;
            }

            if (m_BandwidthSmoothingCache != null)
            {
                // Smoothed bandwidth is non-null if and only if we should display smoothed bandwidth,
                // according to the logic in UpdateBandwidthBackend
                return m_BandwidthSmoothingCache[objectId];
            }

            return m_GetBandwidth.GetBandwidthBytes(
                objectId,
                BandwidthSettings.BandwidthType,
                BandwidthSettings.NetworkDirection);
        }

        public int GetMinBandwidth()
        {
            return BandwidthStats.k_MinBandwidth;
        }

        float GetRawMaxBandwidth()
        {
            if (m_BandwidthSmoothingCache != null)
            {
                return m_BandwidthSmoothingCache.MaxBandwidth;
            }
            if (m_MaxBandwidth == k_InvalidBandwidth)
            {
                RecomputeMaxBandwidth();
            }
            return m_MaxBandwidth;
        }

        public float GetMaxBandwidth()
        {
            return Math.Max(GetRawMaxBandwidth(), BandwidthStats.k_MinimumMaxValue);
        }

        void RecomputeMaxBandwidth()
        {
            m_MaxBandwidth = 0f;
            foreach (var objectId in GetObjectIds())
            {
                var bandwidth = m_GetBandwidth.GetBandwidthBytes(
                    objectId,
                    BandwidthSettings.BandwidthType,
                    BandwidthSettings.NetworkDirection);
                m_MaxBandwidth = MathF.Max(m_MaxBandwidth, bandwidth);
            }
            m_BandwidthStats.MaxBandwidth = m_MaxBandwidth;
        }

        /// <summary>
        /// When the player is paused and the cache is empty, there is no data to display.
        /// This will be reflected in the UI by showing a warning.
        /// </summary>
        void UpdateNoDataState()
        {
            m_Configuration.Settings.Bandwidth.HasNoData = EditorApplication.isPaused && m_BandwidthSmoothingCache == null;
        }

        public event Action<ClientId> ClientConnectionEvent;
        public event Action<ClientId> ClientDisconnectionEvent;

        public IReadOnlyList<ClientId> ConnectedClients =>
            m_GetConnectedClients?.ConnectedClients ?? Array.Empty<ClientId>();
    }
}
