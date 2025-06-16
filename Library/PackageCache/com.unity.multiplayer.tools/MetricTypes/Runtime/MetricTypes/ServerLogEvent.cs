using System;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    internal struct ServerLogEvent : INetworkMetricEvent
    {
        public ServerLogEvent(ConnectionInfo connection, LogLevel logLevel, long bytesCount)
        {
            Connection = connection;
            LogLevel = logLevel;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public LogLevel LogLevel { get; }

        public long BytesCount { get; }

        /// <summary>
        /// Gets a unique identifier for the TreeView based on its connection, logLevel and bytes count.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                var connectionHash = (ulong)Connection.GetHashCode();
                var bytesCountHash = (ulong)BytesCount.GetHashCode();
                var logLevelHash = (ulong)LogLevel.GetHashCode();
                return connectionHash + bytesCountHash + logLevelHash;
            }
        }
        
    }

    // These must stay in sync with NetworkLog.LogType in MLAPI
    internal enum LogLevel
    {
        Info,
        Warning,
        Error,
        None
    }
}
