#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        internal struct PersistentSimActorData{
        }
     internal NavMeshAgent navMeshAgent;
      internal NavMeshQueryFilter navMeshQueryFilter;
        protected override void Awake(){
         base.Awake();
         navMeshAgent=GetComponent<NavMeshAgent>();
         navMeshQueryFilter=new NavMeshQueryFilter(){
          agentTypeID=navMeshAgent.agentTypeID,
             areaMask=navMeshAgent.areaMask,
         };
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
        protected override void EnableInteractions(){
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         interactionsEnabled=false;
        }
        void EnableNavMeshAgent(){
         if(!navMeshAgent.enabled){
          if(NavMesh.SamplePosition(transform.position,out NavMeshHit hitResult,Height,navMeshQueryFilter)){
           transform.position=hitResult.position+Vector3.up*navMeshAgent.height/2f;
           navMeshAgent.enabled=true;
           Log.DebugMessage("navMeshAgent is enabled");
          }
         }
        }
        void DisableNavMeshAgent(){
         navMeshAgent.enabled=false;
        }
     internal bool isUsingAI=true;
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
         }
         if(isUsingAI){
          EnableNavMeshAgent();
          if(!navMeshAgent.isOnNavMesh){
           DisableNavMeshAgent();
          }
          if(navMeshAgent.enabled){
           AI();
          }
         }else{
          DisableNavMeshAgent();
         }
         return result;
        }
     public const int V_STATE      =15;
     public const int V_PATHFINDING=16;
        internal enum State:int{
         IDLE_ST=0,
        }
     protected State MyState=State.IDLE_ST;
        internal enum PathfindingResult:int{
         IDLE                   =0,
         REACHED                =1,
         PENDING                =2,
         TRAVELLING             =3,
         TRAVELLING_BUT_NO_SPEED=4,
        }
        PathfindingResult GetPathfindingResult(){
         if(navMeshAgent.pathPending){
          return PathfindingResult.PENDING;
         }
         return PathfindingResult.REACHED;
        }
        protected virtual void AI(){
        }
        protected virtual void OnIDLE_ST(){
        }
    }
}