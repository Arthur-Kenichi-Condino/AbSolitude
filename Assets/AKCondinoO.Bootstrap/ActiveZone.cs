using AKCondinoO.Terrain;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Terrain.VoxelSystemConst;
namespace AKCondinoO.Bootstrap{
    internal class ActiveZone:MonoBehaviour{
     private static readonly Dictionary<ulong,ActiveZone>zones=new();
     internal static ActiveZone main;
        internal static void EnsureExists(ActiveZone activeZonePrefab){
         if(main!=null)return;
         OnAddZoneFor(0,activeZonePrefab);
        }
        private static void OnAddZoneFor(ulong clientId,ActiveZone activeZonePrefab){
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
         pos=transform.position;
         CalculateChunks();
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
          Vector2Int instDis=VoxelSystem.singleton.instantiationDistance;
          Vector2Int exprDis=VoxelSystem.singleton.expropriationDistance;
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
             VoxelSystem.singleton.AddRef(coord);
            }
            if(Mathf.Abs(x)<=instDis.x&&Mathf.Abs(y)<=instDis.y){
             VoxelSystem.singleton.EnsureExists(coord);
            }
           }
          }
          foreach(var coord in currChunks){
           if(!nextChunks.Contains(coord)){
            VoxelSystem.singleton.RemoveRef(coord);
           }
          }
          var temp=currChunks;
          currChunks=nextChunks;
          nextChunks=temp;
          lastcCoord=cCoord;
          hasLastcCoord=true;
         }
        }
    }
}