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
    internal partial class BaseAI:SimActor{
     protected WeaponTypes MyWeaponType=WeaponTypes.None;internal WeaponTypes weaponType{get{return MyWeaponType;}}
        protected virtual void AI(){
         RenewTargets();
         MyPathfinding=GetPathfindingResult();
         stopPathfindingOnTimeout=true;
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyEnemy!=null){
          if(IsInAttackRange(MyEnemy)){
           MyState=State.ATTACK_ST;
           goto _MyStateSet;
          }else{
           if(MyState!=State.CHASE_ST){
            OnCHASE_ST_START();
           }
           MyState=State.CHASE_ST;
           goto _MyStateSet;
          }
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
           OnIDLE_ST_START();
          }
          MyState=State.IDLE_ST;
          goto _MyStateSet;
         }
         _MyStateSet:{}
         SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
         if(MyState==State.IDLE_ST){SetBestSkillToUse(Skill.SkillUseContext.OnIdle);}
         if(MySkill!=null){
          DoSkill();
         }
         if      (MyState==State.ATTACK_ST){
          OnATTACK_ST();
         }else if(MyState==State. CHASE_ST){
           OnCHASE_ST();
         }else if(MyState==State.FOLLOW_ST){
          OnFOLLOW_ST();
         }else{
            OnIDLE_ST();
         }
         UpdateMotion(true);
        }
    }
}