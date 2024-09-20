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
                Log.DebugMessage("'ask server for chunk data'");
                for(int i=0;i<clientSideChunkChangeRequestsState.Length;++i){
                 if(clientSideChunkChangeRequestsState[i]==ChangeRequestsState.Synchronized||
                    clientSideChunkChangeRequestsState[i]==ChangeRequestsState.Empty
                 ){
                  clientSideChunkChangeRequestsState[i]=ChangeRequestsState.Reset;
                 }
                }
               }
              }
             }
            }
         internal enum ChangeRequestsState:byte{
          Reset=0,
          Pending=1,
          Waiting=2,
          Synchronized=3,
          Empty=4,
         }
         [NonSerialized]readonly ChangeRequestsState[]clientSideChunkChangeRequestsState=new ChangeRequestsState[chunkVoxelArraySplits];
            internal void OnClientSideNetChunkHasChangesValueChanged(NetworkListEvent<bool>change){
             if(Core.singleton.isClient){
              if(!cnkArraySync.IsOwner){
               if(change.Type==NetworkListEvent<bool>.EventType.Full){
                if(cnkArraySync.netChunkHasChanges.Count==clientSideChunkChangeRequestsState.Length){
                 for(int i=0;i<cnkArraySync.netChunkHasChanges.Count;++i){
                  if(cnkArraySync.netChunkHasChanges[i]&&(clientSideChunkChangeRequestsState[i]!=ChangeRequestsState.Pending)){
                   clientSideChunkChangeRequestsState[i]=ChangeRequestsState.Pending;
                   hasPendingSync=true;
                  }else{
                   clientSideChunkChangeRequestsState[i]=ChangeRequestsState.Empty;
                  }
                 }
                }else{
                 Log.Error("'cnkArraySync.netChunkHasChanges.Count!=clientSideChunkChangeRequestsState.Length'");
                }
               }else if(change.Type==NetworkListEvent<bool>.EventType.Value){
                if(change.Index<clientSideChunkChangeRequestsState.Length){
                 if(change.Value&&change.Value!=change.PreviousValue){
                  clientSideChunkChangeRequestsState[change.Index]=ChangeRequestsState.Pending;
                  hasPendingSync=true;
                 }
                }else{
                 Log.Error("'change.Index>=clientSideTerrainChunkArrayChangeRequestsState.Length'");
                }
               }
              }
             }
            }
         [NonSerialized]bool hasPendingSync;
            internal partial void NetClientSideManualUpdate(){
                if(cnkArraySync!=null&&cnkArraySync.netObj.IsSpawned){
                 if(clientSideId!=null){
                  if(hasPendingSync){
                   Log.DebugMessage("hasPendingSync");
                   /*
                     add sizeof(int) for the message type
                     add sizeof(int) for the cnkIdx
                     add sizeof(int) for the segment
                   */
                   //FastBufferWriter writer=new FastBufferWriter(sizeof(int)*3,Allocator.Persistent);
                  }
                 }
                }
            }
        }
    }
}