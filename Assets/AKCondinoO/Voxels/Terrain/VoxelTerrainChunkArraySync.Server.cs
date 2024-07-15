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
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          pendingGetFileEditData=DEBUG_FORCE_SEND_ALL_VOXEL_DATA;
         }
        }
     [NonSerialized]readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
        internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
         Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:'cnkIdx':"+id.Value.cnkIdx);
         clientIdsRequestingData.Add(clientId);
         pendingGetFileEditData=true;
        }
     [SerializeField]bool DEBUG_FORCE_SEND_ALL_VOXEL_DATA=false;
     [NonSerialized]bool waitingGetFileEditData;
     [NonSerialized]bool pendingGetFileEditData;
        internal void NetServerSideManualUpdate(){
            if(cnkMsgr!=null&&cnkMsgr.netObj.IsSpawned){
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
        }
        bool CanGetFileEditData(){
         if(terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          terrainGetFileEditDataToNetSyncBG.DEBUG_FORCE_SEND_ALL_VOXEL_DATA=DEBUG_FORCE_SEND_ALL_VOXEL_DATA;
          terrainGetFileEditDataToNetSyncBG.cCoord=id.Value.cCoord;
          terrainGetFileEditDataToNetSyncBG.cnkRgn=id.Value.cnkRgn;
          terrainGetFileEditDataToNetSyncBG.cnkIdx=id.Value.cnkIdx;
          VoxelTerrainGetFileEditDataToNetSyncMultithreaded.Schedule(terrainGetFileEditDataToNetSyncBG);
          return true;
         }
         return false;
        }
        bool OnGotFileEditData(){
         if(!sending&&terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          //Log.DebugMessage("OnGotFileEditData");
          sending=terrainGetFileEditDataToNetSyncBG.changes.Any(c=>{return c;});
          sendingcnkIdx=terrainGetFileEditDataToNetSyncBG.cnkIdx;
          clientIdsToSendData.AddRange(clientIdsRequestingData);
          clientIdsRequestingData.Clear();
          return true;
         }
         return false;
        }
     [SerializeField]internal VoxelArraySync _VoxelArraySyncPrefab;
      [NonSerialized]internal readonly Dictionary<int,VoxelArraySync>netVoxelArrays=new Dictionary<int,VoxelArraySync>();
     [NonSerialized]Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
      [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=.05f;
       [NonSerialized]internal float delayToSendNewMessages;//  writer.Length * segmentSizeToTimeInSecondsDelayRatio
      [NonSerialized]bool sending;
       [NonSerialized]int sendingcnkIdx;
       [NonSerialized]readonly List<ulong>clientIdsToSendData=new List<ulong>();
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
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
             stopwatch.Restart();
             Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             //int destinationIndex=0;
             for(int i=0;i<terrainGetFileEditDataToNetSyncBG.changes.Length;++i){
              bool changed=terrainGetFileEditDataToNetSyncBG.changes[i];
              terrainGetFileEditDataToNetSyncBG.changes[i]=false;
              while(LimitExecutionTime()){
               yield return null;
               stopwatch.Restart();
              }
              while(LimitMessagesSentPerFrame()){
               yield return null;
              }
              if(changed){
               if(!netVoxelArrays.TryGetValue(i,out VoxelArraySync netVoxelArray)){
                if(!VoxelSystem.singleton.netVoxelArraysPool.TryDequeue(out netVoxelArray)){
                 netVoxelArray=Instantiate(_VoxelArraySyncPrefab);
                 netVoxelArray.OnInstantiated();
                 try{
                  netVoxelArray.netObj.Spawn(destroyWithScene:false);
                 }catch(Exception e){
                  Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                 }
                 netVoxelArray.netObj.DontDestroyWithOwner=true;
                }
                netVoxelArrays.Add(i,netVoxelArray);
                netVoxelArray.arraySync=this;
                netVoxelArray.arraySyncSegment=i;
                VoxelSystem.singleton.netVoxelArraysActive.Add(netVoxelArray);
               }
               netVoxelArray.voxels.Value.cnkIdx=sendingcnkIdx;
               netVoxelArray.voxels.Value.segment=i;
               Array.Copy(terrainGetFileEditDataToNetSyncBG.voxels[i],0,netVoxelArray.voxels.Value.voxelArray,0,terrainGetFileEditDataToNetSyncBG.voxels[i].Length);
               //netVoxelArray.voxels.Value=netVoxelArray.voxels.Value;
               netVoxelArray.clientIdsRequestingData.Union(clientIdsToSendData);
               VoxelSystem.singleton.clientIdsRequestingNetVoxelArray.Union(clientIdsToSendData);
               //Log.DebugMessage("netVoxelArray.voxels.IsDirty():"+netVoxelArray.voxels.IsDirty());
               //destinationIndex+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length;
               totalLengthOfDataSent+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length;
               delayToSendNewMessages+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length*segmentSizeToTimeInSecondsDelayRatio;
               if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
                Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
                yield return waitForDelayToSendNewMessages;
               }
              }
             }
             clientIdsToSendData.Clear();
             sending=false;
            }
            goto Loop;
        }
    }
}