using System;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct ObjectSpawnedEvent : INetworkMetricEvent, INetworkObjectEvent
    {
        public ObjectSpawnedEvent(ConnectionInfo connection, NetworkObjectIdentifier networkId, long bytesCount)
        {
            Connection = connection;
            NetworkId = networkId;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public NetworkObjectIdentifier NetworkId { get; }

        public long BytesCount { get; }

        /// <summary>
        /// Gets a unique identifier for the TreeView based on its properties.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                var networkIDHash = (ulong) NetworkId.GetHashCode();
                var bytesCountHash = (ulong) BytesCount.GetHashCode();
                var connectionHash = (ulong) Connection.GetHashCode();
                return networkIDHash + bytesCountHash + connectionHash;
            }
        }
    }
}
