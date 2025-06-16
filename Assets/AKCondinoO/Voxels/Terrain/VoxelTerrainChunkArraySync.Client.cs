#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainGetFileEditDataToNetSyncContainer;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkArraySync{
        internal partial class ClientData{
            internal partial void NetClientSideOnDestroyingCore(){
             cnkArraySync.terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
            }
            internal partial void NetClientSideDispose(){
             cnkArraySync.terrainGetFileEditDataToNetSyncBG.Dispose();
            }
         [NonSerialized]NetChunkId?clientSideNetChunkId=null;
          [NonSerialized](Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?clientSideId=null;
            internal void OnClientSideNetChunkIdValueChanged(NetChunkId previous,NetChunkId current){
             if(Core.singleton.isClient){
              if(!cnkArraySync.IsOwner){
               if(clientSideNetChunkId==null||current!=clientSideNetChunkId.Value){
                clientSideNetChunkId=current;
                clientSideId=(current.cCoord,current.cnkRgn,current.cnkIdx);
                //Log.DebugMessage("'ask server for chunk data'");
                for(int i=0;i<clientSideChunkVoxelArraySyncState.Length;++i){
                 if(clientSideChunkVoxelArraySyncState[i]!=VoxelArraySyncState.Pending){
                  clientSideChunkVoxelArraySyncState[i]=VoxelArraySyncState.Unsynchronized;
                 }
                }
               }
              }
             }
            }
         internal enum VoxelArraySyncState:byte{
          Unsynchronized=0,
          Pending=1,
          Synchronized=2,
         }
         [NonSerialized]readonly VoxelArraySyncState[]clientSideChunkVoxelArraySyncState=new VoxelArraySyncState[chunkVoxelArraySplits];
            internal void OnClientSideNetChunkHasChangesValueChanged(NetworkListEvent<bool>change){
             if(Core.singleton.isClient){
              if(!cnkArraySync.IsOwner){
               if(cnkArraySync.spawnInitialization){
                Log.DebugMessage("'cnkArraySync.spawnInitialization'");
               }else{
                Log.DebugMessage("change.Type:"+change.Type);
                if(change.Type==NetworkListEvent<bool>.EventType.Value){
                 if(cnkArraySync.netChunkHasChanges.Count==clientSideChunkVoxelArraySyncState.Length){
                  for(int i=0;i<cnkArraySync.netChunkHasChanges.Count;++i){
                   if(cnkArraySync.netChunkHasChanges[i]&&(clientSideChunkVoxelArraySyncState[i]!=VoxelArraySyncState.Pending)){
                    clientSideChunkVoxelArraySyncState[i]=VoxelArraySyncState.Pending;
                    hasPendingSync=true;
                   }else{
                    clientSideChunkVoxelArraySyncState[i]=VoxelArraySyncState.Synchronized;
                   }
                  }
                 }else{
                  Log.Error("'cnkArraySync.netChunkHasChanges.Count!=clientSideChunkChangeRequestsState.Length'");
                 }
                }
               }
              }
             }
             Log.DebugMessage("hasPendingSync:"+hasPendingSync);
            }
         [NonSerialized]bool hasUnsynchronizedState;
         [NonSerialized]float hasUnsynchronizedStateMsgCooldown=1f;
          [NonSerialized]float hasUnsynchronizedStateMsgTimer;
         [NonSerialized]bool hasPendingSync;
         [NonSerialized]float hasPendingSyncMsgCooldown=1f;
          [NonSerialized]float hasPendingSyncMsgTimer;
            internal partial void NetClientSideManualUpdate(){
                if(hasPendingSyncMsgTimer>0f){
                 hasPendingSyncMsgTimer-=Time.deltaTime;
                }
                if(cnkArraySync!=null&&cnkArraySync.netObj.IsSpawned){
                 if(clientSideId!=null){
                  if(hasUnsynchronizedState){
                   Log.DebugMessage("hasUnsynchronizedState");
                  }else{
                      if(hasPendingSync){
                       if(hasPendingSyncMsgTimer<=0f){
                        hasPendingSyncMsgTimer=hasPendingSyncMsgCooldown;
                        Log.DebugMessage("hasPendingSync");
                        hasPendingSync=false;
                        /*
                          add sizeof(int) for the message type
                          add sizeof(int) for the cnkIdx
                          add (sizeof(int)*clientSideChunkChangeRequestsState.Length) for the segments
                        */
                        FastBufferWriter writer=new FastBufferWriter(sizeof(int)*2,Allocator.Persistent,sizeof(int)*2+sizeof(int)*clientSideChunkVoxelArraySyncState.Length);
                        if(writer.TryBeginWrite(sizeof(int)*2)){
                         writer.WriteValue((int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest);
                         writer.WriteValue((int)clientSideId.Value.cnkIdx);
                         for(int i=0;i<clientSideChunkVoxelArraySyncState.Length;++i){
                          if(clientSideChunkVoxelArraySyncState[i]==VoxelArraySyncState.Pending
                          ){
                           if(writer.TryBeginWrite(sizeof(int))){
                            writer.WriteValue((int)i);
                            //Log.DebugMessage("'as client, request segment':"+i);
                           }
                           hasPendingSync=true;
                          }else if(clientSideChunkVoxelArraySyncState[i]==VoxelArraySyncState.Unsynchronized){
                           hasUnsynchronizedState=true;
                          }
                         }
                        }
                        if(VoxelSystem.singleton.asClient.clientSideChunkEditDataRequestsToSend.TryGetValue(clientSideId.Value.cnkIdx,out FastBufferWriter oldRequest)){oldRequest.Dispose();}
                        VoxelSystem.singleton.asClient.clientSideChunkEditDataRequestsToSend[clientSideId.Value.cnkIdx]=writer;
                       }
                      }else{
                      }
                  }
                 }
                }
            }
        }
    }
}