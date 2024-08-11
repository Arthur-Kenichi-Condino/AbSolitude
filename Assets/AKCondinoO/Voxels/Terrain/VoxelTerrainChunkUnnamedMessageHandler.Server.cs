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
            internal partial void NetServerSideDispose(){
             cnkMsgr.terrainSendEditDataToServerBG.Dispose();
            }
        }
     //[NonSerialized]internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     //[NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
     //   internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
     //    //Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:'cnkIdx':"+id.Value.cnkIdx);
     //    pendingWriteEditData=true;
     //    if(cnkArraySync!=null){
     //       cnkArraySync.OnReceivedVoxelTerrainChunkEditDataRequest(clientId);
     //    }
     //   }
     //   internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
     //    if(firstCall||cCoord1!=id.Value.cCoord){
     //     id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
     //     netChunkId.Value=new(
     //      id.Value.cCoord,
     //      id.Value.cnkRgn,
     //      id.Value.cnkIdx
     //     );
     //     pendingWriteEditData=true;
     //    }
     //    if(cnkArraySync!=null){
     //       cnkArraySync.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
     //    }
     //   }
     //   internal void NetServerSideManualUpdate(){
     //   }
    }
}