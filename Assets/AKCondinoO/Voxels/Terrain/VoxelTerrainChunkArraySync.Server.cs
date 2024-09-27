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
            internal partial void NetServerSideOnDestroyingCore(){
             Log.DebugMessage("NetServerSideOnDestroyingCore");
         //    if(this!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
         //     StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
         //    }
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
         [NonSerialized]readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
            internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
             Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:clientId:"+clientId+":'cnkIdx':"+id.Value.cnkIdx);
             clientIdsRequestingData.Add(clientId);
            }
         [NonSerialized]bool sending;
         [NonSerialized]bool hasReadyEditData;
         [NonSerialized]bool synchronizing;
         [NonSerialized]bool waitingGetFileEditData;
         [NonSerialized]bool pendingGetFileEditData;
            internal partial void NetServerSideManualUpdate(){
                if(cnkArraySync!=null&&cnkArraySync.netObj.IsSpawned){
                 if(synchronizing){
                      if(OnSynchronized()){
                          synchronizing=false;
                          hasReadyEditData=true;
                      }
                 }else{
                  if(waitingGetFileEditData){
                      //Log.DebugMessage("waitingGetFileEditData");
                      if(OnGotFileEditData()){
                          waitingGetFileEditData=false;
                          synchronizing=true;
                      }
                  }else{
                      if(pendingGetFileEditData){
                          //Log.DebugMessage("pendingGetFileEditData");
                          if(CanGetFileEditData()){
                              pendingGetFileEditData=false;
                              waitingGetFileEditData=true;
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
              Log.DebugMessage("OnGotFileEditData");
              bool sync=false;
              for(int i=0;i<cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes.Length;++i){
               if(cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i]||(cnkArraySync.netChunkHasChanges[i]!=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i])){
                sync=true;
                cnkArraySync.netChunkHasChanges[i]=cnkArraySync.terrainGetFileEditDataToNetSyncBG.changes[i];
               }
              }
              if(sync){
               Log.DebugMessage("'send==true'");
               cnkArraySync.netChunkHasChanges.SetDirty(true);
              }else{
               Log.DebugMessage("'send==false'");
               cnkArraySync.netChunkHasChanges.ResetDirty();
              }
              return true;
             }
             return false;
            }
            bool OnSynchronized(){
             if(!sending&&cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
              if(!cnkArraySync.netChunkHasChanges.IsDirty()){
               Log.DebugMessage("'!cnkArraySync.netChunkHasChanges.IsDirty()'");
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
         //      //sending=terrainGetFileEditDataToNetSyncBG.changes.Any(c=>{return c;});
         //      //sendingcnkIdx=terrainGetFileEditDataToNetSyncBG.cnkIdx;
         //      //clientIdsToSendData.AddRange(clientIdsRequestingData);
         //      //clientIdsRequestingData.Clear();
         //      //Log.DebugMessage("clientIdsToSendData.Count:"+clientIdsToSendData.Count);
              }else{
               //Log.DebugMessage("'no clients requesting data':clientIdsRequestingData.Count:"+clientIdsRequestingData.Count);
              }
             }
            }
            internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
                WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
                Loop:{
                 yield return waitUntilGetFileData;
                 //
                 sending=false;
                }
                goto Loop;
            }
        }
     // [NonSerialized]internal readonly Dictionary<int,VoxelArraySync>netVoxelArrays=new Dictionary<int,VoxelArraySync>();
     //[NonSerialized]Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
     // [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=1.25f/32f;
     //  [NonSerialized]internal float delayToSendNewMessages;//  writer.Length * segmentSizeToTimeInSecondsDelayRatio
     // [NonSerialized]bool sending;
     //  [NonSerialized]int sendingcnkIdx;
     //  [NonSerialized]readonly List<ulong>clientIdsToSendData=new List<ulong>();
     //   internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
     //    yield return null;
     //    WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
     //    WaitUntil waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});//  delay with WaitUntil and a cooldown
     //       bool LimitMessagesSentPerFrame(){
     //        if(messagesSent>=maxMessagesPerFrame){
     //         return true;
     //        }
     //        messagesSent++;
     //        return false;
     //       }
     //       System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
     //       bool LimitExecutionTime(){
     //        sendingExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
     //        if(sendingExecutionTime>=sendingMaxExecutionTime){
     //         return true;
     //        }
     //        return false;
     //       }
     //       Loop:{
     //        yield return waitUntilGetFileData;
     //        stopwatch.Restart();
     //        Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
     //        //for(int i=0;i<terrainGetFileEditDataToNetSyncBG.changes.Length;++i){
     //        // bool changed=terrainGetFileEditDataToNetSyncBG.changes[i];
     //        // terrainGetFileEditDataToNetSyncBG.changes[i]=false;
     //        // while(LimitExecutionTime()){
     //        //  yield return null;
     //        //  stopwatch.Restart();
     //        // }
     //        // while(LimitMessagesSentPerFrame()){
     //        //  yield return null;
     //        // }
     //        // if(changed){
     //        //  if(!netVoxelArrays.TryGetValue(i,out VoxelArraySync netVoxelArray)){
     //        //   _Dequeue:{}
     //        //   if(!VoxelSystem.singleton.netVoxelArraysPool.TryDequeue(out netVoxelArray)){
     //        //    if(VoxelSystem.singleton.netVoxelArraysCount>=VoxelSystem.singleton.netVoxelArraysMaxCount){
     //        //     //Log.DebugMessage("'VoxelSystem.singleton.netVoxelArraysCount>=VoxelSystem.singleton.netVoxelArraysMaxCount'");
     //        //     yield return null;
     //        //     goto _Dequeue;
     //        //    }
     //        //    VoxelSystem.singleton.netVoxelArraysCount++;
     //        //    netVoxelArray=Instantiate(_VoxelArraySyncPrefab);
     //        //    netVoxelArray.name=nameof(VoxelArraySync)+VoxelSystem.singleton.netVoxelArraysCount;
     //        //    netVoxelArray.OnInstantiated();
     //        //    try{
     //        //     netVoxelArray.netObj.Spawn(destroyWithScene:false);
     //        //    }catch(Exception e){
     //        //     Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
     //        //    }
     //        //    netVoxelArray.netObj.DontDestroyWithOwner=true;
     //        //   }
     //        //   netVoxelArrays.Add(i,netVoxelArray);
     //        //   netVoxelArray.arraySync=this;
     //        //   netVoxelArray.arraySyncSegment=i;
     //        //   VoxelSystem.singleton.netVoxelArraysActive.Add(netVoxelArray);
     //        //  }
     //        //  if(netVoxelArray.voxels.Value!=null){
     //        //   netVoxelArray.voxels.Value.cnkIdx=sendingcnkIdx;
     //        //   netVoxelArray.voxels.Value.segment=i;
     //        //   Array.Copy(terrainGetFileEditDataToNetSyncBG.voxels[i],0,netVoxelArray.voxels.Value.voxelArray,0,terrainGetFileEditDataToNetSyncBG.voxels[i].Length);
     //        //  }
     //        //  netVoxelArray.clientIdsRequestingData.UnionWith(clientIdsToSendData);
     //        //  VoxelSystem.singleton.clientIdsRequestingNetVoxelArray.UnionWith(clientIdsToSendData);
     //        //  //netVoxelArray.voxels.SetDirty(true);
     //        //  //Log.DebugMessage("'netVoxelArray.voxels.SetDirty(true)'",netVoxelArray);
     //        //  totalLengthOfDataSent+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length;
     //        //  delayToSendNewMessages+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length*segmentSizeToTimeInSecondsDelayRatio;
     //        //  if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
     //        //   //Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
     //        //   yield return waitForDelayToSendNewMessages;
     //        //  }
     //        // }
     //        //}
     //        clientIdsToSendData.Clear();
     //        sending=false;
     //       }
     //       goto Loop;
     //   }
    }
}