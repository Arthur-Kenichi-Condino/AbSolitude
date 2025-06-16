using System;
using Unity.Collections;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct NetworkMessageEvent : INetworkMetricEvent
    {
        /// String overload maintained for backwards compatibility
        public NetworkMessageEvent(ConnectionInfo connection, string name, long bytesCount)
            : this(connection, StringConversionUtility.ConvertToFixedString(name), bytesCount) { }

        public NetworkMessageEvent(ConnectionInfo connection, FixedString64Bytes name, long bytesCount)
            : this()
        {
            Connection = connection;
            Name = name;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public FixedString64Bytes Name { get; }

        public long BytesCount { get; }
        
        /// <summary>
        /// Gets a unique identifier for the TreeView based on its properties.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                var nameHash = (ulong) Name.GetHashCode();
                var bytesCountHash = (ulong) BytesCount.GetHashCode();
                var connectionHash = (ulong) Connection.GetHashCode();
                return nameHash + bytesCountHash + connectionHash;
            }
        }
    }
}
