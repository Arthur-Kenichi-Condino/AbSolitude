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
              Log.DebugMessage("densityString:"+densityString);
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
          int        smoothness=editRequest.smoothness;
          switch(editRequest.mode){
           case(EditMode.PlaceCube):{
            Log.DebugMessage("EditMode.PlaceCube");
                //  calcular valores para suavização
                float sqrt_yx_1=Mathf.Sqrt(Mathf.Pow(size.y,2)+Mathf.Pow(size.x,2));
                float sqrt_xz_1=Mathf.Sqrt(Mathf.Pow(size.x,2)+Mathf.Pow(size.z,2));
                float sqrt_zy_1=Mathf.Sqrt(Mathf.Pow(size.z,2)+Mathf.Pow(size.y,2));
                 float sqrt_yx_xz_1=Mathf.Sqrt(Mathf.Pow(sqrt_yx_1,2)+Mathf.Pow(sqrt_xz_1,2));
                  float sqrt_yx_xz_zy_1=Mathf.Sqrt(Mathf.Pow(sqrt_yx_xz_1,2)+Mathf.Pow(sqrt_zy_1,2));
                float sqrt_yx_2;
                float sqrt_xz_2;
                float sqrt_zy_2;
                Vector2Int cCoord1=vecPosTocCoord(center ),        cCoord3;
                Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1),        cnkRgn3;
                Vector3Int vCoord1=vecPosTovCoord(center ),vCoord2,vCoord3;
                //  y
                for(int y=0;y<size.y+smoothness;++y){for(vCoord2=new Vector3Int(vCoord1.x,vCoord1.y-y,vCoord1.z);vCoord2.y<=vCoord1.y+y;vCoord2.y+=y*2){
                        if(vCoord2.y>=0&&vCoord2.y<Height){
                //  x
                for(int x=0;x<size.x+smoothness;++x){for(vCoord2.x=vCoord1.x-x                                  ;vCoord2.x<=vCoord1.x+x;vCoord2.x+=x*2){
                         sqrt_yx_2=Mathf.Sqrt(Mathf.Pow(y,2)+Mathf.Pow(x,2));
                //  z
                for(int z=0;z<size.z+smoothness;++z){for(vCoord2.z=vCoord1.z-z                                  ;vCoord2.z<=vCoord1.z+z;vCoord2.z+=z*2){
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
                         sqrt_xz_2=Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(z,2));
                         sqrt_zy_2=Mathf.Sqrt(Mathf.Pow(z,2)+Mathf.Pow(y,2));
                         double resultDensity;
                         if(y>=size.y||x>=size.x||z>=size.z){
                          if(y>=size.y&&x>=size.x&&z>=size.z){
                           float sqrt_yx_xz_2=Mathf.Sqrt(Mathf.Pow(sqrt_yx_2,2)+Mathf.Pow(sqrt_xz_2,2));
                            float sqrt_yx_xz_zy_2=Mathf.Sqrt(Mathf.Pow(sqrt_yx_xz_2,2)+Mathf.Pow(sqrt_zy_2,2));
                           resultDensity=density*(1f-(sqrt_yx_xz_zy_2-sqrt_yx_xz_1)/(sqrt_yx_xz_zy_2));
                          }else if(y>=size.y&&x>=size.x){resultDensity=density*(1f-(sqrt_yx_2-sqrt_yx_1)/(sqrt_yx_2));
                          }else if(x>=size.x&&z>=size.z){resultDensity=density*(1f-(sqrt_xz_2-sqrt_xz_1)/(sqrt_xz_2));
                          }else if(z>=size.z&&y>=size.y){resultDensity=density*(1f-(sqrt_zy_2-sqrt_zy_1)/(sqrt_zy_2));
                          }else if(y>=size.y){resultDensity=density*(1f-(y-size.y)/(float)y)*1.414f;//  raiz quadrada de 2
                          }else if(x>=size.x){resultDensity=density*(1f-(x-size.x)/(float)x)*1.414f;
                          }else if(z>=size.z){resultDensity=density*(1f-(z-size.z)/(float)z)*1.414f;
                          }else{
                           resultDensity=0d;
                          }
                         }else{
                          resultDensity=density;
                         }
                         //  TO DO: get current file data to merge
                         if(!dataFromFileToMerge.ContainsKey(cCoord3)){
                          if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
                           editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
                          }
                          dataFromFileToMerge.Add(cCoord3,editData);
                          //  TO DO: load data here
                         }
                         Voxel currentVoxel;
                         if(dataFromFileToMerge.ContainsKey(cCoord3)&&dataFromFileToMerge[cCoord3].ContainsKey(vCoord3)){
                          TerrainEditOutputData voxelData=dataFromFileToMerge[cCoord3][vCoord3];
                          currentVoxel=new Voxel(voxelData.density,Vector3.zero,voxelData.material);
                         }else{
                          currentVoxel=new Voxel();
                          Vector3Int noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
                                                        noiseInput.z+=cnkRgn3.y;
                          VoxelSystem.biome.Setvxl(
                           noiseInput,
                            null,
                             null,
                              0,
                               vCoord3.z+vCoord3.x*Depth,
                                ref currentVoxel
                          );
                         }
                         resultDensity=Math.Max(resultDensity,currentVoxel.density);
                         if(material==MaterialId.Air&&!(-resultDensity>=-isoLevel)){
                          resultDensity=-resultDensity;
                         }
                         if(!dataForSavingToFile.ContainsKey(cCoord3)){
                          if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
                           editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
                          }
                          dataForSavingToFile.Add(cCoord3,editData);
                         }
                         dataForSavingToFile[cCoord3][vCoord3]=new TerrainEditOutputData(resultDensity,-resultDensity>=-isoLevel?MaterialId.Air:material);
                         container.dirty.Add(cnkIdx3);
                         //  TO DO: add neighbours that are dirty too
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
         foreach(object syn in container.terrainSynchronization){
          Monitor.Enter(syn);
         }
         try{
          //  Write file safely here
          foreach(var cCoordDataForSavingPair in dataForSavingToFile){
           Vector2Int cCoord=cCoordDataForSavingPair.Key;
           Dictionary<Vector3Int,TerrainEditOutputData>editData=cCoordDataForSavingPair.Value;
           stringBuilder.Clear();
           string fileName=string.Format(VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord.x,cCoord.y);
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
             int x=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             int y=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             int z=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
             Vector3Int vCoord=new Vector3Int(x,y,z);
             int editStringStart=vCoordStringEnd+4;
             editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
             if(editStringStart>=0){
              int editStringEnd=line.IndexOf(" , }",editStringStart);
              string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
              TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
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
         }
        }
    }
}