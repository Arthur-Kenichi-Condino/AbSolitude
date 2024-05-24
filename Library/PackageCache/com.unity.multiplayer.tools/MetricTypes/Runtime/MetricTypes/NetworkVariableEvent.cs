using System;
using Unity.Collections;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct NetworkVariableEvent : INetworkMetricEvent, INetworkObjectEvent
    {
        /// String overload maintained for backwards compatibility
        public NetworkVariableEvent(ConnectionInfo connection, NetworkObjectIdentifier networkId, string name, string networkBehaviourName, long bytesCount)
            : this(connection, networkId, StringConversionUtility.ConvertToFixedString(name), StringConversionUtility.ConvertToFixedString(networkBehaviourName), bytesCount) { }

        public NetworkVariableEvent(ConnectionInfo connection, NetworkObjectIdentifier networkId, FixedString64Bytes name, FixedString64Bytes networkBehaviourName, long bytesCount)
        {
            Connection = connection;
            NetworkId = networkId;
            Name = name;
            NetworkBehaviourName = networkBehaviourName;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public NetworkObjectIdentifier NetworkId { get; }

        public FixedString64Bytes Name { get; }

        public FixedString64Bytes NetworkBehaviourName { get; }

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
                var nameHash = (ulong) Name.GetHashCode();
                var networkBehaviourNameHash = (ulong) NetworkBehaviourName.GetHashCode();
                return networkIDHash + bytesCountHash + connectionHash + nameHash + networkBehaviourNameHash;
            }
        }
    }
}
