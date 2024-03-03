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
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        protected virtual void OnStartUsingAI(){
         if(characterController!=null){
          transform.rotation=characterController.character.transform.rotation;
          characterController.character.transform.rotation=transform.rotation;
         }
        }
        protected virtual void OnStopUsingAI(){
         if(characterController!=null){
          transform.rotation=characterController.character.transform.rotation;
          characterController.character.transform.rotation=transform.rotation;
         }
        }
        protected virtual void AI(){
         bool isInAttackRange=false;
         bool isInAttackRangeWithWeapon=false;
         Vector3 attackDistance;
         Vector3 attackDistanceWithWeapon;
         if(resettingRotation){
          characterController.character.transform.rotation=resettingRotationRotLerp.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
          transform.rotation=characterController.character.transform.rotation;
          if(resettingRotationRotLerp.tgtRotLerpVal>=1f){
           OnResetRotationEnd();
          }
         }
         if(MyEnemy!=null){
          isInAttackRange=IsInAttackRange(MyEnemy,out attackDistance);
          isInAttackRangeWithWeapon=IsInAttackRange(MyEnemy,out attackDistanceWithWeapon,true);
         }else{
          attackDistance=AttackDistance();
          attackDistanceWithWeapon=AttackDistance(true);
         }
         RenewTargets();
         MyPathfinding=GetPathfindingResult();
         stopPathfindingOnTimeout=true;
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         State lastState=MyState;
         if(IsDead()){
          MyState=State.DEAD_ST;
          goto _MyStateSet;
         }else{
          if      (MyState==State. SNIPE_ST){
           if(IsShooting()||IsReloading()){//  so the animation is completed
            MyState=State. SNIPE_ST;
            goto _MyStateSet;
           }
          }else if(MyState==State.ATTACK_ST){
           if(IsAttacking()){//  so the animation is completed
            MyState=State.ATTACK_ST;
            goto _MyStateSet;
           }
          }
          if(MyEnemy!=null){
           if(isInAttackRangeWithWeapon){
            if(!isInAttackRange||(IsFasterThan(MyEnemy)&&(
              attackDistance.z<attackDistanceWithWeapon.z||
              attackDistance.x<attackDistanceWithWeapon.x||
              attackDistance.y<attackDistanceWithWeapon.y
             ))
            ){
             if(MyState!=State.SNIPE_ST){
              OnSNIPE_ST_Start();
             }
             MyState=State.SNIPE_ST;
             goto _MyStateSet;
            }
           }
           if(isInAttackRange){
            MyState=State.ATTACK_ST;
            goto _MyStateSet;
           }
           if(MyState!=State.CHASE_ST){
            OnCHASE_ST_Start();
           }
           MyState=State.CHASE_ST;
           goto _MyStateSet;
          }else{
           if(masterId!=null){
            float disToMaster=GetDistance(this,masterSimObject);
            if(disToMaster>=0f){
             if(disToMaster>8f){
              //Log.DebugMessage("I should follow my master:"+masterSimObject+";this:"+this);
              MyState=State.FOLLOW_ST;
              goto _MyStateSet;
             }
            }
           }
           if(MyState!=State.IDLE_ST){
            OnIDLE_ST_Start();
           }
           MyState=State.IDLE_ST;
           goto _MyStateSet;
          }
         }
         _MyStateSet:{}
         if(lastState!=MyState){
          OnResetRotationEnd();
          if      (lastState==State. SNIPE_ST){
            OnSNIPE_ST_Reset();
          }else if(lastState==State.ATTACK_ST){
           OnATTACK_ST_Reset();
           if(MyState==State.CHASE_ST){
            onChaseAlternateMoveAttack=true;
           }
          }else if(lastState==State. CHASE_ST){
            OnCHASE_ST_Reset();
          }
         }
         bool callingSlaves=false;
         foreach(var slave in slaves){
          if(!SimObjectManager.singleton.active.TryGetValue(slave,out SimObject slaveSimObject)){
           if(!callingSlaves){
            SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
            callingSlaves=true;
           }
          }
         }
         if(MyState==State.IDLE_ST){SetBestSkillToUse(Skill.SkillUseContext.OnIdle);}
         if(MySkill!=null){
          DoSkill();
         }
         if      (MyState==State.  DEAD_ST){
          //
         }else if(MyState==State. SNIPE_ST){
           OnSNIPE_ST_Routine(attackDistance,attackDistanceWithWeapon);
         }else if(MyState==State.ATTACK_ST){
          OnATTACK_ST_Routine(attackDistance,attackDistanceWithWeapon);
         }else if(MyState==State. CHASE_ST){
           OnCHASE_ST_Routine(attackDistance);
         }else if(MyState==State.FOLLOW_ST){
          OnFOLLOW_ST_Routine();
         }else{
            OnIDLE_ST_Routine();
         }
         UpdateMotion(true);
        }
        void ToResetRotation(out Vector3 dir){
         if(MyState==State.ATTACK_ST||
            MyState==State. CHASE_ST
         ){
          dir=(MyEnemy.transform.position-transform.position).normalized;
         }else if(animatorController!=null){
          dir=animatorController.transform.forward;
         }else{
          dir=transform.forward;
         }
         if(dir.x==0f&&dir.z==0f){
          dir=transform.forward;
         }
         dir.y=0f;
        }
     protected bool resettingRotation;
      protected QuaternionRotLerpHelper resettingRotationRotLerp;
        void ResetRotation(QuaternionRotLerpHelper rotLerp,bool instantly=false){
         if(characterController!=null){
          ToResetRotation(out Vector3 dir);
          rotLerp.tgtRot=Quaternion.LookRotation(dir,Vector3.up);
          if(instantly||rotLerp==null){
           characterController.character.transform.rotation=rotLerp.EndRotation();
           transform.rotation=characterController.character.transform.rotation;
          }else{
           resettingRotationRotLerp=rotLerp;
           resettingRotation=true;
          }
         }
        }
        void OnResetRotationEnd(bool end=true){
         if(!resettingRotation){
          return;
         }
         if(end){
          characterController.character.transform.rotation=resettingRotationRotLerp.EndRotation();
          transform.rotation=characterController.character.transform.rotation;
         }
         resettingRotationRotLerp=null;
         resettingRotation=false;
        }
        protected virtual bool LookToMyEnemy(){
         if(characterController!=null){
          Vector3 lookDir=MyEnemy.transform.position-transform.position;
          Vector3 planarLookDir=lookDir;
          planarLookDir.y=0f;
          onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
          characterController.character.transform.rotation=onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
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
           if(Vector3.Angle(characterController.character.transform.forward,animatorPlanarLookDir)<=1f){
            return true;
           }
          }
         }
         return false;
        }
    }
}