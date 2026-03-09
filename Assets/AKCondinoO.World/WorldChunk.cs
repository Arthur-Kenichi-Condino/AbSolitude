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
     internal readonly HashSet<SimObject>simObjects=new();
     [SerializeField]internal bool debugDrawMeshWireframe=false;
     [SerializeField]internal bool debugDrawMeshWireframeWhenSelectedOnly=true;
        void Awake(){
         bounds=new(new(),new(Width,Height,Depth));
         terrain=new(this);
        }
        internal void ManualDestroy(){
         terrain.Destroy();
        }
     private bool firstCall;
        internal void Initialize(){
         firstCall=true;
        }
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
        internal void OnEnsureExists(Vector2Int cCoord){
         if(firstCall||cCoord!=this.cCoord){
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
        internal void OnPool(){
         foreach(var simObject in simObjects){
          simObject.OnChunkPooled();
         }
        }
        void OnDrawGizmos(){
         terrain.GizmosSelected(false);
        }
        void OnDrawGizmosSelected(){
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.gray);
         #endif
         terrain.GizmosSelected(true);
        }
    }
}