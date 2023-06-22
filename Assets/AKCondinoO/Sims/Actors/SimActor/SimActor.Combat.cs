#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        internal virtual void OnSimObjectIsInSight(SimObject simObject){
        }
        internal virtual void AggressionModeFor(SimObject simObject){
        }
        protected virtual void DoAttack(){
        }
    }
}