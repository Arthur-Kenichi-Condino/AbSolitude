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
        void Awake(){
         waitUntilGetFileData=new WaitUntil(()=>{return segmentCount>=0;});
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
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx){
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkcnkIdx(clientId,reader);
          }
         }else if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataFileSegment){
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkEditDataFileSegment(clientId,reader);
          }
         }
        }
        void OnClientSideReceivedVoxelTerrainChunkcnkIdx(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnClientSideReceivedVoxelTerrainChunkcnkIdx");
         //  if this message fails to be received, client may ask for it after there's a missing MessageHandler for its chunk;
         //  spawn and update message handlers in the server with VoxelSystemNetworking
        }
        void OnClientSideReceivedVoxelTerrainChunkEditDataFileSegment(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnClientSideReceivedVoxelTerrainChunkEditDataFileSegment");
         //  Validate segment with the cnkIdx in the message "header" by comparing it to the
         // current cnkIdx set to this MessageHandler
         //  if this message fails to be received, client may ask for it after detecting missing segments in the "dictionary" of
         // segment-data
        }
     int segmentCount=-1;
     WaitUntil waitUntilGetFileData;
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
            Loop:{
             yield return waitUntilGetFileData;
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
            }
            goto Loop;
        }
    }
}