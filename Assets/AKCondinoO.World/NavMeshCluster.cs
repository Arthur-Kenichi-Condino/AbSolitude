using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.World{
    internal class NavMeshCluster{
     internal static readonly Utilities.ObjectPool<NavMeshCluster>pool=
      Pool.GetPool<NavMeshCluster>(
       "",
       ()=>new(),
       (NavMeshCluster item)=>{
        item.OnReturnToPoolRecycle();
       },
       false
      );
     internal NavMeshProvider provider;
     private NavMeshBuildSnapshot snapshot;
     internal Coroutine updateNavMeshAsyncCoroutine;
     internal readonly HashSet<ActiveZone>clusterZones=new();
     internal Bounds clusterBounds;
     internal readonly Dictionary<int,NavMeshData>navMeshByAgentType=new();
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
         snapshot=NavMeshBuildSnapshot.pool.Rent();
         snapshot.Init(this);
        }
        internal void OnReturnToPoolRecycle(){
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
         clusterZones.Clear();
         pendingBuild=false;
         updating=false;
         NavMeshBuildSnapshot.pool.Return(snapshot);snapshot=null;
         provider=null;
        }
        internal void Cancel(){
         if(snapshot!=null){
          snapshot.Cancel();
         }
        }
        internal void EnsureStopThenPool(){
         if(snapshot!=null){
          if(snapshot.IsBusy()){
           Cancel();
          }
         }
         NavMeshCluster.pool.Return(this);
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
     private bool pendingBuild;
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
        internal void AddOrUpdateSource(WorldChunk chunk){
         if(snapshot==null){return;}
         snapshot.sourcesOrdered.RegisterSource(chunk);
         chunk.OnClusteredNotifySource(this);
         pendingBuild=true;
        }
        internal void RemoveSource(WorldChunk chunk){
         if(snapshot==null){return;}
         snapshot.sourcesOrdered.UnregisterSource(chunk);
         pendingBuild=true;
        }
     private bool updating;
     internal bool isBusy{
      get{
       return updating;
      }
     }
        internal IEnumerator UpdateNavMeshAsyncCoroutine(){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         var waitNavMeshUpdateInterval=new WaitForSeconds(.1f);
         var waitAgentTypeInterval=new WaitForSeconds(.01f);
         while(true){
          while(isDirty){
           yield return null;
          }
          yield return waitNavMeshUpdateInterval;
          double startTime=Time.realtimeSinceStartupAsDouble;
          if(pendingBuild){
           pendingBuild=false;//  ...se precisar construir de novo, vai ficar true no meio da coroutine;
           updating=true;
           DoSnapshot();
           snapshot.sourcesOrdered.BeginOrderingSources();
           while(snapshot.sourcesOrdered.OrderingSourcesIncremental()){
            if(ShouldYield())yield return null;
           }
           for(int i=0;i<provider.agentsCount;i++){
            snapshot.sourcesOrdered.PrepareSourcesForAgentBuild();
            bool first=true;
            while(snapshot.sourcesOrdered.AppendSourcesForAgentIncremental()||first){
             first=false;
             if(ShouldYield())yield return null;
             DoUpdateNavMeshAsync(i);
             while(snapshot.IsBusy()){
              yield return null;
             }
            }
            Complete(i);
            yield return waitAgentTypeInterval;
           }
           updating=false;
          }
          bool ShouldYield(){
           if(Time.realtimeSinceStartupAsDouble-startTime>=maxTimePerFrame){
            startTime=Time.realtimeSinceStartupAsDouble;
            return true;
           }
           return false;
          }
         }
        }
        void DoSnapshot(){
         snapshot.bounds=clusterBounds;
        }
        void DoUpdateNavMeshAsync(int agentIndex){
         Logs.Debug(()=>"'do async NavMesh update'");
         snapshot.DoBuild(agentIndex);
        }
        void Complete(int agentIndex){
         snapshot.CompleteUpdate(agentIndex);
        }
    }
}