using System;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetworkProfiler.Editor;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Tests.Editor
{
    internal class TestRowData : IRowData
    {
        public TestRowData(
            string objectName,
            string typeName,
            ulong id = 0,
            long sent = 0,
            long received = 0,
            bool sentOverLocalConnection = false)
        {
            Id = id;
            Name = objectName;
            Bytes = new BytesSentAndReceived(sent, received);
            SentOverLocalConnection = sentOverLocalConnection;
            TypeName = typeName;
        }
        public ulong Id { get; }
        public IRowData Parent => null;
        public string TreeViewPath => Name;
        public string Name { get; }
        public BytesSentAndReceived Bytes { get; }
        public bool SentOverLocalConnection { get; }
        public string TypeDisplayName { get; }
        public string TypeName { get; }
        public Action OnSelectedCallback { get; }
    }
}
