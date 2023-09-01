#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class Hurtboxes:MonoBehaviour{
     internal Rigidbody kinematicRigidbody;
     internal SimActor actor;
        internal void OnTriggerStay(Collider other){
        }
        internal bool OnTakeDamage(Hitboxes fromHitbox){
         if(actor.hitGracePeriod.ContainsKey(fromHitbox)){
          return false;
         }
         actor.hitGracePeriod.Add(fromHitbox,fromHitbox.gracePeriod);
         Log.DebugMessage("OnTakeDamage:fromHitbox:"+fromHitbox);
         actor.OnHit(fromHitbox);
         return true;
        }
    }
}