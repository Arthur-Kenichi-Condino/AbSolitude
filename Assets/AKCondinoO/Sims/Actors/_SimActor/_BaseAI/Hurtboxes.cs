#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class Hurtboxes:MonoBehaviour{
     [SerializeField]internal float bodyPartDamageMultiplier          =1.0f;
     [SerializeField]internal float bodyPartDamageMultiplierForFirearm=1.0f;
     internal Rigidbody kinematicRigidbody;
     internal BaseAI actor;
        internal void OnTriggerStay(Collider other){
        }
        internal bool OnTakeDamage(Hitboxes fromHitbox){
         if(fromHitbox.actor==actor){
          //Log.DebugMessage("ignore damage from my own hitbox");
          return false;
         }
         if(actor.hitGracePeriod.ContainsKey(fromHitbox)){
          return false;
         }
         actor.hitGracePeriod.Add(fromHitbox,fromHitbox.gracePeriod);
         Log.DebugMessage("OnTakeDamage:fromHitbox:"+fromHitbox+";Hurtbox:"+this);
         actor.OnHit(fromHitbox);
         return true;
        }
        internal bool OnTakeDamage(SimWeapon fromWeapon){
         Log.DebugMessage("OnTakeDamage:fromWeapon:"+fromWeapon+";Hurtbox:"+this,actor);
         //actor.OnHit(fromHitbox);
         return false;
        }
    }
}