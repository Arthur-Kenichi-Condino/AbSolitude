#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainGetFileEditDataToNetSyncContainer:BackgroundContainer{
     internal FastBufferWriter dataToSendToClients;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
    }
    internal class VoxelTerrainGetFileEditDataToNetSyncMultithreaded:BaseMultithreaded<VoxelTerrainGetFileEditDataToNetSyncContainer>{
        protected override void Execute(){
         VoxelSystem.Concurrent.terrainFileDatarwl.EnterReadLock();
         try{
          Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:");
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFileDatarwl.ExitReadLock();
         }
   
             container.dataToSendToClients.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkEditDataFileSegment);
             container.dataToSendToClients.WriteValueSafe((int)0);
             container.dataToSendToClients.WriteValueSafe((int)0);
             container.dataToSendToClients.WriteValueSafe((int)0);
             Vector3Int vCoord1;
             for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
             for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
             for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
              container.dataToSendToClients.WriteValueSafe(vCoord1.x);
              container.dataToSendToClients.WriteValueSafe(vCoord1.y);
              container.dataToSendToClients.WriteValueSafe(vCoord1.z);
              container.dataToSendToClients.WriteValueSafe((double)0.0d);
              container.dataToSendToClients.WriteValueSafe((ushort)MaterialId.Air);
             }}}

        }
    }
}