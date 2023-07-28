#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimCollisionsChildTrigger:MonoBehaviour{
        void Awake(){
         this.tag="SimObjectVolume";
        }
     internal SimCollisions simCollisions;
     internal Rigidbody kinematicRigidbody;
        internal void Activate(){
         this.gameObject.SetActive(true);
        }
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         //  OnTriggerExit will not be called
         simObjectColliders.Clear();
        }
     internal readonly HashSet<Collider>simObjectColliders=new HashSet<Collider>();
        void OnTriggerEnter(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         if(other.transform.root==this.transform.root){
          return;
         }
         //Log.DebugMessage("SimCollisions:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
         if(simCollisions.IsValidForCollision(other,out SimObject otherSimObject)){
          //Log.DebugMessage("SimCollisions:OnTriggerEnter:SimObjectVolume:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
          simObjectColliders.Add(other);
          if(!otherSimObject.simCollisions.collidedWithChildTrigger.ContainsKey(this)){
           otherSimObject.simCollisions.collidedWithChildTrigger.Add(this,0);
          }else{
           otherSimObject.simCollisions.collidedWithChildTrigger[this]++;
          }
         }
        }
        void OnTriggerExit(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         simObjectColliders.Remove(other);
         if(simCollisions.IsValidForCollision(other,out SimObject otherSimObject)){
          if(otherSimObject.simCollisions.collidedWithChildTrigger.ContainsKey(this)){
           otherSimObject.simCollisions.collidedWithChildTrigger[this]--;
           if(otherSimObject.simCollisions.collidedWithChildTrigger[this]<0){
            otherSimObject.simCollisions.collidedWithChildTrigger.Remove(this);
           }
          }
         }
        }
    }
}