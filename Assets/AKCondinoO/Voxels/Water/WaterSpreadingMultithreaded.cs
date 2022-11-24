#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Water.Editing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Water.Editing.VoxelWaterEditingMultithreaded;
using System.ComponentModel;

namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystem.Concurrent
    internal class WaterSpreadingContainer:BackgroundContainer{
     internal readonly VoxelWater[]voxelsOutput=new VoxelWater[VoxelsPerChunk];
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
     internal readonly Dictionary<int,string>editsFileName=new Dictionary<int,string>();
     internal readonly Dictionary<int,FileStream>editsFileStream=new Dictionary<int,FileStream>();
     internal readonly Dictionary<int,StreamWriter>editsFileStreamWriter=new Dictionary<int,StreamWriter>();
     internal readonly Dictionary<int,StreamReader>editsFileStreamReader=new Dictionary<int,StreamReader>();
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          foreach(var sW in editsFileStreamWriter){if(sW.Value!=null){sW.Value.Dispose();}}
          foreach(var sR in editsFileStreamReader){if(sR.Value!=null){sR.Value.Dispose();}}
          editsFileStream      .Clear();
          editsFileStreamWriter.Clear();
          editsFileStreamReader.Clear();
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingContainer>{
     readonly VoxelWater[]voxels=new VoxelWater[VoxelsPerChunk];
     readonly Dictionary<Vector3Int,double>absorbing=new();
     readonly Dictionary<Vector3Int,double>spreading=new();
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
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
            if(VoxelSystem.Concurrent.waterVoxelsOutput.TryGetValue(voxelsOutputOldId.cnkIdx,out VoxelWater[]oldIdVoxelsOutput)&&object.ReferenceEquals(oldIdVoxelsOutput,container.voxelsOutput)){
             VoxelSystem.Concurrent.waterVoxelsOutput.Remove(voxelsOutputOldId.cnkIdx);
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
          Vector2Int cCoord1=container.cCoord.Value;
          int oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
          string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelWaterEditing.waterEditingFileFormat,VoxelWaterEditing.waterEditingPath,cCoord1.x,cCoord1.y);
          if(!container.editsFileStream.ContainsKey(oftIdx1)||!container.editsFileName.ContainsKey(oftIdx1)||container.editsFileName[oftIdx1]!=editsFileName){
           container.editsFileName[oftIdx1]=editsFileName;
           if(container.editsFileStream.TryGetValue(oftIdx1,out FileStream fStream)){
            container.editsFileStreamWriter[oftIdx1].Dispose();
            container.editsFileStreamReader[oftIdx1].Dispose();
            container.editsFileStream      .Remove(oftIdx1);
            container.editsFileStreamWriter.Remove(oftIdx1);
            container.editsFileStreamReader.Remove(oftIdx1);
           }
           if(File.Exists(editsFileName)){
            container.editsFileStream.Add(oftIdx1,new FileStream(editsFileName,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite));
            container.editsFileStreamWriter.Add(oftIdx1,new StreamWriter(container.editsFileStream[oftIdx1]));
            container.editsFileStreamReader.Add(oftIdx1,new StreamReader(container.editsFileStream[oftIdx1]));
           }
          }
          if(container.editsFileStream.TryGetValue(oftIdx1,out FileStream fileStream)){
           StreamReader fileStreamReader=container.editsFileStreamReader[oftIdx1];
           fileStream.Position=0L;
           fileStreamReader.DiscardBufferedData();
           string line;
           while((line=fileStreamReader.ReadLine())!=null){
            if(string.IsNullOrEmpty(line)){continue;}
            int vCoordStringStart=line.IndexOf("vCoord=(");
           }
          }
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
         VoxelSystem.Concurrent.waterFileData_rwl.EnterWriteLock();
         try{
          //  salvar
          for(int x=-1;x<=1;x++){
          for(int y=-1;y<=1;y++){
          }}
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterFileData_rwl.ExitWriteLock();
         }
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
         if(hasChangedIndex){
          VoxelSystem.Concurrent.water_rwl.EnterWriteLock();
          try{
           VoxelSystem.Concurrent.waterVoxelsOutput[container.cnkIdx.Value]=container.voxelsOutput;
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