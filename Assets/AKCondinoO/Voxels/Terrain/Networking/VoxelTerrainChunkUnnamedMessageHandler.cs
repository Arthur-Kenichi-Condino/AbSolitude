#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     internal static readonly ConcurrentQueue<Dictionary<int,FastBufferWriter>>dataToSendDictionaryPool=new ConcurrentQueue<Dictionary<int,FastBufferWriter>>();
     internal const int SegmentSize=6144;
     // VoxelsPerChunk*VoxelEditDataSize all edit data if whole chunk is edited
     internal const int VoxelEditDataSize=sizeof(int)*3+sizeof(double)+sizeof(ushort);
     // sizeof(int) message type
     // sizeof(int) cnkIdx
     // sizeof(int) total segments (segment count)
     // sizeof(int) current segment
     // sizeof(int) segment writes count
     internal const int VoxelsPerSegment=(SegmentSize-sizeof(int)*5)/VoxelEditDataSize;
     internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     internal NetworkObject netObj;
     internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         waitUntilGetFileData=new WaitUntil(()=>{return segmentCount>=0;});
        }
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
         if(terrainGetFileEditDataToNetSyncBG.dataToSendToClients!=null){
          foreach(var segmentBufferPair in terrainGetFileEditDataToNetSyncBG.dataToSendToClients){
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients.Clear();
          dataToSendDictionaryPool.Enqueue(terrainGetFileEditDataToNetSyncBG.dataToSendToClients);
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients=null;
         }
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
         if(sendingDataToClients!=null){
          foreach(var segmentBufferPair in sendingDataToClients){
           int segment=segmentBufferPair.Key;
           if(sentSegments.Contains(segment)){
            continue;
           }
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          sendingDataToClients.Clear();
          dataToSendDictionaryPool.Enqueue(sendingDataToClients);
          sendingDataToClients=null;
          sentSegments.Clear();
         }
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
         if(segmentCount<0&&terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          Log.DebugMessage("OnGotFileEditData");
          sendingcnkIdx       =terrainGetFileEditDataToNetSyncBG.cnkIdx;
          sendingDataToClients=terrainGetFileEditDataToNetSyncBG.dataToSendToClients;
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients=null;
          segmentCount=sendingDataToClients.Count;
          Log.DebugMessage("segmentCount:"+segmentCount);
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
          //testing, REMOVE:
         //FastBufferReader dataToReceivedFromServer=new FastBufferReader(reader,Allocator.Persistent,-1,0,Allocator.Persistent);
         //dataToReceivedFromServer.ReadValueSafe(out int readFirstValue);
         ////Log.DebugMessage("readFirstValue:"+readFirstValue);
         //Debug.Log("readFirstValue:"+readFirstValue);
         //dataToReceivedFromServer.Dispose();
         //  Validate segment with the cnkIdx in the message "header" by comparing it to the
         // current cnkIdx set to this MessageHandler
         //  if this message fails to be received, client may ask for it after detecting missing segments in the "dictionary" of
         // segment-data
        }
     Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
      internal static double writingMaxExecutionTime=20.0;
       internal static double writingExecutionTime;
      int segmentCount=-1;
      int sendingcnkIdx;
      Dictionary<int,FastBufferWriter>sendingDataToClients;
      readonly List<int>sentSegments=new List<int>();
      WaitUntil waitUntilGetFileData;
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
            //Log.DebugMessage("writingMaxExecutionTime:"+writingMaxExecutionTime);
            System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
            bool LimitExecutionTime(){
             writingExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
             if(writingExecutionTime>=writingMaxExecutionTime){
              return true;
             }
             return false;
            }
            Loop:{
             yield return waitUntilGetFileData;
             Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             FastBufferWriter writer=new FastBufferWriter(sizeof(int)*3,Allocator.Persistent);
             writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx);//  message type
             writer.WriteValueSafe(sendingcnkIdx);//  cnkIdx
             writer.WriteValueSafe(segmentCount);//  total segments (segment count)
             NetworkManager.CustomMessagingManager.SendUnnamedMessageToAll(writer,NetworkDelivery.ReliableFragmentedSequenced);
             writer.Dispose();
             yield return null;
             stopwatch.Restart();
             foreach(var segmentBufferPair in sendingDataToClients){
              while(LimitExecutionTime()){
               yield return null;
               stopwatch.Restart();
              }
              int segment=segmentBufferPair.Key;
              //Log.DebugMessage("send segment:"+segment);
              writer=segmentBufferPair.Value;
              if(writer.IsInitialized){
               NetworkManager.CustomMessagingManager.SendUnnamedMessageToAll(writer,NetworkDelivery.ReliableFragmentedSequenced);
               writer.Dispose();
              }
              sentSegments.Add(segment);
             }
             sendingDataToClients.Clear();
             dataToSendDictionaryPool.Enqueue(sendingDataToClients);
             sendingDataToClients=null;
             sentSegments.Clear();
             Log.DebugMessage("sent all segments:"+segmentCount);
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
            }
            goto Loop;
        }
    }
}