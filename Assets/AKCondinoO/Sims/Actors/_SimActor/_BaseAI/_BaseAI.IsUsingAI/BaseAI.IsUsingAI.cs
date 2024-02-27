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
        protected virtual void AI(){
         RenewTargets();
         MyPathfinding=GetPathfindingResult();
         stopPathfindingOnTimeout=true;
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         State lastState=MyState;
         if(IsDead()){
          MyState=State.DEAD_ST;
          goto _MyStateSet;
         }else{
          if(MyEnemy!=null){
           bool isInAttackRange=IsInAttackRange(MyEnemy);
           bool isInAttackRangeWithWeapon=IsInAttackRange(MyEnemy,true);
           if(isInAttackRangeWithWeapon){
            Vector3 attackDistance=AttackDistance();
            Vector3 attackDistanceWithWeapon=AttackDistance(true);
            if(!isInAttackRange||(IsFasterThan(MyEnemy)&&(
              attackDistance.z<attackDistanceWithWeapon.z||
              attackDistance.x<attackDistanceWithWeapon.x||
              attackDistance.y<attackDistanceWithWeapon.y
             ))
            ){
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
          if      (lastState==State.ATTACK_ST){
           if(characterController!=null){
            ToResetRotation(out Vector3 dir);
            onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(dir,Vector3.up);
            characterController.character.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.EndRotation();
            transform.rotation=characterController.character.transform.rotation;
           }
          }else if(lastState==State. CHASE_ST){
           if(characterController!=null){
            ToResetRotation(out Vector3 dir);
            onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(dir,Vector3.up);
            characterController.character.transform.rotation=onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.EndRotation();
            transform.rotation=characterController.character.transform.rotation;
           }
          }
          void ToResetRotation(out Vector3 dir){
           if(MyEnemy!=null){
            dir=(MyEnemy.transform.position-transform.position).normalized;
           }else{
            dir=transform.forward;
           }
           if(dir.x==0f&&dir.z==0f){
            dir=transform.forward;
           }
           dir.y=0f;
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
           OnSNIPE_ST_Routine();
         }else if(MyState==State.ATTACK_ST){
          OnATTACK_ST_Routine();
         }else if(MyState==State. CHASE_ST){
           OnCHASE_ST_Routine();
         }else if(MyState==State.FOLLOW_ST){
          OnFOLLOW_ST_Routine();
         }else{
            OnIDLE_ST_Routine();
         }
         UpdateMotion(true);
        }
    }
}