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
         if(target is SimActor targetSimActor){
         }
        }
        protected override void DoAttack(){
        }
    }
}