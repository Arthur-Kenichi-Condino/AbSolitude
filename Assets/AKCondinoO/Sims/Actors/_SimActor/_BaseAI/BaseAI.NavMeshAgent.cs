#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal NavMeshAgent navMeshAgent;
      internal bool navMeshAgentShouldBeStopped=false;
      internal NavMeshQueryFilter navMeshQueryFilter;
       [SerializeField]protected float navMeshAgentWalkSpeed=2f;
        [SerializeField]protected float navMeshAgentRunSpeed=4f;
         protected bool navMeshAgentShouldUseRunSpeed=false;
     protected float onEnableNavMeshAgentProximityTimeout=10f;
      protected float onEnableNavMeshAgentProximityTimer=10f;
        void EnableNavMeshAgent(){
         if(!navMeshAgent.enabled){
          if(NavMesh.SamplePosition(transform.position,out NavMeshHit hitResult,Height+1,navMeshQueryFilter)){
           if(onEnableNavMeshAgentProximityTimer>0f){
            onEnableNavMeshAgentProximityTimer-=Time.deltaTime;
           }
           if(onEnableNavMeshAgentProximityTimer<=0f||(new Vector3(hitResult.position.x,0f,hitResult.position.z)-new Vector3(transform.position.x,0f,transform.position.z)).magnitude<=2f){
            transform.position=hitResult.position+Vector3.up*navMeshAgent.height/2f;
            navMeshAgent.enabled=true;
            onEnableNavMeshAgentProximityTimer=onEnableNavMeshAgentProximityTimeout;
            //Log.DebugMessage("navMeshAgent is enabled");
           }
          }
         }
        }
        void DisableNavMeshAgent(){
         navMeshAgent.enabled=false;
        }
    }
}