#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        protected bool IsMoving(){
         return moveVelocityFlattened!=0f||teleportedMove;
        }
        protected float GetDistance((Type simObjectType,ulong idNumber)?idA,(Type simObjectType,ulong idNumber)?idB){
         if(idA!=null&&
            idB!=null
         ){
          if(SimObjectManager.singleton.active.TryGetValue(idA.Value,out SimObject simObjectA)&&
             SimObjectManager.singleton.active.TryGetValue(idB.Value,out SimObject simObjectB)
          ){
           return GetDistance(simObjectA,simObjectB);
          }
         }
         return -1f;
        }
        protected float GetDistance(SimObject simObjectA,SimObject simObjectB){
         if(simObjectA!=null&&
            simObjectB!=null
         ){
          return GetDistance(simObjectA.transform.position,simObjectB.transform.position);
         }
         return -1f;
        }
        protected float GetDistance(Vector3 positionA,Vector3 positionB){
         return Vector3.Distance(positionA,positionB);
        }
        //  [https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html]
        protected bool GetRandomPosition(Vector3 center,float maxDis,out Vector3 result){
         for(int i=0;i<3;++i){
          Vector3 randomPoint=Util.GetRandomPosition(center,maxDis);
          if(NavMesh.SamplePosition(randomPoint,out NavMeshHit hit,Height,navMeshQueryFilter)){
           result=hit.position;
           return true;
          }
         }
         result=center;
         return false;
        }
        protected void MoveToMasterRandom(BaseAI masterAI,float dis){
         if(GetRandomPosition(masterAI.transform.position,dis,out Vector3 dest)){
          navMeshAgent.destination=dest;
         }
        }
        protected void MoveToMaster      (BaseAI masterAI,float dis){
         Vector3 dir=(this.transform.position-masterAI.transform.position).normalized;
         Vector3 dest=masterAI.transform.position+dir*(this.simActorCharacterController.characterController.radius/2f+masterAI.simActorCharacterController.characterController.radius/2f);
         navMeshAgent.destination=dest;
        }
    }
}