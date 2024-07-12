#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain;
using AKCondinoO.Voxels.Terrain.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
     [SerializeField]VoxelTerrainChunkArraySync _VoxelTerrainChunkArraySyncPrefab;
     [SerializeField]VoxelTerrainChunkUnnamedMessageHandler _VoxelTerrainChunkUnnamedMessageHandlerPrefab;
     internal readonly List<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlers=new List<VoxelTerrainChunkUnnamedMessageHandler>();
     internal readonly     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersPool    =new     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>();
     internal readonly Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersAssigned=new Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>();
     internal readonly List<VoxelTerrainChunkArraySync>terrainArraySyncs=new List<VoxelTerrainChunkArraySync>();
     internal readonly     LinkedList<VoxelTerrainChunkArraySync>terrainArraySyncsPool    =new     LinkedList<VoxelTerrainChunkArraySync>();
     internal readonly Dictionary<int,VoxelTerrainChunkArraySync>terrainArraySyncsAssigned=new Dictionary<int,VoxelTerrainChunkArraySync>();
        internal void NetServerSideInit(){
         Log.DebugMessage("NetServerSideInit");
         Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage+=OnServerReceivedUnnamedMessage;
         serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine=StartCoroutine(ServerSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine());
        }
        internal void NetClientSideInit(){
         Log.DebugMessage("NetClientSideInit");
         Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage+=OnClientReceivedUnnamedMessage;
        }
        internal void OnDestroyingCoreNetworkDestroy(){
         Log.DebugMessage("OnDestroyingCoreNetDestroy");
         if(this!=null&&serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine!=null){
          StopCoroutine(serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine);
         }
         if(Core.singleton.isServer){
          if(Core.singleton.netManager.CustomMessagingManager!=null){
             Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage-=OnServerReceivedUnnamedMessage;
          }
          for(int i=0;i<terrainMessageHandlers.Count;++i){
           terrainMessageHandlers[i].OnDestroyingCore();
          }
          for(int i=0;i<terrainArraySyncs.Count;++i){
           terrainArraySyncs[i].OnDestroyingCore();
          }
         }
         if(Core.singleton.isClient){
          if(Core.singleton.netManager.CustomMessagingManager!=null){
             Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage-=OnClientReceivedUnnamedMessage;
          }
         }
        }
        internal void OnDestroyingCoreNetworkDispose(){
         if(Core.singleton.isServer){
          for(int i=0;i<terrainMessageHandlers.Count;++i){
          }
          terrainMessageHandlers.Clear();
          for(int i=0;i<terrainArraySyncs.Count;++i){
           terrainArraySyncs[i].terrainGetFileEditDataToNetSyncBG.Dispose();
          }
          terrainArraySyncs.Clear();
          //  everything has been disposed
         }
         if(Core.singleton.isClient){
          foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
           FastBufferWriter request=clientSideRequestToSend.Value;
           request.Dispose();
          }
          clientVoxelTerrainChunkEditDataRequestsToSend.Clear();
          foreach(var kvp1 in clientVoxelTerrainChunkEditDataSegmentsReceivedFromServer){
           foreach(var kvp2 in kvp1.Value){
            kvp2.Value.segmentData.Dispose();
           }
           kvp1.Value.Clear();
           clientVoxelTerrainChunkEditDataSegmentsDictionaryPool.Enqueue(kvp1.Value);
          }
          clientVoxelTerrainChunkEditDataSegmentsReceivedFromServer.Clear();
         }
        }
     [SerializeField]ulong DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0;
        internal void NetUpdate(){
         if(DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT>0uL){
          Log.DebugMessage("DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT:"+DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
          foreach(var kvp in terrainMessageHandlersAssigned){
           VoxelTerrainChunkUnnamedMessageHandler cnkMsgr=kvp.Value;
           cnkMsgr.OnReceivedVoxelTerrainChunkEditDataRequest(DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
          }
          foreach(var kvp in terrainArraySyncsAssigned){
           VoxelTerrainChunkArraySync cnkArraySync=kvp.Value;
           cnkArraySync.OnReceivedVoxelTerrainChunkEditDataRequest(DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT);
          }
            DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0uL;
         }
         assigningExecutionTime=0;
         VoxelTerrainChunkUnnamedMessageHandler.StaticUpdate();
         VoxelTerrainChunkArraySync.StaticUpdate();
         foreach(var kvp in terrainMessageHandlersAssigned){
          VoxelTerrainChunkUnnamedMessageHandler cnkMsgr=kvp.Value;
          cnkMsgr.ManualUpdate();
         }
         foreach(var kvp in terrainArraySyncsAssigned){
          VoxelTerrainChunkArraySync cnkArraySync=kvp.Value;
          cnkArraySync.ManualUpdate();
         }
         if(clientSendMessageTimer<=0f){
            clientSendMessageTimer=clientSendMessageDelay;
          clientVoxelTerrainChunkEditDataRequestsSentToRemove.Clear();
          clientVoxelTerrainChunkEditDataRequestsSent=0;
          foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
           FastBufferWriter request=clientSideRequestToSend.Value;
           if(Core.singleton.isClient){
            if(Core.singleton.netManager.IsConnectedClient){
             Core.singleton.netManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,request,NetworkDelivery.ReliableSequenced);
            }
           }
           request.Dispose();
           clientVoxelTerrainChunkEditDataRequestsSentToRemove.Add(clientSideRequestToSend.Key);
           clientVoxelTerrainChunkEditDataRequestsSent++;
           if(clientVoxelTerrainChunkEditDataRequestsSent>=clientMaxVoxelTerrainChunkEditDataRequestsPerFrame){
            break;
           }
          }
          foreach(int toRemove in clientVoxelTerrainChunkEditDataRequestsSentToRemove){
           clientVoxelTerrainChunkEditDataRequestsToSend.Remove(toRemove);
          }
         }else{
          clientSendMessageTimer-=Time.deltaTime;
         }
        }
     Coroutine serverSideVoxelTerrainChunkUnnamedMessageHandlerAssignerCoroutine;
     internal readonly HashSet<Gameplayer>generationRequestedAssignMessageHandlers=new HashSet<Gameplayer>();
      readonly Dictionary<Gameplayer,Vector2Int>  assigningCoordinates=new Dictionary<Gameplayer,Vector2Int>();
      readonly Dictionary<Gameplayer,Vector2Int>deassigningCoordinates=new Dictionary<Gameplayer,Vector2Int>();
      readonly HashSet<Gameplayer>toAssignMessageHandlers=new HashSet<Gameplayer>();
      [NonSerialized]double assigningMaxExecutionTime=1.0;
       [NonSerialized]double assigningExecutionTime;
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
            int poolSize=GameplayerManagement.maxPlayers*(
             (VoxelSystem.expropriationDistance.x*2+1)*
             (VoxelSystem.expropriationDistance.y*2+1)
            );
            Log.DebugMessage("poolSize:"+poolSize);
            //for(int p=0;p<poolSize;++p){
            // //while(LimitExecutionTime()){
            // // yield return null;
            // // stopwatch.Restart();
            // //}
            // Log.DebugMessage("p:"+p);
            // var cnkArraySync=Instantiate(_VoxelTerrainChunkArraySyncPrefab);
            // terrainArraySyncs.Add(cnkArraySync);
            // cnkArraySync.OnInstantiated();
            // try{
            //  cnkArraySync.netObj.Spawn(destroyWithScene:false);
            // }catch(Exception e){
            //  Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
            // }
            // cnkArraySync.netObj.DontDestroyWithOwner=true;
            // cnkArraySync.expropriated=terrainArraySyncsPool.AddLast(cnkArraySync);
            //}
            Loop:{
             yield return null;
             stopwatch.Restart();
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
                #region expropriation
                    for(Vector2Int eCoord=new Vector2Int(),cCoord1=new Vector2Int();eCoord.y<=expropriationDistance.y;eCoord.y++){for(cCoord1.y=-eCoord.y+cCoord_Previous.y;cCoord1.y<=eCoord.y+cCoord_Previous.y;cCoord1.y+=eCoord.y*2){
                    for(           eCoord.x=0                                      ;eCoord.x<=expropriationDistance.x;eCoord.x++){for(cCoord1.x=-eCoord.x+cCoord_Previous.x;cCoord1.x<=eCoord.x+cCoord_Previous.x;cCoord1.x+=eCoord.x*2){
                     if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                        Math.Abs(cCoord1.y)>=MaxcCoordy){
                      goto _skip;
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
                          if(terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr)){
                           if(cnkMsgr.expropriated==null){
                            cnkMsgr.expropriated=terrainMessageHandlersPool.AddLast(cnkMsgr);
                           }
                          }
                          if(terrainArraySyncsAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkArraySync cnkArraySync)){
                           if(cnkArraySync.expropriated==null){
                            cnkArraySync.expropriated=terrainArraySyncsPool.AddLast(cnkArraySync);
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
                    int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                    while(LimitExecutionTime()){
                     yield return null;
                     stopwatch.Restart();
                    }
                    if(!terrainMessageHandlersAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr)){
                     if(terrainMessageHandlersPool.Count>0){
                         cnkMsgr=terrainMessageHandlersPool.First.Value;
                         terrainMessageHandlersPool.RemoveFirst();
                         cnkMsgr.expropriated=null;
                     }else{
                         cnkMsgr=Instantiate(_VoxelTerrainChunkUnnamedMessageHandlerPrefab);
                         terrainMessageHandlers.Add(cnkMsgr);
                         cnkMsgr.OnInstantiated();
                         try{
                          cnkMsgr.netObj.Spawn(destroyWithScene:false);
                         }catch(Exception e){
                          Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                         }
                         cnkMsgr.netObj.DontDestroyWithOwner=true;
                     }
                         bool firstCall=cnkMsgr.id==null;
                         if(!firstCall&&terrainMessageHandlersAssigned.ContainsKey(cnkMsgr.id.Value.cnkIdx)){
                          terrainMessageHandlersAssigned.Remove(cnkMsgr.id.Value.cnkIdx);
                         }
                         terrainMessageHandlersAssigned.Add(cnkIdx1,cnkMsgr);
                         cnkMsgr.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
                    }else{
                         if(cnkMsgr.expropriated!=null){
                          terrainMessageHandlersPool.Remove(cnkMsgr.expropriated);
                          cnkMsgr.expropriated=null;
                         }
                    }
                    while(LimitExecutionTime()){
                     yield return null;
                     stopwatch.Restart();
                    }
                    if(!terrainArraySyncsAssigned.TryGetValue(cnkIdx1,out VoxelTerrainChunkArraySync cnkArraySync)){
                     if(terrainArraySyncsPool.Count>0){
                         cnkArraySync=terrainArraySyncsPool.First.Value;
                         terrainArraySyncsPool.RemoveFirst();
                         cnkArraySync.expropriated=null;
                     }else{
                         cnkArraySync=Instantiate(_VoxelTerrainChunkArraySyncPrefab);
                         terrainArraySyncs.Add(cnkArraySync);
                         cnkArraySync.OnInstantiated();
                         try{
                          cnkArraySync.netObj.Spawn(destroyWithScene:false);
                         }catch(Exception e){
                          Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
                         }
                         cnkArraySync.netObj.DontDestroyWithOwner=true;
                     }
                         bool firstCall=cnkArraySync.id==null;
                         if(!firstCall&&terrainArraySyncsAssigned.ContainsKey(cnkArraySync.id.Value.cnkIdx)){
                          terrainArraySyncsAssigned.Remove(cnkArraySync.id.Value.cnkIdx);
                         }
                         terrainArraySyncsAssigned.Add(cnkIdx1,cnkArraySync);
                         cnkArraySync.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
                    }else{
                         if(cnkArraySync.expropriated!=null){
                          terrainArraySyncsPool.Remove(cnkArraySync.expropriated);
                          cnkArraySync.expropriated=null;
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