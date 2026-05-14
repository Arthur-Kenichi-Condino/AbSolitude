#if COM_UNITY_NETCODE_FOR_GAMEOBJECTS_V2_4_X
using System;
using Unity.Netcode;

namespace Unity.Multiplayer.Tools.GameObjects.Tests
{
    internal class RpcTestComponent : NetworkBehaviour
    {
        public event Action OnServerRpcAction;
        public event Action OnClientRpcAction;

        [ServerRpc]
        public void MyServerRpc()
        {
            OnServerRpcAction?.Invoke();
        }

        [ClientRpc]
        public void MyClientRpc(ClientRpcParams rpcParams = default)
        {
            OnClientRpcAction?.Invoke();
        }
    }
}
#endif
