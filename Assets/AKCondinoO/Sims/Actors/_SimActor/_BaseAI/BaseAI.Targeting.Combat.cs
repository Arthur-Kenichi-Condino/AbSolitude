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
        internal static bool IsInRange(Vector3 pos1,Vector3 pos2,Vector3 dis,float radius1=0f,float radius2=0f){
         Vector3 delta=new Vector3(
          Mathf.Abs(pos1.x-pos2.x),
          Mathf.Abs(pos1.y-pos2.y),
          Mathf.Abs(pos1.z-pos2.z)
         );
         float disXZPlane=new Vector3(delta.x,0f,delta.z).magnitude;
         if((disXZPlane<=radius1+dis.z+radius2||disXZPlane<=radius1+dis.x+radius2)&&delta.y<=dis.y){
          //Log.DebugMessage("is in range:disXZPlane:"+disXZPlane);
          return true;
         }
         return false;
        }
        internal partial class AI{
         protected Vector3 MyAttackRange{get{return me.attackRange;}}
        }
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
            //Log.DebugMessage("DoReloadingOnAnimationEventUsingWeapon():simWeapon:"+simWeapon,simWeapon);
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
     internal Vector3 attackRange=new Vector3((0.125f/8f)*(3f),(0.125f/8f)*(3f),(0.0625f/8f)*(3f));
     readonly List<SimWeapon>attackDistanceSimWeapons=new List<SimWeapon>();
        internal Vector3 AttackDistance(out bool hasWeapon,bool checkWeapon=false){
         hasWeapon=false;
         float radius=GetRadius();
         float weaponRange=0f;
         if(checkWeapon){
          CurrentWeapons(attackDistanceSimWeapons);
          hasWeapon=attackDistanceSimWeapons.Count>0;
          foreach(SimWeapon weapon in attackDistanceSimWeapons){
           weaponRange=Mathf.Max(weaponRange,weapon.shootDis);
          }
         }
         float height=GetHeight();
         return new Vector3(
          radius*.95f+attackRange.x,
          height*.95f+attackRange.y,
          Mathf.Max(radius*.95f+attackRange.z,radius+weaponRange)
         );
        }
        internal virtual bool IsInAttackRange(SimObject simObject,out Vector3 attackDistance,out bool hasWeapon,bool checkWeapon=false){
         hasWeapon=false;
         Vector3 delta=new Vector3(
          Mathf.Abs(simObject.transform.position.x-transform.position.x),
          Mathf.Abs(simObject.transform.position.y-transform.position.y),
          Mathf.Abs(simObject.transform.position.z-transform.position.z)
         );
         float disXZPlane=new Vector3(delta.x,0f,delta.z).magnitude;
         attackDistance=AttackDistance(out hasWeapon,checkWeapon);
         float radius=GetRadius();
         float simObjectRadius=Mathf.Max(simObject.localBounds.extents.x,simObject.localBounds.extents.z);
         if((disXZPlane<=radius+attackDistance.z+simObjectRadius||disXZPlane<=radius+attackDistance.x+simObjectRadius)&&delta.y<=attackDistance.y){
          //Log.DebugMessage("simObject is in my attack range:disXZPlane:"+disXZPlane);
          if(checkWeapon&&!hasWeapon){
          }
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
         ProcessOnHitGracePeriod(out bool canTakeDamage,out bool canSetMotionFlag,out bool canSetMotionResetFlag);
         motionFlagForHitResetAnimation|=canSetMotionResetFlag;
         Log.DebugMessage("motionFlagForHitResetAnimation="+motionFlagForHitResetAnimation);
         motionFlagForHitAnimation|=canSetMotionFlag;
         Log.DebugMessage("motionFlagForHitAnimation="+motionFlagForHitAnimation);
         if(canTakeDamage){
          OnHitProcessStatDamageFrom(hitbox,hitbox.actor);
         }
         ApplyAggressionModeForThenAddTarget(hitbox.actor);
         SetTargetToBeRemoved(hitbox.actor,targetFastTimeout,targetCooldownAfterFastTimeout);
         foreach(var slaveId in slaves){
          if(SimObjectManager.singleton.active.TryGetValue(slaveId,out SimObject slaveSimObject)&&slaveSimObject is BaseAI slaveAI){
           slaveAI.ApplyAggressionModeForThenAddTarget(hitbox.actor,this);
           slaveAI.SetTargetToBeRemoved(hitbox.actor,targetFastTimeout,targetCooldownAfterFastTimeout);
          }
         }
         if(masterSimObject is BaseAI masterAI){
          masterAI.ApplyAggressionModeForThenAddTarget(hitbox.actor,this);
          masterAI.SetTargetToBeRemoved(hitbox.actor,targetFastTimeout,targetCooldownAfterFastTimeout);
         }
        }
        internal virtual void OnHitProcessStatDamageFrom(Hitboxes hitbox,SimObject simObject){
         float preDamageIntegrity=0f;
         if(stats!=null){
          preDamageIntegrity=stats.IntegrityGet(this);
         }
         Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:preDamageIntegrity:"+preDamageIntegrity);
         float postDamageIntegrity=Stats.ProcessStatPhysicalDamageOn(this,fromSimObject:simObject);
         Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:postDamageIntegrity:"+postDamageIntegrity);
         ProcessOnHitDamage(preDamageIntegrity,postDamageIntegrity,simObject);
         if(postDamageIntegrity<=0f){
          Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:set motion dead");
          OnDeath();
         }
        }
        internal override bool OnShotByWeapon(SimWeapon simWeapon,Hurtboxes hurtbox=null){
         if(hurtbox!=null){
          return hurtbox.OnTakeDamage(fromWeapon:simWeapon);
         }
         return false;
        }
     internal readonly Dictionary<SimWeapon,float>weaponGracePeriod=new Dictionary<SimWeapon,float>();
      readonly List<SimWeapon>shots=new List<SimWeapon>();
        internal void UpdateWeaponsGracePeriod(){
         shots.AddRange(weaponGracePeriod.Keys);
         foreach(SimWeapon weapon in shots){
          float gracePeriod=weaponGracePeriod[weapon];
          gracePeriod-=Time.deltaTime;
          if(gracePeriod<=0f){
           weaponGracePeriod.Remove(weapon);
          }else{
           weaponGracePeriod[weapon]=gracePeriod;
          }
         }
         shots.Clear();
        }
     protected readonly HashSet<Skill>skillsToUseOnWillTakeWeaponDamage=new HashSet<Skill>();
        internal virtual void OnHit(SimWeapon simWeapon,Hurtboxes hurtbox,SimObject weaponActor=null){
         Log.DebugMessage("OnHit(SimWeapon simWeapon)",this);
         skillsToUseOnWillTakeWeaponDamage.Clear();
         GetBest(Skill.SkillUseContext.OnWillTakeDamage,skillsToUseOnWillTakeWeaponDamage);
         foreach(Skill skill in skillsToUseOnWillTakeWeaponDamage){
          Type skillType=skill.GetType();
          if(skills.TryGetValue(skillType,out Skill skillToGet)&&skillToGet==skill){
           SimObject target=this;//  TO DO: set best my skill target
           if(skill.IsAvailable(target,skill.level)){
            skill.DoSkillImmediate(target,skill.level);
           }
          }
         }
         ProcessOnHitGracePeriod(out bool canTakeDamage,out bool canSetMotionFlag,out bool canSetMotionResetFlag);
         motionFlagForHitResetAnimation|=canSetMotionResetFlag;
         Log.DebugMessage("motionFlagForHitResetAnimation="+motionFlagForHitResetAnimation);
         motionFlagForHitAnimation|=canSetMotionFlag;
         Log.DebugMessage("motionFlagForHitAnimation="+motionFlagForHitAnimation);
         if(canTakeDamage){
          OnHitProcessStatDamageFrom(simWeapon,hurtbox,weaponActor);
         }
        }
        internal virtual void OnHitProcessStatDamageFrom(SimWeapon simWeapon,Hurtboxes hurtbox,SimObject simObject=null){
         float preDamageIntegrity=0f;
         if(stats!=null){
          preDamageIntegrity=stats.IntegrityGet(this);
         }
         Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:preDamageIntegrity:"+preDamageIntegrity);
         float postDamageIntegrity=Stats.ProcessStatPhysicalDamageOn(this,hurtbox,fromSimWeapon:simWeapon);
         Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:postDamageIntegrity:"+postDamageIntegrity);
         ProcessOnHitDamage(preDamageIntegrity,postDamageIntegrity,simObject);
         if(postDamageIntegrity<=0f){
          Log.DebugMessage(this.name+":OnHitProcessStatDamageFrom:set motion dead");
          OnDeath();
         }
        }
     internal readonly Dictionary<SimObject,(float damage,float timeout)>damageFromActorTempHistory=new();
     internal readonly List<SimObject>damageFromActorTempHistoryIterator=new();
      internal float damageFromActorHistoryTimeout=30f;
       internal readonly List<SimObject>toRemoveFromTempHistory=new();
        protected virtual void ProcessOnHitDamage(float preDamageIntegrity,float postDamageIntegrity,SimObject fromSimObject){
         if(fromSimObject==null){
          Log.DebugMessage(this.name+":ProcessOnHitDamage:'fromSimObject==null'");
          return;
         }
         float damage=preDamageIntegrity-postDamageIntegrity;
         if(damage<=0f){
          Log.DebugMessage(this.name+":ProcessOnHitDamage:'damage<=0f'");
          return;
         }
         if(damageFromActorTempHistory.TryGetValue(fromSimObject,out var damageTempHistory)){
          damageTempHistory.damage+=damage;
          damageTempHistory.timeout=damageFromActorHistoryTimeout;
         }else{
          damageTempHistory=(damage,damageFromActorHistoryTimeout);
         }
         Log.DebugMessage(this.name+":ProcessOnHitDamage:damageTempHistory:"+damageTempHistory);
         damageFromActorTempHistory[fromSimObject]=damageTempHistory;
        }
        protected virtual void ProcessOnHitGracePeriod(out bool canTakeDamage,out bool canSetMotionFlag,out bool canSetMotionResetFlag){
         canTakeDamage=true;
         canSetMotionFlag=true;canSetMotionResetFlag=true;
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
        }
     protected bool motionFlagForDeathAnimation=false;
      protected bool motionFlagForDeathInstantAnimationJumpToEnd=false;
        protected override void OnDeath(bool instant=false){
         Log.DebugMessage("OnDeath()");
         bool wasDead=IsDead();
         if(wasDead){
          Log.DebugMessage("OnDeath():'ignorar repetições de comandos se já estava morto'");
         }
         motionFlagForDeathAnimation=true;
         if(!wasDead){
          motionFlagForDeathInstantAnimationJumpToEnd|=instant;
         }
         //  TO DO: ignorar chamadas subseqüentes de distribuir EXP porque pode ser
         // por um Hit, mas já estava morto.
         if(!wasDead){
          ProcessExpPointsGiven(stats);
          Log.DebugMessage(this.name+":OnDeath:'give sim EXP':"+expPoints.simLevelExpGivenOnDeath);
          float totalDamage=0f;
          foreach(var kvp in damageFromActorTempHistory){
           var damageTempHistory=kvp.Value;
           Log.DebugMessage(this.name+":OnDeath:damageTempHistory:"+damageTempHistory);
           totalDamage+=damageTempHistory.damage;
          }
          foreach(var kvp in damageFromActorTempHistory){
           if(totalDamage<=0f){
            //  dividir igualmente
            continue;
           }
           var damageTempHistory=kvp.Value;
           float damagePercentage=damageTempHistory.damage/totalDamage;
           float simExp=expPoints.simLevelExpGivenOnDeath*damagePercentage;
          }
          damageFromActorTempHistory.Clear();
         }
        }
        internal override bool IsDead(){
         if(motionFlagForDeathAnimation){
          return true;
         }
         if(MyMotion==ActorMotion.MOTION_DEAD||
            MyMotion==ActorMotion.MOTION_DEAD_RIFLE
         ){
          return true;
         }
         return false;
        }
        internal override bool IsMotionComplete(float after=1f){
         if(motionMappedToLayerIndex.TryGetValue(MyMotion,out int layerIndex)){
          //Log.DebugMessage("currentAnimationMapsToMotion[layerIndex]:"+currentAnimationMapsToMotion[layerIndex]);
          if(currentAnimationMapsToMotion[layerIndex]&&animatorController.animationTime[layerIndex]>=after){
           //Log.DebugMessage("IsMotionComplete:yes");
           return true;
          }
         }
         return false;
        }
    }
}