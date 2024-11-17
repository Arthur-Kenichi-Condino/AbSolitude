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
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkArraySync{
     [SerializeField]bool DEBUG_FORCE_SEND_ALL_VOXEL_DATA=false;
        internal partial class ServerData{
            internal partial void OnInstantiated(){
            }
            internal partial void OnSpawn(){
             serverSideSendVoxelTerrainChunkEditDataFileCoroutine=VoxelSystem.singleton.StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
            }
            internal partial void NetServerSideOnDestroyingCore(){
             Log.DebugMessage("NetServerSideOnDestroyingCore");
             if(VoxelSystem.singleton!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
              VoxelSystem.singleton.StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
             }
             cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
            }
            internal partial void NetServerSideDispose(){
             Log.DebugMessage("NetServerSideDispose");
             cnkArraySync.terrainGetFileEditDataToNetSyncBG.Dispose();
            }
            internal partial void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
             if(firstCall||cCoord1!=id.Value.cCoord){
              id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
              cnkArraySync.netChunkId.Value=new(
               id.Value.cCoord,
               id.Value.cnkRgn,
               id.Value.cnkIdx
              );
              pendingGetFileEditData=true;
             }
            }
         [NonSerialized]readonly Dictionary<ulong,HashSet<int>>clientIdsRequestingData=new Dictionary<ulong,HashSet<int>>();
          [NonSerialized]internal static readonly Queue<HashSet<int>>clientIdsRequestingDataSegmentListPool=new Queue<HashSet<int>>();
            internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId,HashSet<int>segmentList){
             Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:clientId:"+clientId+":'cnkIdx':"+id.Value.cnkIdx+":segmentList.Count:"+segmentList.Count);
             if(!clientIdsRequestingData.TryGetValue(clientId,out HashSet<int>currentSegmentList)){
              clientIdsRequestingData.Add(clientId,currentSegmentList=segmentList);
             }else{
              currentSegmentList.UnionWith(segmentList);
              segmentList.Clear();
              clientIdsRequestingDataSegmentListPool.Enqueue(segmentList);
              segmentList=null;
             }
            }
         [NonSerialized]bool hasReadyEditData;
         [NonSerialized]bool synchronizing;
         [NonSerialized]bool waitingGetFileEditData;
         [NonSerialized]bool pendingGetFileEditData;
            internal partial void NetServerSideManualUpdate(){
                if(cnkArraySync!=null&&cnkArraySync.netObj.IsSpawned){
                 if(pendingGetFileEditData){
                     //Log.DebugMessage("pendingGetFileEditData");
                     if(CanGetFileEditData()){
                         pendingGetFileEditData=false;
                         waitingGetFileEditData=true;
                     }
                 }else{
                     if(waitingGetFileEditData){
                         //Log.DebugMessage("waitingGetFileEditData");
                         if(OnGotFileEditData()){
                             waitingGetFileEditData=false;
                             synchronizing=true;
                         }
                     }else{
                         if(synchronizing){
                             if(OnSynchronized()){
                                 synchronizing=false;
                                 hasReadyEditData=true;
                             }
                         }else{
                             if(hasReadyEditData){
                                 //  TO DO: try send
                                 TrySendFileEditData();
                             }else{
                             }
                         }
                     }
                 }
                }
            }
            bool CanGetFileEditData(){
             if(!sending&&cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
              //Log.DebugMessage("CanGetFileEditData");
              cnkArraySync.terrainGetFileEditDataToNetSyncBG.DEBUG_FORCE_SEND_ALL_VOXEL_DATA=cnkArraySync.DEBUG_FORCE_SEND_ALL_VOXEL_DATA;
              cnkArraySync.terrainGetFileEditDataToNetSyncBG.cCoord=id.Value.cCoord;
              cnkArraySync.terrainGetFileEditDataToNetSyncBG.cnkRgn=id.Value.cnkRgn;
              cnkArraySync.terrainGetFileEditDataToNetSyncBG.cnkIdx=id.Value.cnkIdx;
              VoxelTerrainGetFileEditDataToNetSyncMultithreaded.Schedule(cnkArraySync.terrainGetFileEditDataToNetSyncBG);
              return true;
             }
             return false;
            }
            bool OnGotFileEditData(){
             if(!sending&&cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
              //Log.DebugMessage("OnGotFileEditData");
              bool sync=false;
              for(int i=0;i<cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes.Length;++i){
               if(cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i]||(cnkArraySync.netChunkHasChanges[i]!=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i])){
                sync=true;
                cnkArraySync.netChunkHasChanges[i]=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i];
               }
              }
              if(sync){
               //Log.DebugMessage("'sync==true'");
               cnkArraySync.netChunkHasChanges.SetDirty(true);
              }else{
               //Log.DebugMessage("'sync==false'");
               cnkArraySync.netChunkHasChanges.ResetDirty();
              }
              return true;
             }
             return false;
            }
            bool OnSynchronized(){
             if(!sending&&cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
              if(!cnkArraySync.netChunkHasChanges.IsDirty()){
               //Log.DebugMessage("OnSynchronized");
               cnkArraySync.netChunkHasChanges[0]=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[0];
               return true;
              }
             }
             return false;
            }
            void TrySendFileEditData(){
             if(!sending&&cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
              if(clientIdsRequestingData.Count>0){
               Log.DebugMessage("'got clients requesting chunk data':clientIdsRequestingData.Count:"+clientIdsRequestingData.Count);
               sending=true;
               sendingcnkIdx=cnkArraySync.terrainGetFileEditDataToNetSyncBG.cnkIdx;
               foreach(var clientIdSegmentListPair in clientIdsRequestingData){
                clientsSegmentsMissing.UnionWith(clientIdSegmentListPair.Value);
                clientIdsToSendData.Add(clientIdSegmentListPair.Key,clientIdSegmentListPair.Value);
               }
               clientIdsRequestingData.Clear();
               Log.DebugMessage("clientIdsToSendData.Count:"+clientIdsToSendData.Count);
              }else{
               //Log.DebugMessage("'no clients requesting data':clientIdsRequestingData.Count:"+clientIdsRequestingData.Count);
              }
             }
            }
         [NonSerialized]Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
          [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=1.25f/32f;
           [NonSerialized]internal float delayToSendNewMessages;//  length * segmentSizeToTimeInSecondsDelayRatio
          [NonSerialized]bool sending;
           [NonSerialized]int sendingcnkIdx;
           [NonSerialized]readonly Dictionary<ulong,HashSet<int>>clientIdsToSendData=new Dictionary<ulong,HashSet<int>>();
            [NonSerialized]readonly HashSet<int>clientsSegmentsMissing=new HashSet<int>();
            internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
                yield return null;
                WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
                WaitUntil waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});//  delay with WaitUntil / a cooldown
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
                 stopwatch.Restart();
                 Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
                 foreach(int segment in clientsSegmentsMissing){
                  bool changed=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[segment];
                  if(changed){
                   if(!netVoxelArraysActive.TryGetValue(segment,out VoxelArraySync netVoxelArray)){
                    while(LimitExecutionTime()){
                     //Log.DebugMessage("'while LimitExecutionTime':"+sendingExecutionTime);
                     yield return null;
                     stopwatch.Restart();
                    }
                    while(LimitMessagesSentPerFrame()){
                     //Log.DebugMessage("'while LimitMessagesSentPerFrame'");
                     yield return null;
                    }
                    _Dequeue:{}
                    if(!VoxelSystem.singleton.asServer.netVoxelArraysPool.TryDequeue(out netVoxelArray)){
                     if(VoxelSystem.singleton.asServer.netVoxelArraysCount>=VoxelSystem.singleton.asServer.netVoxelArraysMaxCount){
                      Log.DebugMessage("'netVoxelArraysCount>=netVoxelArraysMaxCount'");
                      yield return null;
                      goto _Dequeue;
                     }
                     VoxelSystem.singleton.asServer.netVoxelArraysCount++;
                     netVoxelArray=Instantiate(cnkArraySync._VoxelArraySyncPrefab);
                     netVoxelArray.name=nameof(VoxelArraySync)+VoxelSystem.singleton.asServer.netVoxelArraysCount;
                     netVoxelArray.asServer.OnInstantiated();
                     try{
                      netVoxelArray.netObj.Spawn(destroyWithScene:false);
                     }catch(Exception e){
                      Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                     }
                     netVoxelArray.netObj.DontDestroyWithOwner=true;
                    }
                    netVoxelArray.asServer.OnDequeue(cnkArraySync,segment);
                   }
                   netVoxelArray.asServer.OnSetChanges(sendingcnkIdx);
                   foreach(var clientIdSegmentListPair in clientIdsToSendData){
                    HashSet<int>segmentList=clientIdSegmentListPair.Value;
                    if(segmentList.Contains(segment)){
                     netVoxelArray.asServer.clientIdsRequestingData.Add(clientIdSegmentListPair.Key);
                     VoxelSystem.singleton.asServer.clientIdsRequestingNetVoxelArray.Add(clientIdSegmentListPair.Key);
                    }
                   }
                   //netVoxelArray.voxels.SetDirty(true);
                   totalLengthOfDataSent+=cnkArraySync.terrainGetFileEditDataToNetSyncBG.voxels[segment].Length;
                   delayToSendNewMessages+=cnkArraySync.terrainGetFileEditDataToNetSyncBG.voxels[segment].Length*segmentSizeToTimeInSecondsDelayRatio;
                   if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
                    Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
                    yield return waitForDelayToSendNewMessages;
                   }
                  }
                 }
                 Log.Warning("to do: set cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[segment]=false");
                 foreach(var clientIdSegmentListPair in clientIdsToSendData){
                  HashSet<int>segmentList=clientIdSegmentListPair.Value;
                  segmentList.Clear();
                  clientIdsRequestingDataSegmentListPool.Enqueue(segmentList);
                 }
                 clientIdsToSendData.Clear();
                 clientsSegmentsMissing.Clear();
                 sending=false;
                }
                goto Loop;
            }
        }
    }
}