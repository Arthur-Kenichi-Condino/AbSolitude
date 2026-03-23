using AKCondinoO.World;
using AKCondinoO.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
using System.Collections;
namespace AKCondinoO.Bootstrap{
    internal class ActiveZone:MonoBehaviour{
     private static readonly Dictionary<ulong,ActiveZone>zones=new();
     internal static ActiveZone main;
     internal Bounds bounds;
     internal Bounds worldBounds;
        internal static void EnsureExists(ActiveZone activeZonePrefab){
         if(main!=null)return;
         OnAddZoneFor(0,activeZonePrefab);
        }
        internal static void OnAddZoneFor(ulong clientId,ActiveZone activeZonePrefab){
         if(zones.TryGetValue(clientId,out var zone)&&zone!=null){
          return;
         }
         zone=Instantiate(activeZonePrefab);
         DontDestroyOnLoad(zone.gameObject);
         zone.clientId=clientId;
         if(clientId==0){
          main=zone;
         }
         zones[clientId]=zone;
        }
        internal static void InitializeAny(){
         foreach(var kvp in zones){
          var zone=kvp.Value;
          zone.Initialize();
         }
        }
     private Coroutine spawnCoroutine;
        internal void Initialize(){
         Vector3 size=new(
          (WorldChunkManager.singleton.instantiationDistance.x*2+1)*Width,
          Height,
          (WorldChunkManager.singleton.instantiationDistance.y*2+1)*Depth
         );
         bounds=new(new(),size);
         worldBounds=new(new(),size);
         if(spawnCoroutine!=null){StopCoroutine(spawnCoroutine);}
         spawnCoroutine=StartCoroutine(CalculateChunks());
        }
        internal static void ShutdownAny(){
         main=null;
         foreach(var kvp in zones){
          var zone=kvp.Value;
          zone.Shutdown();
          if(zone!=null){
           Destroy(zone.gameObject);
          }
         }
         zones.Clear();
        }
        internal void Shutdown(){
         if(this!=null){
          if(spawnCoroutine!=null){StopCoroutine(spawnCoroutine);}
         }
         if(!object.ReferenceEquals(WorldChunkManager.singleton,null)){
          foreach(var coord in currChunks){
           WorldChunkManager.singleton.RemoveRef(coord);
          }
         }
         currChunks.Clear();
         nextChunks.Clear();
         cCoord=default;
        }
        internal static void ManualUpdateTransformAll(){
         foreach(var kvp in zones){
          var zone=kvp.Value;
          zone.ManualUpdateTransform();
         }
        }
     private ulong clientId;
     private Vector3 pos;
     private Vector2Int cCoord;
     private Vector2Int lastcCoord;
     private bool hasLastcCoord;
        public void ManualUpdateTransform(){
         if(clientId==0){
          transform.position=MainCamera.singleton.transform.position;
         }
         if(!transform.hasChanged){
          return;
         }
         pos=transform.position;
         bounds.center=pos;
         cCoord=vecPosTocCoord(pos);
         if(!hasLastcCoord||cCoord!=lastcCoord){
          var cnkRgn=cCoordTocnkRgn(cCoord);
          worldBounds.center=new(cnkRgn.x,Height/2f,cnkRgn.y);
          lastcCoord=cCoord;
          hasLastcCoord=true;
         }
         transform.hasChanged=false;
        }
     private readonly HashSet<Vector2Int>currChunks=new();
     private readonly HashSet<Vector2Int>nextChunks=new();
        private IEnumerator CalculateChunks(){
         bool hascCoord=false;
         Vector2Int cCoord=default;
         while(true){
          while(hascCoord&&cCoord==this.cCoord){
           yield return null;
          }
          cCoord=this.cCoord;
          Vector2Int exprDis=WorldChunkManager.singleton.expropriationDistance;
          Vector2Int instDis=WorldChunkManager.singleton.instantiationDistance;
          for(int y=-exprDis.y;y<=exprDis.y;y++){
           int cy=cCoord.y+y;
           if(Mathf.Abs(cy)>=MaxcCoordy)continue;
           for(int x=-exprDis.x;x<=exprDis.x;x++){
            int cx=cCoord.x+x;
            if(Mathf.Abs(cx)>=MaxcCoordx)continue;
            var coord=new Vector2Int(cx,cy);
            nextChunks.Add(coord);
           }
          }
          foreach(var coord in nextChunks){
           if(!currChunks.Contains(coord)){
            WorldChunkManager.singleton.AddRef(coord);
           }
           if(InsideInstantiationDistance(coord)){
            WorldChunkManager.singleton.EnsureExists(coord);
           }
          }
          foreach(var coord in currChunks){
           if(!nextChunks.Contains(coord)){
            WorldChunkManager.singleton.RemoveRef(coord);
           }
          }
          currChunks.Clear();
          currChunks.UnionWith(nextChunks);
          nextChunks.Clear();
          hascCoord=true;
          bool InsideInstantiationDistance(Vector2Int coord){
           if(
            Mathf.Abs(cCoord.x-coord.x)<=instDis.x&&
            Mathf.Abs(cCoord.y-coord.y)<=instDis.y
           ){return true;}
           return false;
          }
         }
        }
        void OnDrawGizmos(){
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.blue);
          DrawGizmos.Bounds(worldBounds,Color.white);
         #endif
        }
    }
}