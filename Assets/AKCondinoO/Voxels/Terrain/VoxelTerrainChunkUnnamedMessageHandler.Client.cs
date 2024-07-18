#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkUnnamedMessageHandler;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler{
     [NonSerialized]int?clientSidecnkIdx=null;
        private void OnClientSideNetcnkIdxValueChanged(int previous,int current){
         if(Core.singleton.isClient){
          if(!IsOwner){
           if(clientSidecnkIdx==null||current!=clientSidecnkIdx.Value){
            clientSidecnkIdx=current;
            Log.DebugMessage("'ask server for chunk data'");
            /*
              add sizeof(int) for the message type
              add sizeof(int) for the cnkIdx
            */
            FastBufferWriter writer=new FastBufferWriter(sizeof(int)*2,Allocator.Persistent);
            if(writer.TryBeginWrite(sizeof(int)*2)){
             writer.WriteValue((int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest);
             writer.WriteValue((int)current);
            }
            if(VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend.TryGetValue(current,out FastBufferWriter oldRequest)){oldRequest.Dispose();}
            VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend[current]=writer;
           }
          }
         }
        }
        private void OnClientSideNetTerrainChunkArrayHasChangesValueChanged(NetworkListEvent<bool>change){
        }
     [SerializeField]bool DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA=false;
     [NonSerialized]bool waitingWriteEditData;
     [NonSerialized]bool pendingWriteEditData;
        internal void NetClientSideManualUpdate(){
            if(netObj.IsSpawned){
             if(waitingWriteEditData){
                 if(OnWroteEditData()){
                     waitingWriteEditData=false;
                 }
             }else{
                 if(pendingWriteEditData){
                     if(CanWriteEditData()){
                         pendingWriteEditData=false;
                         waitingWriteEditData=true;
                     }
                 }
             }
            }
        }
        bool CanWriteEditData(){
         if((DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA)&&!sending&&terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning)){
          terrainSendEditDataToServerBG.DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA=DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA;
          terrainSendEditDataToServerBG.cCoord=id.Value.cCoord;
          terrainSendEditDataToServerBG.cnkRgn=id.Value.cnkRgn;
          terrainSendEditDataToServerBG.cnkIdx=id.Value.cnkIdx;
          VoxelTerrainSendEditDataToServerMultithreaded.Schedule(terrainSendEditDataToServerBG);
          return true;
         }
         return false;
        }
        bool OnWroteEditData(){
         if(!sending&&terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning)){
          Log.DebugMessage("OnWroteEditData");
          sendingcnkIdx      =terrainSendEditDataToServerBG.cnkIdx;
          sendingDataToServer=terrainSendEditDataToServerBG.dataToSendToServer;
          terrainSendEditDataToServerBG.dataToSendToServer=null;
          segmentCount=sendingDataToServer.Count;
          sending=true;
          Log.DebugMessage("segmentCount:"+segmentCount);
          return true;
         }
         return false;
        }
     [NonSerialized]Coroutine clientSideSendVoxelTerrainChunkEditDataFileCoroutine;
     [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=1.25f/32f;
      [NonSerialized]internal float delayToSendNewMessages;//  writer.Length * segmentSizeToTimeInSecondsDelayRatio
     [NonSerialized]bool sending;
      [NonSerialized]int sendingcnkIdx;
      [NonSerialized]Dictionary<int,FastBufferWriter>sendingDataToServer;
      [NonSerialized]int segmentSize;
       [NonSerialized]int lastSegmentSize;
      [NonSerialized]int segmentCount=-1;
      [NonSerialized]readonly List<int>sentSegments=new List<int>();
        internal IEnumerator ClientSideSendVoxelTerrainChunkEditDataFileCoroutine(){
         yield return null;
         WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
         WaitUntil waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});//  delay with WaitUntil and a cooldown
            bool LimitMessagesSentPerFrame(){
             if(messagesSent>=maxMessagesPerFrame){
              return true;
             }
             messagesSent++;
             return false;
            }
            System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
            bool LimitExecutionTime(){
             sendingExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
             if(sendingExecutionTime>=sendingMaxExecutionTime){
              return true;
             }
             return false;
            }
            Loop:{
             yield return waitUntilGetFileData;
             Log.DebugMessage("ClientSideSendVoxelTerrainChunkEditDataFileCoroutine");
             stopwatch.Restart();
             FastBufferWriter writer;
             foreach(var segmentBufferPair in sendingDataToServer){
              int segment=segmentBufferPair.Key;
              Log.DebugMessage("'send segment':"+segment);
              writer=segmentBufferPair.Value;
              if(writer.IsInitialized){
               while(LimitExecutionTime()){
                yield return null;
                stopwatch.Restart();
               }
               while(LimitMessagesSentPerFrame()){
                yield return null;
               }
               totalLengthOfDataSent+=writer.Length;
               delayToSendNewMessages+=writer.Length*segmentSizeToTimeInSecondsDelayRatio;
               Log.DebugMessage("sending segment FastBufferWriter writer.Length:"+writer.Length);
               NetworkManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,writer,NetworkDelivery.ReliableFragmentedSequenced);
               if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
                //Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
                yield return waitForDelayToSendNewMessages;
               }
               writer.Dispose();
              }
              sentSegments.Add(segment);
             }
             sendingDataToServer.Clear();
             dataToSendDictionaryPool.Enqueue(sendingDataToServer);
             sendingDataToServer=null;
             sentSegments.Clear();
             Log.DebugMessage("'sent all segments':"+segmentCount);
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
             sending=false;
            }
            goto Loop;
        }
    }
}