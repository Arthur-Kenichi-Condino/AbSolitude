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
     internal float deltaTime;
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
     internal const double stopSpreadingDensity=30.0d;
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
      readonly Voxel[][]terrainVoxels=new Voxel[9][];
     readonly Dictionary<int,Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>>absorbing=new();
     readonly Dictionary<int,Dictionary<Vector3Int,(double spread,VoxelWater voxel)>>spreading=new();
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
        internal WaterSpreadingMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i]=new Dictionary<int,VoxelWater>();
                       absorbing[i]=new Dictionary<Vector3Int,(double absorb,VoxelWater voxel)>();
                       spreading[i]=new Dictionary<Vector3Int,(double spread,VoxelWater voxel)>();
         }
        }
        protected override void Cleanup(){
         foreach(var voxelsDictionary in voxels){voxelsDictionary.Clear();}
         foreach(var cnkIdxAbsorbingPair in absorbing){cnkIdxAbsorbingPair.Value.Clear();}
         foreach(var cnkIdxSpreadingPair in spreading){cnkIdxSpreadingPair.Value.Clear();}
         foreach(var editData in dataFromFileToMerge){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataFromFileToMerge.Clear();
         foreach(var editData in dataForSavingToFile){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataForSavingToFile.Clear();
        }
        protected override void Execute(){
         if(container.cnkIdx==null){
          return;
         }
         //Log.DebugMessage("WaterSpreadingMultithreaded:Execute()");
         Vector2Int cCoord1=container.cCoord.Value;
         int oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
         if(!waterEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,WaterEditOutputData>editData1)){
          editData1=new Dictionary<Vector3Int,WaterEditOutputData>();
         }
         dataFromFileToMerge.Add(cCoord1,editData1);
         //  carregar arquivo aqui
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
          LoadDataFromFile(cCoord1,editData1,voxels[oftIdx1]);
         }else{
          VoxelSystem.Concurrent.water_rwl.EnterReadLock();
          try{
           lock(container.voxelsOutput){
            for(int i=0;i<container.voxelsOutput.Length;++i){
             voxels[oftIdx1][i]=container.voxelsOutput[i];
            }
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitReadLock();
          }
          LoadDataFromFile(cCoord1,editData1,null);
         }
         VoxelSystem.Concurrent.terrain_rwl.EnterReadLock();
         try{
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrain_rwl.ExitReadLock();
         }
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          //  carregar dados do bioma aqui em voxels,
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          VoxelWater voxel;
          if(!voxels[oftIdx1].TryGetValue(vxlIdx1,out voxel)){
           //  TO DO: valor do bioma
           voxel=new VoxelWater(0.0d,0.0d,true,-1f);
          }
          voxels[oftIdx1].Remove(vxlIdx1);
          if(editData1.ContainsKey(vCoord1)){
           WaterEditOutputData voxelData=editData1[vCoord1];
           //VoxelWater voxelFromFile=new VoxelWater(voxelData.density,voxelData.previousDensity,voxelData.sleeping,voxelData.evaporateAfter);
           //voxel.previousDensity=Math.Max(voxelFromFile.previousDensity,Math.Max(voxelFromFile.density,Math.Max(voxel.previousDensity,voxel.density)));
           //voxel.previousDensity=Math.Max(voxelFromFile.previousDensity,Math.Max(voxelFromFile.density,Math.Max(voxel.previousDensity,voxel.density)));
           //voxel.sleeping=(voxel.sleeping&&voxelFromFile.sleeping);
           //voxel.evaporateAfter=Mathf.Max(voxel.evaporateAfter,voxelFromFile.evaporateAfter);
           //voxel.density=voxelFromFile.density;
           //voxel.density
           voxel.density        =voxelData.density;
           voxel.previousDensity=voxelData.previousDensity;
           voxel.sleeping       =voxelData.sleeping;
           voxel.evaporateAfter =voxelData.evaporateAfter;
          }
          voxel.sleeping=voxel.sleeping&&(voxel.density==voxel.previousDensity);
          //  voxel is not default
          //if(voxel.density==voxel.previousDensity){
          // voxel.sleeping=true;
          //}else{
          // voxel.sleeping=false;
          //}
          if(voxel.density!=0.0d||voxel.previousDensity!=0.0d){
           if(!voxel.sleeping){
            if(voxel.density<voxel.previousDensity){
             Log.DebugMessage("to absorb:"+vCoord1);
             absorbing[oftIdx1][vCoord1]=(voxel.previousDensity-voxel.density,voxel);
            }else{
             Log.DebugMessage("to spread:"+vCoord1);
             spreading[oftIdx1][vCoord1]=(voxel.density-voxel.previousDensity,voxel);
            }
           }
           voxels[oftIdx1][vxlIdx1]=voxel;
          }
          //if(!voxel.sleeping){
          // voxel.previousDensity=voxel.density;
          //voxels[oftIdx1][vxlIdx1]=voxel;
          //}
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
             if(!voxels[oftIdx1].TryGetValue(vxlIdx3,out oldVoxel)){
              //  TO DO: valor do bioma
              oldVoxel=new VoxelWater(0.0d,0.0d,true,-1f);
             }
             VoxelWater newVoxel=new VoxelWater(oldVoxel.density-absorbValue,oldVoxel.density,false,Mathf.Max(absorbVoxel.evaporateAfter,oldVoxel.evaporateAfter));
             newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
             if(newVoxel.density>0d){//  
              newVoxel.density=oldVoxel.density;
             }
             Log.DebugMessage("VerticalAbsorb:Absorb:"+vxlIdx3);
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
          bool VerticalSpread(){
           if(!(vCoord3.y>=0)){
            return false;
           }else{
            Log.DebugMessage("VerticalSpread:"+vCoord3);
            int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            return Spread();
            bool Spread(){
             //  se bloqueado por terreno, retorna falso
             VoxelWater oldVoxel;
             if(!voxels[oftIdx1].TryGetValue(vxlIdx3,out oldVoxel)){
              //  TO DO: valor do bioma
              oldVoxel=new VoxelWater(0.0d,0.0d,true,-1f);
             }
             VoxelWater newVoxel=new VoxelWater(spreadVoxel.density/* sem perda porque é vertical */,oldVoxel.density,false,Mathf.Max(spreadVoxel.evaporateAfter,oldVoxel.evaporateAfter));
             newVoxel.density=Math.Clamp(newVoxel.density,0.0d,100.0d);
             if(oldVoxel.density>=newVoxel.density){//  não há necessidade de espalhar para o voxel caso ele já tenha uma densidade maior
              return true;//  true porque não foi bloqueado por terreno e encostou em outro voxel de água (sim para waterfall, então não espalhar horizontalmente)
             }
             Log.DebugMessage("VerticalSpread:Spread:"+vxlIdx3);
             voxels[oftIdx1][vxlIdx3]=newVoxel;
             return true;
            }
           }
          }
          spreadVoxel.previousDensity=spreadVoxel.density;
          spreadVoxel.sleeping=true;
          voxels[oftIdx1][vxlIdx2]=spreadVoxel;
         }
         bool HasBlockageAt(Vector3Int vCoord){//  testar com array de voxels do terreno ou array de bool para objetos de estruturas
          return false;
         }
         //// e também modificar vizinhos aqui, salvando em arquivo
         //for(int x=-1;x<=1;x++){
         //for(int y=-1;y<=1;y++){
         //}}
         VoxelSystem.Concurrent.waterFileData_rwl.EnterWriteLock();
         try{
         // //  salvar
         // for(int x=-1;x<=1;x++){
         // for(int y=-1;y<=1;y++){
         // }}
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterFileData_rwl.ExitWriteLock();
         }
         VoxelSystem.Concurrent.water_rwl.EnterReadLock();
         try{
          lock(container.voxelsOutput){
           for(int i=0;i<container.voxelsOutput.Length;++i){
            if(!voxels[oftIdx1].TryGetValue(i,out VoxelWater voxel)){
             continue;
            }
            container.voxelsOutput[i]=voxel;
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
           VoxelSystem.Concurrent.waterVoxelsOutput[container.cnkIdx.Value]=container.voxelsOutput;
           VoxelSystem.Concurrent.waterVoxelsId[container.voxelsOutput]=(container.cCoord.Value,container.cnkRgn.Value,container.cnkIdx.Value);
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.water_rwl.ExitWriteLock();
          }
          container.lastcCoord=container.cCoord;
          container.lastcnkRgn=container.cnkRgn;
          container.lastcnkIdx=container.cnkIdx;
         }
         void LoadDataFromFile(Vector2Int cCoord,Dictionary<Vector3Int,WaterEditOutputData>editData,Dictionary<int,VoxelWater>voxelsDictionary){
          VoxelSystem.Concurrent.waterFileData_rwl.EnterReadLock();
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
               if(voxelsDictionary!=null){
                voxelsDictionary[GetvxlIdx(vCoord.x,vCoord.y,vCoord.z)]=new VoxelWater(edit.density,edit.previousDensity,edit.sleeping,edit.evaporateAfter);
               }
              }
             }
            }
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.waterFileData_rwl.ExitReadLock();
          }
         }
        }
    }
}