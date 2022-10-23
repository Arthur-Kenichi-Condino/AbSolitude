#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditing;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingContainer;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using System.Text;
using System.IO;
using System.Globalization;

namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditingContainer:BackgroundContainer{
     internal object[]terrainSynchronization;
     internal readonly Queue<TerrainEditRequest>requests=new Queue<TerrainEditRequest>();
     internal readonly HashSet<int>dirty=new HashSet<int>();
    }
    internal class VoxelTerrainEditingMultithreaded:BaseMultithreaded<VoxelTerrainEditingContainer>{
        internal struct TerrainEditOutputData{
         public double density;
         public MaterialId material;
            internal TerrainEditOutputData(double density,MaterialId material){
             this.density=density;this.material=material;
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"terrainEditOutputData={{ density={0} , material={1} , }}",density,(ushort)material);
            }
            internal static TerrainEditOutputData Parse(string s){
             TerrainEditOutputData result=new TerrainEditOutputData();
             double density=0d;
             MaterialId material=MaterialId.Air;
             int densityStringStart=s.IndexOf("density=");
             if(densityStringStart>=0){
                densityStringStart+=8;
              int densityStringEnd=s.IndexOf(" , ",densityStringStart);
              string densityString=s.Substring(densityStringStart,densityStringEnd-densityStringStart);
              //Log.DebugMessage("densityString:"+densityString);
              density=double.Parse(densityString,NumberStyles.Any,CultureInfoUtil.en_US);
             }
             int materialStringStart=s.IndexOf("material=");
             if(materialStringStart>=0){
                materialStringStart+=9;
              int materialStringEnd=s.IndexOf(" , ",materialStringStart);
              string materialString=s.Substring(materialStringStart,materialStringEnd-materialStringStart);
              //Log.DebugMessage("materialString:"+materialString);
              material=(MaterialId)ushort.Parse(materialString,NumberStyles.Any,CultureInfoUtil.en_US);
              //Log.DebugMessage("material:"+material);
             }
             result.density=density;
             result.material=material;
             return result;
            }
        }
     internal readonly Queue<Dictionary<Vector3Int,TerrainEditOutputData>>terrainEditOutputDataPool=new Queue<Dictionary<Vector3Int,TerrainEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,TerrainEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,TerrainEditOutputData>>dataForSavingToFile=new();
     readonly StringBuilder stringBuilder=new StringBuilder();
        protected override void Cleanup(){
         foreach(var editData in dataFromFileToMerge){editData.Value.Clear();terrainEditOutputDataPool.Enqueue(editData.Value);}
         dataFromFileToMerge.Clear();
         foreach(var editData in dataForSavingToFile){editData.Value.Clear();terrainEditOutputDataPool.Enqueue(editData.Value);}
         dataForSavingToFile.Clear();
        }
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainEditingMultithreaded:Execute()");
         container.dirty.Clear();
         while(container.requests.Count>0){
          TerrainEditRequest editRequest=container.requests.Dequeue();
          Vector3    center    =editRequest.center;
          Vector3Int size      =editRequest.size;
          double     density   =editRequest.density;
          MaterialId material  =editRequest.material;
          Vector3Int smoothness=editRequest.smoothness;
          switch(editRequest.mode){
           case(EditMode.PlaceCube):{
            Log.DebugMessage("EditMode.PlaceCube");
            //  TO DO: validade values before start
                Vector2Int cCoord1=vecPosTocCoord(center ),        cCoord3;
                Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1),        cnkRgn3;
                Vector3Int vCoord1=vecPosTovCoord(center ),vCoord2,vCoord3;
                //  y
                for(int y=0;y<size.y;++y){for(vCoord2=new Vector3Int(vCoord1.x,vCoord1.y-y,vCoord1.z);vCoord2.y<=vCoord1.y+y;vCoord2.y+=y*2){
                        if(vCoord2.y>=0&&vCoord2.y<Height){
                //  x
                for(int x=0;x<size.x;++x){for(vCoord2.x=vCoord1.x-x                                  ;vCoord2.x<=vCoord1.x+x;vCoord2.x+=x*2){
                //  z
                for(int z=0;z<size.z;++z){for(vCoord2.z=vCoord1.z-z                                  ;vCoord2.z<=vCoord1.z+z;vCoord2.z+=z*2){
                         cCoord3=cCoord1;
                         cnkRgn3=cnkRgn1;
                         vCoord3=vCoord2;
                         if(vCoord3.x<0||vCoord3.x>=Width||
                            vCoord3.z<0||vCoord3.z>=Depth
                         ){
                          ValidateCoord(ref cnkRgn3,ref vCoord3);
                          cCoord3=cnkRgnTocCoord(cnkRgn3);
                         }
                         int cnkIdx3=GetcnkIdx(cCoord3.x,cCoord3.y);
                         double resultDensity=density;
                         if(smoothness.x>1&&
                            smoothness.y>1&&
                            smoothness.z>1)
                         {
                          float hardness=1f;
                          Vector3Int smoothnessBeginDis=smoothness-Vector3Int.one;
                             void SmoothDensity(float dis,float totalDis,float beginDis){
                              float value=(totalDis-dis)/(beginDis+1f);
                              value=(value+1f)/2f;
                              value*=.99f;
                              hardness*=value;
                             }
                          if(y>=smoothnessBeginDis.y){
                           SmoothDensity(y,size.y,smoothnessBeginDis.y);
                           if(x>=smoothnessBeginDis.x){
                            SmoothDensity(x,size.x,smoothnessBeginDis.x);
                            if(z>=smoothnessBeginDis.z){
                             SmoothDensity(z,size.z,smoothnessBeginDis.z);
                            }
                           }else if(z>=smoothnessBeginDis.z){
                            SmoothDensity(z,size.z,smoothnessBeginDis.z);
                           }
                          }else if(x>=smoothnessBeginDis.x){
                           SmoothDensity(x,size.x,smoothnessBeginDis.x);
                           if(z>=smoothnessBeginDis.z){
                            SmoothDensity(z,size.z,smoothnessBeginDis.z);
                           }
                          }else if(z>=smoothnessBeginDis.z){
                           SmoothDensity(z,size.z,smoothnessBeginDis.z);
                          }
                          Log.DebugMessage("hardness:"+hardness);
                          resultDensity*=hardness;
                         }
                         //  TO DO: get current file data to merge
                         MergeEdits(cCoord3,vCoord3,cnkRgn3,resultDensity,material);
                 if(z==0){break;}
                }}
                 if(x==0){break;}
                }}
                        }
                 if(y==0){break;}
                }}
            break;
           }
           case(EditMode.PlaceSphere):{
            Log.DebugMessage("EditMode.PlaceSphere");
            //  TO DO: PlaceSphere but validade values before start
                Vector2Int cCoord1=vecPosTocCoord(center ),        cCoord3;
                Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1),        cnkRgn3;
                Vector3Int vCoord1=vecPosTovCoord(center ),vCoord2,vCoord3;
                //  TO DO: calcular valores para suavização
                //  calculate hypotenuses
                float hyp_size_yx=Mathf.Sqrt(Mathf.Pow(size.y,2)+Mathf.Pow(size.x,2));
                float hyp_size_xz=Mathf.Sqrt(Mathf.Pow(size.x,2)+Mathf.Pow(size.z,2));
                float hyp_size_yx_xz=Mathf.Sqrt(Mathf.Pow(hyp_size_yx,2)+Mathf.Pow(hyp_size_xz,2));
                //  y
                for(int y=0;y<size.y;++y){for(vCoord2=new Vector3Int(vCoord1.x,vCoord1.y-y,vCoord1.z);vCoord2.y<=vCoord1.y+y;vCoord2.y+=y*2){
                        if(vCoord2.y>=0&&vCoord2.y<Height){
                //  x
                for(int x=0;x<size.x;++x){for(vCoord2.x=vCoord1.x-x                                  ;vCoord2.x<=vCoord1.x+x;vCoord2.x+=x*2){
                         float sqrt_yx_2=Mathf.Sqrt(Mathf.Pow(y,2)+Mathf.Pow(x,2));
                //  z
                for(int z=0;z<size.z;++z){for(vCoord2.z=vCoord1.z-z                                  ;vCoord2.z<=vCoord1.z+z;vCoord2.z+=z*2){
                         cCoord3=cCoord1;
                         cnkRgn3=cnkRgn1;
                         vCoord3=vCoord2;
                         if(vCoord3.x<0||vCoord3.x>=Width||
                            vCoord3.z<0||vCoord3.z>=Depth
                         ){
                          ValidateCoord(ref cnkRgn3,ref vCoord3);
                          cCoord3=cnkRgnTocCoord(cnkRgn3);
                         }
                         int cnkIdx3=GetcnkIdx(cCoord3.x,cCoord3.y);
                         //  do sphere placing here
                         double resultDensity=density;
                         //  calculate hypotenuses
                         float hyp_dis_xz=Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(z,2));
                         float hyp_dis_zy=Mathf.Sqrt(Mathf.Pow(z,2)+Mathf.Pow(y,2));
                         float hyp_dis_yx_xz=Mathf.Sqrt(Mathf.Pow(sqrt_yx_2,2)+Mathf.Pow(hyp_dis_xz,2));
                         float hyp_dis_yx_xz_zy=Mathf.Sqrt(Mathf.Pow(hyp_dis_yx_xz,2)+Mathf.Pow(hyp_dis_zy,2));
                         if(hyp_size_yx_xz>0f){
                          resultDensity*=(hyp_size_yx_xz-hyp_dis_yx_xz_zy)/hyp_size_yx_xz;
                          Log.DebugMessage("(hyp_size_yx_xz-hyp_dis_yx_xz_zy)/hyp_size_yx_xz:"+((hyp_size_yx_xz-hyp_dis_yx_xz_zy)/hyp_size_yx_xz));
                          Log.DebugMessage("resultDensity:"+resultDensity);
                         }
                         MergeEdits(cCoord3,vCoord3,cnkRgn3,resultDensity,material);
                 if(z==0){break;}
                }}
                 if(x==0){break;}
                }}
                        }
                 if(y==0){break;}
                }}
            break;
           }
          }
         }
         void MergeEdits(Vector2Int cCoord,Vector3Int vCoord,Vector2Int cnkRgn,double resultDensity,MaterialId material){
          if(!dataFromFileToMerge.ContainsKey(cCoord)){
           if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
            editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
           }
           dataFromFileToMerge.Add(cCoord,editData);
           //  TO DO: load data here
           LoadDataFromFile(cCoord,editData);
          }
          Voxel currentVoxel;
          if(dataFromFileToMerge.ContainsKey(cCoord)&&dataFromFileToMerge[cCoord].ContainsKey(vCoord)){
           TerrainEditOutputData voxelData=dataFromFileToMerge[cCoord][vCoord];
           currentVoxel=new Voxel(voxelData.density,Vector3.zero,voxelData.material);
          }else{
           currentVoxel=new Voxel();
           Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
                                        noiseInput.z+=cnkRgn.y;
           VoxelSystem.biome.Setvxl(
            noiseInput,
             null,
              null,
               0,
                vCoord.z+vCoord.x*Depth,
                 ref currentVoxel
           );
          }
          resultDensity=Math.Max(resultDensity,currentVoxel.density);
          if(material==MaterialId.Air&&!(-resultDensity>=-isoLevel)){
           resultDensity=-resultDensity;
          }
          if(!dataForSavingToFile.ContainsKey(cCoord)){
           if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
            editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
           }
           dataForSavingToFile.Add(cCoord,editData);
          }
          dataForSavingToFile[cCoord][vCoord]=new TerrainEditOutputData(resultDensity,-resultDensity>=-isoLevel?MaterialId.Air:material);
          //  TO DO: add neighbours that are dirty too
          for(int ngbx=-1;ngbx<=1;ngbx++){
          for(int ngby=-1;ngby<=1;ngby++){
           Vector2Int cCoord4=cCoord+new Vector2Int(ngbx,ngby);
           if(Math.Abs(cCoord4.x)>=MaxcCoordx||
              Math.Abs(cCoord4.y)>=MaxcCoordy)
           {
            continue;
           }
           int cnkIdx4=GetcnkIdx(cCoord4.x,cCoord4.y);
           container.dirty.Add(cnkIdx4);
          }}
         }
         void LoadDataFromFile(Vector2Int cCoord,Dictionary<Vector3Int,TerrainEditOutputData>editData){
          VoxelSystem.Concurrent.terrainFileDatarwl.EnterReadLock();
          try{
           string fileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord.x,cCoord.y);
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
              //Log.DebugMessage("vCoordString:"+vCoordString);
              string[]xyzString=vCoordString.Split(',');
              int vCoordx=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              int vCoordy=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              int vCoordz=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              Vector3Int vCoord=new Vector3Int(vCoordx,vCoordy,vCoordz);
              if(!editData.ContainsKey(vCoord)){
               int editStringStart=vCoordStringEnd+4;
               editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
               if(editStringStart>=0){
                int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
                string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
                //Log.DebugMessage("editString:"+editString);
                TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
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
           VoxelSystem.Concurrent.terrainFileDatarwl.ExitReadLock();
          }
         }
         VoxelSystem.Concurrent.terrainFileDatarwl.EnterWriteLock();
         foreach(object syn in container.terrainSynchronization){
          Monitor.Enter(syn);
         }
         try{
          //  Write file safely here
          foreach(var cCoordDataForSavingPair in dataForSavingToFile){
           Vector2Int cCoord=cCoordDataForSavingPair.Key;
           Dictionary<Vector3Int,TerrainEditOutputData>editData=cCoordDataForSavingPair.Value;
           stringBuilder.Clear();
           string fileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord.x,cCoord.y);
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
              editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
              if(editStringStart>=0){
               int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
               string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
               TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
               editData.Add(vCoord,edit);
               //Log.DebugMessage("added previous edit from file at vCoord:"+vCoord);
              }
             }
            }
           }
           foreach(var voxelEdited in editData){
            Vector3Int vCoord=voxelEdited.Key;
            TerrainEditOutputData edit=voxelEdited.Value;
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
          foreach(object syn in container.terrainSynchronization){
           Monitor.Exit(syn);
          }
          VoxelSystem.Concurrent.terrainFileDatarwl.ExitWriteLock();
         }
        }
    }
}