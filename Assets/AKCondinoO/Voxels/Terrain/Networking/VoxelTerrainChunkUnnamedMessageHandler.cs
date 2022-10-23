#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     internal NetworkObject netObj;
     internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         waitUntilGetFileData=new WaitUntil(()=>{return segmentCount>=0;});
         terrainGetFileEditDataToNetSyncBG.dataToSendToClients=new FastBufferWriter(sizeof(int)+sizeof(int)+sizeof(int)+sizeof(int)+VoxelsPerChunk*(sizeof(int)*3+sizeof(double)+sizeof(ushort)),Allocator.Persistent);
        }
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
         terrainGetFileEditDataToNetSyncBG.dataToSendToClients.Dispose();
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         NetworkManager.CustomMessagingManager.OnUnnamedMessage+=OnReceivedUnnamedMessage;
         if(Core.singleton.isServer){
          serverSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
        }
        public override void OnNetworkDespawn(){
         if(this!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
          StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
         }
         NetworkManager.CustomMessagingManager.OnUnnamedMessage-=OnReceivedUnnamedMessage;
         base.OnNetworkDespawn();
        }
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          pendingGetFileEditData=true;
         }
        }
     VoxelTerrainChunk cnk;
     bool waitingGetFileEditData;
     bool pendingGetFileEditData;
        internal void ManualUpdate(){
            if(waitingGetFileEditData){
                if(OnGotFileEditData()){
                    waitingGetFileEditData=false;
                }
            }else{
                if(pendingGetFileEditData){
                    if(CanGetFileEditData()){
                        pendingGetFileEditData=false;
                        waitingGetFileEditData=true;
                    }
                }
            }
        }
        bool CanGetFileEditData(){
         terrainGetFileEditDataToNetSyncBG.cCoord=id.Value.cCoord;
         terrainGetFileEditDataToNetSyncBG.cnkRgn=id.Value.cnkRgn;
         terrainGetFileEditDataToNetSyncBG.cnkIdx=id.Value.cnkIdx;
         VoxelTerrainGetFileEditDataToNetSyncMultithreaded.Schedule(terrainGetFileEditDataToNetSyncBG);
         return true;
        }
        bool OnGotFileEditData(){
         if(terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          Log.DebugMessage("OnGotFileEditData");
          segmentCount=0;
          return true;
         }
         return false;
        }
        private void OnReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         Log.DebugMessage("messageType:"+messageType);
         if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx){
          Log.DebugMessage("messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx");
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkcnkIdx(clientId,reader);
          }
         }else if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataFileSegment){
          Log.DebugMessage("messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataFileSegment");
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

         FastBufferReader dataToReceivedFromServer=new FastBufferReader(reader,Allocator.Persistent,-1,0,Allocator.Persistent);
         dataToReceivedFromServer.ReadValueSafe(out int readFirstValue);
         //Log.DebugMessage("readFirstValue:"+readFirstValue);
         Debug.Log("readFirstValue:"+readFirstValue);
         dataToReceivedFromServer.Dispose();

         //  Validate segment with the cnkIdx in the message "header" by comparing it to the
         // current cnkIdx set to this MessageHandler
         //  if this message fails to be received, client may ask for it after detecting missing segments in the "dictionary" of
         // segment-data
        }
     Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
     int segmentCount=-1;
     WaitUntil waitUntilGetFileData;
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
            Loop:{
             yield return waitUntilGetFileData;
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
             Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             NetworkManager.CustomMessagingManager.SendUnnamedMessageToAll(terrainGetFileEditDataToNetSyncBG.dataToSendToClients,NetworkDelivery.ReliableFragmentedSequenced);
            }
            goto Loop;
        }
    }
}