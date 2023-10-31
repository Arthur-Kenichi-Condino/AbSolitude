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
        protected virtual void AI(){
         RenewTargets();
         MyPathfinding=GetPathfindingResult();
         stopPathfindingOnTimeout=true;
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         State lastState=MyState;
         if(MyEnemy!=null){
          if(IsInAttackRange(MyEnemy)){
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
         _MyStateSet:{}
         if(lastState!=MyState){
          if(lastState==State.ATTACK_ST){
           onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.identity;
           characterController.character.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.EndRotation();
          }
         }
         SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
         if(MyState==State.IDLE_ST){SetBestSkillToUse(Skill.SkillUseContext.OnIdle);}
         if(MySkill!=null){
          DoSkill();
         }
         if      (MyState==State.ATTACK_ST){
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