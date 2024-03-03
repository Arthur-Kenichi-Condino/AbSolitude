using System;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal class UnnamedMessageEventViewModel : ViewModelBase
    {
        public UnnamedMessageEventViewModel(ulong TreeViewId, IRowData parent, Action onSelectedCallback = null)
            : base(
                parent,
                MetricType.UnnamedMessage.GetDisplayNameString(),
                MetricType.UnnamedMessage,
                onSelectedCallback,
                TreeViewId,
                treeViewPathComponent: $"{MetricType.UnnamedMessage.GetDisplayNameString()}: {Guid.NewGuid().ToString()}") { }
    }
}
