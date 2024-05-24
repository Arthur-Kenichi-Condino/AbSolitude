using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Adapters.MockNgo
{
    [AddComponentMenu("MP Tools Dev/" + nameof(MockNetworkObject), 1000)]
    public class MockNetworkObject : MonoBehaviour
    {
        // Public properties
        // --------------------------------------------------------------------
        [field:SerializeField]
        public int OwnerClientId { get; set; }

        ClientId Owner
        {
            get => (ClientId)OwnerClientId;
            set => OwnerClientId = (int)value;
        }

        public int ObjectId { get; private set; } = -1;

        // Cached references
        // --------------------------------------------------------------------
        MockNetworkManager NetworkManager { get; set; }
        MockNgoAdapter Adapter => NetworkManager.Adapter;

        // Private fields
        // --------------------------------------------------------------------
        int m_PreviousOwner = 0;
        ClientId PreviousOwner
        {
            get => (ClientId)m_PreviousOwner;
            set => m_PreviousOwner = (int)value;
        }
        Vector3 m_PreviousPosition;
        Quaternion m_PreviousRotation;
        Vector3 m_PreviousScale;

        // Methods to record network traffic
        // --------------------------------------------------------------------
        public void RecordRpcCall(int byteCount)
        {
            Adapter.RecordRpcCall((ObjectId)ObjectId, byteCount);
        }

        public void RecordNetworkVariableUpdate(int byteCount)
        {
            Adapter.RecordNetworkVariableUpdate((ObjectId)ObjectId, byteCount);
        }

        void RecordSpawn(int byteCount)
        {
            PreviousOwner = Owner;
            ObjectId = (int)Adapter.RecordObjectSpawn(gameObject, Owner, byteCount);
        }

        void RecordDepawn(int byteCount)
        {
            Adapter.RecordObjectDespawn((ObjectId)ObjectId, Owner, byteCount);
        }

        void RecordOwnershipChange(ClientId newOwner, int byteCount)
        {
            Adapter.RecordOwnershipChange((ObjectId)ObjectId, newOwner, byteCount);
        }

        // Event method implementations to record any network traffic that
        // would have been incurred if the object was really networked
        // --------------------------------------------------------------------
        void Start()
        {
            NetworkManager = FindAnyObjectByType<MockNetworkManager>();
            RecordSpawn(NetworkManager.SpawnMessageByteCount);
        }

        void OnDestroy()
        {
            RecordDepawn(NetworkManager.DespawnMessageByteCount);
        }

        void Update()
        {
            if (PreviousOwner != Owner)
            {
                RecordOwnershipChange(Owner, NetworkManager.OwnershipChangeByteCount);
                PreviousOwner = Owner;
            }

            if (m_PreviousPosition != transform.position)
            {
                RecordNetworkVariableUpdate(NetworkManager.PositionUpdateByteCount);
                m_PreviousPosition = transform.position;
            }
            if (m_PreviousRotation != transform.rotation)
            {
                RecordNetworkVariableUpdate(NetworkManager.RotationUpdateByteCount);
                m_PreviousRotation = transform.rotation;
            }
            if (m_PreviousScale != transform.localScale)
            {
                RecordNetworkVariableUpdate(NetworkManager.ScaleUpdateByteCount);
                m_PreviousScale = transform.localScale;
            }
        }
    }
}
