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
     internal bool isAiming{
      get{
       if(characterController!=null){
        return characterController.isAiming;
       }
       return false;
      }
     }
     protected Vector3 MyAttackRange=new Vector3(0f,.25f,.25f);internal Vector3 attackRange{get{return MyAttackRange;}}
        internal virtual bool IsInAttackRange(SimObject simObject){
         Vector3 delta=new Vector3(
          Mathf.Abs(simObject.transform.position.x-transform.position.x),
          Mathf.Abs(simObject.transform.position.y-transform.position.y),
          Mathf.Abs(simObject.transform.position.z-transform.position.z)
         );
         float disXZPlane=new Vector3(delta.x,0f,delta.z).magnitude;
         float radius=Mathf.Max(localBounds.extents.x,localBounds.extents.z);
         float simObjectRadius=Mathf.Max(simObject.localBounds.extents.x,simObject.localBounds.extents.z);
         if(disXZPlane<=radius+simObjectRadius+MyAttackRange.z&&delta.y<=localBounds.extents.y+MyAttackRange.y){
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
         if(motionFlagForShootingAnimation){
          return false;
         }
         if(isAiming){
          if(animatorController.animator!=null){
           if(animatorController.animationEventsHandler!=null){
            Log.DebugMessage("DoShootingOnAnimationEventUsingWeapon()");
            animatorController.animationEventsHandler.OnAnimatorShoot+=simWeapon.OnShoot;
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
          if(effect.hitCantTriggerAnimationIsOnGracePeriod<=0f){
           if(motionFlagForHitAnimation){
            if(effect.hitCantTriggerResetAnimationIsOnGracePeriod<=0f){
             canSetMotionResetFlag=true;
             effect.hitCantTriggerResetAnimationIsOnGracePeriod=effect.hitCantTriggerAnimationResetIsOnGracePeriodDuration;
             Log.DebugMessage("effect.hitCantTriggerResetAnimationIsOnGracePeriod="+effect.hitCantTriggerResetAnimationIsOnGracePeriod);
            }
           }
           canSetMotionFlag=true;
           if(effect.hitCanTriggerAnimationIsVulnerablePeriod<=0f){
            effect.hitCanTriggerAnimationIsVulnerablePeriod=effect.hitCanTriggerAnimationIsVulnerablePeriodDuration;
            Log.DebugMessage("effect.hitCanTriggerAnimationIsVulnerablePeriod="+effect.hitCanTriggerAnimationIsVulnerablePeriod);
           }
          }
         }
         motionFlagForHitResetAnimation|=canSetMotionResetFlag;
         Log.DebugMessage("motionFlagForHitAnimationReset="+motionFlagForHitResetAnimation);
         motionFlagForHitAnimation|=canSetMotionFlag;
         Log.DebugMessage("motionFlagForHitAnimation="+motionFlagForHitAnimation);
         if(canTakeDamage){
          OnHitProcessStatDamageFrom(hitbox,hitbox.actor);
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