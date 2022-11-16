#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystem.Concurrent
    internal class WaterSpreadingContainer:BackgroundContainer{
     internal readonly VoxelWater[]voxelsOutput=new VoxelWater[VoxelsPerChunk];
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingContainer>{
        readonly VoxelWater[]voxels=new VoxelWater[VoxelsPerChunk];
        protected override void Cleanup(){
        }
        protected override void Execute(){
         if(container.cnkIdx==null){
          return;
         }
         Log.DebugMessage("WaterSpreadingMultithreaded:Execute()");
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          Array.Clear(voxels,0,voxels.Length);
         }else{
          VoxelSystem.Concurrent.water_rwl.EnterReadLock();
          try{
           lock(container.voxelsOutput){
            Array.Copy(container.voxelsOutput,voxels,container.voxelsOutput.Length);
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitReadLock();
          }
         }
         //  do edges but lock the voxels output only when reading or writing
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
         }}}
         VoxelSystem.Concurrent.water_rwl.EnterReadLock();
         try{
          lock(container.voxelsOutput){
           Array.Copy(voxels,container.voxelsOutput,voxels.Length);
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.water_rwl.ExitReadLock();
         }
        }
    }
}