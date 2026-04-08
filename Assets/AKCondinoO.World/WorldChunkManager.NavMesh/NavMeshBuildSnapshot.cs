using AKCondinoO.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.World{
    internal class NavMeshBuildSnapshot{
     internal static readonly Utilities.ObjectPool<NavMeshBuildSnapshot>pool=
      Pool.GetPool<NavMeshBuildSnapshot>(
       "",
       ()=>new(),
       (NavMeshBuildSnapshot item)=>{
        item.OnReturnToPoolRecycle();
       },
       false
      );
     internal NavMeshCluster cluster;
     internal NavMeshSourcesOrdered sourcesOrdered;
     internal int agentsCount;
     internal readonly List<NavMeshBuildSource>asyncSources=new();
     internal NavMeshDataInstance[]navMeshInstances;
        internal void Init(NavMeshCluster cluster){
         this.cluster=cluster;
         agentsCount=cluster.provider.agentsCount;
         navMeshInstances=Pool.RentArray<NavMeshDataInstance>(agentsCount);
         sourcesOrdered=NavMeshSourcesOrdered.pool.Rent();
         sourcesOrdered.Init(this);
        }
        internal void OnReturnToPoolRecycle(){
         for(int i=0;i<agentsCount;i++){
          if(navMeshInstances[i].valid)navMeshInstances[i].Remove();
         }
         Pool.ReturnArray<NavMeshDataInstance>(navMeshInstances);
         navMeshInstances=null;
         asyncSources.Clear();
         asyncOps.Clear();
         NavMeshSourcesOrdered.pool.Return(sourcesOrdered);sourcesOrdered=null;
         cluster=null;
        }
        internal void Cancel(){
         for(int i=0;i<agentsCount;i++){
          var agentTypeID=cluster.provider.indexToAgentType[i];
          var navMeshData=cluster.navMeshByAgentType[agentTypeID];
          if(navMeshData!=null){
           NavMeshBuilder.Cancel(navMeshData);
          }
         }
        }
     internal Bounds bounds;
     private readonly HashSet<AsyncOperation>asyncOps=new();
        internal void DoBuild(int agentIndex){
         int i=agentIndex;
         var agentTypeID=cluster.provider.indexToAgentType[i];
         var navMeshData=cluster.navMeshByAgentType[agentTypeID];
         var asyncOp=NavMeshBuilder.UpdateNavMeshDataAsync(
          navMeshData,
          cluster.provider.navMeshBuildSettings[i],
          asyncSources,
          bounds
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
         if(!navMeshInstances[i].valid){
          var agentTypeID=cluster.provider.indexToAgentType[i];
          var navMeshData=cluster.navMeshByAgentType[agentTypeID];
          navMeshInstances[i]=NavMesh.AddNavMeshData(navMeshData);
         }
        }
    }
}