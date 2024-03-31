#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.Water.WaterSpreadingMultithreaded;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    internal class MarchingCubesWaterBackgroundContainer:BackgroundContainer{
     internal FileStream[]readCacheStream=new FileStream[9];
     internal BinaryReader[]readCacheBinaryReader=new BinaryReader[9];
     internal NativeList<Vertex>TempVer;[StructLayout(LayoutKind.Sequential)]internal struct Vertex{
          internal Vector4 pos;
          internal Vector3 normal;
             internal Vertex(Vector3 p,Vector3 n){
              pos=p;
              pos.w=1f;
              normal=n;
             }
     }
     internal NativeList<UInt32>TempTri;
     internal Vector2Int?cCoord,lastcCoord;
     internal Vector2Int?cnkRgn,lastcnkRgn;
     internal        int?cnkIdx,lastcnkIdx;
        internal MarchingCubesWaterBackgroundContainer(){
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          for(int i=0;i<readCacheStream.Length;++i){
           if(readCacheStream[i]!=null){
            readCacheStream[i]      .Dispose();
            readCacheBinaryReader[i].Dispose();
            readCacheStream[i]      =null;
            readCacheBinaryReader[i]=null;
           }
          }
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class MarchingCubesWaterMultithreaded:BaseMultithreaded<MarchingCubesWaterBackgroundContainer>{
     readonly Dictionary<int,VoxelWater>[]voxels=new Dictionary<int,VoxelWater>[9];
     readonly VoxelWater[][][]voxelsCache1=new VoxelWater[3][][]{
      new VoxelWater[1            ][]{new VoxelWater[4],},
      new VoxelWater[Depth        ][],//  inicializar no construtor e limpar com Cleanup...
      new VoxelWater[FlattenOffset][],
     };
     readonly VoxelWater[][]voxelsCache2=new VoxelWater[3][]{
      new VoxelWater[1            ],
      new VoxelWater[Depth        ],
      new VoxelWater[FlattenOffset],
     };
     readonly     double[][][]     noiseCache1=new     double[biome.cacheLength][][];
     #region marching cubes
         readonly VoxelWater[]polygonCell=new VoxelWater[8];   
          readonly VoxelWater[]tmpvxl=new VoxelWater[6];
     #endregion
        internal MarchingCubesWaterMultithreaded(){
         for(int i=0;i<voxels.Length;++i){
                       voxels[i]=new Dictionary<int,VoxelWater>();
         }
         for(int i=0;i<voxelsCache1[2].Length;++i){
                       voxelsCache1[2][i]=new VoxelWater[4];
                  if(i<voxelsCache1[1].Length){
                       voxelsCache1[1][i]=new VoxelWater[4];
                  }
         }
         for(int i=0;i<biome.cacheLength;++i){
               noiseCache1[i]=new     double[9][];
         }
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
         }
        }
        protected override void Execute(){
         if(container.cnkIdx==null){
          return;
         }
         Log.DebugMessage("MarchingCubesWaterMultithreaded:Execute()");
         container.TempVer.Clear();
         container.TempTri.Clear();
         bool hasChangedIndex=false;
         if(container.lastcnkIdx==null||container.cnkIdx.Value!=container.lastcnkIdx.Value){
          hasChangedIndex=true;
         }
         //  carregar arquivo aqui
         VoxelSystem.Concurrent.waterCache_rwl.EnterReadLock();
         try{
          for(int x=-1;x<=1;x++){
          for(int y=-1;y<=1;y++){
           Vector2Int cCoord1=container.cCoord.Value+new Vector2Int(x,y);
           if(Math.Abs(cCoord1.x)>=MaxcCoordx||
              Math.Abs(cCoord1.y)>=MaxcCoordy)
           {
            continue;
           }
           int oftIdx1=GetoftIdx(cCoord1-container.cCoord.Value);
           int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
           if(hasChangedIndex){
            if(container.readCacheStream[oftIdx1]!=null){
             container.readCacheStream[oftIdx1]      .Dispose();
             container.readCacheBinaryReader[oftIdx1].Dispose();
             container.readCacheStream[oftIdx1]      =null;
             container.readCacheBinaryReader[oftIdx1]=null;
            }
           }
           if(container.readCacheStream[oftIdx1]==null){
            string cacheFileName=string.Format(CultureInfoUtil.en_US,VoxelSystem.Concurrent.waterCacheFileFormat,VoxelSystem.Concurrent.waterCachePath,cCoord1.x,cCoord1.y);
            if(File.Exists(cacheFileName)){
             container.readCacheStream[oftIdx1]=new FileStream(cacheFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
             container.readCacheBinaryReader[oftIdx1]=new BinaryReader(container.readCacheStream[oftIdx1]);
            }
           }
           if(container.readCacheStream[oftIdx1]!=null){
            container.readCacheStream[oftIdx1].Position=0L;
            while(container.readCacheBinaryReader[oftIdx1].BaseStream.Position!=container.readCacheBinaryReader[oftIdx1].BaseStream.Length){
             var v=BinaryReadVoxelWater(container.readCacheBinaryReader[oftIdx1]);
             voxels[oftIdx1][v.vxlIdx]=v.voxel;
            }
           }
          }}
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
          int corner=0;Vector3Int vCoord2=vCoord1;                                       if(vCoord1.z>0)polygonCell[corner]=voxelsCache1[0][0][0];else if(vCoord1.x>0)polygonCell[corner]=voxelsCache1[1][vCoord1.z][0];else if(vCoord1.y>0)polygonCell[corner]=voxelsCache1[2][vCoord1.z+vCoord1.x*Depth][0];else SetpolygonCellVoxel();
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
                 Vector2Int cnkRgn2=container.cnkRgn.Value;
                 Vector2Int cCoord2=container.cCoord.Value;
                 int oftIdx2=-1;
                 int vxlIdx2=-1;
                 bool cache2=false;
                 /*  fora do mundo, baixo:  */
                 if(vCoord2.y<=0){
                  polygonCell[corner]=VoxelWater.bedrock;
                 /*  fora do mundo, cima:  */
                 }else if(vCoord2.y>=Height){
                  polygonCell[corner]=VoxelWater.air;
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
                  oftIdx2=GetoftIdx(cCoord2-container.cCoord.Value);
                  vxlIdx2=GetvxlIdx(vCoord2.x,vCoord2.y,vCoord2.z);
                  if(voxels[oftIdx2].ContainsKey(vxlIdx2)){
                   polygonCell[corner]=voxels[oftIdx2][vxlIdx2];
                  }else{
                   Vector3Int noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                                                 noiseInput.z+=cnkRgn2.y;
                   VoxelSystem.biome.SetvxlWater(
                    noiseInput,
                     noiseCache1,
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
                         tmpvxl[tmpIdx]=VoxelWater.bedrock;
                        /*  fora do mundo, cima:  */
                        }else if(vCoord3.y>=Height){
                         tmpvxl[tmpIdx]=VoxelWater.air;
                        //  pegar valor do bioma:
                        }else{
                         if(vCoord3.x<0||vCoord3.x>=Width||
                            vCoord3.z<0||vCoord3.z>=Depth
                         ){
                          ValidateCoord(ref cnkRgn3,ref vCoord3);
                          cCoord3=cnkRgnTocCoord(cnkRgn3);
                         }
                         int oftIdx3=GetoftIdx(cCoord3-container.cCoord.Value);
                         int vxlIdx3=GetvxlIdx(vCoord3.x,vCoord3.y,vCoord3.z);
                         if(voxels[oftIdx3].ContainsKey(vxlIdx3)){
                          tmpvxl[tmpIdx]=voxels[oftIdx3][vxlIdx3];
                         }else{
                          Vector3Int noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
                                                        noiseInput.z+=cnkRgn3.y;
                          VoxelSystem.biome.SetvxlWater(
                           noiseInput,
                            noiseCache1,
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
         }}}
        }
    }
}