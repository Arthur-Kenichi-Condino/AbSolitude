#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum AggressionMode:int{
         Defensive=0,
         AggressiveToPotentialEnemies=1,
         AggressiveToAll=2,
        }
     [SerializeField]protected AggressionMode MyAggressionMode=AggressionMode.Defensive;internal AggressionMode aggression{get{return MyAggressionMode;}}
        internal virtual void OnSimObjectIsInSight(SimObject simObject){
         ApplyAggressionModeForThenAddTarget(simObject);
        }
        internal virtual void ApplyAggressionModeForThenAddTarget(SimObject target,SimObject targetOfTarget=null){
         if(target.id==null){
          return;
         }
         if(target.id==id){
          return;
         }
         if(target.id==masterId){
          if(masterSimObject is BaseAI masterAI&&masterAI.enemy!=this){
           return;
          }
         }
         if(slaves.Contains(target.id.Value)){
          if(target is BaseAI targetAI&&targetAI.enemy!=this){
           return;
          }
         }
         if(target.IsDead()){
          return;
         }
         if(MyAggressionMode==AggressionMode.AggressiveToAll){
          if(target is SimActor targetSimActor&&!target.IsMonster()){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Aggressively);
           return;
          }
         }else if(targetOfTarget!=null){
          if(targetOfTarget.id==null){
           return;
          }
          if(masterId==targetOfTarget.id){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromMaster);
           return;
          }
          if(slaves.Contains(targetOfTarget.id.Value)){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromSlave);
           return;
          }
         }
         ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Defensively);
        }
        internal virtual void ApplyEnemyPriorityForThenAddTarget(SimObject target,GotTargetMode gotTargetMode){
         if(target.id==null){
          return;
         }
         EnemyPriority enemyPriority=EnemyPriority.Low;
         //Log.DebugMessage("target to add:"+target.id.Value);
         OnAddTarget(target,gotTargetMode,enemyPriority);
        }
        internal virtual void OnSimObjectIsOutOfSight(SimObject simObject){
         SetTargetToBeRemoved(simObject);
        }
        internal virtual void SetTargetToBeRemoved(SimObject target,float afterSeconds=30f){
         if(target.id==null){
          return;
         }
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          //Log.DebugMessage("target set to be removed:"+target.id.Value);
          targetTimeouts[target.id.Value]=afterSeconds;
         }
        }
    }
}