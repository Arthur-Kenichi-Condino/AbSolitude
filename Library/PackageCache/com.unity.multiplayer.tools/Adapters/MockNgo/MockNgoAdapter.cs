using System;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.Common;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Adapters.MockNgo
{
    class MockNgoAdapter

        : INetworkAdapter

        , IDisposable

        // Events
        // --------------------------------------------------------------------
        , IGetConnectedClients

        // Queries
        // ----------------------------------------------------------------------------------------
        , IGetBandwidth
        , IGetClientId
        , IGetGameObject
        , IGetObjectIds
        , IGetOwnership
        , IGetRpcCount
    {
        public delegate ClientId ClientIdProvider();
        readonly ClientIdProvider m_ClientIdProvider;
        public MockNgoAdapter(ClientIdProvider clientIdProvider)
        {
            m_ClientIdProvider = clientIdProvider;
            NetworkAdapters.AddAdapter(this);
        }
        public void Dispose()
        {
            NetworkAdapters.RemoveAdapter(this);
        }

        public AdapterMetadata Metadata { get; } = new AdapterMetadata
        {
            PackageInfo = new PackageInfo
            {
                PackageName = "com.unity.multiplayer.tools.mock",
                Version = new PackageVersion
                {
                    Major = 0,
                    Minor = 0,
                    Patch = 0,
                    PreRelease = ""
                }
            }
        };
        public T GetComponent<T>() where T : class, IAdapterComponent
        {
            return this as T;
        }

        // Fields for recording network traffic from MockNetworkObjects
        // ----------------------------------------------------------------------------------------
        bool IsServer => LocalClientId == ServerClientId;

        int m_NextValidObjectID = 0;

        readonly List<ClientId> m_ClientIds = new();
        readonly List<ObjectId> m_ObjectIds = new();
        Dictionary<ObjectId, GameObject> GameObjects { get; } = new();
        Dictionary<ObjectId, ClientId> ObjectOwners { get; } = new();
        Dictionary<ClientId, int> ObjectsPerOwner { get; } = new();

        Dictionary<ObjectId, int> RpcCallCounts { get; } = new();
        Dictionary<ObjectId, BytesSentAndReceived> RpcCallBandwidth { get; } = new();
        Dictionary<ObjectId, BytesSentAndReceived> NetVarBandwidth { get; } = new();

        /// <summary>
        /// Additional bandwidth that is not from RPC calls or Network Variable Updates
        /// </summary>
        Dictionary<ObjectId, BytesSentAndReceived> AdditionalBandwidth { get; } = new();

        int m_LastFrameCount = 0;
        void ClearOldData()
        {
            var frameCount = Time.frameCount;
            if (frameCount <= m_LastFrameCount)
            {
                return;
            }
            m_LastFrameCount = frameCount;
            RpcCallCounts.Clear();
            RpcCallBandwidth.Clear();
            NetVarBandwidth.Clear();
            AdditionalBandwidth.Clear();
        }

        // Internal interface for receiving data from MockNetworkObjects
        // ----------------------------------------------------------------------------------------
        void RecordNetworkTraffic(
            Dictionary<ObjectId, BytesSentAndReceived> networkTrafficCache,
            ObjectId objectId,
            int byteCount)
        {
            ClearOldData();
            var owner = ObjectOwners[objectId];
            var isObjectOwner = owner == LocalClientId;
            var bytesSent = (IsServer || isObjectOwner) ? byteCount : 0;
            var bytesReceived = isObjectOwner ? 0 : byteCount;
            var bytesSentAndReceived = new BytesSentAndReceived(bytesSent, bytesReceived);
            if (networkTrafficCache.TryGetValue(objectId, out BytesSentAndReceived existingValue))
            {
                networkTrafficCache[objectId] = bytesSentAndReceived + existingValue;
            }
            else
            {
                networkTrafficCache[objectId] = bytesSentAndReceived;
            }
        }

        internal ObjectId RecordObjectSpawn(GameObject gameObject, ClientId owner, int byteCount)
        {
            var objectId = (ObjectId)m_NextValidObjectID++;
            GameObjects[objectId] = gameObject;
            ObjectOwners[objectId] = owner;
            RecordNetworkTraffic(AdditionalBandwidth, objectId, byteCount);
            IncrementOwnerObjectCount(owner);

            m_ObjectIds.Add(objectId);

            return objectId;
        }

        internal void RecordObjectDespawn(ObjectId objectId, ClientId owner, int byteCount)
        {
            GameObjects.Remove(objectId);
            ObjectOwners.Remove(objectId);

            m_ObjectIds.Remove(objectId);

            DecrementOwnerObjectCount(owner);
        }

        internal void RecordOwnershipChange(ObjectId objectId, ClientId newOwner, int byteCount)
        {
            if (ObjectOwners.TryGetValue(objectId, out var previousOwner) && previousOwner != newOwner)
            {
                DecrementOwnerObjectCount(previousOwner);
                IncrementOwnerObjectCount(newOwner);
            }

            ObjectOwners[objectId] = newOwner;

            RecordNetworkTraffic(AdditionalBandwidth, objectId, byteCount);
        }

        internal void RecordRpcCall(ObjectId objectId, int byteCount)
        {
            RpcCallCounts.TryGetValue(objectId, out var existingRpcCount);
            RpcCallCounts[objectId] = existingRpcCount + 1;
            RecordNetworkTraffic(RpcCallBandwidth, objectId, byteCount);
        }

        internal void RecordNetworkVariableUpdate(ObjectId objectId, int byteCount)
        {
            RecordNetworkTraffic(NetVarBandwidth, objectId, byteCount);
        }

        void IncrementOwnerObjectCount(ClientId owner)
        {
            if (!ObjectsPerOwner.TryGetValue(owner, out var count) || count == 0)
            {
                ObjectsPerOwner[owner] = 1;
                OnClientConnected(owner);
            }
            else
            {
                ObjectsPerOwner[owner] = ++count;
            }
        }

        void DecrementOwnerObjectCount(ClientId owner)
        {
            if (ObjectsPerOwner.TryGetValue(owner, out var count) && count > 0)
            {
                ObjectsPerOwner[owner] = --count;
                if (count == 0)
                {
                    OnClientDisconnected(owner);
                }
            }
        }

        void OnClientConnected(ClientId client)
        {
            if (!m_ClientIds.Contains(client))
            {
                m_ClientIds.Add(client);
            }
            ClientConnectionEvent?.Invoke(client);
        }

        void OnClientDisconnected(ClientId client)
        {
            m_ClientIds.RemoveAll(id => id == client);
            ClientDisconnectionEvent?.Invoke(client);
        }

        // IGetBandwidth
        // ----------------------------------------------------------------------------------------
        public void OnLateUpdate()
        {
            ClearOldData();
            OnBandwidthUpdated?.Invoke();
            OnRpcCountUpdated?.Invoke();
        }

        public bool IsCacheEmpty => false;
        
        public BandwidthTypes SupportedBandwidthTypes =>
            BandwidthTypes.Other | BandwidthTypes.NetVar | BandwidthTypes.Rpc;

        public float GetBandwidthBytes(
            ObjectId objectId,
            BandwidthTypes bandwidthTypes = BandwidthTypes.All,
            NetworkDirection networkDirection = NetworkDirection.SentAndReceived)
        {
            if (OnBandwidthUpdated == null)
            {
                // Although it is not required for implementations of IGetBandwidth to throw
                // NoSubscriberException in the event that there are no subscribers, it is
                // documented as a possibility in the interface. In order to catch cases in
                // which our tools may fail to subscribe before calling GetBandwidthBytes
                // we should throw this exception, even though the information is available
                // without subscribers.
                throw new NoSubscribersException(nameof(IGetBandwidth), nameof(OnBandwidthUpdated));
            }
            var total = new BytesSentAndReceived();
            if (bandwidthTypes.ContainsAny(BandwidthTypes.Other) &&
                AdditionalBandwidth.TryGetValue(objectId, out var additionalBandwidth))
            {
                total += additionalBandwidth;
            }
            if (bandwidthTypes.ContainsAny(BandwidthTypes.NetVar) &&
                NetVarBandwidth.TryGetValue(objectId, out var netVarBandwidth))
            {
                total += netVarBandwidth;
            }
            if (bandwidthTypes.ContainsAny(BandwidthTypes.Rpc) &&
                RpcCallBandwidth.TryGetValue(objectId, out var rpcCallBandwidth))
            {
                total += rpcCallBandwidth;
            }
            return total[networkDirection];
        }
        public event Action OnBandwidthUpdated;

        // IGetClientId
        // ----------------------------------------------------------------------------------------
        public ClientId LocalClientId => m_ClientIdProvider();
        public ClientId ServerClientId => 0;

        // IGetGameObject
        // ----------------------------------------------------------------------------------------
        public GameObject GetGameObject(ObjectId objectId) => GameObjects.GetValueOrDefault(objectId);

        // IGetObjectIds
        // ----------------------------------------------------------------------------------------
        public IReadOnlyList<ObjectId> ObjectIds => m_ObjectIds;

        // IGetOwnership
        // ----------------------------------------------------------------------------------------
        public ClientId GetOwner(ObjectId objectId) => ObjectOwners.GetValueOrDefault(objectId);

        // IGetRpcCount
        // ----------------------------------------------------------------------------------------
        public int GetRpcCount(ObjectId objectId) => RpcCallCounts.GetValueOrDefault(objectId);
        public event Action OnRpcCountUpdated;

        // IClientConnectionEvents
        // ----------------------------------------------------------------------------------------
        public IReadOnlyList<ClientId> ConnectedClients => m_ClientIds;
        public event Action<ClientId> ClientConnectionEvent;
        public event Action<ClientId> ClientDisconnectionEvent;
    }
}
