using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes.paulbourke;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.MarchingCubes{
    internal struct MarchingCubesFlags{
     public bool StitchEdges;
     public bool Prediction;
     public bool CollectUV;
    }
    internal class MarchingCubesContext{
     public NativeList<Vertex>tempVer;
     public NativeList<UInt32>tempTri;
     public readonly Dictionary<int,List<Vector2>>vertexUV=new();//  ...para CollectUV
     public readonly Voxel[]polygonCell=new Voxel[8];
     internal static readonly Vector3Int[]polygonCellCornerOffset={
      new(0,0,0),
      new(1,0,0),
      new(1,1,0),
      new(0,1,0),
      new(0,0,1),
      new(1,0,1),
      new(1,1,1),
      new(0,1,1),
     };
     public Voxel[]polygonCellCache;
     internal static readonly byte[]polygonCellCornerCacheMask={
      0b111,
      0b101,
      0b001,
      0b011,
      0b110,
      0b100,
      0b000,
      0b010,
     };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int PolygonCellCacheIndex(int i){
         return i;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int PolygonCellCacheIndex(int z,int i){
         return 4+z*4+i;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int PolygonCellCacheIndex(int depth,int x,int z,int i){
         return 4+depth*4+(z+x*depth)*4+i;
        }
        internal void UpdatePolygonCellCache(int depth,Vector3Int polygonCoord){
         polygonCellCache[PolygonCellCacheIndex(0)]=polygonCell[4];
         polygonCellCache[PolygonCellCacheIndex(1)]=polygonCell[5];
         polygonCellCache[PolygonCellCacheIndex(2)]=polygonCell[6];
         polygonCellCache[PolygonCellCacheIndex(3)]=polygonCell[7];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,0)]=polygonCell[1];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,1)]=polygonCell[2];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,2)]=polygonCell[5];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,3)]=polygonCell[6];
         polygonCellCache[PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,0)]=polygonCell[3];
         polygonCellCache[PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,1)]=polygonCell[2];
         polygonCellCache[PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,2)]=polygonCell[7];
         polygonCellCache[PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,3)]=polygonCell[6];
        }
     public readonly    Vector3[]vertices =new    Vector3[12];
     public readonly    Vector3[]normals  =new    Vector3[12];
     public readonly MaterialId[]materials=new MaterialId[12];
    }
    internal static class MarchingCubesCore{
     static readonly Utilities.ObjectPool<List<Vector2>>vector2ListPool=
      Pool.GetPool<List<Vector2>>(
       "",
       ()=>new(),
       (List<Vector2>item)=>{
        item.Clear();
       }
      );
     internal static readonly Utilities.ObjectPool<MarchingCubesContext>marchingCubesContextPool=
      Pool.GetPool<MarchingCubesContext>(
       "",
       ()=>new(),
       (MarchingCubesContext item)=>{
        item.tempVer=default;
        item.tempTri=default;
        foreach(var kvp in item.vertexUV){
         var list=kvp.Value;
         vector2ListPool.Return(list);
        }
        item.vertexUV.Clear();
        Pool.ReturnArray<Voxel>(item.polygonCellCache,true);
        item.polygonCellCache=null;
        Array.Clear(item.polygonCell,0,item.polygonCell.Length);
       }
      );
        internal static void GetMeshData(Vector3Int min,Vector3Int max,Vector2Int cCoord,MarchingCubesContext context){
         Logs.Message(Logs.LogType.Debug,$"cCoord:{cCoord}",null,cCoord==Vector2Int.zero);
         int width =max.x-min.x;
         int height=max.y-min.y;
         int depth =max.z-min.z;
         context.polygonCellCache=Pool.RentArray<Voxel>(4+depth*4+width*depth*4);
         Vector3Int polygonCoord;
         Vector3Int coord;
         for(coord=new Vector3Int(0,min.y,0),polygonCoord=new();coord.y<=max.y;coord.y++,polygonCoord.y++){
         for(coord.x=               min.x   ,polygonCoord.x=0  ;coord.x<=max.x;coord.x++,polygonCoord.x++){
         for(coord.z=               min.z   ,polygonCoord.z=0  ;coord.z<=max.z;coord.z++,polygonCoord.z++){
          var polygoncCoord=cCoord;
          var polygonvCoord=coord;
          ValidatevCoord(ref polygoncCoord,ref polygonvCoord);
          BuildPolygonCell(depth,polygonCoord,polygonvCoord,polygoncCoord,context);
          MarchingCubesFlags flags=new(){
           StitchEdges=true,
           Prediction=true,
           CollectUV=true,
          };
          ProcessPolygonCell(polygonCoord,polygonvCoord,polygoncCoord,in flags,context);
         }}}
        }
        private static void BuildPolygonCell(int depth,Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         for(int corner=0;corner<8;corner++){
          SetPolygonCellVoxel(corner,depth,polygonCoord,polygonvCoord,polygoncCoord,context);
         }
         context.UpdatePolygonCellCache(depth,polygonCoord);
        }
        private static void SetPolygonCellVoxel(int corner,int depth,Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         Vector3Int offset=MarchingCubesContext.polygonCellCornerOffset[corner];
         byte mask=MarchingCubesContext.polygonCellCornerCacheMask[corner];
         if((mask&1)!=0&&polygonCoord.z>0){
          context.polygonCell[corner]=context.polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(corner)];
          return;
         }
         if((mask&2)!=0&&polygonCoord.x>0){
          context.polygonCell[corner]=context.polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(polygonCoord.z,corner)];
          return;
         }
         if((mask&4)!=0&&polygonCoord.y>0){
          context.polygonCell[corner]=context.polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,corner)];
          return;
         }
         Vector3Int polygonCellCoord =polygonCoord +offset;
         Vector3Int polygonCellvCoord=polygonvCoord+offset;
         BiomesConfigurationSnapshot.Setvxl(ref context.polygonCell[corner],polygonCellvCoord,polygoncCoord);
        }
        internal static void ProcessPolygonCell(
         Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,in MarchingCubesFlags flags,MarchingCubesContext context,
         float isoLevel=-50.0f
        ){
         Voxel[]polygonCell=context.polygonCell;
         int edgeIndex;
         /*
              Determine the index into the edge table which
             tells us which vertices are inside of the surface
         */
                                             edgeIndex =  0;
         if(-polygonCell[0].density<isoLevel)edgeIndex|=  1;
         if(-polygonCell[1].density<isoLevel)edgeIndex|=  2;
         if(-polygonCell[2].density<isoLevel)edgeIndex|=  4;
         if(-polygonCell[3].density<isoLevel)edgeIndex|=  8;
         if(-polygonCell[4].density<isoLevel)edgeIndex|= 16;
         if(-polygonCell[5].density<isoLevel)edgeIndex|= 32;
         if(-polygonCell[6].density<isoLevel)edgeIndex|= 64;
         if(-polygonCell[7].density<isoLevel)edgeIndex|=128;
         if(Tables.edgeTable[edgeIndex]!=0){
          Vector3[]vertices=context.vertices;
         }
        }
    }
}