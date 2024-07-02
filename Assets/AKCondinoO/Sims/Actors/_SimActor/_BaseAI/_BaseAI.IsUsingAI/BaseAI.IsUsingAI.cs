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
          characterController.isAiming=false;
          aiRotTurnTo.tgtRot=aiRotTurnTo.tgtRot_Last=characterController.character.transform.rotation;
         }
        }
        protected virtual void OnStopUsingAI(){
         if(characterController!=null){
          transform.rotation=characterController.character.transform.rotation;
          characterController.character.transform.rotation=transform.rotation;
         }
        }
     [SerializeField]internal bool sniper=false;
     [SerializeField]protected bool doIdleMove=true;
     [NonSerialized]internal QuaternionRotLerpHelper aiRotTurnTo=new QuaternionRotLerpHelper(76.0f*(1f/2f),0.0005f*(2f));
        internal partial class AI{
         [NonSerialized]BaseAI me;
            internal AI(BaseAI me){
             this.me=me;
              snipeSt=new  SNIPE_ST(me,this);
             attackSt=new ATTACK_ST(me,this);
              chaseSt=new  CHASE_ST(me,this);
             followSt=new FOLLOW_ST(me,this);
               idleSt=new   IDLE_ST(me,this);
            }
            internal class ST{
             [NonSerialized]protected BaseAI me;
             [NonSerialized]protected AI ai;
              protected State MyState{get{return ai.MyState;}}
              protected SimObject MyEnemy{get{return ai.MyEnemy;}}
                internal ST(BaseAI me,AI ai){
                 this.me=me;
                 this.ai=ai;
                }
            }
          [NonSerialized]internal float damageSourceForgiveTime=20f;
           [NonSerialized]internal readonly Dictionary<SimObject,float>damageSources=new Dictionary<SimObject,float>();
            [NonSerialized]protected readonly List<SimObject>damageSourcesIterator=new();
          [NonSerialized]bool isInAttackRange          =false;
          [NonSerialized]bool isInAttackRangeWithWeapon=false;
          [NonSerialized]Vector3 attackDistance          ;
          [NonSerialized]Vector3 attackDistanceWithWeapon;
            internal virtual void Main(){
             me.RenewTargets();
             damageSourcesIterator.AddRange(damageSources.Keys);
             foreach(SimObject damageSourceSim in damageSourcesIterator){
              float damageSourceForgiveTimer=damageSources[damageSourceSim];
              damageSourceForgiveTimer-=Time.deltaTime;
              if(damageSourceForgiveTimer<=0f){
               damageSources.Remove(damageSourceSim);
              }else{
               damageSources[damageSourceSim]=damageSourceForgiveTimer;
              }
             }
             damageSourcesIterator.Clear();
             me.stopPathfindingOnTimeout=true;
             //Log.DebugMessage("'MyPathfinding state is':"+MyPathfinding);
             isInAttackRange          =false;
             isInAttackRangeWithWeapon=false;
             if(MyEnemy!=null){
              isInAttackRange          =me.IsInAttackRange(MyEnemy,out attackDistance               );
              isInAttackRangeWithWeapon=me.IsInAttackRange(MyEnemy,out attackDistanceWithWeapon,true);
             }else{
              attackDistance          =me.AttackDistance(    );
              attackDistanceWithWeapon=me.AttackDistance(true);
             }
             UpdateMyState();
             SetSkill();
             ProcessStateRoutine();
            }
            internal void SetSkill(){
             bool callingSlaves=false;
             if(me.requiredSlaves.Count>0){
              callingSlaves=true;
             }
             foreach(var slave in me.slaves){
              //Log.DebugMessage("slave:"+slave);
              if(!SimObjectManager.singleton.active.TryGetValue(slave,out SimObject slaveSimObject)){
               if(!callingSlaves){
                callingSlaves=true;
               }
              }
             }
             if(callingSlaves){
              me.SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
              //Log.DebugMessage("'me.SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves) needed'");
             }
             //Log.DebugMessage(me+":'me.SetBestSkillToUse next':MyState:"+MyState);
             if      (MyState==State.  DEAD_ST){
              //
             }else if(MyState==State. SNIPE_ST){
              
             }else if(MyState==State.ATTACK_ST){
              
             }else if(MyState==State. CHASE_ST){
              
             }else if(MyState==State.FOLLOW_ST){
              //Log.DebugMessage(me+":'me.SetBestSkillToUse(Skill.SkillUseContext.OnFollow)'");
              me.SetBestSkillToUse(Skill.SkillUseContext.OnFollow);
             }else{
              //Log.DebugMessage(me+":'me.SetBestSkillToUse(Skill.SkillUseContext.OnIdle)'");
              me.SetBestSkillToUse(Skill.SkillUseContext.OnIdle);
             }
            }
            internal void DoSkill(){
             if(MySkill!=null){
              me.DoSkill();
             }
            }
        }
    }
}