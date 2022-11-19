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
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;

namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystem.Concurrent
    internal class WaterSpreadingContainer:BackgroundContainer{
     internal readonly VoxelWater[]voxelsOutput=new VoxelWater[VoxelsPerChunk];
     internal readonly Dictionary<Vector3Int,double>absorbingOutput=new();
     internal readonly Dictionary<Vector3Int,double>spreadingOutput=new();
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingContainer>{
     readonly VoxelWater[]voxels=new VoxelWater[VoxelsPerChunk];
     readonly Dictionary<Vector3Int,double>absorbing=new();
     readonly Dictionary<Vector3Int,double>spreading=new();
        protected override void Cleanup(){
         absorbing.Clear();
         spreading.Clear();
        }
        protected override void Execute(){
         if(container.cnkIdx==null){
          return;
         }
         Log.DebugMessage("WaterSpreadingMultithreaded:Execute()");
         bool hasChangedIndex=false;
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          hasChangedIndex=true;
          VoxelSystem.Concurrent.water_rwl.EnterWriteLock();
          try{
           if(VoxelSystem.Concurrent.waterVoxelsId.TryGetValue(container.voxelsOutput,out var voxelsOutputOldId)){
            if(VoxelSystem.Concurrent.waterVoxels.TryGetValue(voxelsOutputOldId.cnkIdx,out VoxelWater[]oldIdVoxelsOutput)&&object.ReferenceEquals(oldIdVoxelsOutput,container.voxelsOutput)){
             VoxelSystem.Concurrent.waterVoxels.Remove(voxelsOutputOldId.cnkIdx);
             VoxelSystem.Concurrent.absorbing.Remove(voxelsOutputOldId.cnkIdx);
             VoxelSystem.Concurrent.spreading.Remove(voxelsOutputOldId.cnkIdx);
             Log.DebugMessage("removed old value for voxelsOutputOldId.cnkIdx:"+voxelsOutputOldId.cnkIdx);
            }
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitWriteLock();
          }
          Array.Clear(voxels,0,voxels.Length);
         }else{
          VoxelSystem.Concurrent.water_rwl.EnterReadLock();
          try{
           lock(container.voxelsOutput){
            Array.Copy(container.voxelsOutput,voxels,container.voxelsOutput.Length);
           }
           lock(container.absorbingOutput){
            foreach(var kvp in container.absorbingOutput){
             absorbing[kvp.Key]=kvp.Value;
            }
           }
           lock(container.spreadingOutput){
            foreach(var kvp in container.spreadingOutput){
             spreading[kvp.Key]=kvp.Value;
            }
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitReadLock();
          }
         }
         //  carregar arquivo aqui
         VoxelSystem.Concurrent.waterFileData_rwl.EnterReadLock();
         try{
          //  carregar dados de arquivo aqui em voxels, e absorbing e spreading
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterFileData_rwl.ExitReadLock();
         }
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          //  carregar dados do bioma aqui em voxels,
         }}}
         //  calcular absorb e spread aqui
         // e também modificar vizinhos aqui, salvando em arquivo
         VoxelSystem.Concurrent.water_rwl.EnterReadLock();
         try{
          lock(container.voxelsOutput){
           Array.Copy(voxels,container.voxelsOutput,voxels.Length);
          }
          lock(container.absorbingOutput){
           container.absorbingOutput.Clear();
           foreach(var kvp in absorbing){
            container.absorbingOutput[kvp.Key]=kvp.Value;
           }
          }
          lock(container.spreadingOutput){
           container.spreadingOutput.Clear();
           foreach(var kvp in spreading){
            container.spreadingOutput[kvp.Key]=kvp.Value;
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.water_rwl.ExitReadLock();
         }
         if(hasChangedIndex){
          VoxelSystem.Concurrent.water_rwl.EnterWriteLock();
          try{
           VoxelSystem.Concurrent.waterVoxels[container.cnkIdx.Value]=container.voxelsOutput;
            VoxelSystem.Concurrent.absorbing[container.cnkIdx.Value]=container.absorbingOutput;
            VoxelSystem.Concurrent.spreading[container.cnkIdx.Value]=container.spreadingOutput;
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitWriteLock();
          }
          container.lastcCoord=container.cCoord;
          container.lastcnkRgn=container.cnkRgn;
          container.lastcnkIdx=container.cnkIdx;
         }
        }
    }
}