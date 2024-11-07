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
             if(VoxelSystem.singleton!=null&&serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine!=null){
              VoxelSystem.singleton.StopCoroutine(serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine);
             }
             for(int i=0;i<terrainMessageHandlers.Count;++i){
              VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
              cnkMsgr     .asServer.NetServerSideOnDestroyingCore();
             }
             for(int i=0;i<terrainArraySyncs     .Count;++i){
              VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
              cnkArraySync.asServer.NetServerSideOnDestroyingCore();
             }
             foreach(var netVoxelArray in netVoxelArraysActive){
              netVoxelArraysToPool.Add(netVoxelArray);
             }
             foreach(var netVoxelArray in netVoxelArraysToPool){
              netVoxelArray.asServer.OnPool(true);
             }
             netVoxelArraysToPool.Clear();
             netVoxelArraysPool  .Clear();
             netVoxelArraysCount=0;
            }
            internal partial void NetServerSideOnDestroyingCoreNetworkDispose(){
             for(int i=0;i<terrainMessageHandlers.Count;++i){
              VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
              cnkMsgr     .asServer.NetServerSideDispose();
             }
             for(int i=0;i<terrainArraySyncs     .Count;++i){
              VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
              cnkArraySync.asServer.NetServerSideDispose();
             }
             terrainMessageHandlers.Clear();
             terrainArraySyncs     .Clear();
             netVoxelArrays        .Clear();
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
             if(VoxelSystem.singleton.DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT!=0uL){
              Log.DebugMessage("DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT:"+VoxelSystem.singleton.DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
              foreach(var kvp in terrainArraySyncsAssigned){
               VoxelTerrainChunkArraySync cnkArraySync=kvp.Value;
               HashSet<int>segmentList;
               if(!VoxelTerrainChunkArraySync.ServerData.clientIdsRequestingDataSegmentListPool.TryDequeue(out segmentList)){
                segmentList=new HashSet<int>();
               }
               for(int i=0;i<VoxelTerrainGetFileEditDataToNetSyncContainer.chunkVoxelArraySplits;++i){
                segmentList.Add(i);
               }
               cnkArraySync.asServer.OnReceivedVoxelTerrainChunkEditDataRequest(VoxelSystem.singleton.DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT,
                segmentList
               );
              }
                VoxelSystem.singleton.DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0uL;
             }
             assigningExecutionTime=0;
             foreach(var cnkMsgr      in terrainMessageHandlersAssigned){
              cnkMsgr     .Value.asServer.NetServerSideManualUpdate();
             }
             foreach(var cnkArraySync in terrainArraySyncsAssigned     ){
              cnkArraySync.Value.asServer.NetServerSideManualUpdate();
             }
             #region netVoxelArrays
              foreach(ulong clientIdRequestingNetVoxelArray in clientIdsRequestingNetVoxelArray){
               if(!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientIdRequestingNetVoxelArray)){
                clientIdsRequestingNetVoxelArrayDisconnected.Add(clientIdRequestingNetVoxelArray);
               }
              }
              clientIdsRequestingNetVoxelArray.Clear();
              foreach(var netVoxelArray in netVoxelArraysActive){
               netVoxelArray.asServer.NetServerSideManualUpdate(clientIdsRequestingNetVoxelArrayDisconnected,out bool toPool);
               if(toPool){netVoxelArraysToPool.Add(netVoxelArray);}
              }
              foreach(var netVoxelArray in netVoxelArraysToPool){
               netVoxelArray.asServer.OnPool();
              }
              netVoxelArraysToPool.Clear();
              clientIdsRequestingNetVoxelArrayDisconnected.Clear();
             #endregion
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
                  //  TO DO: deassign disconnected player
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
                    #region expropriation
                        for(Vector2Int eCoord=new Vector2Int(),cCoord1=new Vector2Int();eCoord.y<=expropriationDistance.y;eCoord.y++){for(cCoord1.y=-eCoord.y+cCoord_Previous.y;cCoord1.y<=eCoord.y+cCoord_Previous.y;cCoord1.y+=eCoord.y*2){
                        for(           eCoord.x=0                                      ;eCoord.x<=expropriationDistance.x;eCoord.x++){for(cCoord1.x=-eCoord.x+cCoord_Previous.x;cCoord1.x<=eCoord.x+cCoord_Previous.x;cCoord1.x+=eCoord.x*2){
                         if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                            Math.Abs(cCoord1.y)>=MaxcCoordy){
                          goto _skip;
                         }
                         while(LimitExecutionTime()){
                          yield return null;
                          stopwatch.Restart();
                         }
                         if(
                          assigningCoordinates.All(
                           kvp=>{
                            Vector2Int assigningCoordinate=kvp.Value;
                            return Mathf.Abs(cCoord1.x-assigningCoordinate.x)>instantiationDistance.x||
                                   Mathf.Abs(cCoord1.y-assigningCoordinate.y)>instantiationDistance.y;
                           }
                          )
                         ){
                              int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                              if(terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     )){
                               if(cnkMsgr     .asServer.expropriated==null){
                                cnkMsgr     .asServer.expropriated=terrainMessageHandlersPool.AddLast(cnkMsgr     );
                               }
                              }
                              if(terrainArraySyncsAssigned     .TryGetValue(cnkIdx1,out VoxelTerrainChunkArraySync             cnkArraySync)){
                               if(cnkArraySync.asServer.expropriated==null){
                                cnkArraySync.asServer.expropriated=terrainArraySyncsPool     .AddLast(cnkArraySync);
                               }
                              }
                         }
                         _skip:{}
                         if(eCoord.x==0){break;}
                        }}
                         if(eCoord.y==0){break;}
                        }}
                    #endregion
                   }
                   Vector2Int cCoord=assigningCoordinates[gameplayer];
                   #region instantiation
                       for(Vector2Int iCoord=new Vector2Int(),cCoord1=new Vector2Int();iCoord.y<=instantiationDistance.y;iCoord.y++){for(cCoord1.y=-iCoord.y+cCoord.y;cCoord1.y<=iCoord.y+cCoord.y;cCoord1.y+=iCoord.y*2){
                       for(           iCoord.x=0                                      ;iCoord.x<=instantiationDistance.x;iCoord.x++){for(cCoord1.x=-iCoord.x+cCoord.x;cCoord1.x<=iCoord.x+cCoord.x;cCoord1.x+=iCoord.x*2){
                        if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                           Math.Abs(cCoord1.y)>=MaxcCoordy){
                         goto _skip;
                        }
                        while(LimitExecutionTime()){
                         yield return null;
                         stopwatch.Restart();
                        }
                        int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                        if(!terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     )){
                             if(terrainMessageHandlersPool.Count>0){
                              cnkMsgr     =terrainMessageHandlersPool.First.Value;
                              terrainMessageHandlersPool.RemoveFirst();
                              cnkMsgr     .asServer.expropriated=null;
                             }else{
                              cnkMsgr     =Instantiate(VoxelSystem.singleton._VoxelTerrainChunkUnnamedMessageHandlerPrefab);
                              cnkMsgr     .asServer.OnInstantiated();
                              try{
                               cnkMsgr     .netObj.Spawn(destroyWithScene:false);
                              }catch(Exception e){
                               Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                              }
                              cnkMsgr     .netObj.DontDestroyWithOwner=true;
                             }
                             bool firstCall=cnkMsgr     .asServer.id==null;
                             if(!firstCall&&terrainMessageHandlersAssigned.ContainsKey(cnkMsgr     .asServer.id.Value.cnkIdx)){
                              terrainMessageHandlersAssigned.Remove(cnkMsgr     .asServer.id.Value.cnkIdx);
                             }
                             terrainMessageHandlersAssigned.Add(cnkIdx1,cnkMsgr     );
                             cnkMsgr     .asServer.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
                        }else{
                             if(cnkMsgr     .asServer.expropriated!=null){
                              terrainMessageHandlersPool.Remove(cnkMsgr     .asServer.expropriated);
                              cnkMsgr     .asServer.expropriated=null;
                             }
                        }
                        if(!terrainArraySyncsAssigned     .TryGetValue(cnkIdx1,out VoxelTerrainChunkArraySync             cnkArraySync)){
                             if(terrainArraySyncsPool     .Count>0){
                              cnkArraySync=terrainArraySyncsPool     .First.Value;
                              terrainArraySyncsPool     .RemoveFirst();
                              cnkArraySync.asServer.expropriated=null;
                             }else{
                              cnkArraySync=Instantiate(VoxelSystem.singleton._VoxelTerrainChunkArraySyncPrefab            );
                              cnkArraySync.asServer.OnInstantiated();
                              try{
                               cnkArraySync.netObj.Spawn(destroyWithScene:false);
                              }catch(Exception e){
                               Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                              }
                              cnkArraySync.netObj.DontDestroyWithOwner=true;
                             }
                             bool firstCall=cnkArraySync.asServer.id==null;
                             if(!firstCall&&terrainArraySyncsAssigned     .ContainsKey(cnkArraySync.asServer.id.Value.cnkIdx)){
                              terrainArraySyncsAssigned     .Remove(cnkArraySync.asServer.id.Value.cnkIdx);
                             }
                             terrainArraySyncsAssigned     .Add(cnkIdx1,cnkArraySync);
                             cnkArraySync.asServer.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
                        }else{
                             if(cnkArraySync.asServer.expropriated!=null){
                              terrainArraySyncsPool     .Remove(cnkArraySync.asServer.expropriated);
                              cnkArraySync.asServer.expropriated=null;
                             }
                        }
                        _skip:{}
                        if(iCoord.x==0){break;}
                       }}
                        if(iCoord.y==0){break;}
                       }}
                   #endregion
                  }
                  toAssignMessageHandlers.Clear();
                 }
                }
                goto Loop;
            }
        }
    }
}