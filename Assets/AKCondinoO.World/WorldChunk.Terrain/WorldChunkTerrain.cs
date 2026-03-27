using AKCondinoO.World.Terrain;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunkTerrain:MonoBehaviour{
     internal WorldChunk chunk;
     internal TerrainChunkBuilder builder;
     internal TerrainChunkNavMeshBuildData navMeshBuildData;
     internal MeshFilter meshFilter;
     internal MeshCollider meshCollider;
        void Awake(){
         builder=new(chunk,this);
         meshFilter=GetComponent<MeshFilter>();
         meshFilter.mesh=builder.mesh;
         meshCollider=GetComponent<MeshCollider>();
         navMeshBuildData=new(chunk,this);
        }
        internal void OnManualDestroy(){
         if(meshFilter!=null){
          meshFilter.mesh=null;
         }
         builder.Destroy();
        }
        internal void DoGeneration(){
         builder.DoUpdateJob();
        }
        internal void OnGenerateUpdate(bool cancelled){
         if(!cancelled){
          meshCollider.sharedMesh=null;
          meshCollider.sharedMesh=builder.mesh;
          navMeshBuildData.UpdateNavMeshBuildData();
         }
         chunk.OnGeneratedTerrain(cancelled);
        }
        internal void OnChunkPool(){
         builder.OnChunkPool();
        }
    }
}