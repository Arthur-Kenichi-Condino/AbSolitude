using System;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct UnnamedMessageEvent : INetworkMetricEvent
    {
        public UnnamedMessageEvent(ConnectionInfo connection, long bytesCount)
        {
            Connection = connection;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public long BytesCount { get; }

        /// <summary>
        /// Gets a unique identifier for the TreeView based on its connection and bytes count.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                var connectionHash = (ulong)Connection.GetHashCode();
                var bytesCountHash = (ulong)BytesCount.GetHashCode();
                return connectionHash + bytesCountHash;
            }
        }
        
    }
}
