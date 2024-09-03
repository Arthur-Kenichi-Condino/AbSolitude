#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkUnnamedMessageHandler;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler{
        internal partial class ServerData{
            internal partial void OnInstantiated(){
            }
            internal partial void NetServerSideOnDestroyingCore(){
             Log.DebugMessage("NetServerSideOnDestroyingCore");
             cnkMsgr.terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning,-1);
            }
            internal partial void NetServerSideDispose(){
             Log.DebugMessage("NetServerSideDispose");
             cnkMsgr.terrainSendEditDataToServerBG.Dispose();
            }
            internal partial void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
             if(firstCall||cCoord1!=id.Value.cCoord){
              id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
              cnkMsgr.netChunkId.Value=new(
               id.Value.cCoord,
               id.Value.cnkRgn,
               id.Value.cnkIdx
              );
             }
            }
        }
     //         pendingWriteEditData=true;
     //   internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
     //    //Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:'cnkIdx':"+id.Value.cnkIdx);
     //    pendingWriteEditData=true;
     //    if(cnkArraySync!=null){
     //       cnkArraySync.OnReceivedVoxelTerrainChunkEditDataRequest(clientId);
     //    }
     //   }
     //   internal void NetServerSideManualUpdate(){
     //   }
    }
}