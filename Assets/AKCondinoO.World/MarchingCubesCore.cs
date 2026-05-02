using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes.paulbourke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.Terrain.TerrainChunkBuilder;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.MarchingCubes{
    internal struct MarchingCubesFlags{
     public bool StitchEdges;
     public bool Prediction;
     public bool CollectMaterials;
    }
    internal static class MarchingCubesCore{
        internal static void BuildMeshData(Vector3Int min,Vector3Int max,Vector2Int cCoord,MarchingCubesContext context){
         Logs.Debug(()=>$"cCoord:{cCoord}",null,cCoord==Vector2Int.zero);
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         int width =(max.x-min.x)+1;
         int height=(max.y-min.y)+1;
         int depth =(max.z-min.z)+1;
         context.width =width;
         context.height=height;
         context.depth =depth;
         context.biomeContext.terrainHeightNoiseCachePadding=new(1,1,1);
         int bcwidth =width +2;
         int bcheight=height+2;
         int bcdepth =depth +2;
         context.biomeContext.width =bcwidth;
         context.biomeContext.height=bcheight;
         context.biomeContext.depth =bcdepth;
         context.biomeContext.hasTerrainHeightNoiseCache=Pool.RentArray<bool  >(bcwidth*bcdepth);
         context.biomeContext.   terrainHeightNoiseCache=Pool.RentArray<double>(bcwidth*bcdepth);
         context.       polygonCellCache=Pool.RentArray<Voxel  >(4+depth*4+width*depth*4);
         context.          verticesCache=Pool.RentArray<Vector3>(4+depth*4+width*depth*4);
         context.normalOffsetVoxelsCache=Pool.RentArray<Voxel  >(1+depth+width*depth    );
         Vector3Int polygonCoord;
         Vector3Int coord;
         for(coord=new Vector3Int(0,min.y,0),polygonCoord=new();coord.y<=max.y;coord.y++,polygonCoord.y++){
         for(coord.x=               min.x   ,polygonCoord.x=0  ;coord.x<=max.x;coord.x++,polygonCoord.x++){
         for(coord.z=               min.z   ,polygonCoord.z=0  ;coord.z<=max.z;coord.z++,polygonCoord.z++){
          if(Volatile.Read(ref context.cancel)==1){return;}
          var polygoncCoord=cCoord;
          var polygonvCoord=coord;
          ValidatevCoord(ref polygoncCoord,ref polygonvCoord);
          BuildPolygonCell(polygonCoord,polygonvCoord,polygoncCoord,context);
          MarchingCubesFlags flags=new(){
           StitchEdges=polygoncCoord!=cCoord,
           Prediction=false,
           CollectMaterials=true,
          };
          ProcessPolygonCell(cnkRgn,polygonCoord,polygonvCoord,polygoncCoord,in flags,context);
         }}}
         BlendMaterials(context);
        }
        private static void BuildPolygonCell(Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         for(int corner=0;corner<8;corner++){
          SetPolygonCellVoxel(corner,polygonCoord,polygonvCoord,polygoncCoord,context);
         }
         context.UpdatePolygonCellCache(polygonCoord);
        }
        private static void SetPolygonCellVoxel(int corner,Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,MarchingCubesContext context){
         if(!context.TryUsePolygonCellCache(corner,polygonCoord)){
          Vector3Int offset=MarchingCubesContext.polygonCellCornerOffset[corner];
          Vector3Int polygonCellCoord =polygonCoord +offset;
          Vector3Int polygonCellvCoord=polygonvCoord+offset;
          context.biomeContext.coord=polygonCellCoord;
          BiomesConfigurationSnapshot.SetTerrainvxl(ref context.polygonCell[corner],polygonCellvCoord,polygoncCoord,context.biomeContext);
          SetPolygonCellVoxelNormal(ref context.polygonCell[corner],corner,polygonCellvCoord,polygoncCoord,polygonCellCoord,context);
         }
        }
        private static void SetPolygonCellVoxelNormal(ref Voxel polygonCellVoxel,int corner,Vector3Int polygonCellvCoord,Vector2Int polygoncCoord,Vector3Int polygonCellCoord,MarchingCubesContext context){
         if(polygonCellVoxel.normal==Vector3.zero){
          for(int i=0;i<6;i++){
           var offset=MarchingCubesContext.polygonCellVoxelNormalOffsets[i];
           var cCoord=polygoncCoord;
           Vector3Int coord =polygonCellCoord +offset;
           Vector3Int vCoord=polygonCellvCoord+offset;
           if(!context.TryGetPolygonCellVoxelNormalOffsetCachedVoxel(i,polygonCellCoord)){
            context.biomeContext.coord=coord;
            BiomesConfigurationSnapshot.SetTerrainvxl(ref context.normalOffsetVoxels[i],vCoord,cCoord,context.biomeContext);
           }
          }
          polygonCellVoxel.normal=SampleNormal(context.normalOffsetVoxels);
          context.UpdatePolygonCellVoxelNormalOffsetCache(corner,polygonCellCoord);
         }
        }
        internal static Vector3 SampleNormal(
         Voxel[]normalOffsetVoxels
        ){
         float dxL =normalOffsetVoxels[0].density;
         float dxR =normalOffsetVoxels[1].density;
         float dyBo=normalOffsetVoxels[2].density;
         float dyT =normalOffsetVoxels[3].density;
         float dzBa=normalOffsetVoxels[4].density;
         float dzF =normalOffsetVoxels[5].density;
         return SampleNormal(dxL,dxR,dyBo,dyT,dzBa,dzF);
        }
     internal static readonly Vector3Int[]sampleNormalOffsets=new[]{
      new Vector3Int(-1,0,0),
      new Vector3Int( 1,0,0),
      new Vector3Int(0,-1,0),
      new Vector3Int(0, 1,0),
      new Vector3Int(0,0,-1),
      new Vector3Int(0,0, 1),
     };
        internal struct SampleNormalContext{
         public SampleDensityContext dxL ;
         public SampleDensityContext dxR ;
         public SampleDensityContext dyBo;
         public SampleDensityContext dyT ;
         public SampleDensityContext dzBa;
         public SampleDensityContext dzF ;
           internal static void Build(
            Vector3Int center,
            Vector2Int cCoord,
            ref SampleDensityContext baseContext,
            ref SampleNormalContext normalContext
           ){
            BuildSubContext(ref normalContext.dxL ,center,cCoord,sampleNormalOffsets[0],ref baseContext);
            BuildSubContext(ref normalContext.dxR ,center,cCoord,sampleNormalOffsets[1],ref baseContext);
            BuildSubContext(ref normalContext.dyBo,center,cCoord,sampleNormalOffsets[2],ref baseContext);
            BuildSubContext(ref normalContext.dyT ,center,cCoord,sampleNormalOffsets[3],ref baseContext);
            BuildSubContext(ref normalContext.dzBa,center,cCoord,sampleNormalOffsets[4],ref baseContext);
            BuildSubContext(ref normalContext.dzF ,center,cCoord,sampleNormalOffsets[5],ref baseContext);
               static void BuildSubContext(
                ref SampleDensityContext ctx,
                Vector3Int center,
                Vector2Int cCoord,
                Vector3Int offset,
                ref SampleDensityContext baseContext
               ){
                var coord=center+offset;
                var input=new SampleContext(coord,cCoord);
                if(offset.x==0&&offset.z==0){
                 ctx=new SampleDensityContext(input){
                  hasHeight=baseContext.hasHeight,
                  heightValue=baseContext.heightValue
                 };
                }else{
                 ctx=new SampleDensityContext(input);
                }
               }
           }
        }
        internal static Vector3 SampleNormal(ref SampleNormalContext normalContext){
         float dxL =SampleDensity(ref normalContext.dxL ,out _);
         float dxR =SampleDensity(ref normalContext.dxR ,out _);
         float dyBo=SampleDensity(ref normalContext.dyBo,out _);
         float dyT =SampleDensity(ref normalContext.dyT ,out _);
         float dzBa=SampleDensity(ref normalContext.dzBa,out _);
         float dzF =SampleDensity(ref normalContext.dzF ,out _);
         return SampleNormal(dxL,dxR,dyBo,dyT,dzBa,dzF);
        }
        internal static Vector3 SampleNormal(
         float dxL ,float dxR,
         float dyBo,float dyT,
         float dzBa,float dzF
        ){
         Vector3 normal=new(
          dxR-dxL,
          dyT-dyBo,
          dzF-dzBa
         );
         normal.Normalize();
         return normal!=Vector3.zero?normal:Vector3.up;
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
        internal static void ProcessPolygonCell(Vector2Int cnkRgn,
         Vector3Int polygonCoord,Vector3Int polygonvCoord,Vector2Int polygoncCoord,in MarchingCubesFlags flags,
         MarchingCubesContext context,
         float isoLevel=-50.0f
        ){
         int width =context.width;
         int height=context.height;
         int depth =context.depth;
         Vector2Int polygoncnkRgn=cCoordTocnkRgn(polygoncCoord);
         Vector3 polygonPos=polygonvCoord+new Vector3(0.5f,0.5f,0.5f)-new Vector3(Width/2f,Height/2f,Depth/2f)+new Vector3(polygoncnkRgn.x,0,polygoncnkRgn.y)-new Vector3(cnkRgn.x,0,cnkRgn.y);
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
          context.TryUseVerticesCache(polygonCoord);
             Vector3[]vertices =context.vertices;
                bool[]isCached =context.isCached;
          MaterialId[]materials=context.materials;
             Vector3[]normals  =context.normals;
          for(int i=0;i<12;i++){
           if(0!=(Tables.edgeTable[edgeIndex]&interpMask[i])){VertexInterp(interpCorners[i,0],interpCorners[i,1],ref vertices[i],isCached[i],ref materials[i],ref normals[i]);}
          }
          void VertexInterp(int c0,int c1,ref Vector3 p,bool isCached,ref MaterialId m,ref Vector3 n){
              Vector3[]vertex  =context.vertex;
                float[]density =context.density;
           MaterialId[]material=context.material;
           vertex[0]=corners[c0];density[0]=-polygonCell[c0].density;material[0]=polygonCell[c0].material;
           vertex[1]=corners[c1];density[1]=-polygonCell[c1].density;material[1]=polygonCell[c1].material;
           if(isCached){goto _Normal;}
           if(Math.Abs(isoLevel  -density[0])<float.Epsilon){p=vertex[0];goto _Normal;}
           if(Math.Abs(isoLevel  -density[1])<float.Epsilon){p=vertex[1];goto _Normal;}
           if(Math.Abs(density[0]-density[1])<float.Epsilon){p=vertex[0];goto _Normal;}
           float marchingUnit=(isoLevel-density[0])/(density[1]-density[0]);
           p.x=(float)(vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x));
           p.y=(float)(vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y));
           p.z=(float)(vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z));
           goto _Normal;
           _Normal:{}
           float[]distance=context.distance;
           distance[0]=Vector3.Distance(vertex[0],vertex[1]);
           distance[1]=Vector3.Distance(vertex[1],p);
           n=Vector3.Lerp(
            polygonCell[c1].normal,
            polygonCell[c0].normal,
            distance[1]/distance[0]
           );
           n.Normalize();
           n=n!=Vector3.zero?n:Vector3.up;
           goto _Material;
           _Material:{}
           m=material[0];
           if(density[1]<density[0]){
            m=material[1];
           }else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
            m=material[1];
           }
          }
          context.UpdateVerticesCache(polygonCoord);
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
           var mat=Math.Max(Math.Max((uint)materials[idx[0]],(uint)materials[idx[1]]),(uint)materials[idx[2]]);
           Vector2 matUV=MaterialAtlasHelper.GetCoord(mat);
           bool addedVertexes=false;
           if(!flags.StitchEdges){
            ref var tempVer=ref context.meshData.tempVer;
            ref var tempTri=ref context.meshData.tempTri;
            tempVer.Add(new Vertex(verPos[0],normals[idx[0]],matUV));
            tempVer.Add(new Vertex(verPos[1],normals[idx[1]],matUV));
            tempVer.Add(new Vertex(verPos[2],normals[idx[2]],matUV));
            tempTri.Add(context.vertexCount+2u);
            tempTri.Add(context.vertexCount+1u);
            tempTri.Add(context.vertexCount   );
                                context.vertexCount+=3u;
            addedVertexes=true;
           }
           if(flags.CollectMaterials){
            var vertexMaterials=context.vertexMaterials;
            ulong key=GetKey(polygonCoord,idx[0]);if(!vertexMaterials.TryGetValue(key,out var materialCounter)){materialCounter=MaterialCounter.pool.Rent();materialCounter.Create(8);vertexMaterials.Add(key,materialCounter);}materialCounter.Add(mat);if(addedVertexes){uint vertexIndex=context.vertexCount-3u;context.vertexToCounter[vertexIndex]=materialCounter;}
                  key=GetKey(polygonCoord,idx[1]);if(!vertexMaterials.TryGetValue(key,out     materialCounter)){materialCounter=MaterialCounter.pool.Rent();materialCounter.Create(8);vertexMaterials.Add(key,materialCounter);}materialCounter.Add(mat);if(addedVertexes){uint vertexIndex=context.vertexCount-2u;context.vertexToCounter[vertexIndex]=materialCounter;}
                  key=GetKey(polygonCoord,idx[2]);if(!vertexMaterials.TryGetValue(key,out     materialCounter)){materialCounter=MaterialCounter.pool.Rent();materialCounter.Create(8);vertexMaterials.Add(key,materialCounter);}materialCounter.Add(mat);if(addedVertexes){uint vertexIndex=context.vertexCount-1u;context.vertexToCounter[vertexIndex]=materialCounter;}
               static ulong GetKey(Vector3Int coord,int edge){
                EdgeToGrid(
                 coord.x,coord.y,coord.z,edge,
                 out int gx,out int gy,out int gz,out int axis
                );
                return(ulong)
                 (((ulong)(uint)gx)<<42)|
                 (((ulong)(uint)gy)<<21)|
                 (((ulong)(uint)gz)<< 2)|
                 ((ulong)(uint)axis);
                   static void EdgeToGrid(
                    int x,int y,int z,int edge,
                    out int gx,out int gy,out int gz,out int axis
                   ){
                    switch(edge){
                     case  0:gx=x;  gy=y;  gz=z;  axis=0;break;
                     case  1:gx=x+1;gy=y;  gz=z;  axis=1;break;
                     case  2:gx=x;  gy=y+1;gz=z;  axis=0;break;
                     case  3:gx=x;  gy=y;  gz=z;  axis=1;break;
                     case  4:gx=x;  gy=y;  gz=z+1;axis=0;break;
                     case  5:gx=x+1;gy=y;  gz=z+1;axis=1;break;
                     case  6:gx=x;  gy=y+1;gz=z+1;axis=0;break;
                     case  7:gx=x;  gy=y;  gz=z+1;axis=1;break;
                     case  8:gx=x;  gy=y;  gz=z;  axis=2;break;
                     case  9:gx=x+1;gy=y;  gz=z;  axis=2;break;
                     case 10:gx=x+1;gy=y+1;gz=z;  axis=2;break;
                     default:gx=x;  gy=y+1;gz=z;  axis=2;break;
                    }
                   }
               }
           }
          }
         }
        }
        internal static void BlendMaterials(MarchingCubesContext context){
         if(context.vertexToCounter.Count<=0){return;}
         var dest=MaterialCounter.pool.Rent();
         dest.Create(8);
         ref var tempVer=ref context.meshData.tempVer;
         for(int i=0;i<tempVer.Length/3;i++){
          int v0=i*3;
          int v1=i*3+1;
          int v2=i*3+2;
          var materialCounter0=context.vertexToCounter[(uint)v0];MaterialCounter.MergeAdd(materialCounter0,dest);
          var materialCounter1=context.vertexToCounter[(uint)v1];MaterialCounter.MergeAdd(materialCounter1,dest);
          var materialCounter2=context.vertexToCounter[(uint)v2];MaterialCounter.MergeAdd(materialCounter2,dest);
          dest.Sort(context.weights.Length);
          tempVer[v0]=SetVertex(v0,tempVer[v0],materialCounter0);
          tempVer[v1]=SetVertex(v1,tempVer[v1],materialCounter1);
          tempVer[v2]=SetVertex(v2,tempVer[v2],materialCounter2);
          Vertex SetVertex(int vidx,in Vertex v,MaterialCounter vmaterialCounter){
           ReadSlots(v,context.slots);
              void ReadSlots(in Vertex v,Vector2[]slots){
               slots[0]=new(v.texCoord0.x,v.texCoord0.y);
               slots[1]=new(v.texCoord0.z,v.texCoord0.w);
               slots[2]=new(v.texCoord1.x,v.texCoord1.y);
               slots[3]=new(v.texCoord1.z,v.texCoord1.w);
               slots[4]=new(v.texCoord2.x,v.texCoord2.y);
               slots[5]=new(v.texCoord2.z,v.texCoord2.w);
               slots[6]=new(v.texCoord3.x,v.texCoord3.y);
               slots[7]=new(v.texCoord3.z,v.texCoord3.w);
              }
           float total=0;
           for(int j=0;j<dest.sortedLength&&j<context.weights.Length;j++){
            uint mat=dest.sortedId[j];
            Vector2 uv=MaterialAtlasHelper.GetCoord(mat);
            int slot=FindSlot(context.slots,uv);
            if(slot<=-1)slot=FindEmptySlot(context.slots);
            if(slot<=-1)continue;
            context.slots[slot]=uv;
               int FindSlot(Vector2[]slots,Vector2 uv){
                for(int i=0;i<slots.Length;i++){
                 if(slots[i]==uv){
                  return i;
                 }
                }
                return-1;
               }
               int FindEmptySlot(Vector2[]slots){
                for(int i=0;i<slots.Length;i++){
                 if(
                  slots[i].x==Vertex.emptyUV.x&&
                  slots[i].y==Vertex.emptyUV.y
                 ){
                  return i;
                 }
                }
                return-1;
               }
            int count=vmaterialCounter.Count(mat);
            context.weights[slot]=count;
            total+=count;
           }
           if(total>0){
            for(int j=0;j<context.weights.Length;j++)
             context.weights[j]/=total;
           }
           var result=WriteSlots(v,context.slots,context.weights);
              Vertex WriteSlots(in Vertex v,Vector2[]slots,float[]weights){
               SortSlots(slots,weights);
               var result=v;
               result.texCoord0.x=slots[0].x;result.texCoord0.y=slots[0].y;
               result.texCoord0.z=slots[1].x;result.texCoord0.w=slots[1].y;
               result.texCoord1.x=slots[2].x;result.texCoord1.y=slots[2].y;
               result.texCoord1.z=slots[3].x;result.texCoord1.w=slots[3].y;
               result.texCoord2.x=slots[4].x;result.texCoord2.y=slots[4].y;
               result.texCoord2.z=slots[5].x;result.texCoord2.w=slots[5].y;
               result.texCoord3.x=slots[6].x;result.texCoord3.y=slots[6].y;
               result.texCoord3.z=slots[7].x;result.texCoord3.w=slots[7].y;
               result.texCoord6.x=weights[0];
               result.texCoord6.y=weights[1];
               result.texCoord6.z=weights[2];
               result.texCoord6.w=weights[3];
               result.texCoord7.x=weights[4];
               result.texCoord7.y=weights[5];
               result.texCoord7.z=weights[6];
               result.texCoord7.w=weights[7];
               return result;
                  static void SortSlots(Vector2[]slots,float[]weights){
                   for(int i=1;i<slots.Length;i++){
                    Vector2 s=slots[i];
                    float w=weights[i];
                    int j=i-1;
                    while(j>=0&&Compare(slots[j],s)>0){
                     slots[j+1]=slots[j];
                     weights[j+1]=weights[j];
                     j--;
                    }
                    slots[j+1]=s;
                    weights[j+1]=w;
                   }
                  }
                  static int Compare(Vector2 a,Vector2 b){
                   if(a.x<b.x)return-1;
                   if(a.x>b.x)return 1;
                   if(a.y<b.y)return-1;
                   if(a.y>b.y)return 1;
                   return 0;
                  }
              }
           Array.Clear(context.slots  ,0,context.slots  .Length);
           Array.Clear(context.weights,0,context.weights.Length);
           return result;
          }
          dest.Reset();
         }
         MaterialCounter.pool.Return(dest);
        }
    }
    internal class MarchingCubesContext{
     internal static readonly Utilities.ObjectPool<MarchingCubesContext>pool=
      Pool.GetPool<MarchingCubesContext>(
       "",
       ()=>new(),
       (MarchingCubesContext item)=>{
        BiomesConfigurationContext.pool.Return(item.biomeContext);item.biomeContext=null;
        item.meshData=null;
        item.vertexCount=0;
        foreach(var kvp in item.vertexMaterials){
         var materialCounter=kvp.Value;
         MaterialCounter.pool.Return(materialCounter);
        }
        item.vertexMaterials.Clear();
        item.vertexToCounter.Clear();
        Pool.ReturnArray<Voxel  >(item.       polygonCellCache,true);
        item.       polygonCellCache=null;
        Pool.ReturnArray<Vector3>(item.          verticesCache,true);
        item.          verticesCache=null;
        Pool.ReturnArray<Voxel  >(item.normalOffsetVoxelsCache,true);
        item.normalOffsetVoxelsCache=null;
        Array.Clear(item.polygonCell,0,item.polygonCell.Length);
        Volatile.Write(ref item.cancel,0);
       }
      );
     internal int cancel;
     public MeshData meshData;
     public BiomesConfigurationContext biomeContext;
     internal int width;
     internal int height;
     internal int depth;
     public UInt32 vertexCount;
     public readonly Dictionary<ulong,MaterialCounter>vertexMaterials=new();//  ...para CollectMaterials
     public readonly Dictionary<uint ,MaterialCounter>vertexToCounter=new();
     public readonly Voxel[]polygonCell=new Voxel[8];
     public Voxel[]polygonCellCache;
        internal void UpdatePolygonCellCache(Vector3Int polygonCoord){
         polygonCellCache[PolygonCellCacheIndex(0)]=polygonCell[4];
         polygonCellCache[PolygonCellCacheIndex(1)]=polygonCell[5];
         polygonCellCache[PolygonCellCacheIndex(2)]=polygonCell[6];
         polygonCellCache[PolygonCellCacheIndex(3)]=polygonCell[7];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,0)]=polygonCell[1];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,1)]=polygonCell[2];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,2)]=polygonCell[5];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,3)]=polygonCell[6];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.x,polygonCoord.z,0)]=polygonCell[3];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.x,polygonCoord.z,1)]=polygonCell[2];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.x,polygonCoord.z,2)]=polygonCell[7];
         polygonCellCache[PolygonCellCacheIndex(polygonCoord.x,polygonCoord.z,3)]=polygonCell[6];
        }
        internal bool TryUsePolygonCellCache(int corner,Vector3Int polygonCoord){
         byte mask=polygonCellCacheCornerMask[corner];
         int ci;
         if((mask&1)!=0&&polygonCoord.z>0){
          ci=polygonCellCacheCornerMap[0,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[PolygonCellCacheIndex(ci)];
           return true;
          }
         }
         if((mask&2)!=0&&polygonCoord.x>0){
          ci=polygonCellCacheCornerMap[1,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[PolygonCellCacheIndex(polygonCoord.z,ci)];
           return true;
          }
         }
         if((mask&4)!=0&&polygonCoord.y>0){
          ci=polygonCellCacheCornerMap[2,corner];
          if(ci>=0){
           polygonCell[corner]=polygonCellCache[PolygonCellCacheIndex(polygonCoord.x,polygonCoord.z,ci)];
           return true;
          }
         }
         return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int PolygonCellCacheIndex(int i){
         return i;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int PolygonCellCacheIndex(int z,int i){
         return 4+z*4+i;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int PolygonCellCacheIndex(int x,int z,int i){
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
     public readonly Voxel[]normalOffsetVoxels=new Voxel[6];
     public Voxel[]normalOffsetVoxelsCache;
        internal void UpdatePolygonCellVoxelNormalOffsetCache(int corner,Vector3Int coord){
         normalOffsetVoxelsCache[0]                            =polygonCell[corner];
         normalOffsetVoxelsCache[1+coord.z]                    =polygonCell[corner];
         normalOffsetVoxelsCache[1+depth+coord.z+coord.x*depth]=polygonCell[corner];
        }
        internal bool TryGetPolygonCellVoxelNormalOffsetCachedVoxel(int i,Vector3Int coord){
         if(coord.z<=1||coord.x<=1||coord.y<=1)return false;
         switch(i){
          case 1:
           if(normalOffsetVoxelsCache[1+coord.z].isCreated){
            normalOffsetVoxels[i]=normalOffsetVoxelsCache[1+coord.z];
            return true;
           }
          break;
          case 3:
           if(normalOffsetVoxelsCache[1+depth+coord.z+coord.x*depth].isCreated){
            normalOffsetVoxels[i]=normalOffsetVoxelsCache[1+depth+coord.z+coord.x*depth];
            return true;
           }
          break;
          case 5:
           if(normalOffsetVoxelsCache[0].isCreated){
            normalOffsetVoxels[i]=normalOffsetVoxelsCache[0];
            return true;
           }
          break;
         }
         return false;
        }
     internal static readonly Vector3Int[]polygonCellVoxelNormalOffsets={
      new( 1, 0, 0),new(-1, 0, 0),
      new( 0, 1, 0),new( 0,-1, 0),
      new( 0, 0, 1),new( 0, 0,-1),
     };
     public readonly    Vector3[]vertices =new    Vector3[12];
     public readonly       bool[]isCached =new       bool[12];
     public Vector3[]verticesCache;
        internal void UpdateVerticesCache(Vector3Int coord){
         verticesCache[0]=vertices[ 4]+Vector3.back;
         verticesCache[1]=vertices[ 5]+Vector3.back;
         verticesCache[2]=vertices[ 6]+Vector3.back;
         verticesCache[3]=vertices[ 7]+Vector3.back;
         verticesCache[4+coord.z*4+0]=vertices[ 1]+Vector3.left;
         verticesCache[4+coord.z*4+1]=vertices[ 5]+Vector3.left;
         verticesCache[4+coord.z*4+2]=vertices[ 9]+Vector3.left;
         verticesCache[4+coord.z*4+3]=vertices[10]+Vector3.left;
         verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+0]=vertices[ 2]+Vector3.down;
         verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+1]=vertices[ 6]+Vector3.down;
         verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+2]=vertices[10]+Vector3.down;
         verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+3]=vertices[11]+Vector3.down;
        }
        internal void TryUseVerticesCache(Vector3Int coord){
         isCached[ 0]=false;
         if      (coord.z>0){
          vertices[ 0]=verticesCache[0];
          isCached[ 0]=true;
         }else if(coord.y>0){
          vertices[ 0]=verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+0];
          isCached[ 0]=true;
         }
         isCached[ 1]=false;
         if      (coord.z>0){
          vertices[ 1]=verticesCache[1];
          isCached[ 1]=true;
         }
         isCached[ 2]=false;
         if      (coord.z>0){
          vertices[ 2]=verticesCache[2];
          isCached[ 2]=true;
         }
         isCached[ 3]=false;
         if      (coord.z>0){
          vertices[ 3]=verticesCache[3];
          isCached[ 3]=true;
         }else if(coord.x>0){
          vertices[ 3]=verticesCache[4+coord.z*4+0];
          isCached[ 3]=true;
         }
         isCached[ 4]=false;
         if      (coord.y>0){
          vertices[ 4]=verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+1];
          isCached[ 4]=true;
         }
         isCached[ 7]=false;
         if      (coord.x>0){
          vertices[ 7]=verticesCache[4+coord.z*4+1];
          isCached[ 7]=true;
         }
         isCached[ 8]=false;
         if      (coord.x>0){
          vertices[ 8]=verticesCache[4+coord.z*4+2];
          isCached[ 8]=true;
         }else if(coord.y>0){
          vertices[ 8]=verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+3];
          isCached[ 8]=true;
         }
         isCached[ 9]=false;
         if      (coord.y>0){
          vertices[ 9]=verticesCache[4+depth*4+(coord.z+coord.x*depth)*4+2];
          isCached[ 9]=true;
         }
         isCached[11]=false;
         if      (coord.x>0){
          vertices[11]=verticesCache[4+coord.z*4+3];
          isCached[11]=true;
         }
        }
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
     public readonly Vector2[]slots=new Vector2[8];
     public readonly float[]weights=new float[8];
    }
    internal class MaterialCounter{
     internal static readonly Utilities.ObjectPool<MaterialCounter>pool=
      Pool.GetPool<MaterialCounter>(
       "",
       ()=>new(),
       (MaterialCounter item)=>{
        item.Clear();
       }
      );
     public int length;
     public uint[]id;
     public  int[]c;
        internal void Create(int length){
         this.length=length;
         id=Pool.RentArray<uint>(length);
         c =Pool.RentArray< int>(length);
         sortedId=Pool.RentArray<uint>(length);
         sortedC =Pool.RentArray< int>(length);
        }
        internal void Clear(){
         Pool.ReturnArray<uint>(id);id=null;
         Pool.ReturnArray< int>(c );c =null;
         Pool.ReturnArray<uint>(sortedId);sortedId=null;
         Pool.ReturnArray< int>(sortedC );sortedC =null;
        }
        internal void Add(MaterialId mat,int count=1){
         Add((uint)mat,count);
        }
        internal void Add(uint mat,int count=1){
         for(int i=0;i<length;i++){
          if(c[i]!=0&&id[i]==mat){
           c[i]+=count;
           return;
          }
         }
         for(int i=0;i<length;i++){
          if(c[i]==0){
           id[i]=mat;
           c[i]=count;
           return;
          }
         }
         int min=0;
         for(int i=1;i<length;i++)
          if(c[i]<c[min]||(c[i]<=c[min]&&id[i]<=id[min]))
           min=i;
         uint priority=Math.Max((uint)id[min],(uint)mat);
         if(mat==priority){
          id[min]=mat;
          c[min]=count;
         }
        }
        internal void Reset(){
         Array.Clear(id,0,id.Length);
         Array.Clear(c ,0,c .Length);
         sortedLength=0;
         Array.Clear(sortedId,0,sortedId.Length);
         Array.Clear(sortedC ,0,sortedC .Length);
        }
     public int sortedLength;
     public uint[]sortedId;
     public  int[]sortedC;
        internal void Sort(int length){
         int mcount=0;
         for(int i=0;i<this.length;i++){
          int c=this.c[i];
          if(c==0)
           continue;
          sortedId[mcount]=this.id[i];
          sortedC[mcount]=c;
          mcount++;
          if(mcount>=length)
           break;
         }
         for(int i=1;i<mcount;i++){
          int c=sortedC[i];
          uint m=sortedId[i];
          int j=i-1;
          while(j>=0&&(sortedC[j]<c||(sortedC[j]<=c&&sortedId[j]<=m))){
           sortedC[j+1]=sortedC[j];
           sortedId[j+1]=sortedId[j];
           j--;
          }
          sortedC[j+1]=c;
          sortedId[j+1]=m;
         }
         this.sortedLength=mcount;
        }
        internal static void MergeAdd(MaterialCounter src,MaterialCounter dest){
         for(int i=0;i<src.length;i++){
          int count=src.c[i];
          if(count==0)
           continue;
          uint mat=src.id[i];
          bool found=false;
          for(int j=0;j<dest.length;j++){
           if(dest.c[j]>0&&dest.id[j]==mat){
            dest.c[j]+=count;
            found=true;
            break;
           }
          }
          if(found)
           continue;
          dest.Add(mat,count);
         }
        }
        internal int Count(uint mat){
         for(int i=0;i<length;i++){if(id[i]==mat)return c[i];}
         return 0;
        }
    }
}