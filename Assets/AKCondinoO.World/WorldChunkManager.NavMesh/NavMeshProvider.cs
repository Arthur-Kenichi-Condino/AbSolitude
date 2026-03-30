using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
         cluster.EnsureFinished();
         clustersMarkedForPooling.Remove(cluster);
         NavMeshCluster.pool.Return(cluster);
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
         internal Coroutine updateNavMeshAsyncCoroutine;
         internal readonly HashSet<ActiveZone>clusterZones=new();
         internal Bounds clusterBounds;
         internal readonly Dictionary<int,NavMeshData>navMeshByAgentType=new();
         internal readonly SortedDictionary<Vector2Int,NavMeshBuildSource>chunksSources=new(new ChunkCoordComparer());
            class ChunkCoordComparer:IComparer<Vector2Int>{
                public int Compare(Vector2Int a,Vector2Int b){
                 int c=a.y.CompareTo(b.y);
                 if(c!=0)return c;
                 return a.x.CompareTo(b.x);
                }
            }
            internal void Init(NavMeshProvider provider){
             this.provider=provider;
             isDirty=true;
             clusterBounds=default;
             for(int i=0;i<provider.agentsCount;++i){
              var agentTypeID=provider.indexToAgentType[i];
              NavMeshData navMeshData=null;
              while(navMeshData==null){
               navMeshData=provider.navMeshDataPoolByAgentType[agentTypeID].Rent();
              }
              navMeshByAgentType.Add(agentTypeID,navMeshData);
             }
            }
            internal void Reset(){
             isDirty=true;
             clusterBounds=default;
             for(int i=0;i<provider.agentsCount;i++){
              var agentTypeID=provider.indexToAgentType[i];
              var navMeshData=navMeshByAgentType[agentTypeID];
              if(navMeshData!=null){
               provider.navMeshDataPoolByAgentType[agentTypeID].Return(navMeshData);
              }
             }
             navMeshByAgentType.Clear();
             chunksSources.Clear();
             clusterZones.Clear();
             pendingBuild=false;
             updating=false;
             NavMeshBuildSnapshot.pool.Return(currSnapshot);currSnapshot=null;
             provider=null;
            }
            internal async void EnsureFinished(){
             if(currSnapshot!=null){
              while(currSnapshot.IsBusy()){
               await Task.Yield();
              }
             }
            }
         private bool isDirty=true;
         readonly HashSet<ActiveZone>unvisited=new();
         readonly List<(List<ActiveZone>list,Bounds groupBounds)>groups=new();
         internal static readonly Utilities.ObjectPool<List<ActiveZone>>groupPool=
          Pool.GetPool<List<ActiveZone>>("",()=>new(),(List<ActiveZone>item)=>{item.Clear();});
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
                Logs.Debug(()=>"'split':groups.Count:"+groups.Count);
                foreach(var group in groups){
                 if(group.list==clusterGroup){
                  if(clusterZones.Count!=group.list.Count||!clusterZones.IsSupersetOf(group.list)){
                   clusterZones.Clear();
                   clusterZones.UnionWith(group.list);
                  }
                  SetBounds(group.groupBounds);
                  //Logs.Debug("clusterZones.Count:"+clusterZones.Count);
                 }else{
                  var cluster=provider.RequestCluster();
                  cluster.RegisterMany(group.list,false);
                  cluster.SetBounds(group.groupBounds);
                  splits.Add(cluster);
                  //Logs.Debug("'split':cluster.clusterZones.Count:"+cluster.clusterZones.Count);
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
             UpdateBounds();
             Logs.Debug(()=>"clusterZones.Count:"+clusterZones.Count);
            }
            internal void RegisterZone(ActiveZone zone,bool updateBounds=true){
             if(clusterZones.Add(zone)){
              isDirty=true;
              if(updateBounds)UpdateBounds();
             }
            }
            private void RegisterMany(List<ActiveZone>zones,bool updateBounds=true){
             if(!clusterZones.IsSupersetOf(zones)){
              clusterZones.UnionWith(zones);
              isDirty=true;
              if(updateBounds)UpdateBounds();
             }
            }
            void UpdateBounds(){
             if(isDirty){
              Bounds bounds=default;
              bool first=true;
              foreach(var clusterZone in clusterZones){
               if(first){
                first=false;
                bounds=clusterZone.worldBounds;
               }else{
                bounds.Encapsulate(clusterZone.worldBounds);
               }
              }
              SetBounds(bounds);
             }
            }
            private void SetBounds(Bounds bounds){
             if(clusterBounds!=bounds){
              pendingBuild=true;
             }
             clusterBounds=bounds;
             isDirty=false;
            }
            internal bool Intersects(Bounds bounds){
             if(isDirty){
              UpdateBounds();
             }
             if(clusterBounds.Intersects(bounds)){
              return true;
             }
             return false;
            }
         private bool pendingBuild;
            internal void AddOrUpdateSource(WorldChunk chunk){
             chunksSources[chunk.cCoord]=chunk.terrain.navMeshBuildData.navMeshSource;
             chunk.OnClusteredNotifySource(this);
             pendingBuild=true;
            }
            internal void RemoveSource(WorldChunk chunk){
             chunksSources.Remove(chunk.cCoord);
            }
         private bool updating;
         internal bool isBusy{
          get{
           return updating;
          }
         }
            internal IEnumerator UpdateNavMeshAsyncCoroutine(){
             var waitNavMeshUpdateInterval=new WaitForSeconds(.2f);
             while(true){
              while(isDirty){
               yield return null;
              }
              yield return waitNavMeshUpdateInterval;
              if(pendingBuild){
               pendingBuild=false;//  ...se precisar construir de novo, vai ficar true no meio da coroutine;
               updating=true;
               DoSnapshot();
               for(int i=0;i<provider.agentsCount;i++){
                DoUpdateNavMeshAsync(i);
                while(currSnapshot.IsBusy()){
                 yield return null;
                }
                currSnapshot.CompleteUpdate(i);
               }
               PrepareSnapshotForReuse();
               updating=false;
              }
             }
            }
         private NavMeshBuildSnapshot currSnapshot;
            void DoSnapshot(){
             if(currSnapshot==null){
              currSnapshot=NavMeshBuildSnapshot.pool.Rent();
              currSnapshot.Init(this);
             }
             currSnapshot.asyncSources.AddRange(chunksSources.Values);
            }
            void PrepareSnapshotForReuse(){
             currSnapshot.asyncSources.Clear();
            }
            void DoUpdateNavMeshAsync(int agentIndex){
             Logs.Debug(()=>"'do async NavMesh update'");
             currSnapshot.DoBuild(agentIndex);
            }
        }
        internal class NavMeshBuildSnapshot{
         internal static readonly Utilities.ObjectPool<NavMeshBuildSnapshot>pool=
          Pool.GetPool<NavMeshBuildSnapshot>(
           "",
           ()=>new(),
           (NavMeshBuildSnapshot item)=>{
            item.Reset();
           },
           false
          );
         internal NavMeshCluster cluster;
         internal int agentsCount;
         internal readonly List<NavMeshBuildSource>asyncSources=new();
         internal NavMeshDataInstance[]navMeshInstances;
            internal void Init(NavMeshCluster cluster){
             this.cluster=cluster;
             agentsCount=cluster.provider.agentsCount;
             navMeshInstances=Pool.RentArray<NavMeshDataInstance>(agentsCount);
            }
            internal void Reset(){
             for(int i=0;i<agentsCount;i++){
              if(navMeshInstances[i].valid)navMeshInstances[i].Remove();
             }
             Pool.ReturnArray<NavMeshDataInstance>(navMeshInstances);
             navMeshInstances=null;
             asyncSources.Clear();
             asyncOps.Clear();
             cluster=null;
            }
         private readonly HashSet<AsyncOperation>asyncOps=new();
            internal void DoBuild(int agentIndex){
             int i=agentIndex;
             var agentTypeID=cluster.provider.indexToAgentType[i];
             var navMeshData=cluster.navMeshByAgentType[agentTypeID];
             var asyncOp=NavMeshBuilder.UpdateNavMeshDataAsync(
              navMeshData,
              cluster.provider.navMeshBuildSettings[i],
              asyncSources,
              cluster.clusterBounds
             );
             asyncOps.Add(asyncOp);
            }
         private readonly HashSet<AsyncOperation>done=new();
            internal bool IsBusy(){
             foreach(var asyncOp in asyncOps){
              if(!asyncOp.isDone){
               break;
              }else{
               done.Add(asyncOp);
              }
             }
             foreach(var asyncOp in done){
              asyncOps.Remove(asyncOp);
             }
             done.Clear();
             return asyncOps.Count>0;
            }
            internal void CompleteUpdate(int agentIndex){
             int i=agentIndex;
             if(navMeshInstances[i].valid)navMeshInstances[i].Remove();
             var agentTypeID=cluster.provider.indexToAgentType[i];
             var navMeshData=cluster.navMeshByAgentType[agentTypeID];
             navMeshInstances[i]=NavMesh.AddNavMeshData(navMeshData);
            }
        }
    }
}