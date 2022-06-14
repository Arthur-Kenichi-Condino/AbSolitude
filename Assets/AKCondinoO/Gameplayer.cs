#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO{
    internal class Gameplayer:MonoBehaviour{
     internal static Gameplayer main;
     internal static readonly List<Gameplayer>all=new List<Gameplayer>();
     internal Vector2Int cCoord,cCoord_Previous;
     internal Vector2Int cnkRgn;
     internal Bounds activeWorldBounds;
     internal NavMeshData[]navMeshData;
      internal NavMeshDataInstance[]navMeshInstance;
        void Awake(){
        }
        internal void Init(){
         activeWorldBounds=new Bounds(Vector3.zero,
          new Vector3(
           (instantiationDistance.x*2+1)*Width,
           Height,
           (instantiationDistance.y*2+1)*Depth
          )
         );
         for(int agentType=0;agentType<NavMeshHelper.navMeshBuildSettings.Length;++agentType){
          string[]navMeshValidation=NavMeshHelper.navMeshBuildSettings[agentType].ValidationReport(activeWorldBounds);
          if(navMeshValidation.Length>0){
           foreach(string s in navMeshValidation){
            Log.Error(s);
           }
          }else{
           Log.DebugMessage("navMeshValidation:success!");
          }
         }
         cCoord_Previous=cCoord=vecPosTocCoord(transform.position);
         OnCoordinatesChanged();
        }
        internal void OnVoxelTerrainChunkBaked(VoxelTerrainChunk cnk){
         Log.DebugMessage("OnVoxelTerrainChunkBaked:navMeshDirty=true");
         navMeshDirty=true;
        }
        internal void OnSimObjectSpawned(SimObject simObject){
         Log.DebugMessage("OnSimObjectSpawned:navMeshDirty=true");
         navMeshDirty=true;
        }
     bool pendingCoordinatesUpdate=true;
     bool waitingNavMeshDataAsyncOperation;
     bool navMeshDirty;
        void Update(){
         transform.position=Camera.main.transform.position;
         if(transform.hasChanged){
            transform.hasChanged=false;
          pendingCoordinatesUpdate=true;
         }
         if(pendingCoordinatesUpdate){
            pendingCoordinatesUpdate=false;
          cCoord_Previous=cCoord;
          cCoord=vecPosTocCoord(transform.position);
          if(cCoord!=cCoord_Previous){
           OnCoordinatesChanged();
          }
         }
         if(waitingNavMeshDataAsyncOperation){
         }else{
             if(navMeshDirty){
             }else{
             }
         }
        }
        void OnCoordinatesChanged(){
         cnkRgn=cCoordTocnkRgn(cCoord);
         activeWorldBounds.center=new Vector3(cnkRgn.x,0,cnkRgn.y);
         VoxelSystem.singleton.generationRequests.Add(this);
        }
    }
}