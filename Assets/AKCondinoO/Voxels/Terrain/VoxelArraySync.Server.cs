#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelArraySync{
        internal partial class ServerData{
            internal partial void OnInstantiated(){
             Log.DebugMessage("OnInstantiated");
            }
         [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
         [NonSerialized]internal int arraySyncSegment;
         [NonSerialized]internal readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
            internal void OnDequeue(VoxelTerrainChunkArraySync cnkArraySync,int arraySyncSegment){
             this.cnkArraySync=cnkArraySync;
             this.arraySyncSegment=arraySyncSegment;
             this.cnkArraySync.asServer.netVoxelArraysActive.Add(this.arraySyncSegment,netVoxelArray);
             VoxelSystem.singleton.asServer.netVoxelArraysActive.Add(netVoxelArray);
            }
            internal void OnSetChanges(int sendingcnkIdx){
             if(netVoxelArray.voxels.Value!=null){
              netVoxelArray.voxels.Value.cnkIdx=sendingcnkIdx;
              netVoxelArray.voxels.Value.segment=arraySyncSegment;
              Array.Copy(cnkArraySync.terrainGetFileEditDataToNetSyncBG.voxels[arraySyncSegment],0,netVoxelArray.voxels.Value.voxelArray,0,cnkArraySync.terrainGetFileEditDataToNetSyncBG.voxels[arraySyncSegment].Length);
             }
            }
            internal void OnPool(bool isDestroying=false){
             Log.DebugMessage("OnPool");
             VoxelSystem.singleton.asServer.netVoxelArraysActive.Remove(netVoxelArray);
             if(cnkArraySync!=null){
              cnkArraySync.asServer.netVoxelArraysActive.Remove(arraySyncSegment);
              cnkArraySync=null;
              VoxelSystem.singleton.asServer.netVoxelArraysPool.Enqueue(netVoxelArray);
             }
            }
         [NonSerialized]float timeToIgnoreClientIdsRequestingDataToPool=5f;
          [NonSerialized]float timerToIgnoreClientIdsRequestingDataToPool;
            internal partial void NetServerSideManualUpdate(HashSet<ulong>clientIdsDisconnectedToRemove,out bool toPool){
             toPool=false;
             if(cnkArraySync!=null){
              if(clientIdsDisconnectedToRemove.Count>0){
               clientIdsRequestingData.ExceptWith(clientIdsDisconnectedToRemove);
              }
              if(netVoxelArray.voxels.IsDirty()){
               Log.DebugMessage("'netVoxelArray.voxels.IsDirty()':'data wasn't sent yet'",netVoxelArray);
              }else{
               Log.DebugMessage("clientIdsRequestingData.Count:"+clientIdsRequestingData.Count);
               if(clientIdsRequestingData.Count<=0){
                toPool=true;
               }
              }
              if(!toPool){
               if(timerToIgnoreClientIdsRequestingDataToPool<timeToIgnoreClientIdsRequestingDataToPool){
                timerToIgnoreClientIdsRequestingDataToPool+=Time.deltaTime;
                if(timerToIgnoreClientIdsRequestingDataToPool>=timeToIgnoreClientIdsRequestingDataToPool){
                 toPool=true;
                }
               }
              }
              if(toPool){
               timerToIgnoreClientIdsRequestingDataToPool=0f;
               clientIdsRequestingData.Clear();
               Log.DebugMessage("'toPool'",netVoxelArray);
              }
              VoxelSystem.singleton.asServer.clientIdsRequestingNetVoxelArray.UnionWith(clientIdsRequestingData);
             }
            }
        }
    }
}