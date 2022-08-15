#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
       internal AsyncOperation[]navMeshAsyncOperation;
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
         int navMeshSettingsLength=NavMeshHelper.navMeshBuildSettings.Length;
         navMeshData=new NavMeshData[navMeshSettingsLength];
         navMeshInstance=new NavMeshDataInstance[navMeshSettingsLength];
         navMeshAsyncOperation=new AsyncOperation[navMeshSettingsLength];
         for(int agentType=0;agentType<navMeshSettingsLength;++agentType){
          string[]navMeshValidation=NavMeshHelper.navMeshBuildSettings[agentType].ValidationReport(activeWorldBounds);
          if(navMeshValidation.Length>0){
           foreach(string s in navMeshValidation){
            Log.Error(s);
           }
          }else{
           Log.DebugMessage("navMeshValidation:success!");
          }
          navMeshData[agentType]=new NavMeshData(NavMeshHelper.navMeshBuildSettings[agentType].agentTypeID){
           hideFlags=HideFlags.None,
          };
          navMeshInstance[agentType]=NavMesh.AddNavMeshData(navMeshData[agentType]);
         }
         cCoord_Previous=cCoord=vecPosTocCoord(transform.position);
         OnCoordinatesChanged();
        }
        internal void OnRemove(){
         int navMeshSettingsLength=NavMeshHelper.navMeshBuildSettings.Length;
         for(int agentType=0;agentType<navMeshSettingsLength;++agentType){
          NavMesh.RemoveNavMeshData(navMeshInstance[agentType]);
         }
        }
        internal void OnVoxelTerrainChunkBaked(VoxelTerrainChunk cnk){
         //Log.DebugMessage("OnVoxelTerrainChunkBaked:navMeshDirty=true");
         navMeshDirty=true;
        }
        internal void OnSimObjectSpawned(SimObject simObject){
         //Log.DebugMessage("OnSimObjectSpawned:navMeshDirty=true");
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
             if(OnNavMeshDataAsyncOperationEnd()){
                 waitingNavMeshDataAsyncOperation=false;
             }
         }else{
             if(navMeshDirty){
                 if(CanStartNavMeshAsyncUpdate()){
                     navMeshDirty=false;
                     waitingNavMeshDataAsyncOperation=true;
                 }
             }else{
             }
         }
        }
        void OnCoordinatesChanged(){
         cnkRgn=cCoordTocnkRgn(cCoord);
         activeWorldBounds.center=new Vector3(cnkRgn.x,0,cnkRgn.y);
         VoxelSystem.singleton.generationRequests.Add(this);
        }
     [SerializeField]float navMeshDataAsyncUpdateInterval=1.0f;
     float navMeshDataAsyncUpdateTimer=0.0f;
     readonly List<NavMeshBuildSource>sources=new List<NavMeshBuildSource>();
        bool CanStartNavMeshAsyncUpdate(){
         if(navMeshDataAsyncUpdateTimer>0.0f){
            navMeshDataAsyncUpdateTimer-=Time.deltaTime;
         }
         if(navMeshDataAsyncUpdateTimer<=0.0f){
            navMeshDataAsyncUpdateTimer=navMeshDataAsyncUpdateInterval;
          Log.DebugMessage("CanStartNavMeshAsyncUpdate:start async operation");
          VoxelSystem.singleton.CollectNavMeshSources(out List<NavMeshBuildSource>sourcesCollected);
          sources.Clear();
          for(int i=0;i<sourcesCollected.Count;++i){
           if(activeWorldBounds.Contains(sourcesCollected[i].transform.GetColumn(3))){
            sources.Add(sourcesCollected[i]);
           }
          }
          int navMeshSettingsLength=NavMeshHelper.navMeshBuildSettings.Length;
          for(int i=0;i<navMeshSettingsLength;++i){
           navMeshAsyncOperation[i]=NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData[i],NavMeshHelper.navMeshBuildSettings[i],sources,activeWorldBounds);
          }
          return true;
         }
         return false;
        }
        bool OnNavMeshDataAsyncOperationEnd(){
         if(navMeshAsyncOperation.All(o=>o==null||o.isDone)){
          Log.DebugMessage("OnNavMeshDataAsyncOperationEnd");
          return true;
         }
         return false;
        }
    }
}