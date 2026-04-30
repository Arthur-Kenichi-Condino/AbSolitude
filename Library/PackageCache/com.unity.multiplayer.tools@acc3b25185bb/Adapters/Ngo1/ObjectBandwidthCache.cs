using System.Collections.Generic;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.Adapters.Ngo1
{
    class ObjectBandwidthCache
    {
        public bool IsCold { get; private set; } = true;
        readonly Dictionary<ObjectId, BytesSentAndReceived> m_OtherBandwidth = new();
        readonly Dictionary<ObjectId, BytesSentAndReceived> m_NetVarBandwidth = new();
        readonly Dictionary<ObjectId, BytesSentAndReceived> m_RpcBandwidth = new();

        public float GetBandwidth(
            ObjectId objectId,
            BandwidthTypes bandwidthTypes,
            NetworkDirection networkDirection)
        {
            var total = new BytesSentAndReceived();
            if (bandwidthTypes.ContainsAny(BandwidthTypes.Other) &&
                m_OtherBandwidth.TryGetValue(objectId, out var otherBandwidth))
            {
                total += otherBandwidth;
            }
            if (bandwidthTypes.ContainsAny(BandwidthTypes.NetVar) &&
                m_NetVarBandwidth.TryGetValue(objectId, out var netVarBandwidth))
            {
                total += netVarBandwidth;
            }
            if (bandwidthTypes.ContainsAny(BandwidthTypes.Rpc) &&
                m_RpcBandwidth.TryGetValue(objectId, out var rpcBandwidth))
            {
                total += rpcBandwidth;
            }
            return total[networkDirection];
        }

        static readonly NetworkDirection[] k_SentAndReceived = { NetworkDirection.Sent, NetworkDirection.Received };

        public void Update(MetricCollection collection)
        {
            IsCold = false;
            m_OtherBandwidth.Clear();
            m_NetVarBandwidth.Clear();
            m_RpcBandwidth.Clear();

            foreach (var direction in k_SentAndReceived)
            {
                LookupAndCountBytes<RpcEvent>(collection, direction, MetricType.Rpc, m_RpcBandwidth);
                LookupAndCountBytes<NetworkVariableEvent>(collection, direction, MetricType.NetworkVariableDelta, m_NetVarBandwidth);
                LookupAndCountBytes<ObjectSpawnedEvent>(collection, direction, MetricType.ObjectSpawned, m_OtherBandwidth);
                LookupAndCountBytes<ObjectDestroyedEvent>(collection, direction, MetricType.ObjectDestroyed, m_OtherBandwidth);
                LookupAndCountBytes<OwnershipChangeEvent>(collection, direction, MetricType.OwnershipChange, m_OtherBandwidth);
            }
        }

        static void LookupAndCountBytes<TEvent>(
            MetricCollection collection,
            NetworkDirection direction,
            MetricType metricType,
            Dictionary<ObjectId, BytesSentAndReceived> bandwidthBuffer)
            where TEvent : INetworkMetricEvent, INetworkObjectEvent
        {
            var directedMetric = metricType.GetDirectedMetric(direction);
            var metricId = MetricId.Create(directedMetric);
            var events = collection.GetEventValues<TEvent>(metricId);
            CountEventBytesForObjects(events, direction, bandwidthBuffer);
        }

        static void CountEventBytesForObjects<TEvent>(
            IReadOnlyList<TEvent> events,
            NetworkDirection direction,
            Dictionary<ObjectId, BytesSentAndReceived> bandwidthBuffer)
            where TEvent : INetworkMetricEvent, INetworkObjectEvent
        {
            foreach (var objectEvent in events)
            {
                var objectId = (ObjectId)objectEvent.NetworkId.NetworkId;
                var bytesSentAndReceived = new BytesSentAndReceived(objectEvent.BytesCount, direction);
                if (bandwidthBuffer.TryGetValue(objectId, out var value))
                {
                    bandwidthBuffer[objectId] = value + bytesSentAndReceived;
                }
                else
                {
                    bandwidthBuffer[objectId] = bytesSentAndReceived;
                }
            }
        }
    }
}
