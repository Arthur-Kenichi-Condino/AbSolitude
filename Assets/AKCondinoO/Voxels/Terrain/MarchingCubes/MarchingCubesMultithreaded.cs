#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Terrain.Editing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
namespace AKCondinoO.Voxels.Terrain.MarchingCubes{
    internal class MarchingCubesBackgroundContainer:BackgroundContainer{
     internal readonly Voxel[]voxelsOutput=new Voxel[VoxelsPerChunk];
     internal readonly object synchronizer=new object();
     internal readonly Dictionary<int,string>editsFileName=new Dictionary<int,string>();
     internal readonly Dictionary<int,FileStream>editsFileStream=new Dictionary<int,FileStream>();
     internal readonly Dictionary<int,StreamReader>editsFileStreamReader=new Dictionary<int,StreamReader>();
     internal NativeList<Vertex>TempVer;[StructLayout(LayoutKind.Sequential)]internal struct Vertex{
          internal Vector4 pos;
          internal Vector3 normal;
          internal Color color;
          internal Vector4 texCoord0;
          internal Vector4 texCoord1;
          internal Vector4 texCoord2;
          internal Vector4 texCoord3;
          internal Vector4 texCoord4;
          internal Vector4 texCoord5;
          internal Vector4 texCoord6;
          internal Vector4 texCoord7;
             internal Vertex(Vector3 p,Vector3 n,Vector2 uv0){
              pos=p;
              pos.w=1f;
              normal=n;
              color=new Color(0f,0f,0f,0f);
              texCoord0=emptyUV;
              texCoord0.x=uv0.x;
              texCoord0.y=uv0.y;
              texCoord1=emptyUV;
              texCoord2=emptyUV;
              texCoord3=emptyUV;
              texCoord4=emptyUV;
              texCoord5=emptyUV;
              texCoord6=new Vector4(1f,0f,0f,0f);
              texCoord7=new Vector4(0f,0f,0f,0f);
             }
     }
     internal NativeList<UInt32>TempTri;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
        internal MarchingCubesBackgroundContainer(){
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          foreach(var fS in editsFileStream      ){if(fS.Value!=null){fS.Value.Dispose();}}
          foreach(var sR in editsFileStreamReader){if(sR.Value!=null){sR.Value.Dispose();}}
          editsFileStream      .Clear();
          editsFileStreamReader.Clear();
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class MarchingCubesMultithreaded:BaseMultithreaded<MarchingCubesBackgroundContainer>{
     readonly Dictionary<int,Voxel>[]voxels=new Dictionary<int,Voxel>[9];
     readonly Dictionary<int,bool>[]isSolid=new Dictionary<int,bool>[9];
     readonly Voxel[][][]voxelsCache1=new Voxel[3][][]{
      new Voxel[1            ][]{new Voxel[4],},
      new Voxel[Depth        ][],//  inicializar no construtor e limpar com Cleanup...
      new Voxel[FlattenOffset][],
     };
     readonly Voxel[][]voxelsCache2=new Voxel[3][]{
      new Voxel[1            ],
      new Voxel[Depth        ],
      new Voxel[FlattenOffset],
     };
     readonly     double[][][]     noiseCache1=new     double[biome.cacheLength][][];
     readonly MaterialId[][][]materialIdCache1=new MaterialId[biome.cacheLength][][];
     #region marching cubes
         readonly Voxel[]polygonCell=new Voxel[8];   
          readonly Voxel[]tmpvxl=new Voxel[6];
         readonly    Vector3[] vertices=new    Vector3[12];
         readonly MaterialId[]materials=new MaterialId[12];
         readonly    Vector3[]  normals=new    Vector3[12];
          readonly Vector3[][][]verticesCache=new Vector3[3][][]{
           new Vector3[1            ][]{new Vector3[4],},
           new Vector3[Depth        ][],//  inicializar no construtor e limpar com Cleanup...
           new Vector3[FlattenOffset][],
          };
         #region vertexInterp
             readonly     double[] density=new     double[2];
             readonly    Vector3[]  vertex=new    Vector3[2];
             readonly MaterialId[]material=new MaterialId[2];
             readonly      float[]distance=new      float[2];
         #endregion
         readonly     int[]   idx=new     int[3];
         readonly Vector3[]verPos=new Vector3[3];
         readonly Dictionary<Vector3,List<Vector2>>vertexUV=new Dictionary<Vector3,List<Vector2>>();
          readonly Dictionary<Vector2,int>vertexUVCounted=new Dictionary<Vector2,int>();
           readonly SortedDictionary<(int,float,float),Vector2>vertexUVSorted=new SortedDictionary<(int,float,float),Vector2>();
            readonly Dictionary<int,int>weights=new Dictionary<int,int>(4);
     #endregion
        internal MarchingCubesMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i]=new Dictionary<int,Voxel>();
         }
         for(int i=0;i<voxelsCache1[2].Length;++i){
                       voxelsCache1[2][i]=new Voxel[4];
                  if(i<voxelsCache1[1].Length){
                       voxelsCache1[1][i]=new Voxel[4];
                  }
         }
         for(int i=0;i<biome.cacheLength;++i){
               noiseCache1[i]=new     double[9][];
          materialIdCache1[i]=new MaterialId[9][];
         }
         #region marching cubes
             for(int i=0;i<verticesCache[2].Length;++i){
                           verticesCache[2][i]=new Vector3[4];
                      if(i<verticesCache[1].Length){
                           verticesCache[1][i]=new Vector3[4];
                      }
             }
         #endregion
        }
        protected override void Cleanup(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i].Clear();
         }
         for(int i=0;i<voxelsCache1[0].Length;++i){Array.Clear(voxelsCache1[0][i],0,voxelsCache1[0][i].Length);}
         for(int i=0;i<voxelsCache1[1].Length;++i){Array.Clear(voxelsCache1[1][i],0,voxelsCache1[1][i].Length);}
         for(int i=0;i<voxelsCache1[2].Length;++i){Array.Clear(voxelsCache1[2][i],0,voxelsCache1[2][i].Length);}
         for(int i=0;i<voxelsCache2.Length;++i){
                    if(voxelsCache2[i]!=null){
                     Array.Clear(voxelsCache2[i],0,voxelsCache2[i].Length);
                    }
         }
         for(int i=0;i<biome.cacheLength;++i){
          for(int j=0;j<     noiseCache1[i].Length;++j){if(     noiseCache1[i][j]!=null)Array.Clear(     noiseCache1[i][j],0,     noiseCache1[i][j].Length);}
          for(int j=0;j<materialIdCache1[i].Length;++j){if(materialIdCache1[i][j]!=null)Array.Clear(materialIdCache1[i][j],0,materialIdCache1[i][j].Length);}
         }
         #region marching cubes
             for(int i=0;i<verticesCache[0].Length;++i){Array.Clear(verticesCache[0][i],0,verticesCache[0][i].Length);}
             for(int i=0;i<verticesCache[1].Length;++i){Array.Clear(verticesCache[1][i],0,verticesCache[1][i].Length);}
             for(int i=0;i<verticesCache[2].Length;++i){Array.Clear(verticesCache[2][i],0,verticesCache[2][i].Length);}
             foreach(var kvp in vertexUV){
              var list=kvp.Value;
              list.Clear();
              MarchingCubesTerrain.vertexUVListPool.Enqueue(list);
             }
             vertexUV.Clear();
         #endregion
        }
        protected override void Execute(){
         //Log.DebugMessage("do MarchingCubes for cnkIdx:"+container.cnkIdx);
         container.TempVer.Clear();
         container.TempTri.Clear();
         VoxelSystem.Concurrent.terrainFileData_rwl.EnterReadLock();
         try{
          lock(container.synchronizer){
           for(int x=-1;x<=1;x++){
           for(int y=-1;y<=1;y++){
            Vector2Int cCoord1=container.cCoord+new Vector2Int(x,y);
            if(Math.Abs(cCoord1.x)>=MaxcCoordx||
               Math.Abs(cCoord1.y)>=MaxcCoordy)
            {
             continue;
            }
            int oftIdx1=GetoftIdx(cCoord1-container.cCoord);
            string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord1.x,cCoord1.y);
            if(!container.editsFileStream.ContainsKey(oftIdx1)||!container.editsFileName.ContainsKey(oftIdx1)||container.editsFileName[oftIdx1]!=editsFileName){
             container.editsFileName[oftIdx1]=editsFileName;
             if(container.editsFileStream.TryGetValue(oftIdx1,out FileStream fStream)){
              fStream                                 .Dispose();
              container.editsFileStreamReader[oftIdx1].Dispose();
              container.editsFileStream      .Remove(oftIdx1);
              container.editsFileStreamReader.Remove(oftIdx1);
             }
             if(File.Exists(editsFileName)){
              container.editsFileStream.Add(oftIdx1,new FileStream(editsFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
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
               editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
               if(editStringStart>=0){
                int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
                string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
                TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
                voxels[oftIdx1][GetvxlIdx(vCoord.x,vCoord.y,vCoord.z)]=new Voxel(edit.density,Vector3.zero,edit.material);
               }
              }
             }
            }
           }}
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFileData_rwl.ExitReadLock();
         }
         VoxelSystem.Concurrent.terrain_rwl.EnterWriteLock();
         try{
          if(VoxelSystem.Concurrent.terrainVoxelsId.TryGetValue(container.voxelsOutput,out var voxelsOutputOldId)){
           if(VoxelSystem.Concurrent.terrainVoxelsOutput.TryGetValue(voxelsOutputOldId.cnkIdx,out Voxel[]oldIdVoxelsOutput)&&object.ReferenceEquals(oldIdVoxelsOutput,container.voxelsOutput)){
            VoxelSystem.Concurrent.terrainVoxelsOutput.Remove(voxelsOutputOldId.cnkIdx);
            //Log.DebugMessage("removed old value for voxelsOutputOldId.cnkIdx:"+voxelsOutputOldId.cnkIdx);
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrain_rwl.ExitWriteLock();
         }
         UInt32 vertexCount=0;
         Vector3Int vCoord1;
         lock(container.voxelsOutput){
          for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
          for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
          for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
           int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
           int corner=0;Vector3Int vCoord2=vCoord1;                                       if(vCoord1.z>0)polygonCell[corner]=voxelsCache1[0][0][0];else if(vCoord1.x>0)polygonCell[corner]=voxelsCache1[1][vCoord1.z][0];else if(vCoord1.y>0)polygonCell[corner]=voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][0];else SetpolygonCellVoxel();
           container.voxelsOutput[vxlIdx1]=polygonCell[corner];
               corner++;           vCoord2=vCoord1;vCoord2.x+=1;                          if(vCoord1.z>0)polygonCell[corner]=voxelsCache1[0][0][1];                                                                      else if(vCoord1.y>0)polygonCell[corner]=voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][1];else SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;             if(vCoord1.z>0)polygonCell[corner]=voxelsCache1[0][0][2];                                                                                                                                                            else SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;             vCoord2.y+=1;             if(vCoord1.z>0)polygonCell[corner]=voxelsCache1[0][0][3];else if(vCoord1.x>0)polygonCell[corner]=voxelsCache1[1][vCoord1.z][1];                                                                                      else SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;                          vCoord2.z+=1;                                                              if(vCoord1.x>0)polygonCell[corner]=voxelsCache1[1][vCoord1.z][2];else if(vCoord1.y>0)polygonCell[corner]=voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][2];else SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;vCoord2.x+=1;             vCoord2.z+=1;                                                                                                                                    if(vCoord1.y>0)polygonCell[corner]=voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][3];else SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;vCoord2.z+=1;                                                                                                                                                                                                                          SetpolygonCellVoxel();
               corner++;           vCoord2=vCoord1;             vCoord2.y+=1;vCoord2.z+=1;                                                              if(vCoord1.x>0)polygonCell[corner]=voxelsCache1[1][vCoord1.z][3];                                                                                      else SetpolygonCellVoxel();
           voxelsCache1[0][0][0]=polygonCell[4];
           voxelsCache1[0][0][1]=polygonCell[5];
           voxelsCache1[0][0][2]=polygonCell[6];
           voxelsCache1[0][0][3]=polygonCell[7];
           voxelsCache1[1][vCoord1.z][0]=polygonCell[1];
           voxelsCache1[1][vCoord1.z][1]=polygonCell[2];
           voxelsCache1[1][vCoord1.z][2]=polygonCell[5];
           voxelsCache1[1][vCoord1.z][3]=polygonCell[6];
           voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][0]=polygonCell[3];
           voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][1]=polygonCell[2];
           voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][2]=polygonCell[7];
           voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][3]=polygonCell[6];
                 void SetpolygonCellVoxel(){
                  Vector2Int cnkRgn2=container.cnkRgn;
                  Vector2Int cCoord2=container.cCoord;
                  int oftIdx2=-1;
                  int vxlIdx2=-1;
                  bool cache2=false;
                  /*  fora do mundo, baixo:  */
                  if(vCoord2.y<=0){
                   polygonCell[corner]=Voxel.bedrock;
                  /*  fora do mundo, cima:  */
                  }else if(vCoord2.y>=Height){
                   polygonCell[corner]=Voxel.air;
                  //  pegar valor do bioma:
                  }else{
                   if(vCoord2.x<0||vCoord2.x>=Width||
                      vCoord2.z<0||vCoord2.z>=Depth
                   ){
                    ValidateCoord(ref cnkRgn2,ref vCoord2);
                    cCoord2=cnkRgnTocCoord(cnkRgn2);
                   }else{
                    cache2=true;
                   }
                   oftIdx2=GetoftIdx(cCoord2-container.cCoord);
                   vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
                   if(voxels[oftIdx2].ContainsKey(vxlIdx2)){
                    polygonCell[corner]=voxels[oftIdx2][vxlIdx2];
                   }else{
                    Vector3Int noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                                                  noiseInput.z+=cnkRgn2.y;
                    VoxelSystem.biome.Setvxl(
                     noiseInput,
                      noiseCache1,
                       materialIdCache1,
                        oftIdx2,
                         vCoord2.z+vCoord2.x*Depth,
                          ref polygonCell[corner]
                    );
                   }
                  }
                  if(polygonCell[corner].normal==Vector3.zero){
                   //  calcular normal:
                   int tmpIdx=0;Vector3Int vCoord3=vCoord2;vCoord3.x++;                                                                                                                                                                                              Settmpvxl();
                       tmpIdx++;           vCoord3=vCoord2;vCoord3.x--;                        if(cache2&&vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsCache2[1][vCoord2.z].isCreated)                tmpvxl[tmpIdx]=voxelsCache2[1][vCoord2.z];                else Settmpvxl();
                       tmpIdx++;           vCoord3=vCoord2;            vCoord3.y++;                                                                                                                                                                                  Settmpvxl();
                       tmpIdx++;           vCoord3=vCoord2;            vCoord3.y--;            if(cache2&&vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsCache2[2][vCoord2.z+vCoord2.x*Depth].isCreated)tmpvxl[tmpIdx]=voxelsCache2[2][vCoord2.z+vCoord2.x*Depth];else Settmpvxl();
                       tmpIdx++;           vCoord3=vCoord2;                        vCoord3.z++;                                                                                                                                                                      Settmpvxl();
                       tmpIdx++;           vCoord3=vCoord2;                        vCoord3.z--;if(cache2&&vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsCache2[0][0].isCreated)                        tmpvxl[tmpIdx]=voxelsCache2[0][0];                        else Settmpvxl();
                        void Settmpvxl(){
                         Vector2Int cnkRgn3=cnkRgn2;
                         Vector2Int cCoord3=cCoord2;
                         /*  fora do mundo, baixo:  */
                         if(vCoord3.y<=0){
                          tmpvxl[tmpIdx]=Voxel.bedrock;
                         /*  fora do mundo, cima:  */
                         }else if(vCoord3.y>=Height){
                          tmpvxl[tmpIdx]=Voxel.air;
                         //  pegar valor do bioma:
                         }else{
                          if(vCoord3.x<0||vCoord3.x>=Width||
                             vCoord3.z<0||vCoord3.z>=Depth
                          ){
                           ValidateCoord(ref cnkRgn3,ref vCoord3);
                           cCoord3=cnkRgnTocCoord(cnkRgn3);
                          }
                          int oftIdx3=GetoftIdx(cCoord3-container.cCoord);
                          int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
                          if(voxels[oftIdx3].ContainsKey(vxlIdx3)){
                           tmpvxl[tmpIdx]=voxels[oftIdx3][vxlIdx3];
                          }else{
                           Vector3Int noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
                                                         noiseInput.z+=cnkRgn3.y;
                           VoxelSystem.biome.Setvxl(
                            noiseInput,
                             noiseCache1,
                              materialIdCache1,
                               oftIdx3,
                                vCoord3.z+vCoord3.x*Depth,
                                 ref tmpvxl[tmpIdx]
                           );
                           voxels[oftIdx3][vxlIdx3]=tmpvxl[tmpIdx];
                          }
                         }
                        }
                   Vector3 polygonCellNormal=new Vector3{
                    x=(float)(tmpvxl[1].density-tmpvxl[0].density),
                    y=(float)(tmpvxl[3].density-tmpvxl[2].density),
                    z=(float)(tmpvxl[5].density-tmpvxl[4].density)
                   };
                   polygonCell[corner].normal=polygonCellNormal;
                   if(polygonCell[corner].normal!=Vector3.zero){
                      polygonCell[corner].normal.Normalize();
                   }
                  }
                  if(oftIdx2>=0&&
                     vxlIdx2>=0){
                   voxels[oftIdx2][vxlIdx2]=polygonCell[corner];
                  }
                  if(cache2){
                   voxelsCache2[0][0]=polygonCell[corner];
                   voxelsCache2[1][vCoord2.z]=polygonCell[corner];
                   voxelsCache2[2][vCoord2.z+vCoord2.x*Depth]=polygonCell[corner];
                  }
                 }
           DoMarchingCubes(
            polygonCell,
             vCoord1,
              vertices,
               verticesCache,
                materials,
                 normals,
                  density,
                   vertex,
                    material,
                     distance,
                      idx,
                       verPos,
                        ref vertexCount,
                         container.TempVer,
                         container.TempTri,
                          vertexUV
           );
          }}}
         }
         //  TO DO: luz e oclusão de ambiente neste "for":
         //for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         //for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
         //for(vCoord1.y=Height-1      ;vCoord1.y>=0    ;vCoord1.y--){
         //}
         //}}
         VoxelSystem.Concurrent.terrain_rwl.EnterWriteLock();
         try{
          VoxelSystem.Concurrent.terrainVoxelsOutput[container.cnkIdx]=container.voxelsOutput;
          VoxelSystem.Concurrent.terrainVoxelsId[container.voxelsOutput]=(container.cCoord,container.cnkRgn,container.cnkIdx);
          //Log.DebugMessage("added voxelsOutput for container.cnkIdx:"+container.cnkIdx);
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrain_rwl.ExitWriteLock();
         }
         Vector2Int posOffset=Vector2Int.zero;
         Vector2Int crdOffset=Vector2Int.zero;
         for(crdOffset.y=0,posOffset.y=0,
             vCoord1.y=0;vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.z=0;vCoord1.z<Depth ;vCoord1.z++){
             vCoord1.x=0;
          //  east
          crdOffset.x=1;
          posOffset.x=Width;
          AddEdgesvertexUV();   
             vCoord1.x=Width-1;
          //  west
          crdOffset.x=-1;
          posOffset.x=-Width;
          AddEdgesvertexUV();
         }}
         for(crdOffset.x=0,posOffset.x=0,
             vCoord1.y=0;vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0;vCoord1.x<Width ;vCoord1.x++){
             vCoord1.z=0;
          //  north
          crdOffset.y=1;
          posOffset.y=Depth;
          AddEdgesvertexUV();
             vCoord1.z=Depth-1;
          //  south
          crdOffset.y=-1;
          posOffset.y=-Depth;
          AddEdgesvertexUV();
         }}
         void AddEdgesvertexUV(){
          int corner=0;Vector3Int vCoord2=vCoord1;                                       EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;vCoord2.x+=1;                          EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;             EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;             vCoord2.y+=1;             EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;                          vCoord2.z+=1;EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;vCoord2.x+=1;             vCoord2.z+=1;EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;vCoord2.z+=1;EdgeSetpolygonCellVoxel();
              corner++;           vCoord2=vCoord1;             vCoord2.y+=1;vCoord2.z+=1;EdgeSetpolygonCellVoxel();
                void EdgeSetpolygonCellVoxel(){
                 Vector2Int cnkRgn2=container.cnkRgn+posOffset;
                 Vector2Int cCoord2=container.cCoord+crdOffset;
                 int oftIdx2=-1;
                 int vxlIdx2=-1;
                 /*  fora do mundo, baixo:  */
                 if(vCoord2.y<=0){
                  polygonCell[corner]=Voxel.bedrock;
                 /*  fora do mundo, cima:  */
                 }else if(vCoord2.y>=Height){
                  polygonCell[corner]=Voxel.air;
                 //  pegar valor do bioma:
                 }else{
                  if(vCoord2.x<0||vCoord2.x>=Width||
                     vCoord2.z<0||vCoord2.z>=Depth
                  ){
                   ValidateCoord(ref cnkRgn2,ref vCoord2);
                   cCoord2=cnkRgnTocCoord(cnkRgn2);
                  }
                  oftIdx2=GetoftIdx(cCoord2-container.cCoord);
                  vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
                  if(voxels[oftIdx2].ContainsKey(vxlIdx2)){
                   polygonCell[corner]=voxels[oftIdx2][vxlIdx2];
                  }else{
                   Vector3Int noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                                                 noiseInput.z+=cnkRgn2.y;
                   VoxelSystem.biome.Setvxl(
                    noiseInput,
                     noiseCache1,
                      materialIdCache1,
                       oftIdx2,
                        vCoord2.z+vCoord2.x*Depth,
                         ref polygonCell[corner]
                   );
                  }
                 }
                }
          DoEdgeMarchingCubes(
           polygonCell,
            vCoord1,
             vertices,
              //verticesCache,
               materials,
                normals,
                 density,
                  vertex,
                   material,
                    //distance,
                     idx,
                      verPos,
                       //ref vertexCount,
                        //container.TempVer,
                        //container.TempTri,
                         vertexUV,
                          posOffset
          );
         }
         for(int i=0;i<container.TempVer.Length/3;i++){
          idx[0]=i*3  ;
          idx[1]=i*3+1;
          idx[2]=i*3+2;
          for(int j=0;j<3;j++){
           var vertexUVList=vertexUV[verPos[j]=container.TempVer[idx[j]].pos];
           vertexUVCounted.Clear();
           foreach(var uv in vertexUVList){
            if(!vertexUVCounted.ContainsKey(uv)){
             vertexUVCounted.Add(uv,1);
            }else{
             vertexUVCounted[uv]++;
            }
           }
           vertexUVSorted.Clear();
           foreach(var kvp in vertexUVCounted){
            vertexUVSorted.Add((kvp.Value,kvp.Key.x,kvp.Key.y),kvp.Key);
           }
           weights.Clear();
           int total=0;
           Vector2 uv0=container.TempVer[idx[j]].texCoord0;
           foreach(var materialId in vertexUVSorted){
            Vector2 uv=materialId.Value;
            bool add;
            if(uv0==uv){
             total+=weights[0]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord0.z==emptyUV.x&&
                container.TempVer[idx[j]].texCoord0.w==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord1.x!=uv.x&&
              container.TempVer[idx[j]].texCoord1.y!=uv.y&&
              container.TempVer[idx[j]].texCoord1.z!=uv.x&&
              container.TempVer[idx[j]].texCoord1.w!=uv.y&&
              container.TempVer[idx[j]].texCoord2.x!=uv.x&&
              container.TempVer[idx[j]].texCoord2.y!=uv.y&&
              container.TempVer[idx[j]].texCoord2.z!=uv.x&&
              container.TempVer[idx[j]].texCoord2.w!=uv.y&&
              container.TempVer[idx[j]].texCoord3.x!=uv.x&&
              container.TempVer[idx[j]].texCoord3.y!=uv.y&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord0.z==uv.x&&
              container.TempVer[idx[j]].texCoord0.w==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord0.z=uv.x;vertex.texCoord0.w=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord0.z=uv.x;vertex.texCoord0.w=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord0.z=uv.x;vertex.texCoord0.w=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[1]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord1.x==emptyUV.x&&
                container.TempVer[idx[j]].texCoord1.y==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord1.z!=uv.x&&
              container.TempVer[idx[j]].texCoord1.w!=uv.y&&
              container.TempVer[idx[j]].texCoord2.x!=uv.x&&
              container.TempVer[idx[j]].texCoord2.y!=uv.y&&
              container.TempVer[idx[j]].texCoord2.z!=uv.x&&
              container.TempVer[idx[j]].texCoord2.w!=uv.y&&
              container.TempVer[idx[j]].texCoord3.x!=uv.x&&
              container.TempVer[idx[j]].texCoord3.y!=uv.y&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord1.x==uv.x&&
              container.TempVer[idx[j]].texCoord1.y==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord1.x=uv.x;vertex.texCoord1.y=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord1.x=uv.x;vertex.texCoord1.y=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord1.x=uv.x;vertex.texCoord1.y=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[2]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord1.z==emptyUV.x&&
                container.TempVer[idx[j]].texCoord1.w==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord2.x!=uv.x&&
              container.TempVer[idx[j]].texCoord2.y!=uv.y&&
              container.TempVer[idx[j]].texCoord2.z!=uv.x&&
              container.TempVer[idx[j]].texCoord2.w!=uv.y&&
              container.TempVer[idx[j]].texCoord3.x!=uv.x&&
              container.TempVer[idx[j]].texCoord3.y!=uv.y&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord1.z==uv.x&&
              container.TempVer[idx[j]].texCoord1.w==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord1.z=uv.x;vertex.texCoord1.w=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord1.z=uv.x;vertex.texCoord1.w=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord1.z=uv.x;vertex.texCoord1.w=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[3]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord2.x==emptyUV.x&&
                container.TempVer[idx[j]].texCoord2.y==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord2.z!=uv.x&&
              container.TempVer[idx[j]].texCoord2.w!=uv.y&&
              container.TempVer[idx[j]].texCoord3.x!=uv.x&&
              container.TempVer[idx[j]].texCoord3.y!=uv.y&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord2.x==uv.x&&
              container.TempVer[idx[j]].texCoord2.y==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord2.x=uv.x;vertex.texCoord2.y=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord2.x=uv.x;vertex.texCoord2.y=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord2.x=uv.x;vertex.texCoord2.y=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[4]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord2.z==emptyUV.x&&
                container.TempVer[idx[j]].texCoord2.w==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord3.x!=uv.x&&
              container.TempVer[idx[j]].texCoord3.y!=uv.y&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord2.z==uv.x&&
              container.TempVer[idx[j]].texCoord2.w==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord2.z=uv.x;vertex.texCoord2.w=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord2.z=uv.x;vertex.texCoord2.w=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord2.z=uv.x;vertex.texCoord2.w=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[5]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord3.x==emptyUV.x&&
                container.TempVer[idx[j]].texCoord3.y==emptyUV.y
               )
              )&&
              container.TempVer[idx[j]].texCoord3.z!=uv.x&&
              container.TempVer[idx[j]].texCoord3.w!=uv.y
             )||
             (
              container.TempVer[idx[j]].texCoord3.x==uv.x&&
              container.TempVer[idx[j]].texCoord3.y==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord3.x=uv.x;vertex.texCoord3.y=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord3.x=uv.x;vertex.texCoord3.y=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord3.x=uv.x;vertex.texCoord3.y=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[6]=materialId.Key.Item1;
            }else if(
             (
              (
               add=(
                container.TempVer[idx[j]].texCoord3.z==emptyUV.x&&
                container.TempVer[idx[j]].texCoord3.w==emptyUV.y
               )
              )
             )||
             (
              container.TempVer[idx[j]].texCoord3.z==uv.x&&
              container.TempVer[idx[j]].texCoord3.w==uv.y
             )
            ){
             if(add){
              var vertex=container.TempVer[idx[0]];vertex.texCoord3.z=uv.x;vertex.texCoord3.w=uv.y;container.TempVer[idx[0]]=vertex;
                  vertex=container.TempVer[idx[1]];vertex.texCoord3.z=uv.x;vertex.texCoord3.w=uv.y;container.TempVer[idx[1]]=vertex;
                  vertex=container.TempVer[idx[2]];vertex.texCoord3.z=uv.x;vertex.texCoord3.w=uv.y;container.TempVer[idx[2]]=vertex;
             }
             total+=weights[7]=materialId.Key.Item1;
            }
           }
           if(weights.Count>1){
            var vertex2=container.TempVer[idx[j]];
            Vector4 texCoord6=vertex2.texCoord6;
            Vector4 texCoord7=vertex2.texCoord7;
                                       texCoord6.x=(weights[0]/(float)total);
            if(weights.ContainsKey(1)){texCoord6.y=(weights[1]/(float)total);}
            if(weights.ContainsKey(2)){texCoord6.z=(weights[2]/(float)total);}
            if(weights.ContainsKey(3)){texCoord6.w=(weights[3]/(float)total);}
            if(weights.ContainsKey(4)){texCoord7.x=(weights[4]/(float)total);}
            if(weights.ContainsKey(5)){texCoord7.y=(weights[5]/(float)total);}
            if(weights.ContainsKey(6)){texCoord7.z=(weights[6]/(float)total);}
            if(weights.ContainsKey(7)){texCoord7.w=(weights[7]/(float)total);}
            vertex2.texCoord6=texCoord6;
            vertex2.texCoord7=texCoord7;
            container.TempVer[idx[j]]=vertex2;
           }
          }
         }
        }
    }
}