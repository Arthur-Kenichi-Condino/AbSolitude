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
     protected Vector3 MyAttackRange=new Vector3(0f,.25f,.25f);internal Vector3 attackRange{get{return MyAttackRange;}}
     protected SimObject MyEnemy=null;internal SimObject enemy{get{return MyEnemy;}}
        internal enum AggressionMode:int{
         Defensive=0,
         AggressiveToPotentialEnemies=1,
         AggressiveToAll=2,
        }
     [SerializeField]protected AggressionMode MyAggressionMode=AggressionMode.Defensive;internal AggressionMode aggression{get{return MyAggressionMode;}}
        internal virtual void OnSimObjectIsInSight(SimObject simObject){
         ApplyAggressionModeForThenAddAsTarget(simObject);
        }
        internal virtual void ApplyAggressionModeForThenAddAsTarget(SimObject target){
         if(target.id==null){
          return;
         }
         if(MyAggressionMode==AggressionMode.AggressiveToAll){
          if(target is SimActor targetSimActor&&!target.IsMonster()){
           ApplyEnemyPriorityForThenAddAsTarget(target,GotTargetMode.Aggressively);
          }
         }else{
         }
        }
        internal virtual void ApplyEnemyPriorityForThenAddAsTarget(SimObject target,GotTargetMode gotTargetMode){
         if(target.id==null){
          return;
         }
         EnemyPriority enemyPriority=EnemyPriority.Low;
         //Log.DebugMessage("target to add:"+target.id.Value);
         OnAddAsTarget(target,gotTargetMode,enemyPriority);
        }
        internal virtual void OnSimObjectIsOutOfSight(SimObject simObject){
         SetToBeRemovedFromAsTarget(simObject);
        }
        internal virtual void SetToBeRemovedFromAsTarget(SimObject target){
         if(target.id==null){
          return;
         }
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          //Log.DebugMessage("target set to be removed:"+target.id.Value);
          targetTimeouts[target.id.Value]=30f;
         }
        }
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
     protected bool onDoAttackSetMotion=false;
        protected virtual void DoAttack(){
         //Log.DebugMessage("DoAttack()");
         onDoAttackSetMotion=true;
        }
     internal bool onDoShootingSetMotion=false;
        internal virtual bool DoShooting(SimWeapon simWeapon){
         if(onDoShootingSetMotion){
          return false;
         }
         if(isAiming){
          if(animatorController.animator!=null){
           if(animatorController.animationEventsHandler!=null){
            Log.DebugMessage("DoShooting()");
            animatorController.animationEventsHandler.OnAnimatorShoot+=simWeapon.OnShoot;
            onDoShootingSetMotion=true;
            return true;
           }
          }
         }
         return false;
        }
     protected readonly HashSet<Skill>onWillTakeDamageSkillsToUse=new HashSet<Skill>();
     protected bool onHitSetMotion=false;
      protected bool onHitResetMotion=false;
        internal virtual void OnHit(Hitboxes hitbox){
         onWillTakeDamageSkillsToUse.Clear();
         GetBest(Skill.SkillUseContext.OnWillTakeDamage,onWillTakeDamageSkillsToUse);
         foreach(Skill skill in onWillTakeDamageSkillsToUse){
          Type skillType=skill.GetType();
          if(skills.TryGetValue(skillType,out Skill skillToGet)&&skillToGet==skill){
           SimObject target=this;//  TO DO: set best my skill target
           if(skill.IsAvailable(target,skill.level)){
            skill.DoSkillImmediate(target,skill.level);
           }
          }
         }
         bool canTakeDamage=true;
         bool canSetMotion=true,canResetMotion=true;
         OnHitGracePeriodSkillBuff onHitGracePeriodSkillBuff=null;
         if(skillBuffs.Contains(typeof(OnHitGracePeriodSkillBuff),out List<SkillBuff>activeOnHitGracePeriodSkillBuffs)){
          canSetMotion=false;canResetMotion=false;
          onHitGracePeriodSkillBuff=(OnHitGracePeriodSkillBuff)activeOnHitGracePeriodSkillBuffs[0];
          var effect=onHitGracePeriodSkillBuff.onHitGracePeriodEffect;
          if(effect.onHitSetMotionGracePeriod<=0f){
           if(onHitSetMotion){
            if(effect.onHitResetMotionGracePeriod<=0f){
             canResetMotion=true;
             effect.onHitResetMotionGracePeriod=effect.onHitResetMotionGracePeriodDuration;
             Log.DebugMessage("effect.onHitResetMotionGracePeriod="+effect.onHitResetMotionGracePeriod);
            }
           }
           canSetMotion=true;
           if(effect.onHitSetMotionVulnerablePeriod<=0f){
            effect.onHitSetMotionVulnerablePeriod=effect.onHitSetMotionVulnerablePeriodDuration;
            Log.DebugMessage("effect.onHitSetMotionVulnerablePeriod="+effect.onHitSetMotionVulnerablePeriod);
           }
          }
         }
         onHitResetMotion|=canResetMotion;
         Log.DebugMessage("onHitResetMotion="+onHitResetMotion);
         onHitSetMotion|=canSetMotion;
         Log.DebugMessage("onHitSetMotion="+onHitSetMotion);
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