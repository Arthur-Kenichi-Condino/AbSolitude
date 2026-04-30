using System;
using JetBrains.Annotations;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal interface IRowData
    {
        /// <summary>
        /// Unique identifier for each row on treeView
        /// </summary>
        ulong Id { get; }

        string Name { get; }

        /// <summary>
        /// Parent in the TreeView hierarchy
        /// </summary>
        [CanBeNull]
        IRowData Parent { get; }

        /// <summary>
        /// The full path of this row in the tree view
        /// </summary>
        string TreeViewPath { get; }

        /// <summary>
        /// Bytes sent received for the message
        /// </summary>
        BytesSentAndReceived Bytes { get; }

        /// <summary>
        /// This is the connection that sent the message
        /// Connection == LocalConnection
        /// </summary>
        bool SentOverLocalConnection { get; }

        /// <summary>
        /// Type displayed on the Profiler window
        /// Can be Network Variable Deltas, Object Spawned, etc
        /// </summary>
        string TypeDisplayName { get; }

        /// <summary>
        /// A string identifying the type: should be lowercase, all-one-word for filtering
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// OnSelectionCallback is called when the row is selected on the tree view
        /// </summary>
        Action OnSelectedCallback { get; }
    }
}
