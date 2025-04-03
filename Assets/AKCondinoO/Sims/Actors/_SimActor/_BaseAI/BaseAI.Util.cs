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
    internal partial class BaseAI{
        internal override Quaternion GetRotation(){
         if(simUMA!=null){
          return simUMA.transform.rotation;
         }
         if(animatorController!=null&&animatorController.animator!=null){
          return animatorController.animator.transform.rotation;
         }
         return base.GetRotation();
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
          Move(dest);
         }
        }
        protected void MoveToMaster      (BaseAI masterAI,float dis){
         Vector3 dir=(this.transform.position-masterAI.transform.position).normalized;
         Vector3 dest=masterAI.transform.position+dir*(this.characterController.character.radius/2f+masterAI.characterController.character.radius/2f);
         Move(dest);
        }
        protected virtual void TeleportToRandomNearMyEnemy(Vector3 distance){
         if(ai==null){
          return;
         }
         if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
          teleport.targetDest=ai.MyEnemy.transform.position;
          teleport.cooldown=0f;
          teleport.useRandom=true;
          teleport.randomMaxDis=distance.z*1.1f;
          teleport.DoSkill(this,1);
         }
        }
     Vector3 targetDir;
        protected virtual void TurnToTargetDir(Vector3 lookDir){
         if(characterController!=null){
          Vector3 planarLookDir=Vector3.ProjectOnPlane(lookDir,Vector3.up);
          targetDir=planarLookDir.normalized;
          targetDir.Normalize();
          if(targetDir==Vector3.zero){
           targetDir=transform.forward;
          }
          aiRotTurnTo.tgtRot=Quaternion.LookRotation(targetDir);
         }
        }
        protected bool IsTurnedToTargetDir(){
         if(characterController!=null){
          if(simUMA!=null){
           Quaternion animatorAdjustmentsForUMARotation=Quaternion.identity;
           if(animatorController!=null&&animatorController.transformAdjustmentsForUMA!=null){
            animatorAdjustmentsForUMARotation=Quaternion.Inverse(animatorController.transformAdjustmentsForUMA.localRotation);
           }
           Vector3 animatorLookDir=animatorAdjustmentsForUMARotation*-simUMA.transform.parent.forward;
           Vector3 animatorLookEuler=simUMA.transform.parent.eulerAngles+animatorAdjustmentsForUMARotation.eulerAngles;
           animatorLookEuler.y+=180f;
           Vector3 animatorPlanarLookEuler=animatorLookEuler;
           animatorPlanarLookEuler.x=0f;
           animatorPlanarLookEuler.z=0f;
           Vector3 animatorPlanarLookDir=Quaternion.Euler(animatorPlanarLookEuler)*Vector3.forward;
           //Debug.DrawRay(characterController.character.transform.position,animatorPlanarLookDir,Color.white);
           if(Vector3.Angle(targetDir,animatorPlanarLookDir)<=1.25f*(2f)){
            return true;
           }
          }
         }
         return false;
        }
        protected virtual bool TurnToMoveDest(){
         if(ai==null){
          return true;
         }
         if(characterController!=null){
          bool lookDirSet=false;
          Vector3 lookDir=characterController.character.transform.forward;
          if(!lookDirSet){
           if(navMeshAgent!=null&&navMeshAgent.path!=null&&navMeshAgent.path.corners!=null){
            if(Vector3.Distance(navMeshAgent.steeringTarget,transform.position)>.0125f/2f){
             lookDir=navMeshAgent.steeringTarget-transform.position;
             lookDirSet=true;
            }
           }
          }
          if(!lookDirSet){
           if(IsTraversingPath()){
            if(Vector3.Distance(moveDest,transform.position)>.0625f){
             lookDir=moveDest-transform.position;
             lookDirSet=true;
            }
           }
          }
          TurnToTargetDir(lookDir);
          return IsTurnedToTargetDir();
         }
         return false;
        }
        protected virtual bool TurnToMyEnemy(){
         if(ai==null){
          return true;
         }
         if(ai.MyEnemy==null){
          return true;
         }
         if(characterController!=null){
          Vector3 lookDir=ai.MyEnemy.transform.position-transform.position;
          TurnToTargetDir(lookDir);
          return IsTurnedToTargetDir();
         }
         return false;
        }
    }
}