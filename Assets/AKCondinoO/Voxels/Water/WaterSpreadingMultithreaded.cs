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
     internal readonly Dictionary<int,FileStream>neighbourhoodCacheStream=new();
     internal readonly Dictionary<int,BinaryWriter>neighbourhoodCacheBinaryWriter=new();
     internal readonly Dictionary<int,BinaryReader>neighbourhoodCacheBinaryReader=new();
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          foreach(var sW in editsFileStreamWriter){if(sW.Value!=null){sW.Value.Dispose();}}
          foreach(var sR in editsFileStreamReader){if(sR.Value!=null){sR.Value.Dispose();}}
          editsFileStream      .Clear();
          editsFileStreamWriter.Clear();
          editsFileStreamReader.Clear();
          foreach(var nCBW in neighbourhoodCacheBinaryWriter){if(nCBW.Value!=null){nCBW.Value.Dispose();}}
          foreach(var nCBR in neighbourhoodCacheBinaryReader){if(nCBR.Value!=null){nCBR.Value.Dispose();}}
          neighbourhoodCacheStream      .Clear();
          neighbourhoodCacheBinaryWriter.Clear();
          neighbourhoodCacheBinaryReader.Clear();
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
     readonly Dictionary<int,Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>>neighbourhoodAbsorb=new();
     readonly Dictionary<int,Dictionary<Vector3Int,(double spread,VoxelWater voxel)>>neighbourhoodSpread=new();
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
     readonly Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>absorbing=new();
     readonly Dictionary<Vector3Int,(double spread,VoxelWater voxel)>spreading=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>absorbed=new();
     readonly Dictionary<int,Dictionary<int,VoxelWater>>spreaded=new();
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly Dictionary<int,Voxel[]>terrainVoxels=new Dictionary<int,Voxel[]>();
     readonly     double[][][]     noiseCache1=new     double[biome.cacheLength][][];
     readonly MaterialId[][][]materialIdCache1=new MaterialId[biome.cacheLength][][];
        internal WaterSpreadingMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       neighbourhoodAbsorb[i]=new Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>();
                       neighbourhoodSpread[i]=new Dictionary<Vector3Int,(double spread,VoxelWater voxel)>();
                       voxels[i]=new Dictionary<int,VoxelWater>();
                       absorbed[i]=new();
                       spreaded[i]=new();
         }
         for(int i=0;i<biome.cacheLength;++i){
               noiseCache1[i]=new     double[9][];
          materialIdCache1[i]=new MaterialId[9][];
         }
        }
        protected override void Cleanup(){
         foreach(var oftIdxNeighbourhoodAbsorbPair in neighbourhoodAbsorb){oftIdxNeighbourhoodAbsorbPair.Value.Clear();}
         foreach(var oftIdxNeighbourhoodSpreadPair in neighbourhoodSpread){oftIdxNeighbourhoodSpreadPair.Value.Clear();}
         for(int i=0;i<voxels.Length;++i){
                       voxels[i].Clear();
         }
         absorbing.Clear();
         spreading.Clear();
         foreach(var oftIdxAbsorbedPair in absorbed){oftIdxAbsorbedPair.Value.Clear();}
         foreach(var oftIdxSpreadedPair in spreaded){oftIdxSpreadedPair.Value.Clear();}
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
        internal static void BinaryWriteVoxelWater(KeyValuePair<int,VoxelWater>kvp,BinaryWriter cacheBinaryWriter){
         cacheBinaryWriter.Write(kvp.Key);
         cacheBinaryWriter.Write(kvp.Value.density);
         cacheBinaryWriter.Write(kvp.Value.previousDensity);
         cacheBinaryWriter.Write(kvp.Value.sleeping);
         cacheBinaryWriter.Write(kvp.Value.evaporateAfter);
        }
        internal static(int vxlIdx,VoxelWater voxel)BinaryReadVoxelWater(BinaryReader cacheBinaryReader){
         int vxlIdx            =cacheBinaryReader.ReadInt32();
         double density        =cacheBinaryReader.ReadDouble();
         double previousDensity=cacheBinaryReader.ReadDouble();
         bool sleeping         =cacheBinaryReader.ReadBoolean();
         float evaporateAfter  =cacheBinaryReader.ReadSingle();
         return(vxlIdx,new VoxelWater(density,previousDensity,sleeping,evaporateAfter));
        }
        internal static void BinaryWriteNeighbourWaterSpread(Vector3Int vCoord,double spread,BinaryWriter cacheBinaryWriter){
         int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
         cacheBinaryWriter.Write(vxlIdx);
         cacheBinaryWriter.Write(spread);
        }
     //readonly Dictionary<int,double>neighbourAbsorb=new Dictionary<int,double>();
     //readonly Dictionary<int,double>neighbourSpread=new Dictionary<int,double>();
        internal static(int vxlIdx,double spread)BinaryReadNeighbourWaterSpread(BinaryReader cacheBinaryReader){
         int vxlIdx            =cacheBinaryReader.ReadInt32();
         double spread         =cacheBinaryReader.ReadDouble();
         return(vxlIdx,spread);
        }
        bool VerticalAbsorbSetVoxel(int oftIdx,Vector2Int cnkRgn,int vxlIdx,double absorbValue,VoxelWater absorbVoxel,bool hasBlockage){
         //  se bloqueado por terreno, retorna falso
         VoxelWater oldVoxel;
         if(voxels[oftIdx].TryGetValue(vxlIdx,out VoxelWater v3)){
          oldVoxel=v3;
         }else{
          //  valor do bioma
          oldVoxel=new VoxelWater();
          Vector3Int vCoord=GetvCoord[vxlIdx];
          Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                       noiseInput.z+=cnkRgn.y;
          VoxelSystem.biome.SetvxlWater(
           noiseInput,
            noiseCache1,
             oftIdx,
              vCoord.z+vCoord.x*Depth,
               ref oldVoxel
          );
         }
         double previousDensity=oldVoxel.density;
         double density=oldVoxel.density-absorbValue;
         bool wasAbsorbed;
         if(wasAbsorbed=absorbed[oftIdx].TryGetValue(vxlIdx,out VoxelWater absorbedVoxel)){
          previousDensity=absorbedVoxel.density;
          if(absorbedVoxel.density>0f){
           double newDensity=absorbedVoxel.density-absorbValue;
           if(newDensity<oldVoxel.density){
            density=newDensity;
           }
          }
         }
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(absorbVoxel.evaporateAfter,oldVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(newVoxel.density>0d){//  
          newVoxel.density=oldVoxel.density;
         }
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         Log.DebugMessage("VerticalAbsorb:Absorb:"+absorbValue+":newVoxel.density:"+newVoxel.density);
         voxels[oftIdx][vxlIdx]=newVoxel;
         if(!wasAbsorbed){
          absorbed[oftIdx][vxlIdx]=oldVoxel;
         }
         if(hasBlockage){
          return false;
         }
         return true;
        }
        bool HorizontalAbsorbSetVoxel(int oftIdx,Vector2Int cnkRgn,int vxlIdx,double absorbValue,VoxelWater absorbVoxel,bool hasBlockage){
         bool result=true;
         //  se bloqueado por terreno, retorna falso
         VoxelWater oldVoxel;
         if(voxels[oftIdx].TryGetValue(vxlIdx,out VoxelWater v3)){
          oldVoxel=v3;
         }else{
          //  valor do bioma
          oldVoxel=new VoxelWater();
          Vector3Int vCoord=GetvCoord[vxlIdx];
          Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                       noiseInput.z+=cnkRgn.y;
          VoxelSystem.biome.SetvxlWater(
           noiseInput,
            noiseCache1,
             oftIdx,
              vCoord.z+vCoord.x*Depth,
               ref oldVoxel
          );
         }
         Log.DebugMessage("oldVoxel.density-(absorbValue-5.0d):"+(oldVoxel.density-(absorbValue-5.0d)));
         double previousDensity=oldVoxel.density;
         double density=oldVoxel.density-(absorbValue-5.0d);
         bool wasAbsorbed;
         if(wasAbsorbed=absorbed[oftIdx].TryGetValue(vxlIdx,out VoxelWater absorbedVoxel)){
          previousDensity=absorbedVoxel.density;
          if(absorbedVoxel.density>0f){
           double newDensity=absorbedVoxel.density-(absorbValue-5.0d);
           if(newDensity<oldVoxel.density){
            density=newDensity;
           }
          }
         }
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(absorbVoxel.evaporateAfter,oldVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(newVoxel.density>0d){//  
          newVoxel.density=oldVoxel.density;
          result=false;
         }
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
          result=false;
         }
         Log.DebugMessage("HorizontalAbsorb:Absorb:"+absorbValue+":newVoxel.density:"+newVoxel.density+":hasBlockage:"+hasBlockage);
         voxels[oftIdx][vxlIdx]=newVoxel;
         if(!wasAbsorbed){
          absorbed[oftIdx][vxlIdx]=oldVoxel;
         }
         return result;
        }
        bool VerticalSpreadSetVoxel(int oftIdx,Vector2Int cnkRgn,int vxlIdx,double spreadValue,VoxelWater spreadVoxel,bool hasBlockage){
         //  se bloqueado por terreno, retorna falso
         VoxelWater oldVoxel;
         if(voxels[oftIdx].TryGetValue(vxlIdx,out VoxelWater v3)){
          oldVoxel=v3;
         }else{
          //  valor do bioma
          oldVoxel=new VoxelWater();
          Vector3Int vCoord=GetvCoord[vxlIdx];
          Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                       noiseInput.z+=cnkRgn.y;
          VoxelSystem.biome.SetvxlWater(
           noiseInput,
            noiseCache1,
             oftIdx,
              vCoord.z+vCoord.x*Depth,
               ref oldVoxel
          );
         }
         double previousDensity=oldVoxel.density;
         double density=spreadValue;/* sem perda porque é vertical */
         bool wasSpreaded;
         if(wasSpreaded=spreaded[oftIdx].TryGetValue(vxlIdx,out VoxelWater spreadedVoxel)){
          previousDensity=spreadedVoxel.density;
          double newDensity=spreadValue;
          if(newDensity>oldVoxel.density){
           density=newDensity;
          }
         }
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(spreadVoxel.evaporateAfter,oldVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         if(oldVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
          return true;//  true porque não foi bloqueado por terreno e encostou em outro voxel de água (sim para waterfall, então não espalhar horizontalmente)
         }
         Log.DebugMessage("VerticalSpread:Spread:"+spreadValue);
         voxels[oftIdx][vxlIdx]=newVoxel;
         if(!wasSpreaded){
          spreaded[oftIdx][vxlIdx]=oldVoxel;
         }
         if(hasBlockage){
          return false;
         }
         return true;
        }
        void HorizontalSpreadSetVoxel(int oftIdx,Vector2Int cnkRgn,int vxlIdx,double spreadValue,VoxelWater spreadVoxel,bool hasBlockage){
         //  se bloqueado por terreno, retorna falso
         VoxelWater oldVoxel;
         if(voxels[oftIdx].TryGetValue(vxlIdx,out VoxelWater v3)){
          oldVoxel=v3;
         }else{
          //  valor do bioma
          oldVoxel=new VoxelWater();
          Vector3Int vCoord=GetvCoord[vxlIdx];
          Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                       noiseInput.z+=cnkRgn.y;
          VoxelSystem.biome.SetvxlWater(
           noiseInput,
            noiseCache1,
             oftIdx,
              vCoord.z+vCoord.x*Depth,
               ref oldVoxel
          );
         }
         double previousDensity=oldVoxel.density;
         double density=spreadValue-5.0d;
         bool wasSpreaded;
         if(wasSpreaded=spreaded[oftIdx].TryGetValue(vxlIdx,out VoxelWater spreadedVoxel)){
          previousDensity=spreadedVoxel.density;
          double newDensity=spreadValue-5.0d;
          if(newDensity>oldVoxel.density){
           density=newDensity;
          }
         }
         VoxelWater newVoxel=new VoxelWater(density,previousDensity,false,Mathf.Max(spreadVoxel.evaporateAfter,oldVoxel.evaporateAfter));
         newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
         if(hasBlockage){
          newVoxel.sleeping=true;
          newVoxel.previousDensity=newVoxel.density;
         }
         if(oldVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
          return;
         }
         if(newVoxel.density<30.0d){
          return;
         }
         Log.DebugMessage("HorizontalSpread:Spread:"+spreadValue);
         voxels[oftIdx][vxlIdx]=newVoxel;
         if(!wasSpreaded){
          spreaded[oftIdx][vxlIdx]=oldVoxel;
         }
        }
        void SetNeighbourhoodSpread(int oftIdx,(int vxlIdx,double spread)v){
         Vector3Int vCoord=GetvCoord[v.vxlIdx];
         SetNeighbourhoodSpread(oftIdx,vCoord,v.spread);
        }
        void SetNeighbourhoodSpread(int oftIdx,Vector3Int vCoord,double spread){
         if(spread<0d){
          if(neighbourhoodAbsorb[oftIdx].TryGetValue(vCoord,out var currentAbsorb)){
             neighbourhoodAbsorb[oftIdx][vCoord]=(Math.Max(currentAbsorb.absorb,-spread),currentAbsorb.voxel);
          }else{
             neighbourhoodAbsorb[oftIdx].Add(vCoord,(-spread,default(VoxelWater)));
          }
         }else{
          if(neighbourhoodSpread[oftIdx].TryGetValue(vCoord,out var currentSpread)){
             neighbourhoodSpread[oftIdx][vCoord]=(Math.Max(currentSpread.spread, spread),currentSpread.voxel);
          }else{
             neighbourhoodSpread[oftIdx].Add(vCoord,( spread,default(VoxelWater)));
          }
         }
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
            if(container.neighbourhoodCacheStream.TryGetValue(oftIdx2,out FileStream nCS)){
             var neighbourhoodCache=(nCS,
              container.neighbourhoodCacheBinaryWriter[oftIdx2],
              container.neighbourhoodCacheBinaryReader[oftIdx2]);
             if(VoxelSystem.Concurrent.waterNeighbourhoodCacheIds.TryGetValue(nCS,out var cacheOldId)){
              if(VoxelSystem.Concurrent.waterNeighbourhoodCache.TryGetValue(cacheOldId.cnkIdx,out var oldIdCacheList)){
               oldIdCacheList.Remove(neighbourhoodCache);
               if(oldIdCacheList.Count<=0){
                VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.Enqueue(oldIdCacheList);
                VoxelSystem.Concurrent.waterNeighbourhoodCache.Remove(cacheOldId.cnkIdx);
                Log.DebugMessage("removed empty list for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
               }
               Log.DebugMessage("removed old value for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
              }
              VoxelSystem.Concurrent.waterNeighbourhoodCacheIds.Remove(nCS);
             }
             container.neighbourhoodCacheBinaryWriter[oftIdx2].Dispose();
             container.neighbourhoodCacheBinaryReader[oftIdx2].Dispose();
             container.neighbourhoodCacheStream.Remove(oftIdx2);
             container.neighbourhoodCacheBinaryWriter.Remove(oftIdx2);
             container.neighbourhoodCacheBinaryReader.Remove(oftIdx2);
            }
            if(!container.neighbourhoodCacheStream.ContainsKey(oftIdx2)){
             string cacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterNeighbourhoodCacheSpreadingFileFormat,VoxelSystem.Concurrent.waterNeighbourhoodCachePath,cCoord2.x,cCoord2.y);
             //Log.DebugMessage("cacheFileName:"+cacheFileName);
             container.neighbourhoodCacheStream.Add(oftIdx2,new FileStream(cacheFileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
             container.neighbourhoodCacheBinaryWriter.Add(oftIdx2,new BinaryWriter(container.neighbourhoodCacheStream[oftIdx2]));
             container.neighbourhoodCacheBinaryReader.Add(oftIdx2,new BinaryReader(container.neighbourhoodCacheStream[oftIdx2]));
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
           if(container.neighbourhoodCacheStream.TryGetValue(oftIdx2,out FileStream fileStream)){
            lock(fileStream){
             BinaryReader binReader=container.neighbourhoodCacheBinaryReader[oftIdx2];
             fileStream.Position=0L;
             while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
              var v=BinaryReadNeighbourWaterSpread(binReader);
              SetNeighbourhoodSpread(oftIdx2,v);
             }
            }
            if(oftIdx2==oftIdx1){
             VoxelSystem.Concurrent.waterNeighbourhoodCache_rwl.EnterWriteLock();
             try{
              BinaryWriter binWriter=container.neighbourhoodCacheBinaryWriter[oftIdx2];
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
              Log.DebugMessage("removed old value for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
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
             var v=BinaryReadVoxelWater(binReader);
             voxels[oftIdx1][v.vxlIdx]=v.voxel;
            }
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitReadLock();
         }
         bool hadEdits=LoadDataFromFile(cCoord1,editData1,voxels[oftIdx1]);
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          //  carregar dados do bioma aqui em voxels,
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          VoxelWater voxel;
          if(voxels[oftIdx1].TryGetValue(vxlIdx1,out VoxelWater v1)){
           voxel=v1;
          }else{
           //  valor do bioma
           voxel=new VoxelWater();
           Vector3Int noiseInput=vCoord1;noiseInput.x+=cnkRgn1.x;
                                         noiseInput.z+=cnkRgn1.y;
           VoxelSystem.biome.SetvxlWater(
            noiseInput,
             noiseCache1,
              oftIdx1,
               vCoord1.z+vCoord1.x*Depth,
                ref voxel
           );
          }
          if(editData1.ContainsKey(vCoord1)){
           WaterEditOutputData voxelData=editData1[vCoord1];
           voxel.density        =voxelData.density;
           voxel.previousDensity=voxelData.previousDensity;
           voxel.sleeping       =voxelData.sleeping;
           voxel.evaporateAfter =voxelData.evaporateAfter;
          }
          if(neighbourhoodAbsorb[oftIdx1].TryGetValue(vCoord1,out var absorbValue)){
           Log.DebugMessage("cCoord1:"+cCoord1+";absorbValue:"+absorbValue);
           HorizontalAbsorbSetVoxel(oftIdx1,cnkRgn1,vxlIdx1,absorbValue.absorb,absorbValue.voxel,HasBlockageAt(vCoord1));
           hadChanges|=absorbed[oftIdx1].TryGetValue(vxlIdx1,out _);
           voxels[oftIdx1].TryGetValue(vxlIdx1,out voxel);
          }
          if(neighbourhoodSpread[oftIdx1].TryGetValue(vCoord1,out var spreadValue)){
           Log.DebugMessage("cCoord1:"+cCoord1+";spreadValue:"+spreadValue);
           HorizontalSpreadSetVoxel(oftIdx1,cnkRgn1,vxlIdx1,spreadValue.spread,spreadValue.voxel,HasBlockageAt(vCoord1));
           hadChanges|=spreaded[oftIdx1].TryGetValue(vxlIdx1,out _);
           voxels[oftIdx1].TryGetValue(vxlIdx1,out voxel);
          }
          voxel.sleeping=voxel.sleeping&&(voxel.density==voxel.previousDensity);
          if(voxel.density!=0.0d||voxel.previousDensity!=0.0d){
           if(!voxel.sleeping){
            hadChanges=true;
            if(voxel.density<voxel.previousDensity){
             Log.DebugMessage("to absorb:"+vCoord1);
             if(absorbing.TryGetValue(vCoord1,out var currentAbsorb)){
              absorbing[vCoord1]=(Math.Max(currentAbsorb.absorb,voxel.previousDensity-voxel.density),voxel);
             }else{
              absorbing.Add(vCoord1,(voxel.previousDensity-voxel.density,voxel));
             }
            }else if(voxel.density==voxel.previousDensity){
             Log.DebugMessage("to spread (voxel.density==voxel.previousDensity):"+vCoord1);
             if(spreading.TryGetValue(vCoord1,out var currentSpread)){
              spreading[vCoord1]=(Math.Max(currentSpread.spread,voxel.density),voxel);
             }else{
              spreading.Add(vCoord1,(voxel.density,voxel));
             }
            }else{
             Log.DebugMessage("to spread:"+vCoord1);
             if(spreading.TryGetValue(vCoord1,out var currentSpread)){
              spreading[vCoord1]=(Math.Max(currentSpread.spread,voxel.density-voxel.previousDensity),voxel);
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
          VoxelWater absorbVoxel=vCoordAbsorbingPair.Value.voxel;
          if(!absorbVoxel.isCreated){continue;}
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
            Log.DebugMessage("VerticalAbsorb:"+vCoord3);
            bool hasBlockage=HasBlockageAt(vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return VerticalAbsorbSetVoxel(oftIdx1,cnkRgn1,vxlIdx3,absorbValue,absorbVoxel,hasBlockage);
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
            SetNeighbourhoodSpread(oftIdx3,vCoord3,-absorbValue);
            return false;
           }
           Log.DebugMessage("HorizontalAbsorb:"+vCoord3);
           bool hasBlockage=HasBlockageAt(vCoord3);
           int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
           bool absorb=false;
           if(!(absorb=HorizontalAbsorbSetVoxel(oftIdx1,cnkRgn1,vxlIdx3,absorbValue,absorbVoxel,hasBlockage))){
            if(voxels[oftIdx1].TryGetValue(vxlIdx3,out VoxelWater v3)){
             if(v3.density>0f){
              v3.sleeping=false;
              voxels[oftIdx1][vxlIdx3]=v3;
             }
            }
           }
           return absorb;
          }
          absorbVoxel.previousDensity=absorbVoxel.density;
          absorbVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=absorbVoxel;
         }
         foreach(var vCoordSpreadingPair in spreading){
          Vector3Int vCoord2=vCoordSpreadingPair.Key;
                 int vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
              double spreadValue=vCoordSpreadingPair.Value.spread;
          VoxelWater spreadVoxel=vCoordSpreadingPair.Value.voxel;
          if(!spreadVoxel.isCreated){continue;}
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
            Log.DebugMessage("VerticalSpread:"+vCoord3);
            bool hasBlockage=HasBlockageAt(vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return VerticalSpreadSetVoxel(oftIdx1,cnkRgn1,vxlIdx3,spreadValue,spreadVoxel,hasBlockage);
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
            SetNeighbourhoodSpread(oftIdx3,vCoord3, spreadValue);
            return;
           }
           Log.DebugMessage("HorizontalSpread:"+vCoord3);
           bool hasBlockage=HasBlockageAt(vCoord3);
           int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
           HorizontalSpreadSetVoxel(oftIdx1,cnkRgn1,vxlIdx3,spreadValue,spreadVoxel,hasBlockage);
          }
          spreadVoxel.previousDensity=spreadVoxel.density;
          spreadVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=spreadVoxel;
         }
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
           if(container.neighbourhoodCacheStream.TryGetValue(oftIdx2,out FileStream fileStream)){
            //neighbourAbsorb.Clear();
            //neighbourSpread.Clear();
            BinaryWriter binWriter=container.neighbourhoodCacheBinaryWriter[oftIdx2];
            BinaryReader binReader=container.neighbourhoodCacheBinaryReader[oftIdx2];
            fileStream.Position=0L;
            while(binReader.BaseStream.Position!=binReader.BaseStream.Length){
             var v=BinaryReadNeighbourWaterSpread(binReader);
             SetNeighbourhoodSpread(oftIdx2,v);
            }
            fileStream.SetLength(0L);
            foreach(var vCoordAbsorbingPair in neighbourhoodAbsorb[oftIdx2]){
             //Log.DebugMessage("cCoord2:"+cCoord2+";vCoordAbsorbingPair:"+vCoordAbsorbingPair);
             BinaryWriteNeighbourWaterSpread(vCoordAbsorbingPair.Key,-vCoordAbsorbingPair.Value.absorb,binWriter);
            }
            foreach(var vCoordSpreadingPair in neighbourhoodSpread[oftIdx2]){
             //Log.DebugMessage("cCoord2:"+cCoord2+";vCoordSpreadingPair:"+vCoordSpreadingPair);
             BinaryWriteNeighbourWaterSpread(vCoordSpreadingPair.Key, vCoordSpreadingPair.Value.spread,binWriter);
            }
            binWriter.Flush();
            if(hasChangedIndex){
             if(!VoxelSystem.Concurrent.waterNeighbourhoodCache.TryGetValue(cnkIdx2,out var cacheList)){
              if(!VoxelSystem.Concurrent.waterNeighbourhoodCacheListPool.TryDequeue(out cacheList)){
               cacheList=new();
              }
              VoxelSystem.Concurrent.waterNeighbourhoodCache.Add(cnkIdx2,cacheList);
             }
             VoxelSystem.Concurrent.waterNeighbourhoodCache[cnkIdx2].Add((fileStream,binWriter,binReader));
             VoxelSystem.Concurrent.waterNeighbourhoodCacheIds[fileStream]=(cCoord2,cnkRgn2,cnkIdx2);
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
            BinaryWriteVoxelWater(kvp,binWriter);
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
    }
}