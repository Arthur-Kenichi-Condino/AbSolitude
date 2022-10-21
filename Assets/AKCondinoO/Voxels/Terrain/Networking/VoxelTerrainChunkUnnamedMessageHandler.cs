#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         NetworkManager.CustomMessagingManager.OnUnnamedMessage+=OnReceivedUnnamedMessage;
        }
        public override void OnNetworkDespawn(){
         NetworkManager.CustomMessagingManager.OnUnnamedMessage-=OnReceivedUnnamedMessage;
         base.OnNetworkDespawn();
        }
        private void OnReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx){
          if(Core.singleton.isClient){
           OnReceivedVoxelTerrainChunkcnkIdx(clientId,reader);
          }
         }else if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataFileSegment){
          if(Core.singleton.isClient){
           OnReceivedVoxelTerrainChunkEditDataFileSegment(clientId,reader);
          }
         }
        }
        void OnReceivedVoxelTerrainChunkcnkIdx(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnReceivedVoxelTerrainChunkcnkIdx");
         //  if this message fails to be received, client may ask for it after there's a missing MessageHandler for its chunk;
         //  spawn and update message handlers in the server with VoxelSystemNetworking
        }
        void OnReceivedVoxelTerrainChunkEditDataFileSegment(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataFileSegment");
         //  Validate segment with the cnkIdx in the message "header" by comparing it to the
         // current cnkIdx set to this MessageHandler
         //  if this message fails to be received, client may ask for it after detecting missing segments in the "dictionary" of
         // segment-data
        }
        int segmentCount=-1;
        internal IEnumerator SendVoxelTerrainChunkEditDataFileCoroutine(){
            Loop:{
             yield return null;
            }
            goto Loop;
        }
    }
}