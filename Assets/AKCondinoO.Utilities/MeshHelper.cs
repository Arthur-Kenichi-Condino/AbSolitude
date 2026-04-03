using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Utilities{
    internal static class MeshHelper{
        internal static Mesh BuildColliderMeshFromSubMeshes(Mesh source,int[]subMeshes){
         var mesh=new Mesh();
         mesh.vertices=source.vertices;
         if(source.normals!=null&&source.normals.Length>0)
          mesh.normals=source.normals;
         if(source.uv!=null&&source.uv.Length>0)
          mesh.uv=source.uv;
         List<int>triangles=new List<int>();
         foreach(var subMeshIndex in subMeshes){
          if(subMeshIndex<0||subMeshIndex>=source.subMeshCount)
           continue;
          triangles.AddRange(source.GetTriangles(subMeshIndex));
         }
         mesh.SetTriangles(triangles,0);
         mesh.RecalculateBounds();
         return mesh;
        }
    }
}