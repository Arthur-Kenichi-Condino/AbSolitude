#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Water.Editing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Water.Editing.VoxelWaterEditingMultithreaded;
using AKCondinoO.Voxels.Terrain.MarchingCubes;
namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystem.Concurrent
    internal class WaterSpreadingBackgroundContainer:BackgroundContainer{
     internal bool result;
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
     internal readonly Dictionary<int,string>editsFileName=new Dictionary<int,string>();
     internal readonly Dictionary<int,FileStream>editsFileStream=new Dictionary<int,FileStream>();
     internal readonly Dictionary<int,StreamWriter>editsFileStreamWriter=new Dictionary<int,StreamWriter>();
     internal readonly Dictionary<int,StreamReader>editsFileStreamReader=new Dictionary<int,StreamReader>();
     internal float deltaTime;
     internal string cacheFileName;
     internal FileStream cacheStream;
     internal BinaryWriter cacheBinaryWriter;
     internal BinaryReader cacheBinaryReader;
     internal readonly Dictionary<int,FileStream>neighbourhoodSpreadingCacheStream=new();
     internal readonly Dictionary<int,BinaryWriter>neighbourhoodSpreadingCacheBinaryWriter=new();
     internal readonly Dictionary<int,BinaryReader>neighbourhoodSpreadingCacheBinaryReader=new();
     internal readonly Dictionary<int,FileStream>neighbourhoodAbsorbingCacheStream=new();
     internal readonly Dictionary<int,BinaryWriter>neighbourhoodAbsorbingCacheBinaryWriter=new();
     internal readonly Dictionary<int,BinaryReader>neighbourhoodAbsorbingCacheBinaryReader=new();
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          foreach(var sW in editsFileStreamWriter){if(sW.Value!=null){sW.Value.Dispose();}}
          foreach(var sR in editsFileStreamReader){if(sR.Value!=null){sR.Value.Dispose();}}
          editsFileStream      .Clear();
          editsFileStreamWriter.Clear();
          editsFileStreamReader.Clear();
          foreach(var nCBW in neighbourhoodSpreadingCacheBinaryWriter){if(nCBW.Value!=null){nCBW.Value.Dispose();}}
          foreach(var nCBR in neighbourhoodSpreadingCacheBinaryReader){if(nCBR.Value!=null){nCBR.Value.Dispose();}}
          neighbourhoodSpreadingCacheStream      .Clear();
          neighbourhoodSpreadingCacheBinaryWriter.Clear();
          neighbourhoodSpreadingCacheBinaryReader.Clear();
          foreach(var nCBW in neighbourhoodAbsorbingCacheBinaryWriter){if(nCBW.Value!=null){nCBW.Value.Dispose();}}
          foreach(var nCBR in neighbourhoodAbsorbingCacheBinaryReader){if(nCBR.Value!=null){nCBR.Value.Dispose();}}
          neighbourhoodAbsorbingCacheStream      .Clear();
          neighbourhoodAbsorbingCacheBinaryWriter.Clear();
          neighbourhoodAbsorbingCacheBinaryReader.Clear();
          if(cacheStream!=null){
           cacheBinaryWriter.Dispose();
           cacheBinaryReader.Dispose();
           cacheStream=null;
           cacheBinaryWriter=null;
           cacheBinaryReader=null;
          }
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingBackgroundContainer>{
     internal const double stopSpreadingDensity=30.0d;
     readonly Dictionary<int,Dictionary<Vector3Int,(double absorb,Vector3Int fromvCoord,VoxelWater fromVoxel)>>neighbourhoodAbsorbing=new();
     readonly Dictionary<int,Dictionary<Vector3Int,(double spread,Vector3Int fromvCoord,VoxelWater fromVoxel)>>neighbourhoodSpreading=new();
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
      readonly Dictionary<int,VoxelWater>biomeVoxels=new Dictionary<int,VoxelWater>();
     readonly Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>absorbing=new();
     readonly Dictionary<Vector3Int,(double spread,VoxelWater voxel)>spreading=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>beforeAbsorbValue=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>beforeSpreadValue=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>afterAbsorbValue=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>afterSpreadValue=new();
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly Dictionary<int,Voxel[]>terrainVoxels=new Dictionary<int,Voxel[]>();
     readonly     double[][][]     noiseCache1=new     double[biome.cacheLength][][];
     readonly MaterialId[][][]materialIdCache1=new MaterialId[biome.cacheLength][][];
        internal WaterSpreadingMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       neighbourhoodAbsorbing[i]=new Dictionary<Vector3Int,(double absorb,Vector3Int fromvCoord,VoxelWater fromVoxel)>();
                       neighbourhoodSpreading[i]=new Dictionary<Vector3Int,(double spread,Vector3Int fromvCoord,VoxelWater fromVoxel)>();
                       voxels[i]=new Dictionary<int,VoxelWater>();
                       beforeAbsorbValue[i]=new();
                       beforeSpreadValue[i]=new();
                       afterAbsorbValue[i]=new();
                       afterSpreadValue[i]=new();
         }
         for(int i=0;i<biome.cacheLength;++i){
               noiseCache1[i]=new     double[9][];
          materialIdCache1[i]=new MaterialId[9][];
         }
        }
        protected override void Cleanup(){
         foreach(var oftIdxNeighbourhoodAbsorbPair in neighbourhoodAbsorbing){oftIdxNeighbourhoodAbsorbPair.Value.Clear();}
         foreach(var oftIdxNeighbourhoodSpreadPair in neighbourhoodSpreading){oftIdxNeighbourhoodSpreadPair.Value.Clear();}
         for(int i=0;i<voxels.Length;++i){
                       voxels[i].Clear();
         }
         biomeVoxels.Clear();
         absorbing.Clear();
         spreading.Clear();
         foreach(var oftIdxAbsorbedOldValuePair in beforeAbsorbValue){oftIdxAbsorbedOldValuePair.Value.Clear();}
         foreach(var oftIdxSpreadedOldValuePair in beforeSpreadValue){oftIdxSpreadedOldValuePair.Value.Clear();}
         foreach(var oftIdxAbsorbedNewValuePair in afterAbsorbValue){oftIdxAbsorbedNewValuePair.Value.Clear();}
         foreach(var oftIdxSpreadedNewValuePair in afterSpreadValue){oftIdxSpreadedNewValuePair.Value.Clear();}
         foreach(var editData in dataFromFileToMerge){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataFromFileToMerge.Clear();
         foreach(var editData in dataForSavingToFile){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataForSavingToFile.Clear();
         for(int i=0;i<biome.cacheLength;++i){
          for(int j=0;j<     noiseCache1[i].Length;++j){if(     noiseCache1[i][j]!=null)Array.Clear(     noiseCache1[i][j],0,     noiseCache1[i][j].Length);}
          for(int j=0;j<materialIdCache1[i].Length;++j){if(materialIdCache1[i][j]!=null)Array.Clear(materialIdCache1[i][j],0,materialIdCache1[i][j].Length);}
         }
         foreach(var kvp in terrainVoxels){
          Array.Clear(kvp.Value,0,kvp.Value.Length);
          VoxelSystem.Concurrent.voxelsArrayPool.Enqueue(kvp.Value);
         }
         terrainVoxels.Clear();
        }
     readonly System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
        protected override void Execute(){
         sw.Restart();
         if(container.cnkIdx==null){
          return;
         }
         //Log.DebugMessage("WaterSpreadingMultithreaded:Execute()");
         bool hadChanges=false;
         Vector2Int cCoord1=container.cCoord.Value;
         int        oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
         Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1);
         int        cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         if(!waterEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,WaterEditOutputData>editData1)){
          editData1=new Dictionary<Vector3Int,WaterEditOutputData>();
         }
         dataFromFileToMerge.Add(cCoord1,editData1);
         //  carregar arquivo aqui
         bool hasChangedIndex=false;
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          hasChangedIndex=true;
         }
         VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterWriteLock();
         try{
          if(hasChangedIndex){
           for(int x=-1;x<=1;x++){
           for(int y=-1;y<=1;y++){
            Vector2Int cCoord2=cCoord1+new Vector2Int(x,y);
            if(Math.Abs(cCoord2.x)>=MaxcCoordx||
               Math.Abs(cCoord2.y)>=MaxcCoordy)
            {
             continue;
            }
            int oftIdx2=GetoftIdx(cCoord2-cCoord1);
            if(container.neighbourhoodSpreadingCacheStream.TryGetValue(oftIdx2,out FileStream nCS)){
             var neighbourhoodSpreadingCache=(nCS,
              container.neighbourhoodSpreadingCacheBinaryWriter[oftIdx2],
              container.neighbourhoodSpreadingCacheBinaryReader[oftIdx2]);
             var neighbourhoodAbsorbingCache=(container.neighbourhoodAbsorbingCacheStream[oftIdx2],
              container.neighbourhoodAbsorbingCacheBinaryWriter[oftIdx2],
              container.neighbourhoodAbsorbingCacheBinaryReader[oftIdx2]);
             if(VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCacheIds.TryGetValue(nCS,out var cacheOldId)){
              if(VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCache.TryGetValue(cacheOldId.cnkIdx,out var oldIdSpreadingCacheList)){
               oldIdSpreadingCacheList.Remove(neighbourhoodSpreadingCache);
               if(oldIdSpreadingCacheList.Count<=0){
                VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.Enqueue(oldIdSpreadingCacheList);
                VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCache.Remove(cacheOldId.cnkIdx);
                //Log.DebugMessage("removed empty list for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
               }
               var oldIdAbsorbingCacheList=VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCache[cacheOldId.cnkIdx];
               oldIdAbsorbingCacheList.Remove(neighbourhoodAbsorbingCache);
               if(oldIdAbsorbingCacheList.Count<=0){
                VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.Enqueue(oldIdAbsorbingCacheList);
                VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCache.Remove(cacheOldId.cnkIdx);
                //Log.DebugMessage("removed empty list for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
               }
               //Log.DebugMessage("removed old value for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
              }
              VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCacheIds.Remove(nCS);
              VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCacheIds.Remove(container.neighbourhoodAbsorbingCacheStream[oftIdx2]);
             }
             container.neighbourhoodSpreadingCacheBinaryWriter[oftIdx2].Dispose();
             container.neighbourhoodSpreadingCacheBinaryReader[oftIdx2].Dispose();
             container.neighbourhoodSpreadingCacheStream.Remove(oftIdx2);
             container.neighbourhoodSpreadingCacheBinaryWriter.Remove(oftIdx2);
             container.neighbourhoodSpreadingCacheBinaryReader.Remove(oftIdx2);
             container.neighbourhoodAbsorbingCacheBinaryWriter[oftIdx2].Dispose();
             container.neighbourhoodAbsorbingCacheBinaryReader[oftIdx2].Dispose();
             container.neighbourhoodAbsorbingCacheStream.Remove(oftIdx2);
             container.neighbourhoodAbsorbingCacheBinaryWriter.Remove(oftIdx2);
             container.neighbourhoodAbsorbingCacheBinaryReader.Remove(oftIdx2);
            }
            if(!container.neighbourhoodSpreadingCacheStream.ContainsKey(oftIdx2)){
             string spreadingCacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterNeighbourhoodCacheSpreadingFileFormat,VoxelSystem.Concurrent.waterNeighbourhoodCachePath,cCoord2.x,cCoord2.y);
             //Log.DebugMessage("spreadingCacheFileName:"+spreadingCacheFileName);
             container.neighbourhoodSpreadingCacheStream.Add(oftIdx2,new FileStream(spreadingCacheFileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
             container.neighbourhoodSpreadingCacheBinaryWriter.Add(oftIdx2,new BinaryWriter(container.neighbourhoodSpreadingCacheStream[oftIdx2]));
             container.neighbourhoodSpreadingCacheBinaryReader.Add(oftIdx2,new BinaryReader(container.neighbourhoodSpreadingCacheStream[oftIdx2]));
             string absorbingCacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterNeighbourhoodCacheAbsorbingFileFormat,VoxelSystem.Concurrent.waterNeighbourhoodCachePath,cCoord2.x,cCoord2.y);
             //Log.DebugMessage("absorbingCacheFileName:"+absorbingCacheFileName);
             container.neighbourhoodAbsorbingCacheStream.Add(oftIdx2,new FileStream(absorbingCacheFileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
             container.neighbourhoodAbsorbingCacheBinaryWriter.Add(oftIdx2,new BinaryWriter(container.neighbourhoodAbsorbingCacheStream[oftIdx2]));
             container.neighbourhoodAbsorbingCacheBinaryReader.Add(oftIdx2,new BinaryReader(container.neighbourhoodAbsorbingCacheStream[oftIdx2]));
            }
           }}
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.ExitWriteLock();
         }
         VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterUpgradeableReadLock();
         try{
          for(int x=-1;x<=1;x++){
          for(int y=-1;y<=1;y++){
           Vector2Int cCoord2=cCoord1+new Vector2Int(x,y);
           if(Math.Abs(cCoord2.x)>=MaxcCoordx||
              Math.Abs(cCoord2.y)>=MaxcCoordy)
           {
            continue;
           }
           int oftIdx2=GetoftIdx(cCoord2-cCoord1);
           if(container.neighbourhoodSpreadingCacheStream.TryGetValue(oftIdx2,out FileStream fileStream)){
            lock(fileStream){
             BinaryReader binReader=container.neighbourhoodSpreadingCacheBinaryReader[oftIdx2];
             fileStream.Position=0L;
             while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
              var v=BinaryReadNeighbourWaterSpread(binReader);
              OnGotNeighbourhoodSpread(v.fromvxlIdx,v.fromVoxel,v.atvxlIdx,v.spreadValue,oftIdx2);
             }
            }
            if(oftIdx2==oftIdx1){
             VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterWriteLock();
             try{
              BinaryWriter binWriter=container.neighbourhoodSpreadingCacheBinaryWriter[oftIdx2];
              fileStream.SetLength(0L);
              binWriter.Flush();
             }catch{
              throw;
             }finally{
              VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.ExitWriteLock();
             }
            }
            fileStream=container.neighbourhoodAbsorbingCacheStream[oftIdx2];
            lock(fileStream){
             BinaryReader binReader=container.neighbourhoodAbsorbingCacheBinaryReader[oftIdx2];
             fileStream.Position=0L;
             while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
              var v=BinaryReadNeighbourWaterAbsorb(binReader);
              OnGotNeighbourhoodAbsorb(v.fromvxlIdx,v.fromVoxel,v.atvxlIdx,v.absorbValue,oftIdx2);
             }
            }
            if(oftIdx2==oftIdx1){
             VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterWriteLock();
             try{
              BinaryWriter binWriter=container.neighbourhoodAbsorbingCacheBinaryWriter[oftIdx2];
              fileStream.SetLength(0L);
              binWriter.Flush();
             }catch{
              throw;
             }finally{
              VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.ExitWriteLock();
             }
            }
           }
          }}
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.ExitUpgradeableReadLock();
         }
         VoxelSystem.Concurrent.waterCache_rwl.EnterWriteLock();
         try{
          if(hasChangedIndex){
           if(container.cacheStream!=null){
            var cS=container.cacheStream;
            if(VoxelSystem.Concurrent.waterCacheIds.TryGetValue(cS,out var cacheOldId)){
             if(VoxelSystem.Concurrent.waterCache.TryGetValue(cacheOldId.cnkIdx,out var oldIdCache)&&
              object.ReferenceEquals(oldIdCache.stream,cS)
             ){
              VoxelSystem.Concurrent.waterCache.Remove(cacheOldId.cnkIdx);
              VoxelSystem.Concurrent.waterCacheIds.Remove(cS);
              //Log.DebugMessage("removed old value for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
             }
            }
            container.cacheBinaryWriter.Dispose();
            container.cacheBinaryReader.Dispose();
            container.cacheStream=null;
            container.cacheBinaryWriter=null;
            container.cacheBinaryReader=null;
           }
          }
          if(container.cacheStream==null){
           string cacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterCacheFileFormat,VoxelSystem.Concurrent.waterCachePath,container.cCoord.Value.x,container.cCoord.Value.y);
           container.cacheStream=new FileStream(cacheFileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           container.cacheBinaryWriter=new BinaryWriter(container.cacheStream);
           container.cacheBinaryReader=new BinaryReader(container.cacheStream);
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitWriteLock();
         }
         VoxelSystem.Concurrent.waterCache_rwl.EnterReadLock();
         try{
          if(container.cacheStream!=null){
           FileStream fileStream=container.cacheStream;
           lock(fileStream){
            BinaryReader binReader=container.cacheBinaryReader;
            fileStream.Position=0L;
            while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
               int vxlIdx=BinaryReadvxlIdx    (binReader);
             VoxelWater v=BinaryReadVoxelWater(binReader);
             voxels[oftIdx1][vxlIdx]=v;
            }
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitReadLock();
         }
         bool hadEdits=LoadDataFromFile(cCoord1,editData1,voxels[oftIdx1]);
         foreach(var vCoordAbsorbingPair in neighbourhoodAbsorbing[oftIdx1]){
          Vector3Int vCoord=vCoordAbsorbingPair.Key;
          int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
          Log.DebugMessage("neighbourhoodAbsorbing:cCoord1:"+cCoord1+":vCoord:"+vCoord+":vCoordAbsorbingPair.Value.absorb:"+vCoordAbsorbingPair.Value.absorb);
          HorizontalAbsorbSetVoxel(vCoordAbsorbingPair.Value.fromVoxel,vxlIdx,vCoordAbsorbingPair.Value.absorb,oftIdx1,cnkRgn1,HasBlockageAt(vCoord),editData1);
          neighbourhoodSpreading[oftIdx1].Remove(vCoord);
         }
         foreach(var vCoordSpreadingPair in neighbourhoodSpreading[oftIdx1]){
          Vector3Int vCoord=vCoordSpreadingPair.Key;
          int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
          Log.DebugMessage("neighbourhoodSpreading:cCoord1:"+cCoord1+":vCoord:"+vCoord+":vCoordSpreadingPair.Value.spread:"+vCoordSpreadingPair.Value.spread);
          //  carregar dados do bioma aqui em voxels,
          HorizontalSpreadSetVoxel(vCoordSpreadingPair.Value.fromVoxel,vxlIdx,vCoordSpreadingPair.Value.spread,oftIdx1,cnkRgn1,HasBlockageAt(vCoord),editData1);
         }
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          //  carregar dados do bioma aqui em voxels,
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          VoxelWater voxel=GetVoxelAt(vxlIdx1,oftIdx1,cnkRgn1,editData1);
          voxel.sleeping=voxel.sleeping&&(voxel.density==voxel.previousDensity);
          if(voxel.density!=0.0d||voxel.previousDensity!=0.0d){
           if(!voxel.sleeping){
            hadChanges=true;
            if(voxel.density<voxel.previousDensity){
             //Log.DebugMessage("to absorb:"+vCoord1);
             if(absorbing.TryGetValue(vCoord1,out var currentAbsorb)){
              double absorbValueOld=currentAbsorb.absorb;
              double absorbValueNew=voxel.previousDensity-voxel.density;
              if(absorbValueNew>absorbValueOld){
                absorbing[vCoord1]=(absorbValueNew,voxel);
              }
             }else{
                absorbing.Add(vCoord1,(voxel.previousDensity-voxel.density,voxel));
             }
            }else if(voxel.density==voxel.previousDensity){
             //Log.DebugMessage("to spread (voxel.density==voxel.previousDensity):"+vCoord1);
             if(spreading.TryGetValue(vCoord1,out var currentSpread)){
              double spreadValueOld=currentSpread.spread;
              double spreadValueNew=voxel.density;
              if(spreadValueNew>spreadValueOld){
                spreading[vCoord1]=(spreadValueNew,voxel);
              }
             }else{
                spreading.Add(vCoord1,(voxel.density,voxel));
             }
            }else{
             //Log.DebugMessage("to spread:"+vCoord1);
             if(spreading.TryGetValue(vCoord1,out var currentSpread)){
              double spreadValueOld=currentSpread.spread;
              double spreadValueNew=voxel.density-voxel.previousDensity;
              if(spreadValueNew>spreadValueOld){
                spreading[vCoord1]=(spreadValueNew,voxel);
              }
             }else{
                spreading.Add(vCoord1,(voxel.density-voxel.previousDensity,voxel));
             }
            }
           }
           voxels[oftIdx1][vxlIdx1]=voxel;
          }else{
           voxels[oftIdx1].Remove(vxlIdx1);
          }
         }}}
         //  calcular absorb e spread aqui
         foreach(var vCoordAbsorbingPair in absorbing){
          Vector3Int vCoord2=vCoordAbsorbingPair.Key;
                 int vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
              double absorbValue=vCoordAbsorbingPair.Value.absorb;
            VoxelWater fromVoxel=vCoordAbsorbingPair.Value.voxel;
          if(!fromVoxel.isCreated){continue;}
          Vector3Int vCoord3=new Vector3Int(vCoord2.x,vCoord2.y-1,vCoord2.z);
          bool waterfall=VerticalAbsorb();
          if(!waterfall){
           vCoord3=new Vector3Int(vCoord2.x+1,vCoord2.y,vCoord2.z);
           if(HorizontalAbsorb()){WakeTop();}
           vCoord3=new Vector3Int(vCoord2.x-1,vCoord2.y,vCoord2.z);
           if(HorizontalAbsorb()){WakeTop();}
           vCoord3=new Vector3Int(vCoord2.x,vCoord2.y,vCoord2.z+1);
           if(HorizontalAbsorb()){WakeTop();}
           vCoord3=new Vector3Int(vCoord2.x,vCoord2.y,vCoord2.z-1);
           if(HorizontalAbsorb()){WakeTop();}
           void WakeTop(){
            Vector3Int vCoord4=new Vector3Int(vCoord3.x,vCoord3.y+1,vCoord3.z);
                   int vxlIdx4=GetvxlIdx(vCoord4.x,vCoord4.y,vCoord4.z);
            if(voxels[oftIdx1].TryGetValue(vxlIdx4,out VoxelWater v4)){
             if(v4.density>0f){
              v4.sleeping=false;
              voxels[oftIdx1][vxlIdx4]=v4;
             }
            }
           }
          }else{
           Vector3Int vCoord4=new Vector3Int(vCoord2.x,vCoord2.y+1,vCoord2.z);
                  int vxlIdx4=GetvxlIdx(vCoord4.x,vCoord4.y,vCoord4.z);
           if(voxels[oftIdx1].TryGetValue(vxlIdx4,out VoxelWater v4)){
            if(v4.density>0f){
             v4.sleeping=false;
             voxels[oftIdx1][vxlIdx4]=v4;
            }
           }
          }
          bool VerticalAbsorb(){
           if(!(vCoord3.y>=0)){
            return false;
           }else{
            //Log.DebugMessage("VerticalAbsorb:"+vCoord3);
            bool hasBlockage=HasBlockageAt(vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return   VerticalAbsorbSetVoxel(fromVoxel,vxlIdx3,absorbValue,oftIdx1,cnkRgn1,hasBlockage,null);
           }
          }
          bool HorizontalAbsorb(){
           if(vCoord3.x<0||vCoord3.x>=Width||
              vCoord3.z<0||vCoord3.z>=Depth
           ){
            Vector2Int cnkRgn3=cCoordTocnkRgn(cCoord1);
            ValidateCoord(ref cnkRgn3,ref vCoord3);
            Vector2Int cCoord3=cnkRgnTocCoord(cnkRgn3);
            int        cnkIdx3=GetcnkIdx(cCoord3.x,cCoord3.y);
            int        oftIdx3=GetoftIdx(cCoord3-container.cCoord.Value);
            SetNeighbourhoodAbsorb(vCoord2,fromVoxel,vCoord3,absorbValue,oftIdx3);//  passar para vizinho dados usados abaixo...
            //  ...o chunk vizinho terá que realizar a operação abaixo ao adicionar os dados em 'absorbing'
            return false;
           }
           //Log.DebugMessage("HorizontalAbsorb:"+vCoord3);
           bool hasBlockage=HasBlockageAt(vCoord3);
           int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
           bool absorb=HorizontalAbsorbSetVoxel(fromVoxel,vxlIdx3,absorbValue,oftIdx1,cnkRgn1,hasBlockage,null);
           return absorb;
          }
          fromVoxel.previousDensity=fromVoxel.density;
          fromVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=fromVoxel;
         }
         foreach(var vCoordSpreadingPair in spreading){
          Vector3Int vCoord2=vCoordSpreadingPair.Key;
                 int vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
              double spreadValue=vCoordSpreadingPair.Value.spread;
            VoxelWater fromVoxel=vCoordSpreadingPair.Value.voxel;
          if(!fromVoxel.isCreated){continue;}
          Vector3Int vCoord3=new Vector3Int(vCoord2.x,vCoord2.y-1,vCoord2.z);
          bool waterfall=VerticalSpread();
          if(!waterfall){
           vCoord3=new Vector3Int(vCoord2.x+1,vCoord2.y,vCoord2.z);
           HorizontalSpread();
           vCoord3=new Vector3Int(vCoord2.x-1,vCoord2.y,vCoord2.z);
           HorizontalSpread();
           vCoord3=new Vector3Int(vCoord2.x,vCoord2.y,vCoord2.z+1);
           HorizontalSpread();
           vCoord3=new Vector3Int(vCoord2.x,vCoord2.y,vCoord2.z-1);
           HorizontalSpread();
          }
          bool VerticalSpread(){
           if(!(vCoord3.y>=0)){
            return false;
           }else{
            //Log.DebugMessage("VerticalSpread:"+vCoord3);
            bool hasBlockage=HasBlockageAt(vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return   VerticalSpreadSetVoxel(fromVoxel,vxlIdx3,spreadValue,oftIdx1,cnkRgn1,hasBlockage,null);
           }
          }
          void HorizontalSpread(){
           if(vCoord3.x<0||vCoord3.x>=Width||
              vCoord3.z<0||vCoord3.z>=Depth
           ){
            Vector2Int cnkRgn3=cCoordTocnkRgn(cCoord1);
            ValidateCoord(ref cnkRgn3,ref vCoord3);
            Vector2Int cCoord3=cnkRgnTocCoord(cnkRgn3);
            int        cnkIdx3=GetcnkIdx(cCoord3.x,cCoord3.y);
            int        oftIdx3=GetoftIdx(cCoord3-container.cCoord.Value);
            SetNeighbourhoodSpread(vCoord2,fromVoxel,vCoord3,spreadValue,oftIdx3);
            return;
           }
           //Log.DebugMessage("HorizontalSpread:"+vCoord3);
           bool hasBlockage=HasBlockageAt(vCoord3);
           int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
           HorizontalSpreadSetVoxel(fromVoxel,vxlIdx3,spreadValue,oftIdx1,cnkRgn1,hasBlockage,null);
          }
          fromVoxel.previousDensity=fromVoxel.density;
          fromVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=fromVoxel;
         }
         Log.Warning("TO DO:não repetir dados para o vizinho...");
         neighbourhoodAbsorbing[oftIdx1].Clear();
         neighbourhoodSpreading[oftIdx1].Clear();
         bool HasBlockageAt(Vector3Int vCoord){//  testar com array de voxels do terreno ou array de bool para objetos de estruturas
          int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
          bool arrayIsNull;
          if((arrayIsNull=!terrainVoxels.TryGetValue(cnkIdx1,out Voxel[]terrainVoxelArray))||!terrainVoxelArray[vxlIdx].isCreated){
           VoxelSystem.Concurrent.GetVoxelsBG(cCoord1,new Vector2Int(0,0),terrainVoxels,noiseCache1,materialIdCache1,vCoord);
           if(arrayIsNull){terrainVoxelArray=terrainVoxels[cnkIdx1];}
          }
          if(-terrainVoxelArray[vxlIdx].density<MarchingCubesTerrain.isoLevel){
           return true;
          }
          return false;
         }
         VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterWriteLock();
         try{
          for(int x=-1;x<=1;x++){
          for(int y=-1;y<=1;y++){
           Vector2Int cCoord2=cCoord1+new Vector2Int(x,y);
           if(Math.Abs(cCoord2.x)>=MaxcCoordx||
              Math.Abs(cCoord2.y)>=MaxcCoordy)
           {
            continue;
           }
           int oftIdx2=GetoftIdx(cCoord2-cCoord1);
           Vector2Int cnkRgn2=cCoordTocnkRgn(cCoord2);
           int        cnkIdx2=GetcnkIdx(cCoord2.x,cCoord2.y);
           if(container.neighbourhoodSpreadingCacheStream.TryGetValue(oftIdx2,out FileStream fileStream)){
            BinaryWriter binWriter=container.neighbourhoodSpreadingCacheBinaryWriter[oftIdx2];
            BinaryReader binReader=container.neighbourhoodSpreadingCacheBinaryReader[oftIdx2];
            fileStream.Position=0L;
            while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
             var v=BinaryReadNeighbourWaterSpread(binReader);
             OnGotNeighbourhoodSpread(v.fromvxlIdx,v.fromVoxel,v.atvxlIdx,v.spreadValue,oftIdx2);
            }
            fileStream.SetLength(0L);
            foreach(var vCoordSpreadingPair in neighbourhoodSpreading[oftIdx2]){
             //Log.DebugMessage("cCoord2:"+cCoord2+";vCoordSpreadingPair:"+vCoordSpreadingPair);
             BinaryWriteNeighbourWaterSpread(vCoordSpreadingPair.Value.fromvCoord,vCoordSpreadingPair.Value.fromVoxel,vCoordSpreadingPair.Key,vCoordSpreadingPair.Value.spread,binWriter);
            }
            binWriter.Flush();
            if(hasChangedIndex){
             if(!VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCache.TryGetValue(cnkIdx2,out var cacheList)){
              if(!VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.TryDequeue(out cacheList)){
               cacheList=new();
              }
              VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCache.Add(cnkIdx2,cacheList);
             }
             VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCache[cnkIdx2].Add((fileStream,binWriter,binReader));
             VoxelSystem.Concurrent.waterNeighbourhoodSpreadingCacheIds[fileStream]=(cCoord2,cnkRgn2,cnkIdx2);
             //VoxelSystem.Concurrent.waterCache[container.cnkIdx.Value]=(fileStream,binWriter,binReader);
             //VoxelSystem.Concurrent.waterCacheIds[fileStream]=(container.cCoord.Value,container.cnkRgn.Value,container.cnkIdx.Value);
            }
            fileStream=container.neighbourhoodAbsorbingCacheStream[oftIdx2];
            binWriter=container.neighbourhoodAbsorbingCacheBinaryWriter[oftIdx2];
            binReader=container.neighbourhoodAbsorbingCacheBinaryReader[oftIdx2];
            fileStream.Position=0L;
            while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
             var v=BinaryReadNeighbourWaterAbsorb(binReader);
             OnGotNeighbourhoodAbsorb(v.fromvxlIdx,v.fromVoxel,v.atvxlIdx,v.absorbValue,oftIdx2);
            }
            fileStream.SetLength(0L);
            foreach(var vCoordAbsorbingPair in neighbourhoodAbsorbing[oftIdx2]){
             //Log.DebugMessage("cCoord2:"+cCoord2+";vCoordAbsorbingPair:"+vCoordAbsorbingPair);
             BinaryWriteNeighbourWaterAbsorb(vCoordAbsorbingPair.Value.fromvCoord,vCoordAbsorbingPair.Value.fromVoxel,vCoordAbsorbingPair.Key,vCoordAbsorbingPair.Value.absorb,binWriter);
            }
            binWriter.Flush();
            if(hasChangedIndex){
             if(!VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCache.TryGetValue(cnkIdx2,out var cacheList)){
              if(!VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.TryDequeue(out cacheList)){
               cacheList=new();
              }
              VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCache.Add(cnkIdx2,cacheList);
             }
             VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCache[cnkIdx2].Add((fileStream,binWriter,binReader));
             VoxelSystem.Concurrent.waterNeighbourhoodAbsorbingCacheIds[fileStream]=(cCoord2,cnkRgn2,cnkIdx2);
             //VoxelSystem.Concurrent.waterCache[container.cnkIdx.Value]=(fileStream,binWriter,binReader);
             //VoxelSystem.Concurrent.waterCacheIds[fileStream]=(container.cCoord.Value,container.cnkRgn.Value,container.cnkIdx.Value);
            }
           }
          }}
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.ExitWriteLock();
         }
         VoxelSystem.Concurrent.waterCache_rwl.EnterWriteLock();
         try{
          if(container.cacheStream!=null){
           FileStream fileStream=container.cacheStream;
           BinaryWriter binWriter=container.cacheBinaryWriter;
           BinaryReader binReader=container.cacheBinaryReader;
           fileStream.SetLength(0L);
           foreach(var kvp in voxels[oftIdx1]){
            if(biomeVoxels.TryGetValue(kvp.Key,out VoxelWater biomeVoxel)){
             if(biomeVoxel==kvp.Value){
              continue;
             }
            }
            BinaryWritevxlIdx    (kvp.Key  ,binWriter);
            BinaryWriteVoxelWater(kvp.Value,binWriter);
           }
           binWriter.Flush();
           if(hasChangedIndex){
            VoxelSystem.Concurrent.waterCache[container.cnkIdx.Value]=(fileStream,binWriter,binReader);
            VoxelSystem.Concurrent.waterCacheIds[fileStream]=(container.cCoord.Value,container.cnkRgn.Value,container.cnkIdx.Value);
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitWriteLock();
         }
         if(hadEdits){
          VoxelSystem.Concurrent.waterFiles_rwl.EnterWriteLock();
          try{
           if(container.editsFileStream.TryGetValue(oftIdx1,out FileStream fileStream)){
            StreamWriter fileStreamWriter=container.editsFileStreamWriter[oftIdx1];
            fileStream.SetLength(0L);
            fileStreamWriter.Flush();
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.waterFiles_rwl.ExitWriteLock();
          }
         }
         if(hasChangedIndex){
          container.lastcCoord=container.cCoord;
          container.lastcnkRgn=container.cnkRgn;
          container.lastcnkIdx=container.cnkIdx;
         }
         bool LoadDataFromFile(Vector2Int cCoord,Dictionary<Vector3Int,WaterEditOutputData>editData,Dictionary<int,VoxelWater>voxels){
          bool result=false;
          VoxelSystem.Concurrent.waterFiles_rwl.EnterReadLock();
          try{
           int oftIdx=GetoftIdx(cCoord-container.cCoord.Value);
           string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelWaterEditing.waterEditingFileFormat,VoxelWaterEditing.waterEditingPath,cCoord.x,cCoord.y);
           if(!container.editsFileStream.ContainsKey(oftIdx)||!container.editsFileName.ContainsKey(oftIdx)||container.editsFileName[oftIdx]!=editsFileName){
            container.editsFileName[oftIdx]=editsFileName;
            if(container.editsFileStream.TryGetValue(oftIdx,out FileStream fStream)){
             container.editsFileStreamWriter[oftIdx].Dispose();
             container.editsFileStreamReader[oftIdx].Dispose();
             container.editsFileStream      .Remove(oftIdx);
             container.editsFileStreamWriter.Remove(oftIdx);
             container.editsFileStreamReader.Remove(oftIdx);
            }
            if(File.Exists(editsFileName)){
             container.editsFileStream.Add(oftIdx,new FileStream(editsFileName,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite));
             container.editsFileStreamWriter.Add(oftIdx,new StreamWriter(container.editsFileStream[oftIdx]));
             container.editsFileStreamReader.Add(oftIdx,new StreamReader(container.editsFileStream[oftIdx]));
            }
           }
           if(container.editsFileStream.TryGetValue(oftIdx,out FileStream fileStream)){
            StreamReader fileStreamReader=container.editsFileStreamReader[oftIdx];
            fileStream.Position=0L;
            fileStreamReader.DiscardBufferedData();
            string line;
            while((line=fileStreamReader.ReadLine())!=null){
             if(string.IsNullOrEmpty(line)){continue;}
             result=true;
             int vCoordStringStart=line.IndexOf("vCoord=(");
             if(vCoordStringStart>=0){
                vCoordStringStart+=8;
              int vCoordStringEnd=line.IndexOf(") , ",vCoordStringStart);
              string vCoordString=line.Substring(vCoordStringStart,vCoordStringEnd-vCoordStringStart);
              string[]xyzString=vCoordString.Split(',');
              int vCoordx=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              int vCoordy=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              int vCoordz=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              Vector3Int vCoord=new Vector3Int(vCoordx,vCoordy,vCoordz);
              int editStringStart=vCoordStringEnd+4;
              editStringStart=line.IndexOf("waterEditOutputData=",editStringStart);
              if(editStringStart>=0){
               int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
               string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
               WaterEditOutputData edit=WaterEditOutputData.Parse(editString);
               if(editData!=null&&!editData.ContainsKey(vCoord)){
                editData.Add(vCoord,edit);
               }
               if(voxels!=null){
                lock(voxels){
                 voxels[GetvxlIdx(vCoord.x,vCoord.y,vCoord.z)]=new VoxelWater(edit.density,edit.previousDensity,edit.sleeping,edit.evaporateAfter);
                }
               }
              }
             }
            }
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.waterFiles_rwl.ExitReadLock();
          }
          return result;
         }
         container.result=hadChanges;
         sw.Stop();
         //Log.DebugMessage("WaterSpreadingMultithreaded Execute time:"+sw.ElapsedMilliseconds+" ms");
        }
#region Absorb
        bool   VerticalAbsorbSetVoxel(VoxelWater fromVoxel,int atvxlIdx,double absorbValue,int oftIdx,Vector2Int cnkRgn,bool hasBlockage,Dictionary<Vector3Int,WaterEditOutputData>editData){
         //  se bloqueado por terreno, retorna falso
         VoxelWater curVoxel=GetVoxelAt(atvxlIdx,oftIdx,cnkRgn,editData);
         double previousDensity=curVoxel.density;
         double density=curVoxel.density-absorbValue;
         if(beforeAbsorbValue[oftIdx].TryGetValue(atvxlIdx,out VoxelWater beforeAbsorbVoxel)){
          previousDensity=beforeAbsorbVoxel.density;
          if(beforeAbsorbVoxel.density>0f){
           double newDensity=beforeAbsorbVoxel.density-absorbValue;
           if(newDensity<curVoxel.density){
            density=newDensity;
           }
          }
         }
         bool wasAbsorbed=false;
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(fromVoxel.evaporateAfter,curVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(newVoxel.density>0d){//  
          newVoxel.density=curVoxel.density;
         }else{
          wasAbsorbed=true;
         }
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         //Log.DebugMessage("VerticalAbsorbSetVoxel:absorbValue:"+absorbValue+":newVoxel.density:"+newVoxel.density);
         voxels[oftIdx][atvxlIdx]=newVoxel;
         SetPostAbsorb(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasAbsorbed,oftIdx);
         if(hasBlockage){
          return false;
         }
         return true;
        }
        bool HorizontalAbsorbSetVoxel(VoxelWater fromVoxel,int atvxlIdx,double absorbValue,int oftIdx,Vector2Int cnkRgn,bool hasBlockage,Dictionary<Vector3Int,WaterEditOutputData>editData){
         bool result=true;
         //  se bloqueado por terreno, retorna falso
         VoxelWater curVoxel=GetVoxelAt(atvxlIdx,oftIdx,cnkRgn,editData);
         //Log.DebugMessage("curVoxel.density-(absorbValue-5.0d):"+(curVoxel.density-(absorbValue-5.0d)));
         double previousDensity=curVoxel.density;
         double density=curVoxel.density-(absorbValue-5.0d);
         if(beforeAbsorbValue[oftIdx].TryGetValue(atvxlIdx,out VoxelWater beforeAbsorbVoxel)){
          previousDensity=beforeAbsorbVoxel.density;
          if(beforeAbsorbVoxel.density>0f){
           double newDensity=beforeAbsorbVoxel.density-(absorbValue-5.0d);
           if(newDensity<curVoxel.density){
            density=newDensity;
           }
          }
         }
         bool wasAbsorbed=false;
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(fromVoxel.evaporateAfter,curVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(newVoxel.density>0d){//  
          newVoxel.density=curVoxel.density;
          result=false;
         }else{
          wasAbsorbed=true;
         }
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
          result=false;
         }
         Log.DebugMessage("HorizontalAbsorbSetVoxel:absorbValue:"+absorbValue+":newVoxel.density:"+newVoxel.density+":hasBlockage:"+hasBlockage);
         voxels[oftIdx][atvxlIdx]=newVoxel;
         if(!result){
          if(voxels[oftIdx].TryGetValue(atvxlIdx,out VoxelWater v)){
           if(v.density>0f){
            v.sleeping=false;
            voxels[oftIdx][atvxlIdx]=v;
           }
          }
         }
         SetPostAbsorb(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasAbsorbed,oftIdx);
         return result;
        }
        void SetPostAbsorb(int atvxlIdx,VoxelWater oldVoxel,VoxelWater newVoxel,bool wasAbsorbed,int oftIdx){
         if(wasAbsorbed){
          if(!beforeAbsorbValue[oftIdx].ContainsKey(atvxlIdx)){
           beforeAbsorbValue[oftIdx][atvxlIdx]=oldVoxel;
          }
          afterAbsorbValue[oftIdx][atvxlIdx]=newVoxel;
         }
        }
        //  esta parte é igual ao sistema de absorver e espalhar verificando o valor do voxel
        //  no loop principal acima, mas está preparando os valores para chunks vizinhos
        void SetNeighbourhoodAbsorb(Vector3Int fromvCoord,VoxelWater fromVoxel,Vector3Int atvCoord,double absorbValue,int oftIdx){
          if(neighbourhoodAbsorbing[oftIdx].TryGetValue(atvCoord,out var currentAbsorb)){
           double absorbValueOld=currentAbsorb.absorb;
           double absorbValueNew=fromVoxel.previousDensity-fromVoxel.density;
           if(absorbValueNew>absorbValueOld){
             neighbourhoodAbsorbing[oftIdx][atvCoord]=(absorbValueNew,fromvCoord,fromVoxel);
           }
          }else{
             neighbourhoodAbsorbing[oftIdx].Add(atvCoord,(fromVoxel.previousDensity-fromVoxel.density,fromvCoord,fromVoxel));
          }
        }
        void OnGotNeighbourhoodAbsorb(int fromvxlIdx,VoxelWater fromVoxel,int atvxlIdx,double absorbValue,int oftIdx){
         Vector3Int fromvCoord=GetvCoord[fromvxlIdx];
         Vector3Int   atvCoord=GetvCoord[  atvxlIdx];
         SetNeighbourhoodAbsorb(fromvCoord,fromVoxel,atvCoord,absorbValue,oftIdx);
        }
#endregion
    #region Binary NeighbourWaterAbsorb
        internal static void BinaryWriteNeighbourWaterAbsorb(Vector3Int fromvCoord,VoxelWater fromVoxel,Vector3Int atvCoord,double absorbValue,BinaryWriter cacheBinaryWriter){
         int fromvxlIdx=GetvxlIdx(fromvCoord.x,fromvCoord.y,fromvCoord.z);
         BinaryWritevxlIdx(fromvxlIdx,cacheBinaryWriter);
         BinaryWriteVoxelWater(fromVoxel,cacheBinaryWriter);
         int   atvxlIdx=GetvxlIdx(  atvCoord.x,  atvCoord.y,  atvCoord.z);
         BinaryWritevxlIdx(  atvxlIdx,cacheBinaryWriter);
         cacheBinaryWriter.Write(absorbValue);
        }
         internal static(int fromvxlIdx,VoxelWater fromVoxel,int atvxlIdx,double absorbValue)BinaryReadNeighbourWaterAbsorb(BinaryReader cacheBinaryReader){
          int fromvxlIdx        =cacheBinaryReader.ReadInt32();
          VoxelWater fromVoxel  =BinaryReadVoxelWater(cacheBinaryReader);
          int   atvxlIdx        =cacheBinaryReader.ReadInt32();
          double absorb         =cacheBinaryReader.ReadDouble();
          return(fromvxlIdx,fromVoxel,atvxlIdx,absorb);
         }
    #endregion
#region Spread
        bool   VerticalSpreadSetVoxel(VoxelWater fromVoxel,int atvxlIdx,double spreadValue,int oftIdx,Vector2Int cnkRgn,bool hasBlockage,Dictionary<Vector3Int,WaterEditOutputData>editData){
         //  se bloqueado por terreno, retorna falso
         VoxelWater curVoxel=GetVoxelAt(atvxlIdx,oftIdx,cnkRgn,editData);
         double previousDensity=curVoxel.density;
         double density=spreadValue;/* sem perda porque é vertical */
         if(beforeSpreadValue[oftIdx].TryGetValue(atvxlIdx,out VoxelWater beforeSpreadVoxel)){
          previousDensity=beforeSpreadVoxel.density;
         }
         bool wasSpreaded=false;
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(fromVoxel.evaporateAfter,curVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         if(curVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
          SetPostSpread(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasSpreaded,oftIdx);
          return true;//  true porque não foi bloqueado por terreno e encostou em outro voxel de água (sim para waterfall, então não espalhar horizontalmente)
         }
         //Log.DebugMessage("VerticalSpreadSetVoxel:spreadValue:"+spreadValue);
         voxels[oftIdx][atvxlIdx]=newVoxel;
         wasSpreaded=true;
         SetPostSpread(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasSpreaded,oftIdx);
         if(hasBlockage){
          return false;
         }
         return true;
        }
        void HorizontalSpreadSetVoxel(VoxelWater fromVoxel,int atvxlIdx,double spreadValue,int oftIdx,Vector2Int cnkRgn,bool hasBlockage,Dictionary<Vector3Int,WaterEditOutputData>editData){
         //  se bloqueado por terreno, retorna falso
         VoxelWater curVoxel=GetVoxelAt(atvxlIdx,oftIdx,cnkRgn,editData);
         double previousDensity=curVoxel.density;
         double density=spreadValue-5.0d;
         if(beforeSpreadValue[oftIdx].TryGetValue(atvxlIdx,out VoxelWater beforeSpreadVoxel)){
          previousDensity=beforeSpreadVoxel.density;
         }
         bool wasSpreaded=false;
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(fromVoxel.evaporateAfter,curVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         if(curVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
          SetPostSpread(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasSpreaded,oftIdx);
          return;
         }
         if(newVoxel.density<30.0d){
          SetPostSpread(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasSpreaded,oftIdx);
          return;
         }
         //Log.DebugMessage("HorizontalSpreadSetVoxel:spreadValue:"+spreadValue);
         voxels[oftIdx][atvxlIdx]=newVoxel;
         wasSpreaded=true;
         SetPostSpread(atvxlIdx,oldVoxel:curVoxel,newVoxel,wasSpreaded,oftIdx);
        }
        void SetPostSpread(int atvxlIdx,VoxelWater oldVoxel,VoxelWater newVoxel,bool wasSpreaded,int oftIdx){
         if(wasSpreaded){
          if(!beforeSpreadValue[oftIdx].ContainsKey(atvxlIdx)){
           beforeSpreadValue[oftIdx][atvxlIdx]=oldVoxel;
          }
          afterSpreadValue[oftIdx][atvxlIdx]=newVoxel;
         }
        }
        void SetNeighbourhoodSpread(Vector3Int fromvCoord,VoxelWater fromVoxel,Vector3Int atvCoord,double spreadValue,int oftIdx){
         if(fromVoxel.density==fromVoxel.previousDensity){
          if(neighbourhoodSpreading[oftIdx].TryGetValue(atvCoord,out var currentSpread)){
           double spreadValueOld=currentSpread.spread;
           double spreadValueNew=fromVoxel.density;
           if(spreadValueNew>spreadValueOld){
             neighbourhoodSpreading[oftIdx][atvCoord]=(spreadValueNew,fromvCoord,fromVoxel);
           }
          }else{
             neighbourhoodSpreading[oftIdx].Add(atvCoord,(fromVoxel.density,fromvCoord,fromVoxel));
          }
         }else{
          if(neighbourhoodSpreading[oftIdx].TryGetValue(atvCoord,out var currentSpread)){
           double spreadValueOld=currentSpread.spread;
           double spreadValueNew=fromVoxel.density-fromVoxel.previousDensity;
           if(spreadValueNew>spreadValueOld){
             neighbourhoodSpreading[oftIdx][atvCoord]=(spreadValueNew,fromvCoord,fromVoxel);
           }
          }else{
             neighbourhoodSpreading[oftIdx].Add(atvCoord,(fromVoxel.density-fromVoxel.previousDensity,fromvCoord,fromVoxel));
          }
         }
        }
        void OnGotNeighbourhoodSpread(int fromvxlIdx,VoxelWater fromVoxel,int atvxlIdx,double spreadValue,int oftIdx){
         Vector3Int fromvCoord=GetvCoord[fromvxlIdx];
         Vector3Int   atvCoord=GetvCoord[  atvxlIdx];
         SetNeighbourhoodSpread(fromvCoord,fromVoxel,atvCoord,spreadValue,oftIdx);
        }
#endregion
    #region Binary NeighbourWaterSpread
        internal static void BinaryWriteNeighbourWaterSpread(Vector3Int fromvCoord,VoxelWater fromVoxel,Vector3Int atvCoord,double spreadValue,BinaryWriter cacheBinaryWriter){
         int fromvxlIdx=GetvxlIdx(fromvCoord.x,fromvCoord.y,fromvCoord.z);
         BinaryWritevxlIdx(fromvxlIdx,cacheBinaryWriter);
         BinaryWriteVoxelWater(fromVoxel,cacheBinaryWriter);
         int   atvxlIdx=GetvxlIdx(  atvCoord.x,  atvCoord.y,  atvCoord.z);
         BinaryWritevxlIdx(  atvxlIdx,cacheBinaryWriter);
         cacheBinaryWriter.Write(spreadValue);
        }
         internal static(int fromvxlIdx,VoxelWater fromVoxel,int atvxlIdx,double spreadValue)BinaryReadNeighbourWaterSpread(BinaryReader cacheBinaryReader){
          int fromvxlIdx        =cacheBinaryReader.ReadInt32();
          VoxelWater fromVoxel  =BinaryReadVoxelWater(cacheBinaryReader);
          int   atvxlIdx        =cacheBinaryReader.ReadInt32();
          double spread         =cacheBinaryReader.ReadDouble();
          return(fromvxlIdx,fromVoxel,atvxlIdx,spread);
         }
    #endregion
#region Util
        internal VoxelWater GetVoxelAt(int vxlIdx,int oftIdx,Vector2Int cnkRgn,Dictionary<Vector3Int,WaterEditOutputData>editData){
         Vector3Int vCoord=GetvCoord[vxlIdx];
         VoxelWater voxel;
         if(!voxels[oftIdx].TryGetValue(vxlIdx,out voxel)){
          //  valor do bioma
          if(!biomeVoxels.TryGetValue(vxlIdx,out VoxelWater biomeVoxel)){
           Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                        noiseInput.z+=cnkRgn.y;
           VoxelSystem.biome.SetvxlWater(
            noiseInput,
             noiseCache1,
              oftIdx,
               vCoord.z+vCoord.x*Depth,
                ref biomeVoxel
           );
           biomeVoxels[vxlIdx]=biomeVoxel;
          }
          voxel=biomeVoxel;
         }
         if(editData!=null){
          if(editData.TryGetValue(vCoord,out WaterEditOutputData voxelData)){
           voxel.density        =voxelData.density;
           voxel.previousDensity=voxelData.previousDensity;
           voxel.sleeping       =voxelData.sleeping;
           voxel.evaporateAfter =voxelData.evaporateAfter;
          }
         }
         return voxel;
        }
#endregion
#region Binary vxlIdx
        internal static void BinaryWritevxlIdx(int vxlIdx,BinaryWriter cacheBinaryWriter){
         cacheBinaryWriter.Write(vxlIdx);
        }
         internal static int BinaryReadvxlIdx(BinaryReader cacheBinaryReader){
          int vxlIdx            =cacheBinaryReader.ReadInt32();
          return vxlIdx;
         }
#endregion
#region Binary VoxelWater
        internal static void BinaryWriteVoxelWater(VoxelWater voxel,BinaryWriter cacheBinaryWriter){
         cacheBinaryWriter.Write(voxel.density);
         cacheBinaryWriter.Write(voxel.previousDensity);
         cacheBinaryWriter.Write(voxel.sleeping);
         cacheBinaryWriter.Write(voxel.evaporateAfter);
        }
         internal static VoxelWater BinaryReadVoxelWater(BinaryReader cacheBinaryReader){
          double density        =cacheBinaryReader.ReadDouble();
          double previousDensity=cacheBinaryReader.ReadDouble();
          bool sleeping         =cacheBinaryReader.ReadBoolean();
          float evaporateAfter  =cacheBinaryReader.ReadSingle();
          return new VoxelWater(density,previousDensity,sleeping,evaporateAfter);
         }
#endregion
    }
}