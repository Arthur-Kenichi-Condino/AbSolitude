using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes.paulbourke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
     public BiomesConfigurationContext biomeContext;
     public NativeList<Vertex>tempVer;
     public NativeList<UInt32>tempTri;
     public UInt32 vertexCount;
     public readonly Dictionary<int,List<Vector2>>vertexUV=new();//  ...para CollectUV
     public readonly Voxel[]polygonCell=new Voxel[8];
     public Voxel[]polygonCellCache;
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
        internal bool TryUsePolygonCellCache(int corner,int depth,Vector3Int polygonCoord){
         byte mask=polygonCellCacheCornerMask[corner];
         int ci;
         if((mask&1)!=0&&polygonCoord.z>0){
          ci=polygonCellCacheCornerMap[0,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(ci)];
           return true;
          }
         }
         if((mask&2)!=0&&polygonCoord.x>0){
          ci=polygonCellCacheCornerMap[1,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(polygonCoord.z,ci)];
           return true;
          }
         }
         if((mask&4)!=0&&polygonCoord.y>0){
          ci=polygonCellCacheCornerMap[2,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[MarchingCubesContext.PolygonCellCacheIndex(depth,polygonCoord.x,polygonCoord.z,ci)];
           return true;
          }
         }
         return false;
        }
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
     internal static readonly byte[]polygonCellCacheCornerMask={
      0b111,0b101,0b001,0b011,
      0b110,0b100,0b000,0b010,
     };
     internal static readonly int[,]polygonCellCacheCornerMap={
      {0, 1, 2, 3,-1,-1,-1,-1,},
      {0,-1,-1, 1, 2,-1,-1, 3,},
      {0, 1,-1,-1, 2, 3,-1,-1,},
     };
     public readonly    Vector3[]vertices =new    Vector3[12];
     public readonly       bool[]isCached =new       bool[12];
     public readonly MaterialId[]materials=new MaterialId[12];
     public readonly    Vector3[]normals  =new    Vector3[12];
     public readonly    Vector3[]vertex  =new    Vector3[2];
     public readonly      float[]density =new      float[2];
     public readonly MaterialId[]material=new MaterialId[2];
     public readonly      float[]distance=new      float[2];
     public readonly     int[]idx   =new     int[3];
     public readonly Vector3[]verPos=new Vector3[3];
     internal static readonly Vector3Int[]polygonCellCornerOffset={
      new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),
      new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1),
     };
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
        item.vertexCount=0;
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
        internal static void BuildMeshData(Vector3Int min,Vector3Int max,Vector2Int cCoord,MarchingCubesContext context){
         Logs.Message(Logs.LogType.Debug,$"cCoord:{cCoord}",null,cCoord==Vector2Int.zero);
         int width =(max.x-min.x)+1;
         int height=(max.y-min.y)+1;
         int depth =(max.z-min.z)+1;
         context.biomeContext.depth=depth;
         context.biomeContext.hasTerrainHeightNoiseCache=Pool.RentArray<bool  >(width*depth);
         context.biomeContext.   terrainHeightNoiseCache=Pool.RentArray<double>(width*depth);
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
           StitchEdges=false,
           Prediction=false,
           CollectUV=true,
          };
          ProcessPolygonCell(width,height,depth,polygonCoord,polygonvCoord,polygoncCoord,in flags,context);
         }}}
        }
        private static void BuildPolygonCell(int depth,Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         for(int corner=0;corner<8;corner++){
          SetPolygonCellVoxel(corner,depth,polygonCoord,polygonvCoord,polygoncCoord,context);
         }
         context.UpdatePolygonCellCache(depth,polygonCoord);
        }
        private static void SetPolygonCellVoxel(int corner,int depth,Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         if(!context.TryUsePolygonCellCache(corner,depth,polygonCoord)){
          Vector3Int offset=MarchingCubesContext.polygonCellCornerOffset[corner];
          Vector3Int polygonCellCoord =polygonCoord +offset;
          Vector3Int polygonCellvCoord=polygonvCoord+offset;
          context.biomeContext.coord=polygonCellCoord;
          BiomesConfigurationSnapshot.Setvxl(ref context.polygonCell[corner],polygonCellvCoord,polygoncCoord,context.biomeContext);
         }
        }
     private static readonly Vector3[]corners=new Vector3[8]{
      new(-.5f,-.5f,-.5f),
      new( .5f,-.5f,-.5f),
      new( .5f, .5f,-.5f),
      new(-.5f, .5f,-.5f),
      new(-.5f,-.5f, .5f),
      new( .5f,-.5f, .5f),
      new( .5f, .5f, .5f),
      new(-.5f, .5f, .5f),
     };
     private static readonly int[,]interpCorners=new int[,]{
      {0,1,},{1,2,},{2,3,},
      {3,0,},{4,5,},{5,6,},
      {6,7,},{7,4,},{0,4,},
      {1,5,},{2,6,},{3,7,},
     };
     private static readonly int[]interpMask=new int[]{
         1,
         2,
         4,
         8,
        16,
        32,
        64,
       128,
       256,
       512,
      1024,
      2048,
     };
        internal static void ProcessPolygonCell(int width,int height,int depth,
         Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,in MarchingCubesFlags flags,
         MarchingCubesContext context,
         float isoLevel=-50.0f
        ){
         Vector2Int cnkRgn=cCoordTocnkRgn(polygoncCoord);
         Vector3 polygonPos=polygonvCoord+new Vector3(0.5f,0.5f,0.5f)+new Vector3(cnkRgn.x,0,cnkRgn.y);
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
             Vector3[]vertices =context.vertices;
                bool[]isCached =context.isCached;
          MaterialId[]materials=context.materials;
             Vector3[]normals  =context.normals;
          for(int i=0;i<12;i++){
           if(0!=(Tables.edgeTable[edgeIndex]&interpMask[i])){VertexInterp(interpCorners[i,0],interpCorners[i,1],ref vertices[i],ref isCached[i],ref materials[i],ref normals[i]);}
          }
          void VertexInterp(int c0,int c1,ref Vector3 p,ref bool isCached,ref MaterialId m,ref Vector3 n){
              Vector3[]vertex  =context.vertex;
                float[]density =context.density;
           MaterialId[]material=context.material;
           vertex[0]=corners[c0];density[0]=-polygonCell[c0].density;material[0]=polygonCell[c0].material;
           vertex[1]=corners[c1];density[1]=-polygonCell[c1].density;material[1]=polygonCell[c1].material;
           if(Math.Abs(isoLevel  -density[0])<float.Epsilon){p=vertex[0];goto _Material;}
           if(Math.Abs(isoLevel  -density[1])<float.Epsilon){p=vertex[1];goto _Material;}
           if(Math.Abs(density[0]-density[1])<float.Epsilon){p=vertex[0];goto _Material;}
           float marchingUnit=(isoLevel-density[0])/(density[1]-density[0]);
           p.x=(float)(vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x));
           p.y=(float)(vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y));
           p.z=(float)(vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z));
           goto _Material;
           _Material:{}
           m=material[0];
           if(density[1]<density[0]){
            m=material[1];
           }else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
            m=material[1];
           }
           goto _Normal;
           _Normal:{}
          }
          /*  Create the triangle  */
              int[]idx   =context.idx;
          Vector3[]verPos=context.verPos;
          for(int i=0;Tables.triangleTable[edgeIndex,i]!=-1;i+=3){
           idx[0]=Tables.triangleTable[edgeIndex,i  ];
           idx[1]=Tables.triangleTable[edgeIndex,i+1];
           idx[2]=Tables.triangleTable[edgeIndex,i+2];
           verPos[0]=polygonPos+vertices[idx[0]];
           verPos[1]=polygonPos+vertices[idx[1]];
           verPos[2]=polygonPos+vertices[idx[2]];
           Vector2 materialUV=default;
           context.tempVer.Add(new Vertex(verPos[0],normals[idx[0]],materialUV));
           context.tempVer.Add(new Vertex(verPos[1],normals[idx[1]],materialUV));
           context.tempVer.Add(new Vertex(verPos[2],normals[idx[2]],materialUV));
           context.tempTri.Add(context.vertexCount+2u);
           context.tempTri.Add(context.vertexCount+1u);
           context.tempTri.Add(context.vertexCount   );
                               context.vertexCount+=3u;
          }
         }
        }
    }
}