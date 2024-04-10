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
     internal readonly Dictionary<int,FileStream>cacheStream=new();
     internal readonly Dictionary<int,BinaryWriter>cacheBinaryWriter=new();
     internal readonly Dictionary<int,BinaryReader>cacheBinaryReader=new();
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          foreach(var sW in editsFileStreamWriter){if(sW.Value!=null){sW.Value.Dispose();}}
          foreach(var sR in editsFileStreamReader){if(sR.Value!=null){sR.Value.Dispose();}}
          editsFileStream      .Clear();
          editsFileStreamWriter.Clear();
          editsFileStreamReader.Clear();
          foreach(var cBW in cacheBinaryWriter){if(cBW.Value!=null){cBW.Value.Dispose();}}
          foreach(var cBR in cacheBinaryReader){if(cBR.Value!=null){cBR.Value.Dispose();}}
          cacheStream      .Clear();
          cacheBinaryWriter.Clear();
          cacheBinaryReader.Clear();
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingBackgroundContainer>{
     internal const double stopSpreadingDensity=30.0d;
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
     readonly Dictionary<int,Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>>absorbing=new();
     readonly Dictionary<int,Dictionary<Vector3Int,(double spread,VoxelWater voxel)>>spreading=new();
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly Dictionary<int,Voxel[]>terrainVoxels=new Dictionary<int,Voxel[]>();
     readonly     double[][][]     noiseCache1=new     double[biome.cacheLength][][];
     readonly MaterialId[][][]materialIdCache1=new MaterialId[biome.cacheLength][][];
        internal WaterSpreadingMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i]=new Dictionary<int,VoxelWater>();
                       absorbing[i]=new Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>();
                       spreading[i]=new Dictionary<Vector3Int,(double spread,VoxelWater voxel)>();
         }
         for(int i=0;i<biome.cacheLength;++i){
               noiseCache1[i]=new     double[9][];
          materialIdCache1[i]=new MaterialId[9][];
         }
        }
        protected override void Cleanup(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i].Clear();
         }
         foreach(var cnkIdxAbsorbingPair in absorbing){cnkIdxAbsorbingPair.Value.Clear();}
         foreach(var cnkIdxSpreadingPair in spreading){cnkIdxSpreadingPair.Value.Clear();}
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
        internal static(int vxlIdx,VoxelWater voxel)BinaryReadVoxelWater(BinaryReader cacheBinaryReader){
         int vxlIdx            =cacheBinaryReader.ReadInt32();
         double density        =cacheBinaryReader.ReadDouble();
         double previousDensity=cacheBinaryReader.ReadDouble();
         bool sleeping         =cacheBinaryReader.ReadBoolean();
         float evaporateAfter  =cacheBinaryReader.ReadSingle();
         return(vxlIdx,new VoxelWater(density,previousDensity,sleeping,evaporateAfter));
        }
        internal static void BinaryWriteVoxelWater(KeyValuePair<int,VoxelWater>kvp,BinaryWriter cacheBinaryWriter){
         cacheBinaryWriter.Write(kvp.Key);
         cacheBinaryWriter.Write(kvp.Value.density);
         cacheBinaryWriter.Write(kvp.Value.previousDensity);
         cacheBinaryWriter.Write(kvp.Value.sleeping);
         cacheBinaryWriter.Write(kvp.Value.evaporateAfter);
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
         int oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
         int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         if(!waterEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,WaterEditOutputData>editData1)){
          editData1=new Dictionary<Vector3Int,WaterEditOutputData>();
         }
         dataFromFileToMerge.Add(cCoord1,editData1);
         //  carregar arquivo aqui
         bool hasChangedIndex=false;
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          hasChangedIndex=true;
         }
         VoxelSystem.Concurrent.waterCache_rwl.EnterWriteLock();
         try{
          if(hasChangedIndex){
           if(container.cacheStream.TryGetValue(oftIdx1,out FileStream cS)){
            if(VoxelSystem.Concurrent.waterCacheIds.TryGetValue(cS,out var cacheOldId)){
             if(VoxelSystem.Concurrent.waterCache.TryGetValue(cacheOldId.cnkIdx,out var oldIdCache)&&
              object.ReferenceEquals(oldIdCache.stream,cS)
             ){
              VoxelSystem.Concurrent.waterCache.Remove(cacheOldId.cnkIdx);
              VoxelSystem.Concurrent.waterCacheIds.Remove(cS);
              Log.DebugMessage("removed old value for cacheOldId.cnkIdx:"+cacheOldId.cnkIdx);
             }
            }
            container.cacheBinaryWriter[oftIdx1].Dispose();
            container.cacheBinaryReader[oftIdx1].Dispose();
            container.cacheStream.Remove(oftIdx1);
            container.cacheBinaryWriter.Remove(oftIdx1);
            container.cacheBinaryReader.Remove(oftIdx1);
           }
          }
          if(!container.cacheStream.ContainsKey(oftIdx1)){
           string cacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterCacheFileFormat,VoxelSystem.Concurrent.waterCachePath,container.cCoord.Value.x,container.cCoord.Value.y);
           container.cacheStream.Add(oftIdx1,new FileStream(cacheFileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
           container.cacheBinaryWriter.Add(oftIdx1,new BinaryWriter(container.cacheStream[oftIdx1]));
           container.cacheBinaryReader.Add(oftIdx1,new BinaryReader(container.cacheStream[oftIdx1]));
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterCache_rwl.ExitWriteLock();
         }
         VoxelSystem.Concurrent.waterCache_rwl.EnterReadLock();
         try{
          if(container.cacheStream.TryGetValue(oftIdx1,out FileStream fileStream)){
           lock(fileStream){
            BinaryReader binReader=container.cacheBinaryReader[oftIdx1];
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
           //  TO DO: valor do bioma
           voxel=new VoxelWater(0.0d,0.0d,true,-1f);
          }
          if(editData1.ContainsKey(vCoord1)){
           WaterEditOutputData voxelData=editData1[vCoord1];
           voxel.density        =voxelData.density;
           voxel.previousDensity=voxelData.previousDensity;
           voxel.sleeping       =voxelData.sleeping;
           voxel.evaporateAfter =voxelData.evaporateAfter;
          }
          voxel.sleeping=voxel.sleeping&&(voxel.density==voxel.previousDensity);
          if(voxel.density!=0.0d||voxel.previousDensity!=0.0d){
           if(!voxel.sleeping){
            hadChanges=true;
            if(voxel.density<voxel.previousDensity){
             Log.DebugMessage("to absorb:"+vCoord1);
             absorbing[oftIdx1][vCoord1]=(voxel.previousDensity-voxel.density,voxel);
            }else{
             Log.DebugMessage("to spread:"+vCoord1);
             spreading[oftIdx1][vCoord1]=(voxel.density-voxel.previousDensity,voxel);
            }
           }
           voxels[oftIdx1][vxlIdx1]=voxel;
          }else{
           voxels[oftIdx1].Remove(vxlIdx1);
          }
         }}}
         //  calcular absorb e spread aqui
         foreach(var vCoordAbsorbingPair in absorbing[oftIdx1]){
          Vector3Int vCoord2=vCoordAbsorbingPair.Key;
                 int vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
              double absorbValue=vCoordAbsorbingPair.Value.absorb;
          VoxelWater absorbVoxel=vCoordAbsorbingPair.Value.voxel;
          Vector3Int vCoord3=new Vector3Int(vCoord2.x,vCoord2.y-1,vCoord2.z);
          bool waterfall=VerticalAbsorb();
          bool VerticalAbsorb(){
           if(!(vCoord3.y>=0)){
            return false;
           }else{
            Log.DebugMessage("VerticalAbsorb:"+vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return Absorb();
            bool Absorb(){
             //  se bloqueado por terreno, retorna falso
             VoxelWater oldVoxel;
             if(voxels[oftIdx1].TryGetValue(vxlIdx3,out VoxelWater v3)){
              oldVoxel=v3;
             }else{
              //  TO DO: valor do bioma
              oldVoxel=new VoxelWater(0.0d,0.0d,true,-1f);
             }
             VoxelWater newVoxel=new VoxelWater(oldVoxel.density-absorbValue,oldVoxel.density,false,Mathf.Max(absorbVoxel.evaporateAfter,oldVoxel.evaporateAfter));
             newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
             if(newVoxel.density>0d){//  
              newVoxel.density=oldVoxel.density;
             }
             Log.DebugMessage("VerticalAbsorb:Absorb:"+absorbValue+":newVoxel.density:"+newVoxel.density);
             voxels[oftIdx1][vxlIdx3]=newVoxel;
             return true;
            }
           }
          }
          absorbVoxel.previousDensity=absorbVoxel.density;
          absorbVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=absorbVoxel;
         }
         foreach(var vCoordSpreadingPair in spreading[oftIdx1]){
          Vector3Int vCoord2=vCoordSpreadingPair.Key;
                 int vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
              double spreadValue=vCoordSpreadingPair.Value.spread;
          VoxelWater spreadVoxel=vCoordSpreadingPair.Value.voxel;
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
           }else if(HasBlockageAt(vCoord3)){
            return false;
           }else{
            Log.DebugMessage("VerticalSpread:"+vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return Spread();
            bool Spread(){
             //  se bloqueado por terreno, retorna falso
             VoxelWater oldVoxel;
             if(voxels[oftIdx1].TryGetValue(vxlIdx3,out VoxelWater v3)){
              oldVoxel=v3;
             }else{
              //  TO DO: valor do bioma
              oldVoxel=new VoxelWater(0.0d,0.0d,true,-1f);
             }
             VoxelWater newVoxel=new VoxelWater(spreadValue/* sem perda porque é vertical */,oldVoxel.density,false,Mathf.Max(spreadVoxel.evaporateAfter,oldVoxel.evaporateAfter));
             newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
             if(oldVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
              return true;//  true porque não foi bloqueado por terreno e encostou em outro voxel de água (sim para waterfall, então não espalhar horizontalmente)
             }
             Log.DebugMessage("VerticalSpread:Spread:"+spreadValue);
             voxels[oftIdx1][vxlIdx3]=newVoxel;
             return true;
            }
           }
          }
          void HorizontalSpread(){
           Log.DebugMessage("HorizontalSpread:"+vCoord3);
           if(vCoord3.x<0||vCoord3.x>=Width||
              vCoord3.z<0||vCoord3.z>=Depth
           ){
            return;
           }
           if(HasBlockageAt(vCoord3)){
            return;
           }else{
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            Spread();
            void Spread(){
             //  se bloqueado por terreno, retorna falso
             VoxelWater oldVoxel;
             if(voxels[oftIdx1].TryGetValue(vxlIdx3,out VoxelWater v3)){
              oldVoxel=v3;
             }else{
              //  TO DO: valor do bioma
              oldVoxel=new VoxelWater(0.0d,0.0d,true,-1f);
             }
             VoxelWater newVoxel=new VoxelWater(spreadValue-5.0d,oldVoxel.density,false,Mathf.Max(spreadVoxel.evaporateAfter,oldVoxel.evaporateAfter));
             newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
             if(oldVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
              return;
             }
             if(newVoxel.density<30.0d){
              return;
             }
             Log.DebugMessage("HorizontalSpread:Spread:"+spreadValue);
             voxels[oftIdx1][vxlIdx3]=newVoxel;
            }
           }
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
         VoxelSystem.Concurrent.waterCache_rwl.EnterWriteLock();
         try{
          if(container.cacheStream.TryGetValue(oftIdx1,out FileStream fileStream)){
           BinaryWriter binWriter=container.cacheBinaryWriter[oftIdx1];
           BinaryReader binReader=container.cacheBinaryReader[oftIdx1];
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
         //  salvar neighbourhood changes
         //for(int x=-1;x<=1;x++){
         //for(int y=-1;y<=1;y++){
         //}}
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
         Log.DebugMessage("WaterSpreadingMultithreaded Execute time:"+sw.ElapsedMilliseconds+" ms");
        }
    }
}