#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class AnimationEventsHandler:MonoBehaviour{
     internal BaseAI actor;
        internal void OnShootEvent(){
         Log.DebugMessage("OnShootEvent");
         OnAnimatorShoot?.Invoke(actor);
         OnAnimatorShoot=null;
        }
        internal delegate void OnAnimatorShootEvent(SimObject simAiming);
        internal OnAnimatorShootEvent OnAnimatorShoot;
        internal void OnCanDamageAnimationEvent(string bodyPartName){
         Log.DebugMessage("OnCanDamageAnimationEvent:"+bodyPartName);
         if(actor.nameToHitboxes.TryGetValue(bodyPartName,out Hitboxes bodyPartHitbox)){
          bodyPartHitbox.OnCanDamage();
         }
        }
        internal void OnCantDamageAnimationEvent(string bodyPartName){
         Log.DebugMessage("OnCantDamageAnimationEvent:"+bodyPartName);
         if(actor.nameToHitboxes.TryGetValue(bodyPartName,out Hitboxes bodyPartHitbox)){
          bodyPartHitbox.OnCantDamage();
         }
        }
    }
}