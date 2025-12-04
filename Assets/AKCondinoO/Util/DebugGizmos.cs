using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesBackgroundContainer;
namespace AKCondinoO{
    internal static class DebugGizmos{
        //#if UNITY_EDITOR
            [System.Diagnostics.Conditional("ENABLE_DEBUG_GIZMOS")]
            internal static void DrawBounds(Bounds b,Color color,float duration=0){//[https://gist.github.com/unitycoder/58f4b5d80f423d29e35c814a9556f9d9]
             var p1=new Vector3(b.min.x,b.min.y,b.min.z);// bottom
             var p2=new Vector3(b.max.x,b.min.y,b.min.z);
             var p3=new Vector3(b.max.x,b.min.y,b.max.z);
             var p4=new Vector3(b.min.x,b.min.y,b.max.z);
             var p5=new Vector3(b.min.x,b.max.y,b.min.z);// top
             var p6=new Vector3(b.max.x,b.max.y,b.min.z);
             var p7=new Vector3(b.max.x,b.max.y,b.max.z);
             var p8=new Vector3(b.min.x,b.max.y,b.max.z);
             Debug.DrawLine(p1,p2,color,duration);
             Debug.DrawLine(p2,p3,color,duration);
             Debug.DrawLine(p3,p4,color,duration);
             Debug.DrawLine(p4,p1,color,duration);
             Debug.DrawLine(p5,p6,color,duration);
             Debug.DrawLine(p6,p7,color,duration);
             Debug.DrawLine(p7,p8,color,duration);
             Debug.DrawLine(p8,p5,color,duration);
             Debug.DrawLine(p1,p5,color,duration);// sides
             Debug.DrawLine(p2,p6,color,duration);
             Debug.DrawLine(p3,p7,color,duration);
             Debug.DrawLine(p4,p8,color,duration);
            }
    //  estrutura para comparar pares de vértices independentemente da ordem
    private struct Edge
    {
        public int A, B;
        public Edge(int a, int b)
        {
            // Garantir ordem consistente (menor primeiro)
            if (a < b) { A = a; B = b; }
            else { A = b; B = a; }
        }
    }

    private static readonly HashSet<Edge> edgeSet = new();

    public static void DrawMeshWireframe(List<Vertex>vertices,List<UInt32>triangles,Color color,Vector3 offset=default)
    {
        Gizmos.color = color;
        edgeSet.Clear();

        int triCount = triangles.Count / 3;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = (int)triangles[t * 3 + 0];
            int i1 = (int)triangles[t * 3 + 1];
            int i2 = (int)triangles[t * 3 + 2];

            // 3 arestas do tri (evita duplicados por hash)
            AddEdge(i0, i1);
            AddEdge(i1, i2);
            AddEdge(i2, i0);
        }

        // Desenhar linhas únicas já filtradas
        foreach (var e in edgeSet)
        {
            Gizmos.DrawLine((Vector3)vertices[e.A].pos+offset,(Vector3)vertices[e.B].pos+offset);
        }
    }

    private static void AddEdge(int a, int b)
    {
        edgeSet.Add(new Edge(a, b));
    }
            [System.Diagnostics.Conditional("ENABLE_DEBUG_GIZMOS")]
            internal static void DrawMesh(){
            }
        //#endif
    }
}