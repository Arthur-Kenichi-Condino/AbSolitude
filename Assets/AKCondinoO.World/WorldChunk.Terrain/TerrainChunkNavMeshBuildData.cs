using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.World.NavMeshProvider;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Terrain{
    internal class TerrainChunkNavMeshBuildData{
     private readonly WorldChunk chunk;
     private readonly WorldChunkTerrain terrain;
     internal NavMeshBuildSource navMeshSource;
        internal TerrainChunkNavMeshBuildData(WorldChunk chunk,WorldChunkTerrain terrain){
         this.chunk=chunk;this.terrain=terrain;
         navMeshSource=new NavMeshBuildSource{
          transform=chunk.transform.localToWorldMatrix,//  ...deve ser atualizado sempre que o chunk é usado para outra coordenada
          shape=NavMeshBuildSourceShape.Mesh,
          sourceObject=terrain.builder.mesh,
          component=terrain.meshCollider,
          area=0,
         };
        }
        internal void UpdateNavMeshBuildData(){
         navMeshSource.transform=chunk.transform.localToWorldMatrix;
        }
     internal readonly HashSet<NavMeshCluster>clusteredIn=new();
        internal void RegisterCluter(NavMeshCluster cluster){
         clusteredIn.Add(cluster);
        }
        internal void UnregisterSource(){
         foreach(var cluster in clusteredIn){
          cluster.RemoveSource(chunk);
         }
         clusteredIn.Clear();
        }
    }
}