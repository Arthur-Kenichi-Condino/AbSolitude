using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
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
           zoneCluster.AddZone(zone,false);
           zoneCluster.Merge(toMerge);
          }
          foreach(var clusterZone in zoneCluster.clusterZones){
           zoneToCluster[clusterZone]=zoneCluster;
          }
         }
         foreach(var merged in toMerge){
          clusters.Remove(merged);
          NavMeshCluster.pool.Return(merged);
         }
         toMerge.Clear();
         if(zoneCluster==null){
          zoneCluster=NavMeshCluster.pool.Rent();
          zoneCluster.provider=this;
          zoneCluster.Init();
          zoneCluster.AddZone(zone);
          clusters.Add(zoneCluster);
          zoneToCluster[zone]=zoneCluster;
         }
        }
        internal class NavMeshCluster{
         internal static readonly Utilities.ObjectPool<NavMeshCluster>pool=
          Pool.GetPool<NavMeshCluster>(
           "",
           ()=>new(),
           (NavMeshCluster item)=>{
            item.Reset();
           },
           false
          );
         internal NavMeshProvider provider;
         internal readonly HashSet<ActiveZone>clusterZones=new();
         internal Bounds clusterBounds;
         internal readonly Dictionary<int,NavMeshData>navMeshByAgentType=new();
         internal readonly Dictionary<Vector2Int,NavMeshBuildSource>chunksSources=new();
         internal NavMeshDataInstance[]navMeshInstances;
            internal void Init(){
             navMeshInstances=Pool.RentArray<NavMeshDataInstance>(provider.agentsCount);
             for(int i=0;i<provider.agentsCount;++i){
              var agentTypeID=provider.indexToAgentType[i];
              navMeshByAgentType.Add(agentTypeID,provider.navMeshDataPoolByAgentType[agentTypeID].Rent());
             }
            }
            internal void Reset(){
             isDirty=true;
             for(int i=0;i<provider.agentsCount;i++){
              var agentTypeID=provider.indexToAgentType[i];
              if(navMeshInstances[i].valid)navMeshInstances[i].Remove();
              provider.navMeshDataPoolByAgentType[agentTypeID].Return(navMeshByAgentType[agentTypeID]);
             }
             navMeshByAgentType.Clear();
             chunksSources.Clear();
             Pool.ReturnArray<NavMeshDataInstance>(navMeshInstances);
             navMeshInstances=null;
             clusterZones.Clear();
             provider=null;
            }
         private bool isDirty=true;
            internal void AddZone(ActiveZone zone,bool updateBounds=true){
             if(clusterZones.Add(zone)){
              isDirty=true;
              if(updateBounds)UpdateBounds();
             }
            }
            internal void AddZonesRange(List<ActiveZone>zones,bool updateBounds=true){
             if(!clusterZones.IsSupersetOf(zones)){
              clusterZones.UnionWith(zones);
              isDirty=true;
             }
             if(updateBounds)UpdateBounds();
            }
         readonly HashSet<ActiveZone>unvisited=new();
         readonly List<(List<ActiveZone>list,Bounds groupBounds)>groups=new();
         internal static readonly Utilities.ObjectPool<List<ActiveZone>>groupPool=
          Pool.GetPool<List<ActiveZone>>("",()=>new(),(List<ActiveZone> item)=>{item.Clear();});
            internal void UpdateOrSplit(ActiveZone zone,List<NavMeshCluster>splits){
             if(clusterZones.Contains(zone)){
              if(clusterZones.Count<=1){
               isDirty=true;
               UpdateBounds();
               return;
              }else{
               List<ActiveZone>clusterGroup=null;
               unvisited.UnionWith(clusterZones);
               while(unvisited.Count>0){
                ActiveZone groupFirstZone=null;
                foreach(var clusterZone in unvisited){
                 groupFirstZone=clusterZone;
                 break;
                }
                unvisited.Remove(groupFirstZone);
                var group=groupPool.Rent();
                group.Add(groupFirstZone);
                Bounds groupBounds=groupFirstZone.worldBounds;
                if(groupFirstZone==zone){clusterGroup=group;}
                bool expanded;
                do{
                 expanded=false;
                 foreach(var clusterZone in unvisited){
                  if(groupBounds.Intersects(clusterZone.worldBounds)){
                   group.Add(clusterZone);
                   groupBounds.Encapsulate(clusterZone.worldBounds);
                   if(clusterZone==zone){clusterGroup=group;}
                   expanded=true;
                  }
                 }
                 foreach(var groupZone in group){
                  unvisited.Remove(groupZone);
                 }
                }while(expanded);
                groups.Add((group,groupBounds));
               }
               unvisited.Clear();
               if(groups.Count<=1){
                groupPool.Return(groups[0].list);
                groups.Clear();
                isDirty=true;
                UpdateBounds();
                return;
               }else{
                for(int i=0;i<groups.Count;i++){
                 var curr=groups[i];
                 bool currHasZone=(curr.list==clusterGroup);
                 bool expanded;
                 do{
                  expanded=false;
                  for(int j=i+1;j<groups.Count;j++){
                   var other=groups[j];
                   bool otherHasZone=(other.list==clusterGroup);
                   if(curr.groupBounds.Intersects(other.groupBounds)){
                    curr.groupBounds.Encapsulate(other.groupBounds);
                    curr.list.AddRange(other.list);
                    if(otherHasZone){
                     currHasZone=true;
                    }
                    int lastIndex=groups.Count-1;
                    groups[j]=groups[lastIndex];
                    groups.RemoveAt(lastIndex);
                    groupPool.Return(other.list);
                    expanded=true;
                    j--;
                   }
                  }
                 }while(expanded);
                 if(currHasZone){
                  clusterGroup=curr.list;
                 }
                 groups[i]=curr;
                }
                Logs.Debug("'split':groups.Count:"+groups.Count);
                //Logs.Debug("'split':clusterGroup:"+clusterGroup);
                foreach(var group in groups){
                 if(group.list==clusterGroup){
                  if(clusterZones.Count!=group.list.Count||!clusterZones.IsSupersetOf(group.list)){
                   clusterZones.Clear();
                   clusterZones.UnionWith(group.list);
                  }
                  //Logs.Debug("clusterZones.Count:"+clusterZones.Count);
                  SetBounds(group.groupBounds);
                 }else{
                  var cluster=NavMeshCluster.pool.Rent();
                  cluster.provider=provider;
                  cluster.Init();
                  cluster.AddZonesRange(group.list,false);
                  cluster.SetBounds(group.groupBounds);
                  //Logs.Debug("'split':cluster.clusterZones.Count:"+cluster.clusterZones.Count);
                  splits.Add(cluster);
                 }
                 groupPool.Return(group.list);
                }
                groups.Clear();
               }
              }
             }
            }
            internal void Merge(HashSet<NavMeshCluster>toMerge){
             foreach(var clusterToMerge in toMerge){
              if(!clusterZones.IsSupersetOf(clusterToMerge.clusterZones)){
               clusterZones.UnionWith(clusterToMerge.clusterZones);
               isDirty=true;
              }
             }
             Logs.Debug("clusterZones.Count:"+clusterZones.Count);
             UpdateBounds();
            }
            internal void SetBounds(Bounds bounds){
             if(clusterBounds!=bounds){
             }
             clusterBounds=bounds;
             isDirty=false;
            }
            void UpdateBounds(){
             if(isDirty){
              var oldBounds=clusterBounds;
              bool first=true;
              foreach(var clusterZone in clusterZones){
               if(first){
                first=false;
                clusterBounds=clusterZone.worldBounds;
               }else{
                clusterBounds.Encapsulate(clusterZone.worldBounds);
               }
              }
              if(clusterBounds!=oldBounds){
              }
             }
             isDirty=false;
            }
            internal bool Intersects(Bounds bounds){
             if(clusterBounds.Intersects(bounds)){
              return true;
             }
             return false;
            }
        }
        internal void OnChunkExists(WorldChunk chunk){
        }
    }
}