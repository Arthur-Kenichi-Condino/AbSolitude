#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO{
    internal class Gameplayer:NetworkBehaviour{
     internal static Gameplayer main;
     internal NetworkObject netObj;
      private readonly NetworkVariable<Vector3>netPosition=new NetworkVariable<Vector3>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
     internal Vector2Int cCoord,cCoord_Previous;
     internal Vector2Int cnkRgn;
     internal Bounds activeWorldBounds;
     internal Bounds worldBounds;
     internal NavMeshData[]navMeshData;
      internal NavMeshDataInstance[]navMeshInstance;
       internal AsyncOperation[]navMeshAsyncOperation;
     internal readonly Queue<SimActor>camFollowableActors=new();
        void Awake(){
         netObj=GetComponent<NetworkObject>();
        }
        internal void Init(ulong clientId){
         activeWorldBounds=new Bounds(Vector3.zero,
          new Vector3(
           (instantiationDistance.x*2+1)*Width,
           Height,
           (instantiationDistance.y*2+1)*Depth
          )
         );
         worldBounds=new Bounds(Vector3.zero,
          new Vector3(
           (expropriationDistance.x*2+1)*Width,
           Height,
           (expropriationDistance.y*2+1)*Depth
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
          if(this==Gameplayer.main){
           navMeshData[agentType]=new NavMeshData(NavMeshHelper.navMeshBuildSettings[agentType].agentTypeID){
            hideFlags=HideFlags.None,
           };
           navMeshInstance[agentType]=NavMesh.AddNavMeshData(navMeshData[agentType]);
          }
         }
         if(Core.singleton.isServer){
          if(this==Gameplayer.main){
           netObj.Spawn(destroyWithScene:false);
           netObj.DontDestroyWithOwner=true;
          }else{
           netObj.SpawnWithOwnership(clientId,destroyWithScene:false);
           netObj.DontDestroyWithOwner=true;
          }
         }
         cCoord_Previous=cCoord=vecPosTocCoord(transform.position);
         OnCoordinatesChanged();
        }
        internal void OnRemove(){
         int navMeshSettingsLength=NavMeshHelper.navMeshBuildSettings.Length;
         for(int agentType=0;agentType<navMeshSettingsLength;++agentType){
          if(this==Gameplayer.main){
           NavMesh.RemoveNavMeshData(navMeshInstance[agentType]);
          }
         }
         if(this!=null){
          if(Core.singleton.isServer){
           if(Core.singleton.netManager.SpawnManager!=null){
            netObj.DontDestroyWithOwner=true;
            netObj.Despawn(destroy:false);
           }
          }
         }
        }
        internal void OnVoxelTerrainChunkBaked(VoxelTerrainChunk cnk){
         //Log.DebugMessage("OnVoxelTerrainChunkBaked:navMeshDirty=true");
         navMeshDirty=true;
        }
        internal void OnSimObjectSpawned(SimObject simObject,int layer){
         //Log.DebugMessage("OnSimObjectSpawned:layer:"+layer);
         OnNavMeshShouldUpdate(simObject,layer);
        }
        internal void OnSimObjectTransformHasChanged(SimObject simObject,int layer){
         Log.DebugMessage("OnSimObjectTransformHasChanged:layer:"+layer);
         OnNavMeshShouldUpdate(simObject,layer);
        }
        internal void OnSimObjectDespawned(SimObject simObject,int layer){
         Log.DebugMessage("OnSimObjectDespawned:layer:"+layer);
         OnNavMeshShouldUpdate(simObject,layer);
        }
        void OnNavMeshShouldUpdate(SimObject simObject,int layer){
         if(simObject.navMeshObstacleCarving){
          navMeshDirty=true;
          Log.DebugMessage("OnNavMeshShouldUpdate:navMeshDirty=true");
         }
         if(PhysUtil.LayerMaskContains(NavMeshHelper.navMeshLayer,layer)){
          navMeshSourcesDirty=true;
          navMeshDirty=true;
          Log.DebugMessage("OnNavMeshShouldUpdate:navMeshDirty=true;navMeshSourcesDirty=true;");
         }
        }
     bool pendingCoordinatesUpdate=true;
     bool waitingNavMeshDataAsyncOperation;
     bool navMeshDirty;
      bool navMeshSourcesDirty;
        void Update(){
         if(this==Gameplayer.main){
          transform.position=Camera.main.transform.position;
         }
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
                      navMeshSourcesDirty=false;
                     waitingNavMeshDataAsyncOperation=true;
                 }
             }else{
             }
         }
        }
        void OnCoordinatesChanged(){
         cnkRgn=cCoordTocnkRgn(cCoord);
         activeWorldBounds.center=new Vector3(cnkRgn.x,0,cnkRgn.y);
         worldBounds.center=activeWorldBounds.center;
         if(this==Gameplayer.main){
          VoxelSystem.singleton.generationRequests.Add(this);
         }
         if(Core.singleton.isServer){
          VoxelSystem.singleton.generationRequestedAssignMessageHandlers.Add(this);
         }
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
          VoxelSystem.singleton.CollectNavMeshSources(out List<NavMeshBuildSource>sourcesCollected,navMeshSourcesDirty);
          sources.Clear();
          for(int i=0;i<sourcesCollected.Count;++i){
           if(activeWorldBounds.Contains(sourcesCollected[i].transform.GetColumn(3))){
            sources.Add(sourcesCollected[i]);
           }
          }
          int navMeshSettingsLength=NavMeshHelper.navMeshBuildSettings.Length;
          for(int i=0;i<navMeshSettingsLength;++i){
           if(this==Gameplayer.main){
            navMeshAsyncOperation[i]=NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData[i],NavMeshHelper.navMeshBuildSettings[i],sources,activeWorldBounds);
           }
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
        #if UNITY_EDITOR
        void OnDrawGizmos(){
         Util.DrawBounds(worldBounds,Color.blue);
        }
        #endif
    }
}