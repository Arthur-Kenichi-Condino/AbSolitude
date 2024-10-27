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
            internal void OnPool(){
             Log.DebugMessage("OnPool");
             VoxelSystem.singleton.asServer.netVoxelArraysActive.Remove(netVoxelArray);
             if(cnkArraySync!=null){
              cnkArraySync.asServer.netVoxelArraysActive.Remove(arraySyncSegment);
              cnkArraySync=null;
              VoxelSystem.singleton.asServer.netVoxelArraysPool.Enqueue(netVoxelArray);
             }
            }
            internal partial void NetServerSideManualUpdate(HashSet<ulong>clientIdsDisconnectedToRemove,out bool toPool){
             toPool=false;
            }
        }
    }
}