using System;
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
            internal void OnDequeueAt(VoxelTerrainChunkArraySync cnkArraySync,int arraySyncSegment){
             this.cnkArraySync=cnkArraySync;
             this.arraySyncSegment=arraySyncSegment;
             cnkArraySync.asServer.netVoxelArraysActive.Add(arraySyncSegment,netVoxelArray);
            }
            internal void OnPool(){
             Log.DebugMessage("OnPool");
     //    if(arraySync!=null){
     //     arraySync.netVoxelArrays.Remove(arraySyncSegment);
     //     arraySync=null;
     //    }
     //    if(VoxelSystem.singleton.netVoxelArraysActive.Remove(this)){
     //     VoxelSystem.singleton.netVoxelArraysPool.Enqueue(this);
     //    }
            }
        }
    }
}