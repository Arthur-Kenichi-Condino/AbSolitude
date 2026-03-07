using AKCondinoO.World;
using AKCondinoO.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.Bootstrap{
    internal class ActiveZone:MonoBehaviour{
     private static readonly Dictionary<ulong,ActiveZone>zones=new();
     internal static ActiveZone main;
     internal Bounds bounds;
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
        internal void Initialize(){
         Vector3 size=new(
          (WorldChunkManager.singleton.instantiationDistance.x*2+1)*Width,
          Height,
          (WorldChunkManager.singleton.instantiationDistance.y*2+1)*Depth
         );
         bounds=new(new(),size);
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
         if(!object.ReferenceEquals(WorldChunkManager.singleton,null)){
          foreach(var coord in currChunks){
           WorldChunkManager.singleton.RemoveRef(coord);
          }
         }
         currChunks.Clear();
         nextChunks.Clear();
         cCoord=default;
         lastcCoord=default;
         hasLastcCoord=false;
        }
        internal static void ManualUpdateTransformAll(){
         foreach(var kvp in zones){
          var zone=kvp.Value;
          zone.ManualUpdateTransform();
         }
        }
     private ulong clientId;
     private Vector3 pos;
        public void ManualUpdateTransform(){
         if(clientId==0){
          transform.position=MainCamera.singleton.transform.position;
         }
         if(!transform.hasChanged){
          return;
         }
         //Logs.Message(Logs.LogType.Debug,"transform.hasChanged:"+transform.hasChanged);
         pos=transform.position;
         bounds.center=pos;
         CalculateChunks();
         transform.hasChanged=false;
        }
     private HashSet<Vector2Int>currChunks=new();
     private HashSet<Vector2Int>nextChunks=new();
     private Vector2Int cCoord;
     private Vector2Int lastcCoord;
     private bool hasLastcCoord;
        private void CalculateChunks(){
         cCoord=vecPosTocCoord(pos);
         if(!hasLastcCoord||cCoord!=lastcCoord){
          nextChunks.Clear();
          Vector2Int instDis=WorldChunkManager.singleton.instantiationDistance;
          Vector2Int exprDis=WorldChunkManager.singleton.expropriationDistance;
          for(int y=-exprDis.y;y<=exprDis.y;y++){
           int cy=cCoord.y+y;
           if(Mathf.Abs(cy)>=MaxcCoordy)continue;
           for(int x=-exprDis.x;x<=exprDis.x;x++){
            int cx=cCoord.x+x;
            if(Mathf.Abs(cx)>=MaxcCoordx)continue;
            var coord=new Vector2Int(cx,cy);
            bool hasRef=currChunks.Contains(coord);
            nextChunks.Add(coord);
            if(!hasRef){
             WorldChunkManager.singleton.AddRef(coord);
            }
            if(Mathf.Abs(x)<=instDis.x&&Mathf.Abs(y)<=instDis.y){
             WorldChunkManager.singleton.EnsureExists(coord);
            }
           }
          }
          foreach(var coord in currChunks){
           if(!nextChunks.Contains(coord)){
            WorldChunkManager.singleton.RemoveRef(coord);
           }
          }
          var temp=currChunks;
          currChunks=nextChunks;
          nextChunks=temp;
          lastcCoord=cCoord;
          hasLastcCoord=true;
         }
        }
        void OnDrawGizmosSelected(){
         #if UNITY_EDITOR
          DrawGizmos.Bounds(bounds,Color.blue);
         #endif
        }
    }
}