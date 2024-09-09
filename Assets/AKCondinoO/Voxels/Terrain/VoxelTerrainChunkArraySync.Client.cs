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
         //       for(int i=0;i<clientSideTerrainChunkArrayChangeRequestsState.Length;++i){
         //        if(clientSideTerrainChunkArrayChangeRequestsState[i]==ChangeRequestsState.Waiting||
         //           clientSideTerrainChunkArrayChangeRequestsState[i]==ChangeRequestsState.Synchronized
         //        ){
         //         clientSideTerrainChunkArrayChangeRequestsState[i]=ChangeRequestsState.Reset;
         //        }
         //       }
               }
              }
             }
            }
        }
        //internal void NetClientSideManualUpdate(){
        //}
    }
}