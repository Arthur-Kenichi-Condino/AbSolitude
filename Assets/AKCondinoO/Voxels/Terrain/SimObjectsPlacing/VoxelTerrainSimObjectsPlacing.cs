#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal class VoxelTerrainSimObjectsPlacing{
     internal readonly VoxelTerrainChunk cnk;
     internal readonly VoxelTerrainSurfaceSimObjectsPlacer surface;
        internal VoxelTerrainSimObjectsPlacing(VoxelTerrainChunk cnk){
         this.cnk=cnk;
         surface=new VoxelTerrainSurfaceSimObjectsPlacer(this);
        }
     internal bool isBusy{
      get{
       return surface.isBusy;
      }
      private set{
       surface.isBusy=true;
      }
     }
        internal void OnVoxelTerrainReady(){
         isBusy=true;
        }
        internal void AddingSimObjectsSubroutine(){
         surface.OnAddingSurfaceSimObjects();
        }
    }
}