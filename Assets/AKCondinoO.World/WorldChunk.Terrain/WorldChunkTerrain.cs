using AKCondinoO.World.Terrain;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunkTerrain:MonoBehaviour{
     internal WorldChunk chunk;
     internal TerrainChunkBuilder builder;
     MeshFilter terrainFilter;
        void Awake(){
         builder=new(chunk,this);
         terrainFilter=GetComponent<MeshFilter>();
         terrainFilter.mesh=builder.mesh;
        }
        internal void OnManualDestroy(){
         if(terrainFilter!=null){
          terrainFilter.mesh=null;
         }
         builder.Destroy();
        }
        internal void OnGenerate(){
         builder.DoUpdateJob();
        }
        internal void OnChunkPool(){
         builder.OnChunkPool();
        }
    }
}