#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
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
     bool spawning;
     bool fillingSpawnData;
     bool doingRaycasts;
     internal JobHandle doRaycastsHandle{get;private set;}
     bool settingGetGroundRays;
        internal void OnAddingSurfaceSimObjects(){
            if(spawning){
                if(surfaceSimObjectsPlacerBG.spawnData.dequeued){
                    spawning=false;
                    Log.DebugMessage("spawning=false;");
                    isBusy=false;
                }
            }else{
                if(fillingSpawnData){
                    if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                        fillingSpawnData=false;
                        Log.DebugMessage("fillingSpawnData=false;");
                        surfaceSimObjectsPlacerBG.spawnData.dequeued=false;
                        SimObjectSpawner.singleton.spawnQueue.Enqueue(surfaceSimObjectsPlacerBG.spawnData);
                        spawning=true;
                        Log.DebugMessage("spawning=true;");
                    }
																}else{
																				if(doingRaycasts){
																								if(doRaycastsHandle.IsCompleted){
																												doingRaycasts=false;
																												Log.DebugMessage("doingRaycasts=false;");
																												doRaycastsHandle.Complete();
																												Vector3Int vCoord1=new Vector3Int(0,0,0);
																												int i=0;
																												for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
																												for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
																													RaycastHit hit=surfaceSimObjectsPlacerBG.GetGroundHits[i++];
																													int index=vCoord1.z+vCoord1.x*Depth;
																													if(hit.collider!=null){
																														surfaceSimObjectsPlacerBG.gotGroundHits[index]=hit;
																																	#if UNITY_EDITOR
																																					Debug.DrawRay(
																																						surfaceSimObjectsPlacerBG.GetGroundHits[i-1].point,
																																						(surfaceSimObjectsPlacerBG.GetGroundRays[i-1].from-surfaceSimObjectsPlacerBG.GetGroundHits[i-1].point).normalized,
																																						Color.gray,
																																						5f
																																					);
																																	#endif
																													}else{
																														surfaceSimObjectsPlacerBG.gotGroundHits[index]=null;
																													}
																												}}
																												surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.FillSpawnData;
																												fillingSpawnData=true;
																												Log.DebugMessage("fillingSpawnData=true;");
																												VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
																								}
																				}else{
																								if(settingGetGroundRays){
																												if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
																																settingGetGroundRays=false;
																																Log.DebugMessage("settingGetGroundRays=false;");
																																doRaycastsHandle=RaycastCommand.ScheduleBatch(surfaceSimObjectsPlacerBG.GetGroundRays,surfaceSimObjectsPlacerBG.GetGroundHits,surfaceSimObjectsPlacerBG.GetGroundRays.Length/Environment.ProcessorCount,default(JobHandle));
																																doingRaycasts=true;
																																Log.DebugMessage("doingRaycasts=true;");
																												}
																								}else{
																												surfaceSimObjectsPlacerBG.GetGroundRays.Clear();
																												surfaceSimObjectsPlacerBG.GetGroundHits.Clear();
																												surfaceSimObjectsPlacerBG.gotGroundHits.Clear();
																												Array.Clear(surfaceSimObjectsPlacerBG.blocked,0,surfaceSimObjectsPlacerBG.blocked.Length);
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
        }
    }
}
