#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using paulbourke.MarchingCubes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWaterBackgroundContainer;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    //  marching cubes functions
    internal class MarchingCubesWater{
     internal const double isoLevel=-50.0d;
     internal static readonly ReadOnlyCollection<Vector3>corners=new ReadOnlyCollection<Vector3>(
      new Vector3[8]{
       new Vector3(-.5f,-.5f,-.5f),
       new Vector3( .5f,-.5f,-.5f),
       new Vector3( .5f, .5f,-.5f),
       new Vector3(-.5f, .5f,-.5f),
       new Vector3(-.5f,-.5f, .5f),
       new Vector3( .5f,-.5f, .5f),
       new Vector3( .5f, .5f, .5f),
       new Vector3(-.5f, .5f, .5f),
      }
     );
        internal struct VoxelWater{
         internal double density;
         internal double previousDensity;
         internal bool sleeping;
         internal float evaporateAfter;
         internal Vector3 normal;
         internal bool isCreated;
            internal VoxelWater(double density,double previousDensity,bool sleeping):this(density,previousDensity,sleeping,-1f){
            }
            internal VoxelWater(double density,double previousDensity,bool sleeping,float evaporateAfter){
             this.density=density;this.previousDensity=previousDensity;this.sleeping=sleeping;this.evaporateAfter=evaporateAfter;normal=Vector3.zero;isCreated=true;
            }
         internal static VoxelWater air    {get;}=new VoxelWater(0.0,0.0,true);
         internal static VoxelWater bedrock{get;}=new VoxelWater(0.0,0.0,true);
        }
     internal static Vector3 trianglePosAdj{get;}=new Vector3(
      (Width /2.0f)-0.5f,
      (Height/2.0f)-0.5f,
      (Depth /2.0f)-0.5f
     );
        internal static void DoMarchingCubes(
         VoxelWater[]polygonCell,
          Vector3Int vCoord1,
           Vector3[]vertices,
            Vector3[][][]verticesCache,
             Vector3[]normals,
              double[]density,
               Vector3[]vertex,
                float[]distance,
                 int[]idx,
                  Vector3[]verPos,
                   ref UInt32 vertexCount,
                    NativeList<Vertex>TempVer,
                    NativeList<UInt32>TempTri
        ){
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
         if(Tables.EdgeTable[edgeIndex]!=0){
          /*  Cube is not entirely in/out of the surface  */
          Array.Clear(vertices,0,vertices.Length);
          //  Use cached data if available
          vertices[ 0]=vCoord1.z>0?verticesCache[0][0][0]:(vCoord1.y>0?verticesCache[2][vCoord1.z+vCoord1.x*Depth][0]:Vector3.zero);
          vertices[ 1]=vCoord1.z>0?verticesCache[0][0][1]:Vector3.zero;
          vertices[ 2]=vCoord1.z>0?verticesCache[0][0][2]:Vector3.zero;
          vertices[ 3]=vCoord1.z>0?verticesCache[0][0][3]:(vCoord1.x>0?verticesCache[1][vCoord1.z][0]:Vector3.zero);
          vertices[ 4]=vCoord1.y>0?verticesCache[2][vCoord1.z+vCoord1.x*Depth][1]:Vector3.zero;
          vertices[ 7]=vCoord1.x>0?verticesCache[1][vCoord1.z][1]:Vector3.zero;
          vertices[ 8]=vCoord1.x>0?verticesCache[1][vCoord1.z][2]:(vCoord1.y>0?verticesCache[2][vCoord1.z+vCoord1.x*Depth][3]:Vector3.zero);
          vertices[ 9]=vCoord1.y>0?verticesCache[2][vCoord1.z+vCoord1.x*Depth][2]:Vector3.zero;
          vertices[11]=vCoord1.x>0?verticesCache[1][vCoord1.z][3]:Vector3.zero;
          if(0!=(Tables.EdgeTable[edgeIndex]&   1)){VertexInterp(0,1,ref vertices[ 0],ref normals[ 0]/*,ref materials[ 0]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&   2)){VertexInterp(1,2,ref vertices[ 1],ref normals[ 1]/*,ref materials[ 1]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&   4)){VertexInterp(2,3,ref vertices[ 2],ref normals[ 2]/*,ref materials[ 2]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&   8)){VertexInterp(3,0,ref vertices[ 3],ref normals[ 3]/*,ref materials[ 3]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&  16)){VertexInterp(4,5,ref vertices[ 4],ref normals[ 4]/*,ref materials[ 4]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&  32)){VertexInterp(5,6,ref vertices[ 5],ref normals[ 5]/*,ref materials[ 5]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&  64)){VertexInterp(6,7,ref vertices[ 6],ref normals[ 6]/*,ref materials[ 6]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]& 128)){VertexInterp(7,4,ref vertices[ 7],ref normals[ 7]/*,ref materials[ 7]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]& 256)){VertexInterp(0,4,ref vertices[ 8],ref normals[ 8]/*,ref materials[ 8]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]& 512)){VertexInterp(1,5,ref vertices[ 9],ref normals[ 9]/*,ref materials[ 9]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&1024)){VertexInterp(2,6,ref vertices[10],ref normals[10]/*,ref materials[10]*/);}
          if(0!=(Tables.EdgeTable[edgeIndex]&2048)){VertexInterp(3,7,ref vertices[11],ref normals[11]/*,ref materials[11]*/);}
          void VertexInterp(int c0,int c1,ref Vector3 p,ref Vector3 n/*,ref MaterialId m*/){
           density[0]=-polygonCell[c0].density;vertex[0]=corners[c0];//material[0]=polygonCell[c0].material;
           density[1]=-polygonCell[c1].density;vertex[1]=corners[c1];//material[1]=polygonCell[c1].material;
           //  position p
           if(p!=Vector3.zero){goto _Normal;}
           if(Math.Abs(isoLevel-density[0])<double.Epsilon){p=vertex[0];goto _Normal;}
           if(Math.Abs(isoLevel-density[1])<double.Epsilon){p=vertex[1];goto _Normal;}
           if(Math.Abs(density[0]-density[1])<double.Epsilon){p=vertex[0];goto _Normal;}
           double marchingUnit=(isoLevel-density[0])/(density[1]-density[0]);
           p.x=(float)(vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x));
           p.y=(float)(vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y));
           p.z=(float)(vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z));
           //  normal n
           _Normal:{
            distance[0]=Vector3.Distance(vertex[0],vertex[1]);
            distance[1]=Vector3.Distance(vertex[1],p);
            n=Vector3.Lerp(
             polygonCell[c1].normal,
             polygonCell[c0].normal,
             distance[1]/distance[0]
            );
            n=n!=Vector3.zero?n.normalized:Vector3.up;
           }
           //  material m
           //m=material[0];
           //if(density[1]<density[0]){
           // m=material[1];
           //}else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
           // m=material[1];
           //}
          }
          //  Cache the data
          //  Adiciona um valor "negativo" porque o voxelCoord próximo vai usar
          // este valor mas precisa obtê-lo como se fosse "a posição anterior ou de trás"
          verticesCache[0][0][0]=vertices[ 4]+Vector3.back;
          verticesCache[0][0][1]=vertices[ 5]+Vector3.back;
          verticesCache[0][0][2]=vertices[ 6]+Vector3.back;
          verticesCache[0][0][3]=vertices[ 7]+Vector3.back;
          verticesCache[1][vCoord1.z][0]=vertices[ 1]+Vector3.left;
          verticesCache[1][vCoord1.z][1]=vertices[ 5]+Vector3.left;
          verticesCache[1][vCoord1.z][2]=vertices[ 9]+Vector3.left;
          verticesCache[1][vCoord1.z][3]=vertices[10]+Vector3.left;
          verticesCache[2][vCoord1.z+vCoord1.x*Depth][0]=vertices[ 2]+Vector3.down;
          verticesCache[2][vCoord1.z+vCoord1.x*Depth][1]=vertices[ 6]+Vector3.down;
          verticesCache[2][vCoord1.z+vCoord1.x*Depth][2]=vertices[10]+Vector3.down;
          verticesCache[2][vCoord1.z+vCoord1.x*Depth][3]=vertices[11]+Vector3.down;
          /*  Create the triangle  */
          for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
           idx[0]=Tables.TriangleTable[edgeIndex][i  ];
           idx[1]=Tables.TriangleTable[edgeIndex][i+1];
           idx[2]=Tables.TriangleTable[edgeIndex][i+2];
           Vector3 pos=vCoord1-trianglePosAdj;
           //Vector2 materialUV=AtlasHelper.uv[
           // Mathf.Max(
           //  (int)materials[idx[0]],
           //  (int)materials[idx[1]],
           //  (int)materials[idx[2]]
           // )
           //];
           TempVer.Add(new Vertex(verPos[0]=pos+vertices[idx[0]],normals[idx[0]]/*,materialUV*/));
           TempVer.Add(new Vertex(verPos[1]=pos+vertices[idx[1]],normals[idx[1]]/*,materialUV*/));
           TempVer.Add(new Vertex(verPos[2]=pos+vertices[idx[2]],normals[idx[2]]/*,materialUV*/));
           TempTri.Add(vertexCount+2u);
           TempTri.Add(vertexCount+1u);
           TempTri.Add(vertexCount   );
                       vertexCount+=3u;
           //if(!vertexUV.ContainsKey(verPos[0])){
           // if(vertexUVListPool.TryDequeue(out List<Vector2>list)){
           //  vertexUV.Add(verPos[0],list);
           // }else{
           //  vertexUV.Add(verPos[0],new List<Vector2>());
           // }   
           //}
           //if(!vertexUV.ContainsKey(verPos[1])){
           // if(vertexUVListPool.TryDequeue(out List<Vector2>list)){
           //  vertexUV.Add(verPos[1],list);
           // }else{
           //  vertexUV.Add(verPos[1],new List<Vector2>());
           // }   
           //}
           //if(!vertexUV.ContainsKey(verPos[2])){
           // if(vertexUVListPool.TryDequeue(out List<Vector2>list)){
           //  vertexUV.Add(verPos[2],list);
           // }else{
           //  vertexUV.Add(verPos[2],new List<Vector2>());
           // }   
           //}
           //vertexUV[verPos[0]].Add(materialUV);
           //vertexUV[verPos[1]].Add(materialUV);
           //vertexUV[verPos[2]].Add(materialUV);
          }
         }
        }
    }
}