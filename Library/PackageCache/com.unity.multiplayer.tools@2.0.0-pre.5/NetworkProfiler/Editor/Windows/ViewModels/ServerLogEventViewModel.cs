using System;
using Unity.Multiplayer.Tools.MetricTypes;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal class ServerLogEventViewModel : ViewModelBase
    {
        public ServerLogEventViewModel(ulong treeViewId, LogLevel logLevel, IRowData parent, Action onSelectedCallback = null)
            : base(
                parent,
                $"{logLevel.ToString()} Log",
                MetricType.ServerLog,
                onSelectedCallback,
                treeViewId) { }
    }
}
