using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
        protected virtual int MessageType(){
         return 0;
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         NetworkManager.CustomMessagingManager.OnUnnamedMessage+=OnReceivedUnnamedMessage;
        }
        public override void OnNetworkDespawn(){
         NetworkManager.CustomMessagingManager.OnUnnamedMessage-=OnReceivedUnnamedMessage;
         base.OnNetworkDespawn();
        }
        private void OnReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
        }
    }
}