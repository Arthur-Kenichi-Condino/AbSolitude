#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class Hitboxes:MonoBehaviour{
     internal Rigidbody kinematicRigidbody;
     internal SimActor actor;
     [SerializeField]float maxCanDamageTime=.5f;
     [SerializeField]internal float gracePeriod=.2f;
      float canDamage=-1f;
        internal void OnTriggerStay(Collider other){
         if(canDamage>=0f){
          if(Time.time-canDamage<=maxCanDamageTime){
           Hurtboxes hurtbox;
           if((hurtbox=other.GetComponent<Hurtboxes>())!=null){
            //Log.DebugMessage("OnTriggerStay:do damage to:"+other,other);
            hurtbox.OnTakeDamage(this);
           }
          }
         }
        }
        internal void OnCanDamage(){
         Log.DebugMessage(this+":OnCanDamage");
         canDamage=Time.time;
        }
        internal void OnCantDamage(){
         Log.DebugMessage(this+":OnCantDamage");
         canDamage=-1f;
        }
    }
}