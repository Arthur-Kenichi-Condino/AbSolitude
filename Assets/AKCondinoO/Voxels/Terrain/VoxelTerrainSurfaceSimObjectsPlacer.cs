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
                        if(tryingReserveBounds){
                            if(surfaceSimObjectsPlacerBG.IsCompleted(VoxelSystem.singleton.surfaceSimObjectsPlacerBGThreads[0].IsRunning)){
                                tryingReserveBounds=false;
                                Log.DebugMessage("tryingReserveBounds=false;");
                                Array.Copy(surfaceSimObjectsPlacerBG.testArray,testArray,surfaceSimObjectsPlacerBG.testArray.Length);
                                testArrayReady=true;
                                surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.FillSpawnData;
                                fillingSpawnData=true;
                                //Log.DebugMessage("fillingSpawnData=true;");
                                VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Schedule(surfaceSimObjectsPlacerBG);
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
                                    surfaceSimObjectsPlacerBG.execution=VoxelTerrainSurfaceSimObjectsPlacerContainer.Execution.ReserveBounds;
                                    tryingReserveBounds=true;
                                    Log.DebugMessage("tryingReserveBounds=true;");
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
                                    surfaceSimObjectsPlacerBG.cCoord=simObjectsPlacing.cnk.id.Value.cCoord;
                                    surfaceSimObjectsPlacerBG.cnkRgn=simObjectsPlacing.cnk.id.Value.cnkRgn;
                                    surfaceSimObjectsPlacerBG.cnkIdx=simObjectsPlacing.cnk.id.Value.cnkIdx;
                                    surfaceSimObjectsPlacerBG.surfaceSimObjectsHadBeenAdded=false;
                                    //surfaceSimObjectsPlacerBG.maxSpawnSize=BaseBiomeSimObjectsSpawnSettings.maxSpawnSize;
                                    //surfaceSimObjectsPlacerBG.margin      =BaseBiomeSimObjectsSpawnSettings.margin      ;
                                    surfaceSimObjectsPlacerBG.GetGroundRays.Clear();
                                    surfaceSimObjectsPlacerBG.GetGroundHits.Clear();
                                    surfaceSimObjectsPlacerBG.gotGroundHits.Clear();
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
        bool testArrayReady=false;
        readonly (Color color,Bounds bounds,Vector3 scale)[]testArray=new(Color,Bounds,Vector3)[FlattenOffset];
        //[SerializeField]int seqSize1=3;
        //[SerializeField]int seqSize2=3;
        internal void OnDrawGizmos(){
         if(simObjectsPlacing.cnk.id!=null){
          Vector2Int cnkRgn=simObjectsPlacing.cnk.id.Value.cnkRgn;
          if((true||(cnkRgn.x==0&&cnkRgn.y==0))&&!(cnkRgn.x==16&&cnkRgn.y==16)&&!(cnkRgn.x==-16&&cnkRgn.y==-16)){
           if(true&&testArrayReady){
            Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
            for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
            for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
             int index1=vCoord1.z+vCoord1.x*Depth;
             Vector3 pos1=vCoord1;
             pos1.x+=cnkRgn.x;
             pos1.z+=cnkRgn.y;
             Gizmos.color=testArray[index1].color;
             //if(Gizmos.color==Color.cyan&&vCoord1.z==0){
              Gizmos.DrawWireCube(pos1,Vector3.Scale(testArray[index1].bounds.size,testArray[index1].scale));
             //}else{
              //Gizmos.DrawCube(pos1,Vector3.one/2f);
             //}
            }}
           }
           //if(testArray==null){
           // testArray=new SpawnMapInfo[FlattenOffset];
           // Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           // for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           // for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           //  int testArrayIdx=vCoord1.z+vCoord1.x*Depth;
           //  testArray[testArrayIdx]=new SpawnMapInfo{
           //   isBlocked=false,
           //  };
           // }}
           //}
           if(false){
            Vector3Int maxSpawnSize=new Vector3Int(7,256,7);
            Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
            for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
            for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
             Vector3 pos1=vCoord1;
             pos1.x+=cnkRgn.x;
             pos1.z+=cnkRgn.y;
             Vector3 margin1=Vector3.zero;
             Vector3 size1=new Vector3(7,1,7);
             //  [scale1 here]
             Vector3Int extent1=new Vector3Int(
              Mathf.CeilToInt(size1.x/2f)+Mathf.CeilToInt(margin1.x),
              0,
              Mathf.CeilToInt(size1.z/2f)+Mathf.CeilToInt(margin1.z)
             );
             int priority1=0;
             int seqResult1a=MathUtil.AlternatingSequenceWithSeparator((int)pos1.x,extent1.x+3,0);
             bool priorityOverWest1 =seqResult1a==0;
             bool priorityOverEast1 =seqResult1a==1;
             bool priorityOverBothX1=seqResult1a==2;
             bool canSpawnInX2=true;
             bool canSpawnInZ2=true;
             bool unknownInX2=false;
             bool unknownInZ2=false;
             Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
             for(coord2.x=-extent1.x+1;coord2.x<=extent1.x-1;coord2.x++){
             for(coord2.z=-extent1.z+1;coord2.z<=extent1.z-1;coord2.z++){
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
              Vector3 margin2=Vector3.zero;
              Vector3 size2=new Vector3(7,1,7);
              //  [scale2 here]
              Vector3Int extent2=new Vector3Int(
               Mathf.CeilToInt(size2.x/2f)+Mathf.CeilToInt(margin2.x),
               0,
               Mathf.CeilToInt(size2.z/2f)+Mathf.CeilToInt(margin2.z)
              );
              int priority2=0;
              int seqResult2a=MathUtil.AlternatingSequenceWithSeparator((int)pos2.x,extent2.x+3,0);
              bool priorityOverWest2 =seqResult2a==0;
              bool priorityOverEast2 =seqResult2a==1;
              bool priorityOverBothX2=seqResult2a==2;
              if(coord2.x!=0){
               if(priorityOverBothX2){
                if(priorityOverBothX1){
                 //  [empate: testar prioridade1 e prioridade2]
                 //unknownInX2=true;
                 if(priority2>priority1){
                  canSpawnInX2=false;
                  goto _End2;
                 }else{
                  if(size2.x>=size1.x){
                   canSpawnInX2=false;
                   goto _End2;
                  }
                 }
                }else{
                 canSpawnInX2=false;
                 goto _End2;
                }
               }else{
                if(coord2.x>0){
                 if(priorityOverWest2){
                  if(priorityOverWest1){
                   canSpawnInX2=false;
                   goto _End2;
                  }
                 }
                }else{
                 if(priorityOverEast2){
                  if(priorityOverEast1){
                   canSpawnInX2=false;
                   goto _End2;
                  }
                 }
                }
               }
              }
             }}
             _End2:{}
             if(unknownInX2||unknownInZ2){
              Gizmos.color=Color.yellow;
             }else{
              if(canSpawnInX2&&canSpawnInZ2){
               Vector3Int extentMax=new Vector3Int(
                Mathf.CeilToInt(maxSpawnSize.x/2f)+1,
                0,
                Mathf.CeilToInt(maxSpawnSize.z/2f)+1
               );
               bool canSpawnInX3=true;
               bool canSpawnInZ3=true;
               bool unknownInX3=false;
               bool unknownInZ3=false;
               foreach(var coord3 in MathUtil.GetCoords(
                minX:-extent1.x+1-extentMax.x+1,maxX:extent1.x-1+extentMax.x-1,
                minZ:-extent1.z+1-extentMax.z+1,maxZ:extent1.z-1+extentMax.z-1,
                innerMinX:-extent1.x+1,innerMaxX:extent1.x-1,
                innerMinZ:-extent1.z+1,innerMaxZ:extent1.z-1
               )){
                if(
                coord3.x==0&&
                coord3.z==0
                ){
                 continue;
                }
                //Log.DebugMessage("coord3:"+coord3);
                Vector3 pos3=pos1;
                pos3.x+=coord3.x;
                pos3.z+=coord3.z;
                Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
                Vector3 margin3=Vector3.zero;
                Vector3 size3=new Vector3(7,1,7);
                //  [scale3 here]
                Vector3Int extent3=new Vector3Int(
                 Mathf.CeilToInt(size3.x/2f)+Mathf.CeilToInt(margin3.x),
                 0,
                 Mathf.CeilToInt(size3.z/2f)+Mathf.CeilToInt(margin3.z)
                );
                int priority3=0;
                int seqResult3a=MathUtil.AlternatingSequenceWithSeparator((int)pos3.x,extent3.x+3,0);
                bool priorityOverWest3 =seqResult3a==0;
                bool priorityOverEast3 =seqResult3a==1;
                bool priorityOverBothX3=seqResult3a==2;
                if(coord3.x!=0){
                 //  [bounds check:]
                 if(
                  (pos3.x+extent3.x)>=(pos1.x-extent1.x)||
                  (pos3.x-extent3.x)<=(pos1.x+extent1.x)
                 ){
                  if(priorityOverBothX3){
                   if(priorityOverBothX1){
                    //  [empate: testar prioridade1 e prioridade3]
                    //unknownInX3=true;
                    if(priority3>priority1){
                     canSpawnInX3=false;
                     goto _End3;
                    }{
                     if(size3.x>=size1.x){
                      canSpawnInX3=false;
                      goto _End3;
                     }
                    }
                   }else{
                    canSpawnInX3=false;
                    goto _End3;
                   }
                  }else{
                   if(coord3.x>0){
                    if(priorityOverWest3){
                     if(priorityOverWest1){
                      canSpawnInX3=false;
                      goto _End3;
                     }
                    }
                   }else{
                    if(priorityOverEast3){
                     if(priorityOverEast1){
                      canSpawnInX3=false;
                      goto _End3;
                     }
                    }
                   }
                  }
                 }
                }
               }
               _End3:{}
               if(unknownInX3||unknownInZ3){
                Gizmos.color=Color.yellow;
               }else{
                if(canSpawnInX3&&canSpawnInZ3){
                 Gizmos.color=Color.green;
                }else{
                 Gizmos.color=Color.white;
                }
               }
              }else{
               Gizmos.color=Color.gray;
              }
             }
             Gizmos.DrawCube(pos1,Vector3.one/2f);
            }}
             //Gizmos.color=Color.gray;
             //int seqResult1a=MathUtil.AlternatingSequenceWithSeparator((int)pos1.x,20,0);
             //bool priorityOverWest1 =seqResult1a==0;
             //bool priorityOverBothX1=seqResult1a==2;
             //Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
             //for(coord2.x=-2             ;coord2.x<=2;coord2.x++){
             //for(coord2.z=-2             ;coord2.z<=2;coord2.z++){
             // if(
             //  coord2.x==0&&
             //  coord2.z==0
             // ){
             //  continue;
             // }
             // Vector3 pos2=pos1;
             // pos2.x+=coord2.x;
             // pos2.z+=coord2.z;
             // Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
             // //if(coord2.x>0){
             //  if(priorityOverBothX1){
             //   if(!priorityOverBothX2){
             //    Log.DebugMessage("!priorityOverBothX2:coord2:"+coord2);
             //    //canSpawnInX2=true;
             // //Gizmos.color=Color.green;
             //   }
             //   if(priorityOverBothX2){
             //    //canSpawnInX2=false;
             //    Log.DebugMessage("priorityOverBothX2:coord2:"+coord2);
             // Gizmos.color=Color.red;
             //    //goto _End2;
             //    //canSpawnInX2=true;
             //   }
             //  }
             // //}
             // //if(coord2.x<0){
             // // if(!priorityOverWest1&&!priorityOverBothX1){
             // //  canSpawnInX2=false;
             // //  goto _End2;
             // // }
             // //}
             // //if(coord2.x>0){
             // // if(priorityOverWest2){
             // //  canSpawnInX2=false;
             // //  goto _End2;
             // // }
             // //}
             //}}
             //_End2:{}
             //if(canSpawnInX2){
             //}
             // 
             //}else if(seqResult1a==2){
             // Gizmos.color=Color.gray;
             //}else{
             // Gizmos.color=Color.red;
             //int testArrayIdx=vCoord1.z+vCoord1.x*Depth;
             //SpawnMapInfo info=testArray[testArrayIdx];
             //Vector3 pos1=vCoord1;
             //pos1.x+=cnkRgn.x;
             //pos1.z+=cnkRgn.y;
             //if(pos1.x==0){Log.DebugMessage("AlternatingSequence:"+seqResult1a);}
             //if(pos1.x==1){Log.DebugMessage("AlternatingSequence:"+seqResult1a);}
             //if(pos1.x==2){Log.DebugMessage("AlternatingSequence:"+seqResult1a);}
             //bool priorityOverEast1 =seqResult1a==0;
             //bool priorityOverWest1 =seqResult1a==1;
             //bool priorityOverBothX1=seqResult1a==2;
             //int seqResult1b=MathUtil.AlternatingSequenceWithSeparator((int)pos1.z,BaseBiomeSimObjectsSpawnSettings.maxSpawnSize.z,0);
             //bool priorityOverNorth1=seqResult1b==0;
             //bool priorityOverSouth1=seqResult1b==1;
             //bool priorityOverBothZ1=seqResult1b==2;
             ////Gizmos.color=Color.gray;
             ////if(info.isBlocked){
             //bool canSpawnInX2=false;
             //bool canSpawnInZ2=false;
             //Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
             //for(coord2.x=-2             ;coord2.x<=2;coord2.x++){
             //for(coord2.z=-2             ;coord2.z<=2;coord2.z++){
             // if(
             //  coord2.x==0&&
             //  coord2.z==0
             // ){
             //  continue;
             // }
             // Vector3 pos2=pos1;
             // pos2.x+=coord2.x;
             // pos2.z+=coord2.z;
             // Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
             // int seqResult2a=MathUtil.AlternatingSequenceWithSeparator((int)pos2.x,BaseBiomeSimObjectsSpawnSettings.maxSpawnSize.x,0);
             // bool priorityOverEast2 =seqResult2a==0;
             // bool priorityOverWest2 =seqResult2a==1;
             // bool priorityOverBothX2=seqResult2a==1;
             // int seqResult2b=MathUtil.AlternatingSequenceWithSeparator((int)pos2.z,BaseBiomeSimObjectsSpawnSettings.maxSpawnSize.z,0);
             // bool priorityOverNorth2=seqResult2b==0;
             // bool priorityOverSouth2=seqResult2b==1;
             // bool priorityOverBothZ2=seqResult2b==1;
             // //  spawn?
             // if(!priorityOverBothX2){
             //  if(coord2.x<0){
             //   if(priorityOverWest1){
             //    if(priorityOverWest2){
             //     canSpawnInX2=true;
             //    }
             //   }
             //  }
             //  if(coord2.x>0){
             //   if(priorityOverWest2){
             //    canSpawnInX2=false;
             //    goto _End2;
             //   }
             //  }
             // }
             // if(!priorityOverBothZ2){
             //  if(coord2.z<0){
             //   if(priorityOverSouth1){
             //    if(priorityOverSouth2){
             //     if(canSpawnInX2){
             //      canSpawnInZ2=true;
             //     }
             //    }
             //   }
             //  }
             //  if(coord2.z>0){
             //   if(priorityOverSouth2){
             //    canSpawnInZ2=false;
             //    goto _End2;
             //   }
             //  }
             // }
             //}}
             //_End2:{}
             ////Gizmos.color=Color.gray;
             //if(canSpawnInX2&&canSpawnInZ2){
             // bool canSpawnInX3=false;
             // bool canSpawnInZ3=false;
             // foreach(var coord3 in MathUtil.GetCoords(
             //  minX:-3,maxX:3,
             //  minZ:-3,maxZ:3,
             //  innerMinX:-2,innerMaxX:2,
             //  innerMinZ:-2,innerMaxZ:2
             // )){
             //  //Log.DebugMessage("check coord:"+coord3);
             //  Vector3 pos3=pos1;
             //  pos3.x+=coord3.x;
             //  pos3.z+=coord3.z;
             //  Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
             //  int seqResult3a=MathUtil.AlternatingSequenceWithSeparator((int)pos3.x,BaseBiomeSimObjectsSpawnSettings.maxSpawnSize.x,0);
             //  bool priorityOverEast3 =seqResult3a==0;
             //  bool priorityOverWest3 =seqResult3a==1;
             //  bool priorityOverBothX3=seqResult3a==1;
             //  int seqResult3b=MathUtil.AlternatingSequenceWithSeparator((int)pos3.z,BaseBiomeSimObjectsSpawnSettings.maxSpawnSize.z,0);
             //  bool priorityOverNorth3=seqResult3b==0;
             //  bool priorityOverSouth3=seqResult3b==1;
             //  bool priorityOverBothZ3=seqResult3b==1;
             //  //  check bounds
             //  //  spawn?
             //  if(!priorityOverBothX3){
             //   if(coord3.x<0){
             //    if(priorityOverWest1){
             //     if(priorityOverWest3){
             //      canSpawnInX3=true;
             //     }
             //    }
             //   }
             //   if(coord3.x>0){
             //    if(priorityOverWest3){
             //     canSpawnInX3=false;
             //     goto _End3;
             //    }
             //   }
             //  }
             //  if(!priorityOverBothZ3){
             //   if(coord3.z<0){
             //    if(priorityOverSouth1){
             //     if(priorityOverSouth3){
             //      if(canSpawnInX3){
             //       canSpawnInZ3=true;
             //      }
             //     }
             //    }
             //   }
             //   if(coord3.z>0){
             //    if(priorityOverSouth3){
             //     canSpawnInZ3=false;
             //     goto _End3;
             //    }
             //   }
             //  }
             // }
             // _End3:{}
             // if(canSpawnInX3&&canSpawnInZ3){
             //  //Gizmos.color=Color.green;
             // }
             //}
           }
          }
         }
        }
    }
}