#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
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
     bool savingStateToFile;
     bool spawning;
     bool fillingSpawnData;
     bool doingRaycasts;
      internal JobHandle doRaycastsHandle{get;private set;}
     bool settingGetGroundRays;
     bool tryingReserveBounds;
        internal void OnAddingSurfaceSimObjects(){
            if(savingStateToFile){
                if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                    savingStateToFile=false;
                    Log.DebugMessage("savingStateToFile=false;");
                    OnPlacingFinished();
                }
            }else{
                if(spawning){
                    if(surfaceSimObjectsPlacerBG.spawnData.dequeued){
                        spawning=false;
                        //Log.DebugMessage("spawning=false;");
                        OnSaveStateToFile();
                    }
                }else{
                    if(fillingSpawnData){
                        if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                            fillingSpawnData=false;
                            //Log.DebugMessage("fillingSpawnData=false;");
                            surfaceSimObjectsPlacerBG.spawnData.dequeued=false;
                            SimObjectSpawner.singleton.spawnQueue.Enqueue(surfaceSimObjectsPlacerBG.spawnData);
                            spawning=true;
                            //Log.DebugMessage("spawning=true;");
                        }
                    }else{
                        if(doingRaycasts){
                            if(doRaycastsHandle.IsCompleted){
                                doingRaycasts=false;
                                //Log.DebugMessage("doingRaycasts=false;");
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
                                         //Debug.DrawRay(
                                         // surfaceSimObjectsPlacerBG.GetGroundHits[i-1].point,
                                         // (surfaceSimObjectsPlacerBG.GetGroundRays[i-1].from-surfaceSimObjectsPlacerBG.GetGroundHits[i-1].point).normalized,
                                         // Color.gray,
                                         // 5f
                                         //);
                                     #endif
                                 }else{
                                  surfaceSimObjectsPlacerBG.gotGroundHits[index]=null;
                                 }
                                }}
                                surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.FillSpawnData;
                                fillingSpawnData=true;
                                //Log.DebugMessage("fillingSpawnData=true;");
                                VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
                            }
                        }else{
                            if(settingGetGroundRays){
                                if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                                    settingGetGroundRays=false;
                                    //Log.DebugMessage("settingGetGroundRays=false;");
                                    if(surfaceSimObjectsPlacerBG.surfaceSimObjectsHadBeenAdded){
                                        OnPlacingFinished();
                                    }else{
                                        if(surfaceSimObjectsPlacerBG.GetGroundRays.Length<=0){
                                            OnSaveStateToFile();
                                        }else{
                                            doRaycastsHandle=RaycastCommand.ScheduleBatch(surfaceSimObjectsPlacerBG.GetGroundRays.AsArray(),surfaceSimObjectsPlacerBG.GetGroundHits.AsArray(),surfaceSimObjectsPlacerBG.GetGroundRays.Length/Environment.ProcessorCount,default(JobHandle));
                                            doingRaycasts=true;
                                            //Log.DebugMessage("doingRaycasts=true;");
                                        }
                                    }
                                }
                            }else{
                                if(tryingReserveBounds){
                                    if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                                        tryingReserveBounds=false;
                                        Log.DebugMessage("tryingReserveBounds=false;");
                                        surfaceSimObjectsPlacerBG.GetGroundRays.Clear();
                                        surfaceSimObjectsPlacerBG.GetGroundHits.Clear();
                                        surfaceSimObjectsPlacerBG.gotGroundHits.Clear();
                                        surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.GetGround;
                                        settingGetGroundRays=true;
                                        //Log.DebugMessage("settingGetGroundRays=true;");
                                        VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
                                    }
                                }else{
                                    surfaceSimObjectsPlacerBG.cCoord=simObjectsPlacing.cnk.id.Value.cCoord;
                                    surfaceSimObjectsPlacerBG.cnkRgn=simObjectsPlacing.cnk.id.Value.cnkRgn;
                                    surfaceSimObjectsPlacerBG.cnkIdx=simObjectsPlacing.cnk.id.Value.cnkIdx;
                                    surfaceSimObjectsPlacerBG.surfaceSimObjectsHadBeenAdded=false;
                                    surfaceSimObjectsPlacerBG.maxSpawnSize=BaseBiomeSimObjectsSpawnSettings.maxSpawnSize;
                                    surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.ReserveBounds;
                                    tryingReserveBounds=true;
                                    Log.DebugMessage("tryingReserveBounds=true;");
                                    VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
                                }
                            }
                        }
                    }
                }
            }
        }
        void OnSaveStateToFile(){
         surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.SaveStateToFile;
         savingStateToFile=true;
         //Log.DebugMessage("savingStateToFile=true;");
         VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
        }
        void OnPlacingFinished(){
         //Log.DebugMessage("OnPlacingFinished()");
         isBusy=false;
        }
        SpawnMapInfo[]testArray;
        internal void OnDrawGizmos(){
         if(simObjectsPlacing.cnk.id!=null){
          Vector2Int cnkRgn=simObjectsPlacing.cnk.id.Value.cnkRgn;
          if(false||cnkRgn.x==0&&cnkRgn.y==0){
           if(testArray==null){
            testArray=new SpawnMapInfo[FlattenOffset];
            Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
            for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
            for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
             int testArrayIdx=vCoord1.z+vCoord1.x*Depth;
             testArray[testArrayIdx]=new SpawnMapInfo{
              isBlocked=false,
             };
            }}
           }
           if(testArray!=null){
            Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
            for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
            for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
             int testArrayIdx=vCoord1.z+vCoord1.x*Depth;
             SpawnMapInfo info=testArray[testArrayIdx];
             Vector3 pos1=vCoord1;
             pos1.x+=cnkRgn.x;
             pos1.z+=cnkRgn.y;
             int seqResult1a=MathUtil.AlternatingSequenceWithSeparator((int)pos1.x,3,0);
             bool priorityOverEast1 =seqResult1a==0;
             bool priorityOverWest1 =seqResult1a==1;
             bool priorityOverBothX1=seqResult1a==2;
             int seqResult1b=MathUtil.AlternatingSequenceWithSeparator((int)pos1.z,3,0);
             bool priorityOverNorth1=seqResult1b==0;
             bool priorityOverSouth1=seqResult1b==1;
             bool priorityOverBothZ1=seqResult1b==2;
             Gizmos.color=Color.gray;
             //if(info.isBlocked){
             //if(priorityOverEast){
             // Gizmos.color=Color.green;
             //}else{
             // Gizmos.color=Color.red;
             //}
             bool canSpawnInX2=false;
             bool canSpawnInZ2=false;
             Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
             for(coord2.x=-2             ;coord2.x<=2;coord2.x++){
             for(coord2.z=-2             ;coord2.z<=2;coord2.z++){
              if(
               coord2.x==0&&
               coord2.z==0
              ){
               continue;
              }
              Vector3 pos2=pos1;
              pos2.x+=coord2.x;
              pos2.z+=coord2.z;
              Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
              int seqResult2a=MathUtil.AlternatingSequenceWithSeparator((int)pos2.x,3,0);
              bool priorityOverEast2 =seqResult2a==0;
              bool priorityOverWest2 =seqResult2a==1;
              bool priorityOverBothX2=seqResult2a==2;
              int seqResult2b=MathUtil.AlternatingSequenceWithSeparator((int)pos2.z,3,0);
              bool priorityOverNorth2=seqResult2b==0;
              bool priorityOverSouth2=seqResult2b==1;
              bool priorityOverBothZ2=seqResult2b==2;
              //  spawn?
              if(!priorityOverBothX2){
               if(coord2.x<0){
                if(priorityOverWest1){
                 if(priorityOverWest2){
                  canSpawnInX2=true;
                 }
                }
               }
               if(coord2.x>0){
                if(priorityOverWest2){
                 canSpawnInX2=false;
                 goto _End2;
                }
               }
              }
              if(!priorityOverBothZ2){
               if(coord2.z<0){
                if(priorityOverSouth1){
                 if(priorityOverSouth2){
                  if(canSpawnInX2){
                   canSpawnInZ2=true;
                  }
                 }
                }
               }
               if(coord2.z>0){
                if(priorityOverSouth2){
                 canSpawnInZ2=false;
                 goto _End2;
                }
               }
              }
             }}
             _End2:{}
             Gizmos.color=Color.gray;
             if(canSpawnInX2&&canSpawnInZ2){
              bool canSpawnInX3=false;
              bool canSpawnInZ3=false;
              foreach(var coord3 in MathUtil.GetCoords(
               minX:-3,maxX:3,
               minZ:-3,maxZ:3,
               innerMinX:-2,innerMaxX:2,
               innerMinZ:-2,innerMaxZ:2
              )){
               //Log.DebugMessage("check coord:"+coord3);
               Vector3 pos3=pos1;
               pos3.x+=coord3.x;
               pos3.z+=coord3.z;
               Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
               int seqResult3a=MathUtil.AlternatingSequenceWithSeparator((int)pos3.x,3,0);
               bool priorityOverEast3 =seqResult3a==0;
               bool priorityOverWest3 =seqResult3a==1;
               bool priorityOverBothX3=seqResult3a==2;
               int seqResult3b=MathUtil.AlternatingSequenceWithSeparator((int)pos3.z,3,0);
               bool priorityOverNorth3=seqResult3b==0;
               bool priorityOverSouth3=seqResult3b==1;
               bool priorityOverBothZ3=seqResult3b==2;
               //  check bounds
               //  spawn?
               if(!priorityOverBothX3){
                if(coord3.x<0){
                 if(priorityOverWest1){
                  if(priorityOverWest3){
                   canSpawnInX3=true;
                  }
                 }
                }
                if(coord3.x>0){
                 if(priorityOverWest3){
                  canSpawnInX3=false;
                  goto _End3;
                 }
                }
               }
               if(!priorityOverBothZ3){
                if(coord3.z<0){
                 if(priorityOverSouth1){
                  if(priorityOverSouth3){
                   if(canSpawnInX3){
                    canSpawnInZ3=true;
                   }
                  }
                 }
                }
                if(coord3.z>0){
                 if(priorityOverSouth3){
                  canSpawnInZ3=false;
                  goto _End3;
                 }
                }
               }
              }
              _End3:{}
              if(canSpawnInX3&&canSpawnInZ3){
               Gizmos.color=Color.green;
              }
             }
             Gizmos.DrawCube(pos1,Vector3.one/2f);
            }}
           }
          }
         }
        }
    }
}