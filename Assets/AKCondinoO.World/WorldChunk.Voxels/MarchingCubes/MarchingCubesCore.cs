using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
namespace AKCondinoO.World.Voxels.MarchingCubes{
    internal struct Voxel{
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
         ref Voxel[]polygonCell,Vector3Int coord,in MarchingCubesFlags flags,ref MarchingCubesContext context
        ){
         
        }
    }
}