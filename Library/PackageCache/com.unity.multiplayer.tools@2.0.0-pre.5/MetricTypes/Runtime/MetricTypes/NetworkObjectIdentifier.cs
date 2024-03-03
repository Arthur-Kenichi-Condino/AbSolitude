using System;
using Unity.Collections;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct NetworkObjectIdentifier
    {
        /// String overload maintained for backwards compatibility
        public NetworkObjectIdentifier(string name, ulong networkId)
            : this(StringConversionUtility.ConvertToFixedString(name), networkId) { }

        public NetworkObjectIdentifier(FixedString64Bytes name, ulong networkId)
        {
            Name = name;
            NetworkId = networkId;
        }

        public FixedString64Bytes Name { get; }

        public ulong NetworkId { get; }
        
        /// <summary>
        /// Gets a unique identifier for the TreeView based on its properties.
        /// Note: This ID is not guaranteed to be unique as different objects might have the same hash code due to collisions.
        /// </summary>
        public ulong TreeViewId
        {
            get
            {
                var networkIDHash = (ulong) NetworkId.GetHashCode();
                var nameHash = (ulong) Name.GetHashCode();
                return networkIDHash + nameHash;
            }
        }
    }
}
