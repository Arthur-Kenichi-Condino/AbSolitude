#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkUnnamedMessageHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainGetFileEditDataToNetSyncContainer:BackgroundContainer{
     internal Dictionary<Vector3Int,TerrainEditOutputData>dataToSendToClients;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
    }
    internal class VoxelTerrainGetFileEditDataToNetSyncMultithreaded:BaseMultithreaded<VoxelTerrainGetFileEditDataToNetSyncContainer>{
        protected override void Execute(){
         VoxelSystem.Concurrent.terrainFileDatarwl.EnterReadLock();
         try{
          Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:VoxelsPerSegment:"+VoxelsPerSegment);
          if(!dataToSendDictionaryPool.TryDequeue(out container.dataToSendToClients)){
           container.dataToSendToClients=new Dictionary<Vector3Int,TerrainEditOutputData>();
          }

          //testing, REMOVE:
             Vector3Int vCoord1;
             for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
             for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
             for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
              container.dataToSendToClients[vCoord1]=new TerrainEditOutputData(0.0d,MaterialId.Air);
             }}}

         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFileDatarwl.ExitReadLock();
         }
        }
    }
}