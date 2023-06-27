#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Vector3 MyAttackRange=new Vector3(0f,.4375f,.25f);internal Vector3 attackRange{get{return MyAttackRange;}}
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
        protected override void DoAttack(){
        }
    }
}