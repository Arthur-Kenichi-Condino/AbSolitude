using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Utilities{
    internal static class DrawGizmos{
        ///<summary>
        ///  [https://gist.github.com/unitycoder/58f4b5d80f423d29e35c814a9556f9d9]
        ///</summary>
        ///<param name="b"></param>
        ///<param name="color"></param>
        ///<param name="duration"></param>
        internal static void Bounds(Bounds b,Color color,float duration=0){
         Vector3 p1=new(b.min.x,b.min.y,b.min.z);// bottom
         Vector3 p2=new(b.max.x,b.min.y,b.min.z);
         Vector3 p3=new(b.max.x,b.min.y,b.max.z);
         Vector3 p4=new(b.min.x,b.min.y,b.max.z);
         Vector3 p5=new(b.min.x,b.max.y,b.min.z);// top
         Vector3 p6=new(b.max.x,b.max.y,b.min.z);
         Vector3 p7=new(b.max.x,b.max.y,b.max.z);
         Vector3 p8=new(b.min.x,b.max.y,b.max.z);
         UnityEngine.Debug.DrawLine(p1,p2,color,duration);
         UnityEngine.Debug.DrawLine(p2,p3,color,duration);
         UnityEngine.Debug.DrawLine(p3,p4,color,duration);
         UnityEngine.Debug.DrawLine(p4,p1,color,duration);
         UnityEngine.Debug.DrawLine(p5,p6,color,duration);
         UnityEngine.Debug.DrawLine(p6,p7,color,duration);
         UnityEngine.Debug.DrawLine(p7,p8,color,duration);
         UnityEngine.Debug.DrawLine(p8,p5,color,duration);
         UnityEngine.Debug.DrawLine(p1,p5,color,duration);// sides
         UnityEngine.Debug.DrawLine(p2,p6,color,duration);
         UnityEngine.Debug.DrawLine(p3,p7,color,duration);
         UnityEngine.Debug.DrawLine(p4,p8,color,duration);
        }
        internal static void RotatedBounds(
         Bounds bounds,
         Transform t,
         Color color,
         float duration=0
        ){
         var v=Pool.RentArray<Vector3>(8);
         BoundsHelper.TransformBoundsVertices(bounds,t,v);
         RotatedBounds(v,color,duration);
         Pool.ReturnArray(v);
        }
        internal static void RotatedBounds(
         Bounds bounds,
         Vector3 pos,
         Quaternion rot,
         Vector3 scale,
         Color color,
         float duration=0
        ){
         var v=Pool.RentArray<Vector3>(8);
         BoundsHelper.TransformBoundsVertices(bounds,pos,rot,scale,v);
         RotatedBounds(v,color,duration);
         Pool.ReturnArray(v);
        }
        internal static void RotatedBounds(
         Vector3[]v,
         Color color,
         float duration=0
        ){
         Debug.DrawLine(v[0],v[1],color,duration);
         Debug.DrawLine(v[1],v[2],color,duration);
         Debug.DrawLine(v[2],v[3],color,duration);
         Debug.DrawLine(v[3],v[0],color,duration);
         Debug.DrawLine(v[4],v[5],color,duration);
         Debug.DrawLine(v[5],v[6],color,duration);
         Debug.DrawLine(v[6],v[7],color,duration);
         Debug.DrawLine(v[7],v[4],color,duration);
         Debug.DrawLine(v[0],v[4],color,duration);
         Debug.DrawLine(v[1],v[5],color,duration);
         Debug.DrawLine(v[2],v[6],color,duration);
         Debug.DrawLine(v[3],v[7],color,duration);
        }
        internal static void MeshWireframe(List<Vertex>vertices,List<UInt32>triangles,Color color,bool drawTriangles,bool drawNormals,Vector3 offset=default){
         Gizmos.color=color;
         int triCount=triangles.Count/3;
         for(int t=0;t<triCount;t++){
          int i0=(int)triangles[t*3+0];
          int i1=(int)triangles[t*3+1];
          int i2=(int)triangles[t*3+2];
          DrawTriangleEdge(i0,i1);DrawNormal(i0);
          DrawTriangleEdge(i1,i2);DrawNormal(i1);
          DrawTriangleEdge(i2,i0);DrawNormal(i2);
         }
            void DrawTriangleEdge(int a,int b){
             int A,B;
             if(a<b){A=a;B=b;}else{A=b;B=a;}
             if(drawTriangles)Gizmos.DrawLine((Vector3)vertices[A].pos+offset,(Vector3)vertices[B].pos+offset);
            }
            void DrawNormal(int a){
             if(drawNormals)Gizmos.DrawLine((Vector3)vertices[a].pos+offset,(Vector3)vertices[a].pos+offset+(Vector3)vertices[a].normal);
            }
        }
        //  Com ajuda de Gemini
        internal static void WireCapsule(Vector3 pos,float height,float radius,Color color){
         Gizmos.color=color;
         Vector3 point1=pos+Vector3.up*(height/2-radius);
         Vector3 point2=pos-Vector3.up*(height/2-radius);
         Gizmos.DrawWireSphere(point1,radius);
         Gizmos.DrawWireSphere(point2,radius);
         Gizmos.DrawLine(point1+Vector3.left   *radius,point2+Vector3.left   *radius);
         Gizmos.DrawLine(point1+Vector3.right  *radius,point2+Vector3.right  *radius);
         Gizmos.DrawLine(point1+Vector3.forward*radius,point2+Vector3.forward*radius);
         Gizmos.DrawLine(point1+Vector3.back   *radius,point2+Vector3.back   *radius);
        }
    }
}