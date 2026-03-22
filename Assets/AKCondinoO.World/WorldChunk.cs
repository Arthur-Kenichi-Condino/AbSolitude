using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Terrain;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunk:MonoBehaviour{
     internal Bounds bounds;
     internal TerrainChunk terrain;
     [SerializeField]internal GameObject terrainObject;
     MeshFilter terrainFilter;
     internal readonly HashSet<SimObject>simObjects=new();
     [SerializeField]internal bool debugDrawMeshWireframe=false;
     [SerializeField]internal bool debugDrawMeshWireframeWhenSelectedOnly=true;
     [SerializeField]internal bool debugDrawMeshWireframeDrawTriangles=true;
     [SerializeField]internal bool debugDrawMeshWireframeDrawNormals=true;
        void Awake(){
         bounds=new(new(),new(Width,Height,Depth));
         terrain=new(this);
         terrainFilter=terrainObject.GetComponent<MeshFilter>();
         terrainFilter.mesh=terrain.mesh;
        }
        internal void ManualDestroy(){
         if(terrainFilter!=null){
          terrainFilter.mesh=null;
         }
         terrain.Destroy();
        }
     private bool firstCall;
        internal void Initialize(){
         if(!pooled)firstCall=true;
         pooled=false;
        }
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
        internal void OnEnsureExists(Vector2Int cCoord){
         if(firstCall||cCoord!=this.cCoord||terrain.cancelled){
          firstCall=false;
          this.cCoord=cCoord;
          this.cnkRgn=cCoordTocnkRgn(this.cCoord);
          Generate();
         }
        }
        internal void Generate(){
         terrain.DoUpdateJob();
        }
        internal void AddSimObject(SimObject simObject){
         simObjects.Add(simObject);
        }
     private bool pooled;
        internal void OnPool(){
         terrain.OnChunkPool();
         pooled=true;
         foreach(var simObject in simObjects){
          simObject.OnChunkPooled();
         }
        }
        void OnDrawGizmos(){
         terrain?.GizmosSelected(false);
        }
        void OnDrawGizmosSelected(){
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.gray);
         #endif
         terrain?.GizmosSelected(true);
        }
    }
}