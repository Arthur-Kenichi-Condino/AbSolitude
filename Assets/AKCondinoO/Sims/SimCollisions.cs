#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
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
           (Vector3 direction,
            int enumDirection,
            float height,
            float radius,
            Vector3 center,
            Vector3 localPoint0,
            Vector3 localPoint1,
            Vector3 point0,
            Vector3 point1
           )values=GetCapsuleValuesForCollisionTesting(capsule,transform.root);
           trigger.isTrigger=true;
           trigger.direction=values.enumDirection;
           trigger.height=values.height;
           trigger.radius=values.radius;
           trigger.center=values.center;
           triggers.Add(trigger);
          }
         }
        }
        internal(
         Vector3 direction,
         int enumDirection,
         float height,
         float radius,
         Vector3 center,
         Vector3 localPoint0,
         Vector3 localPoint1,
         Vector3 point0,
         Vector3 point1
        )GetCapsuleValuesForCollisionTesting(CapsuleCollider capsule,Transform transform){
         var direction=new Vector3{[capsule.direction]=1};
         //Log.DebugMessage("capsule direction:"+direction);
         var offset=capsule.height/2f-capsule.radius;
         var localPoint0=capsule.center-direction*offset;
         var localPoint1=capsule.center+direction*offset;
         var point0=transform.TransformPoint(localPoint0);
         var point1=transform.TransformPoint(localPoint1);
         Vector3 r=transform.TransformVector(capsule.radius,capsule.radius,capsule.radius);
         float radius=Enumerable.Range(0,3).Select(xyz=>xyz==capsule.direction?0:r[xyz]).Select(Mathf.Abs).Max();
         return(
          direction,
          capsule.direction,
          capsule.height,
          radius,
          capsule.center,
          localPoint0,
          localPoint1,
          point0,
          point1
         );
        }
        internal void Activate(){
         this.gameObject.SetActive(true);
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Activate();
         }
        }
     internal readonly Dictionary<SimObject,int>collidedWith=new Dictionary<SimObject,int>();
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         //  OnTriggerExit will not be called
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Deactivate();
         }
         simObjectCollisions.Clear();
         foreach(var kvp in collidedWith){
          SimObject simObjectCollidedWith=kvp.Key;
          if(simObjectCollidedWith.simCollisions!=null){
           simObjectCollidedWith.simCollisions.simObjectCollisions.RemoveWhere(collider=>{return collider.transform.root==this.transform.root;});
          }
         }
         collidedWith.Clear();
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
           if(!otherSimObject.simCollisions.collidedWith.ContainsKey(simObject)){
            otherSimObject.simCollisions.collidedWith.Add(simObject,0);
           }else{
            otherSimObject.simCollisions.collidedWith[simObject]++;
           }
          }
          simObject.OnOverlapping(other);
         }
        }
        void OnTriggerExit(Collider other){
         //Log.DebugMessage("SimCollisions:OnTriggerExit:"+other.transform.root.gameObject.name);
         simObjectCollisions.Remove(other);
         if(other.CompareTag("SimObjectVolume")){
          SimObject otherSimObject=other.GetComponentInParent<SimObject>();
          if(otherSimObject.simCollisions!=null){
           if(otherSimObject.simCollisions.collidedWith.ContainsKey(simObject)){
            otherSimObject.simCollisions.collidedWith[simObject]--;
            if(otherSimObject.simCollisions.collidedWith[simObject]<0){
             otherSimObject.simCollisions.collidedWith.Remove(simObject);
            }
           }
          }
         }
        }
    }
}