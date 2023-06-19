#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimCollisions:MonoBehaviour{
     [SerializeField]internal SimCollisionsChildTrigger simCollisionsChildTriggerPrefab;
        void Awake(){
         this.tag="SimObjectVolume";
        }
     internal SimObject simObject;
     internal Rigidbody kinematicRigidbody;
     internal readonly List<Collider>triggers=new List<Collider>();
      internal readonly List<SimCollisionsChildTrigger>childTriggers=new List<SimCollisionsChildTrigger>();
        internal void AddTriggers(){
         this.gameObject.layer=simObject.gameObject.layer;
         kinematicRigidbody=this.gameObject.AddComponent<Rigidbody>();
         kinematicRigidbody.isKinematic=true;
         foreach(var volumeCollider in simObject.volumeColliders){
          if(volumeCollider is CharacterController characterController){
           CapsuleCollider trigger=this.gameObject.AddComponent<CapsuleCollider>();
           trigger.isTrigger=true;
           trigger.height=characterController.height;
           trigger.radius=characterController.radius;
           trigger.center=characterController.center;
           triggers.Add(trigger);
           if(simCollisionsChildTriggerPrefab){
            var height=characterController.height;
            var section=height/3f;
            if((section/2f)>characterController.radius){
             var offset=(section/2f)-characterController.radius;
             var center=characterController.center;
             center.y+=(height/2f)-(section/2f);
             Vector3 r=transform.TransformVector(
              characterController.radius,
              characterController.radius,
              characterController.radius
             );
             float radius=Enumerable.Range(0,3).Select(xyz=>xyz==1?0:r[xyz]).Select(Mathf.Abs).Max();
             SimCollisionsChildTrigger simCollisionsChild=Instantiate(simCollisionsChildTriggerPrefab,transform).GetComponent<SimCollisionsChildTrigger>();
             CapsuleCollider upperTrigger=simCollisionsChild.AddComponent<CapsuleCollider>();
             upperTrigger.isTrigger=true;
             upperTrigger.height=(offset+radius)*2f;
             upperTrigger.radius=radius;
             upperTrigger.center=center;
             childTriggers.Add(simCollisionsChild);
            }
           }
          }else if(volumeCollider is CapsuleCollider capsule){
           CapsuleCollider trigger=this.gameObject.AddComponent<CapsuleCollider>();
           trigger.isTrigger=true;
           trigger.direction=capsule.direction;
           trigger.height=capsule.height;
           trigger.radius=capsule.radius;
           trigger.center=capsule.center;
           triggers.Add(trigger);
          }
         }
        }
        internal void Activate(){
         this.gameObject.SetActive(true);
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Activate();
         }
        }
     internal readonly HashSet<SimObject>collidedWith=new HashSet<SimObject>();
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         //  OnTriggerExit will not be called
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Deactivate();
         }
         simObjectCollisions.Clear();
         foreach(SimObject simObjectCollidedWith in collidedWith){
          if(simObjectCollidedWith.simCollisions!=null){
           simObjectCollidedWith.simCollisions.simObjectCollisions.RemoveWhere(collider=>{return triggers.Contains(collider)||simObject.volumeColliders.Contains(collider);});
          }
         }
        }
      internal readonly HashSet<Collider>simObjectCollisions=new HashSet<Collider>();
        void OnTriggerEnter(Collider other){
         if(other.transform.root==this.transform.root){
          return;
         }
         Log.DebugMessage("SimCollisions:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
         if(other.CompareTag("SimObjectVolume")){
          Log.DebugMessage("SimCollisions:OnTriggerEnter:SimObjectVolume:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
          simObjectCollisions.Add(other);
          SimObject otherSimObject=other.GetComponentInParent<SimObject>();
          if(otherSimObject.simCollisions!=null){
           otherSimObject.simCollisions.collidedWith.Add(simObject);
          }
          simObject.OnOverlapped(other);
         }
        }
        void OnTriggerExit(Collider other){
         //Log.DebugMessage("SimCollisions:OnTriggerExit:"+other.transform.root.gameObject.name);
         simObjectCollisions.Remove(other);
        }
    }
}