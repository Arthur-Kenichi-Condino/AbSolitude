using AKCondinoO.World.Voxels.MarchingCubes.paulbourke;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
namespace AKCondinoO.World.Voxels.MarchingCubes{
    internal struct Voxel{
     internal float density;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex{
     internal static Vector4 emptyUV{get;}=new Vector4(-1f,-1f,-1f,-1f);
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
         pos=p;pos.w=1f;
         normal=n;
         color=new(0f,0f,0f,0f);
         texCoord0=emptyUV;texCoord0.x=uv0.x;texCoord0.y=uv0.y;
         texCoord1=emptyUV;
         texCoord2=emptyUV;
         texCoord3=emptyUV;
         texCoord4=emptyUV;
         texCoord5=emptyUV;
         texCoord6=new(1f,0f,0f,0f);
         texCoord7=new(0f,0f,0f,0f);
        }
    }
    internal struct MarchingCubesContext{
     public NativeList<Vertex>tempVer;
     public NativeList<UInt32>tempTri;
     public readonly Dictionary<Vector3,List<Vector2>>vertexUV;//  ...para CollectUV
    }
    internal struct MarchingCubesFlags{
     public bool StitchEdges;
     public bool Prediction;
     public bool CollectUV;
    }
    internal static class MarchingCubesCore{
        internal static void ProcessCell(
         ref Voxel[]polygonCell,Vector3Int coord,in MarchingCubesFlags flags,ref MarchingCubesContext context,
         float isoLevel=-50.0f
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
         if(Tables.edgeTable[edgeIndex]!=0){
          Vector3[]vertices;
         }
        }
    }
}