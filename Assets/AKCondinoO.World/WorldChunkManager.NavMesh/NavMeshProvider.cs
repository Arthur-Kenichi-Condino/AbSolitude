using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.World{
    internal class NavMeshProvider{
     internal readonly Dictionary<int,ObjectPool<NavMeshData>>navMeshDataPoolByAgentType=new();
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
            "",
            ()=>new(agentTypeID),
            (NavMeshData item)=>{},false
           )
          );
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
         internal Bounds bounds;
         internal readonly Dictionary<int,NavMeshData>navMeshByAgentType=new();
         internal readonly List<NavMeshBuildSource>sources=new();
         internal NavMeshDataInstance[]navMeshInstances;
            internal void Init(){
             navMeshInstances=Pool.RentArray<NavMeshDataInstance>(provider.agentsCount);
             for(int i=0;i<provider.agentsCount;++i){
              var agentTypeID=provider.indexToAgentType[i];
              navMeshByAgentType.Add(agentTypeID,provider.navMeshDataPoolByAgentType[agentTypeID].Rent());
             }
            }
            internal void Reset(){
             for(int i=0;i<provider.agentsCount;i++){
              var agentTypeID=provider.indexToAgentType[i];
              navMeshInstances[i].Remove();
              provider.navMeshDataPoolByAgentType[agentTypeID].Return(navMeshByAgentType[agentTypeID]);
             }
             navMeshByAgentType.Clear();
             sources.Clear();
             Pool.ReturnArray<NavMeshDataInstance>(navMeshInstances);
             navMeshInstances=null;
             provider=null;
            }
            internal void DoUpdateNavMeshAsync(){
            }
        }
        internal void RegisterActiveZone(ActiveZone zone){
         OnActiveZoneChangedcCoord(zone);
        }
        internal void OnActiveZoneChangedcCoord(ActiveZone zone){
        }
        internal void OnChunkExists(WorldChunk chunk){
        }
    }
}