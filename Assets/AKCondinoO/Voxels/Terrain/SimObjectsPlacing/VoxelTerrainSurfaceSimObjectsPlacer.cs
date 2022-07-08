#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal class VoxelTerrainSurfaceSimObjectsPlacer{
     readonly VoxelTerrainSimObjectsPlacing simObjectsPlacing;
     internal VoxelTerrainSurfaceSimObjectsPlacerContainer surfaceSimObjectsPlacerBG=new VoxelTerrainSurfaceSimObjectsPlacerContainer();
        internal VoxelTerrainSurfaceSimObjectsPlacer(VoxelTerrainSimObjectsPlacing simObjectsPlacing){
         this.simObjectsPlacing=simObjectsPlacing;
        }
     internal bool isBusy;
     internal JobHandle doRaycastsHandle{get;private set;}
     bool settingGetGroundRays;
        internal void OnAddingSurfaceSimObjects(){
            if(settingGetGroundRays){
                if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                    settingGetGroundRays=false;
                    Log.DebugMessage("settingGetGroundRays=false;");
                    doRaycastsHandle=RaycastCommand.ScheduleBatch(surfaceSimObjectsPlacerBG.GetGroundRays,surfaceSimObjectsPlacerBG.GetGroundHits,16,default(JobHandle));
                    //if(){
                    //}
                    isBusy=false;
                }
            }else{
                surfaceSimObjectsPlacerBG.GetGroundRays.Clear();
                surfaceSimObjectsPlacerBG.GetGroundHits.Clear();
                surfaceSimObjectsPlacerBG.gotGroundHits.Clear();
                surfaceSimObjectsPlacerBG.cCoord=simObjectsPlacing.cnk.id.Value.cCoord;
                surfaceSimObjectsPlacerBG.cnkRgn=simObjectsPlacing.cnk.id.Value.cnkRgn;
                surfaceSimObjectsPlacerBG.cnkIdx=simObjectsPlacing.cnk.id.Value.cnkIdx;
                surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.GetGround;
                settingGetGroundRays=true;
                Log.DebugMessage("settingGetGroundRays=true;");
                VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
            }
        }
    }
}
