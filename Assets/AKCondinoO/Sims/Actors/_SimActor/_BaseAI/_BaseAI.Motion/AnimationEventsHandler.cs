#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class AnimationEventsHandler:MonoBehaviour{
     internal BaseAI actor;
        internal void CancelAllEvents(){
         CancelReloadEvent();
          CancelShootEvent();
        }
        internal delegate void OnAnimatorReloadEvent(SimObject simAiming);
        internal OnAnimatorReloadEvent onAnimatorReload;
        internal void OnReloadEvent(string weaponMotionName){
         Log.DebugMessage("OnReloadEvent:"+weaponMotionName);
         onAnimatorReload?.Invoke(actor);
        }
        internal void CancelReloadEvent(){
         Log.DebugMessage("CancelReloadEvent:");
         onAnimatorReload=null;
        }
        internal delegate void OnAnimatorShootEvent(SimObject simAiming);
        internal OnAnimatorShootEvent onAnimatorShoot;
        internal void OnShootEvent(string weaponMotionName){
         Log.DebugMessage("OnShootEvent:"+weaponMotionName);
         onAnimatorShoot?.Invoke(actor);
        }
        internal void CancelShootEvent(){
         Log.DebugMessage("CancelShootEvent:");
         onAnimatorShoot=null;
        }
        internal void OnCanDamageAnimationEvent(string bodyPartName){
         //Log.DebugMessage("OnCanDamageAnimationEvent:"+bodyPartName);
         if(actor.nameToHitboxes.TryGetValue(bodyPartName,out Hitboxes bodyPartHitbox)){
          bodyPartHitbox.OnCanDamage();
         }
        }
        internal void OnCantDamageAnimationEvent(string bodyPartName){
         //Log.DebugMessage("OnCantDamageAnimationEvent:"+bodyPartName);
         if(actor.nameToHitboxes.TryGetValue(bodyPartName,out Hitboxes bodyPartHitbox)){
          bodyPartHitbox.OnCantDamage();
         }
        }
    }
}