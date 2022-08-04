#if UNITY_EDITOR
				#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        internal struct PersistentSimActorData{
			      public List<Type>skills;
         public float timerToRandomMove;
        }
		   internal readonly List<Skill>skills=new List<Skill>();
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
        protected virtual void AI(){
        }
    }
}