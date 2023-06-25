#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected SimObject MyEnemy=null;internal SimObject enemy{get{return MyEnemy;}}
        internal enum AggressionMode:int{
         Defensive=0,
         AggressiveToPotentialEnemies=1,
         AggressiveToAll=2,
        }
     [SerializeField]protected AggressionMode MyAggressionMode=AggressionMode.Defensive;internal AggressionMode aggression{get{return MyAggressionMode;}}
        internal override void OnSimObjectIsInSight(SimObject simObject){
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
         Log.DebugMessage("target to add:"+target.id.Value);
         OnAddAsTarget(target,gotTargetMode,enemyPriority);
        }
        internal override void OnSimObjectIsOutOfSight(SimObject simObject){
         SetToBeRemovedFromAsTarget(simObject);
        }
        internal virtual void SetToBeRemovedFromAsTarget(SimObject target){
         if(target.id==null){
          return;
         }
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          Log.DebugMessage("target set to be removed:"+target.id.Value);
          targetTimeouts[target.id.Value]=30f;
         }
        }
        internal virtual bool IsInAttackRange(SimObject simObject){
         return false;
        }
        protected override void DoAttack(){
        }
    }
}