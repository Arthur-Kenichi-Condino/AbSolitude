using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.World{
    internal class NavMeshProvider{
     internal readonly Dictionary<int,ObjectPool<NavMeshData>>navMeshDataPoolByAgentType=new();
     static NavMeshData CreateNavMeshData(int agentTypeID)=>new(agentTypeID);
     internal readonly WorldChunkManager world;
     internal readonly int navMeshLayer;
     internal readonly int agentsCount;
     internal readonly NavMeshBuildSettings[]navMeshBuildSettings;
     internal readonly Dictionary<int,int>indexToAgentType=new();
        internal NavMeshProvider(WorldChunkManager world){
         this.world=world;
         var navMeshLayerNames=world.navMeshLayerNames;
         navMeshLayer=LayerMask.GetMask(navMeshLayerNames[0]);
         for(int i=1;i<navMeshLayerNames.Length;i++){
          navMeshLayer|=LayerMask.GetMask(navMeshLayerNames[i]);
         }
         agentsCount=NavMesh.GetSettingsCount();
         navMeshBuildSettings=new NavMeshBuildSettings[agentsCount];
         for(int i=0;i<agentsCount;i++){
          NavMeshBuildSettings settings=NavMesh.GetSettingsByIndex(i);
          var agentTypeID=settings.agentTypeID;
          navMeshBuildSettings[i]=settings;
          indexToAgentType[i]=agentTypeID;
          navMeshDataPoolByAgentType.Add(agentTypeID,
           Pool.GetPool<NavMeshData>(
            agentTypeID.ToString(),
            ()=>CreateNavMeshData(agentTypeID),
            (NavMeshData item)=>{},false
           )
          );
         }
        }
     internal readonly HashSet<NavMeshCluster>onDestroyToPool=new();
        internal void Destroy(){
         foreach(var cluster in clusters){
          onDestroyToPool.Add(cluster);
         }
         foreach(var clusterToPool in onDestroyToPool){
          MarkClusterForPooling(clusterToPool);
         }
         onDestroyToPool.Clear();
         foreach(var markedCluster in clustersMarkedForPooling){
          onDestroyToPool.Add(markedCluster);
         }
         foreach(var clusterToPool in onDestroyToPool){
          IfMarkedThenPool(clusterToPool);
         }
         onDestroyToPool.Clear();
         clustersMarkedForPooling.Clear();
         clusters.Clear();
         zoneToCluster.Clear();
         zones.Clear();
        }
        internal void OnManualUpdate(){
        }
     internal readonly HashSet<ActiveZone>zones=new();
        internal void RegisterActiveZone(ActiveZone zone){
         zones.Add(zone);
        }
        internal void OnActiveZoneChangedcCoord(ActiveZone zone,HashSet<Vector2Int>currChunks){
         RebuildClustersAround(zone,currChunks);
        }
     internal readonly HashSet<NavMeshCluster>clusters=new();
     internal readonly Dictionary<ActiveZone,NavMeshCluster>zoneToCluster=new();
     readonly HashSet<NavMeshCluster>toMerge=new();
     readonly List<NavMeshCluster>splits=new();
        void RebuildClustersAround(ActiveZone zone,HashSet<Vector2Int>currChunks){
         zoneToCluster.TryGetValue(zone,out var zoneCluster);
         if(zoneCluster!=null){
          zoneCluster.UpdateOrSplit(zone,splits);
          if(splits.Count>0){
           foreach(var clusterZone in zoneCluster.clusterZones){
            zoneToCluster[clusterZone]=zoneCluster;
           }
           foreach(var split in splits){
            clusters.Add(split);
            foreach(var clusterZone in split.clusterZones){
             zoneToCluster[clusterZone]=split;
            }
           }
          }
          splits.Clear();
         }
         bool expanded;
         Bounds predictBounds=zone.worldBounds;
         do{
          expanded=false;
          foreach(var cluster in clusters){
           if(cluster==zoneCluster)continue;
           if(toMerge.Contains(cluster)){
            continue;
           }
           if(cluster.Intersects(predictBounds)){
            predictBounds.Encapsulate(cluster.clusterBounds);
            toMerge.Add(cluster);
            expanded=true;
           }
          }
         }while(expanded);
         if(toMerge.Count>0){
          if(zoneCluster!=null){
           zoneCluster.Merge(toMerge);
          }else{
           foreach(var clusterToMerge in toMerge){
            zoneCluster=clusterToMerge;
            break;
           }
           toMerge.Remove(zoneCluster);
           zoneCluster.RegisterZone(zone,false);
           zoneCluster.Merge(toMerge);
          }
          foreach(var clusterZone in zoneCluster.clusterZones){
           zoneToCluster[clusterZone]=zoneCluster;
          }
         }
         foreach(var merged in toMerge){
          MarkClusterForPooling(merged);
         }
         toMerge.Clear();
         if(zoneCluster==null){
          zoneCluster=RequestCluster();
          zoneCluster.RegisterZone(zone);
          clusters.Add(zoneCluster);
          zoneToCluster[zone]=zoneCluster;
         }
        }
        internal void OnChunkExists(ActiveZone zone,WorldChunk chunk){
         Logs.Debug(()=>"'chunk exists:rebuild nav mesh':"+chunk.cCoord);
         if(zoneToCluster.TryGetValue(zone,out var cluster)){
          cluster.AddOrUpdateSource(chunk);
         }
        }
        internal NavMeshCluster RequestCluster(){
         var cluster=NavMeshCluster.pool.Rent();
         cluster.Init(this);
         if(cluster.updateNavMeshAsyncCoroutine!=null){world.StopCoroutine(cluster.updateNavMeshAsyncCoroutine);}
         cluster.updateNavMeshAsyncCoroutine=world.StartCoroutine(cluster.UpdateNavMeshAsyncCoroutine());
         return cluster;
        }
     internal readonly HashSet<NavMeshCluster>clustersMarkedForPooling=new();
        internal void MarkClusterForPooling(NavMeshCluster cluster){
         clusters.Remove(cluster);
         clustersMarkedForPooling.Add(cluster);
         if(!cluster.isBusy){
          IfMarkedThenPool(cluster);
         }
        }
        internal void IfMarkedThenPool(NavMeshCluster cluster){
         if(!clustersMarkedForPooling.Contains(cluster)){
          return;
         }
         if(world!=null){
          if(cluster.updateNavMeshAsyncCoroutine!=null){world.StopCoroutine(cluster.updateNavMeshAsyncCoroutine);}
          cluster.updateNavMeshAsyncCoroutine=null;
         }
         clustersMarkedForPooling.Remove(cluster);
         cluster.EnsureStopThenPool();
        }
    }
}