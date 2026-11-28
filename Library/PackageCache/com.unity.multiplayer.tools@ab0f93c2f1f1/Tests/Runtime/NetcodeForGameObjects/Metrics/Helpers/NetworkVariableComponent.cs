#if COM_UNITY_NETCODE_FOR_GAMEOBJECTS_V2_4_X
using UnityEngine;
using Unity.Netcode;

namespace Unity.Multiplayer.Tools.GameObjects.Tests
{
    internal class NetworkVariableComponent : NetworkBehaviour
    {
        public NetworkVariable<int> MyNetworkVariable { get; } = new NetworkVariable<int>();

        private void Update()
        {
            if (IsServer)
            {
                MyNetworkVariable.Value = Random.Range(100, 999);
            }
        }
    }
}
#endif
