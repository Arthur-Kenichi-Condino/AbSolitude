using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Adapters.MockNgo
{
    /// <summary>
    /// A mock network manager containing the mock client ID of the local instance
    /// and parameters of the underlying mock network solution (like how many bytes
    /// must be sent or received to update an object's position)
    /// </summary>
    /// <remarks>
    /// There should be only one of these.
    /// </remarks>
    [AddComponentMenu("MP Tools Dev/" + nameof(MockNetworkManager), 1000)]
    public class MockNetworkManager : MonoBehaviour
    {
        internal MockNgoAdapter Adapter { get; private set; }

        void Awake()
        {
            Adapter = new(() => (ClientId)LocalClientID);
        }

        void LateUpdate()
        {
            Adapter.OnLateUpdate();
        }

        void OnDestroy()
        {
            Adapter.Dispose();
        }

        /// <summary>
        /// The client ID of the local instance
        /// </summary>
        public int LocalClientID { get; set; } = 0;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object is spawned.
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int SpawnMessageByteCount { get; set; } = Constants.k_DefaultSpawnMessageByteCount;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object is despawned.
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int DespawnMessageByteCount { get; set; } = Constants.k_DefaultDespawnMessageByteCount;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object's owner is changed
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int OwnershipChangeByteCount { get; set; } = Constants.k_DefaultOwnershipChangeByteCount;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object's position changes.
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int PositionUpdateByteCount { get; set; } = Constants.k_DefaultPositionChangeByteCount;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object's rotation changes.
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int RotationUpdateByteCount { get; set; } = Constants.k_DefaultRotationChangeByteCount;

        /// <summary>
        /// The number of bytes of network traffic incurred when an object's scale changes.
        /// </summary>
        [field:Min(0)]
        [field:SerializeField]
        public int ScaleUpdateByteCount { get; set; } = Constants.k_DefaultScaleChangeByteCount;
    }
}
