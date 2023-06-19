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
        internal void Activate(){
         this.gameObject.SetActive(true);
        }
     internal readonly HashSet<SimObject>collidedWith=new HashSet<SimObject>();
        internal void Deactivate(){
         this.gameObject.SetActive(false);
        }
        void OnTriggerEnter(Collider other){
         if(other.transform.root==this.transform.root){
          return;
         }
        }
        void OnTriggerExit(Collider other){
        }
    }
}