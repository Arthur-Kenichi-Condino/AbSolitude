using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Terrain;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunk:MonoBehaviour{
     internal Bounds bounds;
     private TerrainChunk terrain;
     internal readonly HashSet<SimObject>simObjects=new();
        void Awake(){
         bounds=new(new(),new(Width,Height,Depth));
         terrain=new(this);
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
          terrain.DoMarchingCubes();
         }
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
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.gray);
         #endif
        }
    }
}