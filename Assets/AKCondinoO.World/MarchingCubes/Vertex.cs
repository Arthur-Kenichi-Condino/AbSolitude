using System.Runtime.InteropServices;
using UnityEngine;
namespace AKCondinoO.World.MarchingCubes{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex{
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
         texCoord0=emptyUV;
         texCoord0.x=uv0.x;
         texCoord0.y=uv0.y;
         texCoord1=emptyUV;
         texCoord2=emptyUV;
         texCoord3=emptyUV;
         texCoord4=emptyUV;
         texCoord5=emptyUV;
         texCoord6=new(1f,0f,0f,0f);
         texCoord7=new(0f,0f,0f,0f);
        }
     internal static Vector4 emptyUV{get;}=new(-1f,-1f,-1f,-1f);
    }
}