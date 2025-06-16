#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.Editing.VoxelWaterEditing;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
namespace AKCondinoO.Voxels.Water.Editing{
    internal class VoxelWaterEditingContainer:BackgroundContainer{
     internal readonly Queue<WaterEditRequest>requests=new Queue<WaterEditRequest>();
    }
    internal class VoxelWaterEditingMultithreaded:BaseMultithreaded<VoxelWaterEditingContainer>{
        internal struct WaterEditOutputData{
         public bool wakeUp;
         public double density;
         public double previousDensity;
         public bool hasBlockage;
         public float evaporateAfter;
            internal WaterEditOutputData(bool wakeUp,double density,double previousDensity,bool hasBlockage,float evaporateAfter){
             this.wakeUp=wakeUp;this.density=density;this.previousDensity=previousDensity;this.hasBlockage=hasBlockage;this.evaporateAfter=evaporateAfter;
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"waterEditOutputData={{ wakeUp={0} , density={1} , previousDensity={2} , hasBlockage={3} , evaporateAfter={4} , }}",wakeUp,density,previousDensity,hasBlockage,evaporateAfter);
            }
            internal static WaterEditOutputData Parse(string s){
             WaterEditOutputData result=new WaterEditOutputData();
             bool wakeUp=false;
             double density=0d;
             double previousDensity=0d;
             bool hasBlockage=false;
             float evaporateAfter=-1f;
             int wakeUpStringStart=s.IndexOf("wakeUp=");
             if(wakeUpStringStart>=0){
                wakeUpStringStart+=7;
              int wakeUpStringEnd=s.IndexOf(" , ",wakeUpStringStart);
              string wakeUpString=s.Substring(wakeUpStringStart,wakeUpStringEnd-wakeUpStringStart);
              wakeUp=bool.Parse(wakeUpString);
             }
             int densityStringStart=s.IndexOf("density=");
             if(densityStringStart>=0){
                densityStringStart+=8;
              int densityStringEnd=s.IndexOf(" , ",densityStringStart);
              string densityString=s.Substring(densityStringStart,densityStringEnd-densityStringStart);
              //Log.DebugMessage("densityString:"+densityString);
              density=double.Parse(densityString,NumberStyles.Any,CultureInfoUtil.en_US);
             }
             int previousDensityStringStart=s.IndexOf("previousDensity=");
             if(previousDensityStringStart>=0){
                previousDensityStringStart+=16;
              int previousDensityStringEnd=s.IndexOf(" , ",previousDensityStringStart);
              string previousDensityString=s.Substring(previousDensityStringStart,previousDensityStringEnd-previousDensityStringStart);
              previousDensity=double.Parse(previousDensityString,NumberStyles.Any,CultureInfoUtil.en_US);
             }
             int hasBlockageStringStart=s.IndexOf("hasBlockage=");
             if(hasBlockageStringStart>=0){
                hasBlockageStringStart+=12;
              int hasBlockageStringEnd=s.IndexOf(" , ",hasBlockageStringStart);
              string hasBlockageString=s.Substring(hasBlockageStringStart,hasBlockageStringEnd-hasBlockageStringStart);
              hasBlockage=bool.Parse(hasBlockageString);
             }
             int evaporateAfterStringStart=s.IndexOf("evaporateAfter=");
             if(evaporateAfterStringStart>=0){
                evaporateAfterStringStart+=15;
              int evaporateAfterStringEnd=s.IndexOf(" , ",evaporateAfterStringStart);
              string evaporateAfterString=s.Substring(evaporateAfterStringStart,evaporateAfterStringEnd-evaporateAfterStringStart);
              evaporateAfter=float.Parse(evaporateAfterString);
             }
             result.wakeUp=wakeUp;
             result.density=density;
             result.previousDensity=previousDensity;
             result.hasBlockage=hasBlockage;
             result.evaporateAfter=evaporateAfter;
             return result;
            }
        }
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly StringBuilder stringBuilder=new StringBuilder();
        protected override void Cleanup(){
         foreach(var editData in dataFromFileToMerge){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataFromFileToMerge.Clear();
         foreach(var editData in dataForSavingToFile){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataForSavingToFile.Clear();
        }
        protected override void Execute(){
         Log.DebugMessage("VoxelWaterEditingMultithreaded:Execute()");
         while(container.requests.Count>0){
          WaterEditRequest editRequest=container.requests.Dequeue();
          Vector3    center         =editRequest.center;
          Vector3Int size           =editRequest.size;
          bool       wakeUp         =editRequest.wakeUp;
          double     density        =editRequest.density;
          double     previousDensity=editRequest.previousDensity;
          bool       hasBlockage    =editRequest.hasBlockage;
          float      evaporateAfter =editRequest.evaporateAfter;
          Vector2Int cCoord1=vecPosTocCoord(center );
          Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1);
          int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
          Vector3Int vCoord1=vecPosTovCoord(center );
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          double resultDensity=density;
          MergeEdits(cCoord1,vCoord1,cnkRgn1,wakeUp,resultDensity,previousDensity,hasBlockage,evaporateAfter);
         }
         void MergeEdits(Vector2Int cCoord,Vector3Int vCoord,Vector2Int cnkRgn,bool wakeUp,double resultDensity,double previousDensity,bool hasBlockage,float evaporateAfter){
          resultDensity=Math.Clamp(resultDensity,0.0d,100.0d);
          if(!dataFromFileToMerge.ContainsKey(cCoord)){
           if(!waterEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,WaterEditOutputData>editData)){
            editData=new Dictionary<Vector3Int,WaterEditOutputData>();
           }
           dataFromFileToMerge.Add(cCoord,editData);
           //  TO DO: load data here
           LoadDataFromFile(cCoord,editData);
          }
          VoxelWater currentVoxel;
          if(dataFromFileToMerge.ContainsKey(cCoord)&&dataFromFileToMerge[cCoord].ContainsKey(vCoord)){
           WaterEditOutputData voxelData=dataFromFileToMerge[cCoord][vCoord];
           currentVoxel=new VoxelWater(voxelData.wakeUp,voxelData.density,voxelData.previousDensity,voxelData.hasBlockage,voxelData.evaporateAfter);
          }else{
           //  valor do bioma
           currentVoxel=new VoxelWater();
           Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                        noiseInput.z+=cnkRgn.y;
           VoxelSystem.biome.SetvxlWater(
            noiseInput,
             null,
              0,
               vCoord.z+vCoord.x*Depth,
                ref currentVoxel
           );
          }
          if(previousDensity<0d){
           previousDensity=currentVoxel.density;
          }
          hasBlockage=false;
          if(!dataForSavingToFile.ContainsKey(cCoord)){
           if(!waterEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,WaterEditOutputData>editData)){
            editData=new Dictionary<Vector3Int,WaterEditOutputData>();
           }
           dataForSavingToFile.Add(cCoord,editData);
          }
          dataForSavingToFile[cCoord][vCoord]=new WaterEditOutputData(wakeUp,resultDensity,previousDensity,hasBlockage,evaporateAfter);
          //  TO DO: add neighbours that are dirty too
         }
         void LoadDataFromFile(Vector2Int cCoord,Dictionary<Vector3Int,WaterEditOutputData>editData){
          VoxelSystem.Concurrent.waterFiles_rwl.EnterReadLock();
          try{
           string fileName=string.Format(CultureInfoUtil.en_US,VoxelWaterEditing.waterEditingFileFormat,VoxelWaterEditing.waterEditingPath,cCoord.x,cCoord.y);
           if(File.Exists(fileName)){
            FileStream fileStream=new FileStream(fileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            StreamReader fileStreamReader=new StreamReader(fileStream);
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
              if(!editData.ContainsKey(vCoord)){
               int editStringStart=vCoordStringEnd+4;
               editStringStart=line.IndexOf("waterEditOutputData=",editStringStart);
               if(editStringStart>=0){
                int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
                string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
                WaterEditOutputData edit=WaterEditOutputData.Parse(editString);
                editData.Add(vCoord,edit);
               }
              }
             }
            }
            fileStream      .Dispose();
            fileStreamReader.Dispose();
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.waterFiles_rwl.ExitReadLock();
          }
         }
         VoxelSystem.Concurrent.waterFiles_rwl.EnterWriteLock();
         try{
          //  salvar dados em arquivos
          foreach(var cCoordDataForSavingPair in dataForSavingToFile){
           Vector2Int cCoord=cCoordDataForSavingPair.Key;
           Dictionary<Vector3Int,WaterEditOutputData>editData=cCoordDataForSavingPair.Value;
           stringBuilder.Clear();
           string fileName=string.Format(CultureInfoUtil.en_US,VoxelWaterEditing.waterEditingFileFormat,VoxelWaterEditing.waterEditingPath,cCoord.x,cCoord.y);
           Log.DebugMessage("save edit data in fileName:"+fileName);
           FileStream fileStream=new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           StreamWriter fileStreamWriter=new StreamWriter(fileStream);
           StreamReader fileStreamReader=new StreamReader(fileStream);
           //  TO DO: read or write to file here then dispose
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
             //Log.DebugMessage("vCoordString:"+vCoordString);
             string[]xyzString=vCoordString.Split(',');
             int vCoordx=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             int vCoordy=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             int vCoordz=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             Vector3Int vCoord=new Vector3Int(vCoordx,vCoordy,vCoordz);
             if(!editData.ContainsKey(vCoord)){
              int editStringStart=vCoordStringEnd+4;
              editStringStart=line.IndexOf("waterEditOutputData=",editStringStart);
              if(editStringStart>=0){
               int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
               string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
               WaterEditOutputData edit=WaterEditOutputData.Parse(editString);
               editData.Add(vCoord,edit);
               //Log.DebugMessage("added previous edit from file at vCoord:"+vCoord);
              }
             }
            }
           }
           foreach(var voxelEdited in editData){
            Vector3Int vCoord=voxelEdited.Key;
            WaterEditOutputData edit=voxelEdited.Value;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ vCoord={0} , {{ {1} }} }} , endOfLine{2}",vCoord,edit.ToString(),Environment.NewLine);
           }
           fileStream.SetLength(0L);
           fileStreamWriter.Write(stringBuilder.ToString());
           fileStreamWriter.Flush();
           //  dispose
           fileStreamWriter.Dispose();
           fileStreamReader.Dispose();
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterFiles_rwl.ExitWriteLock();
         }
        }
    }
}