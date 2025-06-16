using System;
using Unity.Collections;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct NamedMessageEvent : INetworkMetricEvent
    {
        /// String overload maintained for backwards compatibility
        public NamedMessageEvent(ConnectionInfo connection, string name, long bytesCount)
            : this(connection, StringConversionUtility.ConvertToFixedString(name), bytesCount) { }

        public NamedMessageEvent(ConnectionInfo connection, FixedString64Bytes name, long bytesCount)
        {
            Connection = connection;
            Name = name;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public FixedString64Bytes Name { get; }

        public long BytesCount { get; }

        /// <summary>
        /// Gets a unique identifier for the TreeView based on its connection, name, and bytes count.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                ulong connectionHash = (ulong) Connection.GetHashCode();
                ulong nameHash = (ulong) Name.GetHashCode();
                ulong bytesCountHash = (ulong) BytesCount.GetHashCode();

                return connectionHash + nameHash + bytesCountHash;
            }
        }
    }
}
