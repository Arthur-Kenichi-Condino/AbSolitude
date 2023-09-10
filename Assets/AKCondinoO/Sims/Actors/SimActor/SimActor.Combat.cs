#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        internal virtual void OnSimObjectIsInSight(SimObject simObject){
        }
        internal virtual void OnSimObjectIsOutOfSight(SimObject simObject){
        }
        protected virtual void DoAttack(){
        }
        internal virtual void OnHit(Hitboxes hitbox){
        }
        internal virtual void OnHitProcessStatDamageFrom(Hitboxes hitbox,SimObject simObject){
        }
    }
}