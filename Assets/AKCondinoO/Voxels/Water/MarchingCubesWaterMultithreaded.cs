#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.Water.WaterSpreadingMultithreaded;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    internal class MarchingCubesWaterBackgroundContainer:BackgroundContainer{
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
     internal FileStream readCacheStream;
     internal BinaryReader readCacheBinaryReader;
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          if(readCacheStream!=null){
           readCacheStream      .Dispose();
           readCacheBinaryReader.Dispose();
           readCacheStream      =null;
           readCacheBinaryReader=null;
          }
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class MarchingCubesWaterMultithreaded:BaseMultithreaded<MarchingCubesWaterBackgroundContainer>{
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
        internal MarchingCubesWaterMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i]=new Dictionary<int,VoxelWater>();
         }
        }
        protected override void Cleanup(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i].Clear();
         }
        }
        protected override void Execute(){
         if(container.cnkIdx==null){
          return;
         }
         Log.DebugMessage("MarchingCubesWaterMultithreaded:Execute()");
         Vector2Int cCoord1=container.cCoord.Value;
         int oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
         int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         bool hasChangedIndex=false;
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          hasChangedIndex=true;
         }
         //  carregar arquivo aqui
         VoxelSystem.Concurrent.waterCache_rwl.EnterReadLock();
         try{
          if(hasChangedIndex){
           if(container.readCacheStream!=null){
            container.readCacheStream      .Dispose();
            container.readCacheBinaryReader.Dispose();
            container.readCacheStream      =null;
            container.readCacheBinaryReader=null;
           }
          }
          if(container.readCacheStream==null){
           string cacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterCacheFileFormat,VoxelSystem.Concurrent.waterCachePath,container.cCoord.Value.x,container.cCoord.Value.y);
           if(File.Exists(cacheFileName)){
            container.readCacheStream=new FileStream(cacheFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            container.readCacheBinaryReader=new BinaryReader(container.readCacheStream);
           }
          }
          if(container.readCacheStream!=null){
           container.readCacheStream.Position=0L;
           while(container.readCacheBinaryReader.BaseStream.Position!=container.readCacheBinaryReader.BaseStream.Length){
            var v=BinaryReadVoxelWater(container.readCacheBinaryReader);
            voxels[oftIdx1][v.vxlIdx]=v.voxel;
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitReadLock();
         }
         UInt32 vertexCount=0;
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
         }}}
        }
    }
}