#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Voxels.Terrain.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        internal partial class ServerData{
            internal partial void NetServerSideInit(){
             Log.DebugMessage("NetServerSideInit");
             Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage+=OnServerReceivedUnnamedMessage;
             int poolSize=(
              (expropriationDistance.x*2+1)*
              (expropriationDistance.y*2+1)*
              1
             );
             netVoxelArraysMaxCount=poolSize;
             serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine=VoxelSystem.singleton.StartCoroutine(ServerSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine());
            }
            internal partial void NetServerSideOnDestroyingCoreNetworkDestroy(){
             if(Core.singleton.netManager.CustomMessagingManager!=null){
                Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage-=OnServerReceivedUnnamedMessage;
             }
             if(this!=null&&serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine!=null){
              VoxelSystem.singleton.StopCoroutine(serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine);
             }
             for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
              if(i<terrainMessageHandlers.Count){
               VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
               cnkMsgr     .asServer.NetServerSideOnDestroyingCore();
              }
              if(i<terrainArraySyncs     .Count){
               VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
               cnkArraySync.asServer.NetServerSideOnDestroyingCore();
              }
             }
            }
            internal partial void NetServerSideOnDestroyingCoreNetworkDispose(){
             for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
              if(i<terrainMessageHandlers.Count){
               VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
               cnkMsgr     .asServer.NetServerSideDispose();
              }
              if(i<terrainArraySyncs     .Count){
               VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
               cnkArraySync.asServer.NetServerSideDispose();
              }
             }
             terrainMessageHandlers.Clear();
             terrainArraySyncs     .Clear();
     //    foreach(var kvp1 in serverVoxelTerrainChunkEditDataSegmentsReceivedFromClient){
     //     foreach(var kvp2 in kvp1.Value){
     //      kvp2.Value.segmentData.Dispose();
     //     }
     //     kvp1.Value.Clear();
     //     serverVoxelTerrainChunkEditDataSegmentsDictionaryPool.Enqueue(kvp1.Value);
     //    }
     //    serverVoxelTerrainChunkEditDataSegmentsReceivedFromClient.Clear();
     //    //  everything at server has been disposed
            }
            internal partial void NetServerSideNetUpdate(){
            }
         [NonSerialized]Coroutine serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine;
          [NonSerialized]double assigningMaxExecutionTime=1.0;
           [NonSerialized]double assigningExecutionTime;
          [NonSerialized]readonly Dictionary<Gameplayer,Vector2Int>  assigningCoordinates=new Dictionary<Gameplayer,Vector2Int>();
          [NonSerialized]readonly Dictionary<Gameplayer,Vector2Int>deassigningCoordinates=new Dictionary<Gameplayer,Vector2Int>();
          [NonSerialized]readonly HashSet<Gameplayer>toAssignMessageHandlers=new HashSet<Gameplayer>();
            internal IEnumerator ServerSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine(){
             System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
                bool LimitExecutionTime(){
                 assigningExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
                 //Log.DebugMessage("stopwatch.Elapsed.TotalMilliseconds:"+stopwatch.Elapsed.TotalMilliseconds);
                 if(assigningExecutionTime>=assigningMaxExecutionTime){
                  return true;
                 }
                 return false;
                }
             stopwatch.Restart();
                Loop:{
                 yield return null;
                 stopwatch.Restart();
                 var generationRequestedAssignMessageHandlers=VoxelSystem.singleton.generationRequestedAssignMessageHandlers;
                 if(generationRequestedAssignMessageHandlers.Count>0){
                  foreach(var gameplayer in generationRequestedAssignMessageHandlers){
                   if(assigningCoordinates.TryGetValue(gameplayer,out Vector2Int cCoord_Previous)){
                    deassigningCoordinates[gameplayer]=cCoord_Previous;
                   }
                   assigningCoordinates[gameplayer]=gameplayer.cCoord;
                   toAssignMessageHandlers.Add(gameplayer);
                  }
                  generationRequestedAssignMessageHandlers.Clear();
                  foreach(var gameplayer in toAssignMessageHandlers){
                   if(deassigningCoordinates.TryGetValue(gameplayer,out Vector2Int cCoord_Previous)){
         //           #region expropriation
         //               for(Vector2Int eCoord=new Vector2Int(),cCoord1=new Vector2Int();eCoord.y<=expropriationDistance.y;eCoord.y++){for(cCoord1.y=-eCoord.y+cCoord_Previous.y;cCoord1.y<=eCoord.y+cCoord_Previous.y;cCoord1.y+=eCoord.y*2){
         //               for(           eCoord.x=0                                      ;eCoord.x<=expropriationDistance.x;eCoord.x++){for(cCoord1.x=-eCoord.x+cCoord_Previous.x;cCoord1.x<=eCoord.x+cCoord_Previous.x;cCoord1.x+=eCoord.x*2){
         //                if(Math.Abs(cCoord1.x)>=MaxcCoordx||
         //                   Math.Abs(cCoord1.y)>=MaxcCoordy){
         //                 goto _skip;
         //                }
         //                if(
         //                 assigningCoordinates.All(
         //                  kvp=>{
         //                   Vector2Int assigningCoordinate=kvp.Value;
         //                   return Mathf.Abs(cCoord1.x-assigningCoordinate.x)>instantiationDistance.x||
         //                          Mathf.Abs(cCoord1.y-assigningCoordinate.y)>instantiationDistance.y;
         //                  }
         //                 )
         //                ){
         //                     int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         //                     if(terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr)){
         //                      if(cnkMsgr.expropriated==null){
         //                       cnkMsgr.expropriated=terrainMessageHandlersPool.AddLast(cnkMsgr);
         //                      }
         //                     }
         //                }
         //                _skip:{}
         //                if(eCoord.x==0){break;}
         //               }}
         //                if(eCoord.y==0){break;}
         //               }}
         //           #endregion
                   }
         //          Vector2Int cCoord=assigningCoordinates[gameplayer];
         //          #region instantiation
         //              for(Vector2Int iCoord=new Vector2Int(),cCoord1=new Vector2Int();iCoord.y<=instantiationDistance.y;iCoord.y++){for(cCoord1.y=-iCoord.y+cCoord.y;cCoord1.y<=iCoord.y+cCoord.y;cCoord1.y+=iCoord.y*2){
         //              for(           iCoord.x=0                                      ;iCoord.x<=instantiationDistance.x;iCoord.x++){for(cCoord1.x=-iCoord.x+cCoord.x;cCoord1.x<=iCoord.x+cCoord.x;cCoord1.x+=iCoord.x*2){
         //               if(Math.Abs(cCoord1.x)>=MaxcCoordx||
         //                  Math.Abs(cCoord1.y)>=MaxcCoordy){
         //                goto _skip;
         //               }
         //               int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         //               while(LimitExecutionTime()){
         //                yield return null;
         //                stopwatch.Restart();
         //               }
         //               if(!terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr)){
         //                if(terrainMessageHandlersPool.Count>0){
         //                    cnkMsgr=terrainMessageHandlersPool.First.Value;
         //                    terrainMessageHandlersPool.RemoveFirst();
         //                    cnkMsgr.expropriated=null;
         //                }else{
         //                    cnkMsgr=Instantiate(_VoxelTerrainChunkUnnamedMessageHandlerPrefab);
         //                    terrainMessageHandlers.Add(cnkMsgr);
         //                    cnkMsgr.OnInstantiated();
         //                    try{
         //                     cnkMsgr.netObj.Spawn(destroyWithScene:false);
         //                    }catch(Exception e){
         //                     Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         //                    }
         //                    cnkMsgr.netObj.DontDestroyWithOwner=true;
         //                }
         //                    bool firstCall=cnkMsgr.id==null;
         //                    if(!firstCall&&terrainMessageHandlersAssigned.ContainsKey(cnkMsgr.id.Value.cnkIdx)){
         //                     terrainMessageHandlersAssigned.Remove(cnkMsgr.id.Value.cnkIdx);
         //                    }
         //                    terrainMessageHandlersAssigned.Add(cnkIdx1,cnkMsgr);
         //                    cnkMsgr.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
         //               }else{
         //                    if(cnkMsgr.expropriated!=null){
         //                     terrainMessageHandlersPool.Remove(cnkMsgr.expropriated);
         //                     cnkMsgr.expropriated=null;
         //                    }
         //               }
         //               _skip:{}
         //               if(iCoord.x==0){break;}
         //              }}
         //               if(iCoord.y==0){break;}
         //              }}
         //          #endregion
                  }
                  toAssignMessageHandlers.Clear();
                 }
                }
                goto Loop;
            }
        }
     //   internal void NetServerSideNetUpdate(){
     //    if(DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT!=0uL){
     //     Log.DebugMessage("DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT:"+DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
     //     foreach(var kvp in terrainMessageHandlersAssigned){
     //      VoxelTerrainChunkUnnamedMessageHandler cnkMsgr=kvp.Value;
     //      cnkMsgr.OnReceivedVoxelTerrainChunkEditDataRequest(DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
     //     }
     //       DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0uL;
     //    }
     //    assigningExecutionTime=0;
     //    foreach(var kvp in terrainMessageHandlersAssigned){
     //     VoxelTerrainChunkUnnamedMessageHandler cnkMsgr=kvp.Value;
     //     cnkMsgr             .NetServerSideManualUpdate();
     //     cnkMsgr.cnkArraySync.NetServerSideManualUpdate();
     //    }
     //    foreach(ulong clientIdRequestingNetVoxelArray in clientIdsRequestingNetVoxelArray){
     //     if(!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientIdRequestingNetVoxelArray)){
     //      clientIdsRequestingNetVoxelArrayDisconnected.Add(clientIdRequestingNetVoxelArray);
     //     }
     //    }
     //    #region netVoxelArrays
     //        foreach(var netVoxelArray in netVoxelArraysActive){
     //         netVoxelArray.NetServerSideManualUpdate(clientIdsRequestingNetVoxelArrayDisconnected,out bool toPool);
     //         if(toPool){netVoxelArraysToPool.Add(netVoxelArray);}
     //        }
     //        foreach(var netVoxelArray in netVoxelArraysToPool){
     //         netVoxelArray.OnPool();
     //        }
     //        netVoxelArraysToPool.Clear();
     //    #endregion
     //    clientIdsRequestingNetVoxelArrayDisconnected.Clear();
     //   }
    }
}