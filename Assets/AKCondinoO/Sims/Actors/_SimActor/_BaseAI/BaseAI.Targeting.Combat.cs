#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal override bool IsMonster(){
         return MyAggressionMode==AggressionMode.AggressiveToAll;
        }
        internal override bool IsFriendlyTo(SimObject sim){
         if(id==sim.masterId){
          return true;
         }
         return base.IsFriendlyTo(sim);
        }
     internal bool isAiming{
      get{
       if(characterController!=null){
        return characterController.isAiming;
       }
       return false;
      }
     }
        internal virtual bool HasListenersForAnimationEventUsingWeapon(){
         if(animatorController.animationEventsHandler.onAnimatorReload!=null){
          Log.DebugMessage("onAnimatorReload!=null");
          return true;
         }
         if(animatorController.animationEventsHandler.onAnimatorShoot!=null){
          Log.DebugMessage("onAnimatorShoot!=null");
          return true;
         }
         return false;
        }
     protected bool motionFlagForReloadingAnimation=false;
        internal virtual bool DoReloadingOnAnimationEventUsingWeapon(SimWeapon simWeapon){
         if(HasListenersForAnimationEventUsingWeapon()){
          return false;
         }
         if(isAiming){
          if(animatorController.animator!=null){
           if(animatorController.animationEventsHandler!=null){
            Log.DebugMessage("DoReloadingOnAnimationEventUsingWeapon():simWeapon:"+simWeapon,simWeapon);
            animatorController.animationEventsHandler.onAnimatorReload+=simWeapon.OnReload;
            motionFlagForReloadingAnimation=true;
            return true;
           }
          }
         }
         return false;
        }
     internal bool isReloading{
      get{
       return motionFlagForReloadingAnimation;
      }
     }
     [SerializeField]protected Vector3 MyAttackRange=new Vector3(0f,.25f,.25f);internal Vector3 attackRange{get{return MyAttackRange;}}
        internal Vector3 AttackDistance(){
         float radius=GetRadius();
         return new Vector3(
          radius+MyAttackRange.x,
          GetHeight()+MyAttackRange.y,
          radius+MyAttackRange.z
         );
        }
        internal virtual bool IsInAttackRange(SimObject simObject){
         Vector3 delta=new Vector3(
          Mathf.Abs(simObject.transform.position.x-transform.position.x),
          Mathf.Abs(simObject.transform.position.y-transform.position.y),
          Mathf.Abs(simObject.transform.position.z-transform.position.z)
         );
         float disXZPlane=new Vector3(delta.x,0f,delta.z).magnitude;
         Vector3 attackDistance=AttackDistance();
         float simObjectRadius=Mathf.Max(simObject.localBounds.extents.x,simObject.localBounds.extents.z);
         if((disXZPlane<=attackDistance.z+simObjectRadius||disXZPlane<=attackDistance.x+simObjectRadius)&&delta.y<=attackDistance.y){
          //Log.DebugMessage("simObject is in my attack range:disXZPlane:"+disXZPlane);
          return true;
         }
         return false;
        }
     protected bool motionFlagForAttackAnimation=false;
        protected virtual void DoAttackOnAnimationEvent(){
         //Log.DebugMessage("DoAttackOnAnimationEvent()");
         motionFlagForAttackAnimation=true;
        }
     protected bool motionFlagForShootingAnimation=false;
        internal virtual bool DoShootingOnAnimationEventUsingWeapon(SimWeapon simWeapon){
         if(HasListenersForAnimationEventUsingWeapon()){
          return false;
         }
         if(isAiming){
          if(animatorController.animator!=null){
           if(animatorController.animationEventsHandler!=null){
            Log.DebugMessage("DoShootingOnAnimationEventUsingWeapon()");
            animatorController.animationEventsHandler.onAnimatorShoot+=simWeapon.OnShoot;
            motionFlagForShootingAnimation=true;
            return true;
           }
          }
         }
         return false;
        }
     internal bool isShooting{
      get{
       return motionFlagForShootingAnimation;
      }
     }
     protected readonly HashSet<Skill>skillsToUseOnWillTakeDamage=new HashSet<Skill>();
     protected bool motionFlagForHitAnimation=false;
      protected bool motionFlagForHitResetAnimation=false;
        internal virtual void OnHit(Hitboxes hitbox){
         skillsToUseOnWillTakeDamage.Clear();
         GetBest(Skill.SkillUseContext.OnWillTakeDamage,skillsToUseOnWillTakeDamage);
         foreach(Skill skill in skillsToUseOnWillTakeDamage){
          Type skillType=skill.GetType();
          if(skills.TryGetValue(skillType,out Skill skillToGet)&&skillToGet==skill){
           SimObject target=this;//  TO DO: set best my skill target
           if(skill.IsAvailable(target,skill.level)){
            skill.DoSkillImmediate(target,skill.level);
           }
          }
         }
         bool canTakeDamage=true;
         bool canSetMotionFlag=true,canSetMotionResetFlag=true;
         OnHitGracePeriodSkillBuff onHitGracePeriodSkillBuff=null;
         if(skillBuffs.Contains(typeof(OnHitGracePeriodSkillBuff),out List<SkillBuff>activeOnHitGracePeriodSkillBuffs)){
          canSetMotionFlag=false;canSetMotionResetFlag=false;
          onHitGracePeriodSkillBuff=(OnHitGracePeriodSkillBuff)activeOnHitGracePeriodSkillBuffs[0];
          var effect=onHitGracePeriodSkillBuff.onHitGracePeriodEffect;
          if(effect.hitCantTriggerAnimation<=0f){
           if(motionFlagForHitAnimation){
            if(effect.hitCantTriggerResetAnimation<=0f){
             canSetMotionResetFlag=true;
             effect.hitCantTriggerResetAnimation=effect.hitCantTriggerResetAnimationDuration;
             Log.DebugMessage("effect.hitCantTriggerResetAnimation="+effect.hitCantTriggerResetAnimation);
            }
           }else{
            effect.hitCantTriggerResetAnimation=effect.hitCantTriggerResetAnimationDuration;
            Log.DebugMessage("effect.hitCantTriggerResetAnimation="+effect.hitCantTriggerResetAnimation);
           }
           canSetMotionFlag=true;
           if(effect.hitCanTriggerAnimation<=0f){
            effect.hitCanTriggerAnimation=effect.hitCanTriggerAnimationDuration;
            Log.DebugMessage("effect.hitCanTriggerAnimation="+effect.hitCanTriggerAnimation);
           }
          }
         }
         motionFlagForHitResetAnimation|=canSetMotionResetFlag;
         Log.DebugMessage("motionFlagForHitResetAnimation="+motionFlagForHitResetAnimation);
         motionFlagForHitAnimation|=canSetMotionFlag;
         Log.DebugMessage("motionFlagForHitAnimation="+motionFlagForHitAnimation);
         if(canTakeDamage){
          OnHitProcessStatDamageFrom(hitbox,hitbox.actor);
         }
         ApplyAggressionModeForThenAddTarget(hitbox.actor);
         SetTargetToBeRemoved(hitbox.actor,15f);
         foreach(var slaveId in slaves){
          if(SimObjectManager.singleton.active.TryGetValue(slaveId,out SimObject slaveSimObject)&&slaveSimObject is BaseAI slaveAI){
           slaveAI.ApplyAggressionModeForThenAddTarget(hitbox.actor,this);
           slaveAI.SetTargetToBeRemoved(hitbox.actor,15f);
          }
         }
         if(masterSimObject is BaseAI masterAI){
          masterAI.ApplyAggressionModeForThenAddTarget(hitbox.actor,this);
          masterAI.SetTargetToBeRemoved(hitbox.actor,15f);
         }
        }
        internal virtual void OnHitProcessStatDamageFrom(Hitboxes hitbox,SimObject simObject){
         float postDamageIntegrity=Stats.ProcessStatPhysicalDamageOn(this,simObject);
         Log.DebugMessage("OnHitProcessStatDamageFrom:postDamageIntegrity:"+postDamageIntegrity);
         if(postDamageIntegrity<=0f){
          Log.DebugMessage("OnHitProcessStatDamageFrom:set motion dead");
         }
        }
    }
}