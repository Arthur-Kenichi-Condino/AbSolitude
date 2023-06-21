#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
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
           SimActor simActor=simObject as SimActor;
           CapsuleCollider trigger=this.gameObject.AddComponent<CapsuleCollider>();
           var values=GetCapsuleValuesForCollisionTesting(characterController,transform.root);
           trigger.isTrigger=true;
           trigger.height=values.height;
           trigger.radius=values.radius;
           trigger.center=values.center;
           triggers.Add(trigger);
           if(simCollisionsChildTriggerPrefab){
            var upperMiddleLowerValues=GetCapsuleValuesForUpperMiddleLowerCollisionTesting(characterController,transform.root,characterController.height,characterController.center);
            if(upperMiddleLowerValues!=null){
             SimCollisionsChildTrigger simCollisionsChild=Instantiate(simCollisionsChildTriggerPrefab,transform).GetComponent<SimCollisionsChildTrigger>();
             simCollisionsChild.gameObject.layer=simObject.gameObject.layer;
             simCollisionsChild.simCollisions=this;
             CapsuleCollider upperTrigger=simCollisionsChild.AddComponent<CapsuleCollider>();
             upperTrigger.isTrigger=true;
             upperTrigger.height=upperMiddleLowerValues.Value.upperValues.height;
             upperTrigger.radius=upperMiddleLowerValues.Value.upperValues.radius;
             upperTrigger.center=upperMiddleLowerValues.Value.upperValues.center;
             childTriggers.Add(simCollisionsChild);
             if(simActor!=null){
              simActor.simCollisionsTouchingUpper=simCollisionsChild;
             }
             simCollisionsChild=Instantiate(simCollisionsChildTriggerPrefab,transform).GetComponent<SimCollisionsChildTrigger>();
             simCollisionsChild.gameObject.layer=simObject.gameObject.layer;
             simCollisionsChild.simCollisions=this;
             CapsuleCollider middleTrigger=simCollisionsChild.AddComponent<CapsuleCollider>();
             middleTrigger.isTrigger=true;
             middleTrigger.height=upperMiddleLowerValues.Value.middleValues.height;
             middleTrigger.radius=upperMiddleLowerValues.Value.middleValues.radius;
             middleTrigger.center=upperMiddleLowerValues.Value.middleValues.center;
             childTriggers.Add(simCollisionsChild);
             if(simActor!=null){
              simActor.simCollisionsTouchingMiddle=simCollisionsChild;
             }
            }
           }
          }else if(volumeCollider is CapsuleCollider capsule){
           CapsuleCollider trigger=this.gameObject.AddComponent<CapsuleCollider>();
           var values=GetCapsuleValuesForCollisionTesting(capsule,transform.root);
           trigger.isTrigger=true;
           trigger.direction=values.enumDirection;
           trigger.height=values.height;
           trigger.radius=values.radius;
           trigger.center=values.center;
           triggers.Add(trigger);
          }
         }
        }
        #region Get Values For Collision Testing
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
             )GetCapsuleValuesForCollisionTesting(CharacterController capsule,Transform transform){
              var direction=Vector3.up;
              //Log.DebugMessage("capsule direction:"+direction);
              var offset=capsule.height/2f-capsule.radius;
              var localPoint0=capsule.center-direction*offset;
              var localPoint1=capsule.center+direction*offset;
              var point0=transform.TransformPoint(localPoint0);
              var point1=transform.TransformPoint(localPoint1);
              float radius=capsule.radius;
              return(
               direction,
               1,
               capsule.height,
               radius,
               capsule.center,
               localPoint0,
               localPoint1,
               point0,
               point1
              );
             }
              internal(
               (
                Vector3 direction,
                int enumDirection,
                float height,
                float radius,
                Vector3 center,
                Vector3 localPoint0,
                Vector3 localPoint1,
                Vector3 point0,
                Vector3 point1
               )upperValues,
               (
                Vector3 direction,
                int enumDirection,
                float height,
                float radius,
                Vector3 center,
                Vector3 localPoint0,
                Vector3 localPoint1,
                Vector3 point0,
                Vector3 point1
               )middleValues
              )?GetCapsuleValuesForUpperMiddleLowerCollisionTesting(CharacterController capsule,Transform transform,float capsuleHeight,Vector3 capsuleCenter){
               var section=capsuleHeight/3f;
               if(!((section/2f)>capsule.radius)){
                return null;
               }
               var direction=Vector3.up;
               var offset=(section/2f)-capsule.radius;
               var center=capsuleCenter;
                   center.y+=(capsuleHeight/2f)-(section/2f);
               float radius=capsule.radius;
               var localPoint0=center-direction*offset;
               var localPoint1=center+direction*offset;
               var point0=transform.TransformPoint(localPoint0);
               var point1=transform.TransformPoint(localPoint1);
               var upper=(
                direction,
                1,
                (offset+radius)*2f,
                radius,
                center,
                localPoint0,
                localPoint1,
                point0,
                point1
               );
               center=capsuleCenter;
               localPoint0=center-direction*offset;
               localPoint1=center+direction*offset;
               point0=transform.TransformPoint(localPoint0);
               point1=transform.TransformPoint(localPoint1);
               var middle=(
                direction,
                1,
                (offset+radius)*2f,
                radius,
                center,
                localPoint0,
                localPoint1,
                point0,
                point1
               );
               return(upper,middle);
              }
        #endregion
        internal void Activate(){
         this.gameObject.SetActive(true);
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Activate();
         }
        }
     internal readonly Dictionary<SimObject,int>collidedWith=new Dictionary<SimObject,int>();
      internal readonly Dictionary<SimCollisionsChildTrigger,int>collidedWithChildTrigger=new Dictionary<SimCollisionsChildTrigger,int>();
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         foreach(SimCollisionsChildTrigger childTrigger in childTriggers){
          childTrigger.Deactivate();
         }
         foreach(var kvp in collidedWithChildTrigger){
          SimCollisionsChildTrigger simObjectCollidedWithChildTrigger=kvp.Key;
          simObjectCollidedWithChildTrigger.simObjectColliders.RemoveWhere(collider=>{return collider.transform.root==this.transform.root;});
         }
         collidedWithChildTrigger.Clear();
         //  OnTriggerExit will not be called
         simObjectColliders.Clear();
         foreach(var kvp in collidedWith){
          SimObject simObjectCollidedWith=kvp.Key;
          if(simObjectCollidedWith.simCollisions!=null){
           simObjectCollidedWith.simCollisions.simObjectColliders.RemoveWhere(collider=>{return collider.transform.root==this.transform.root;});
          }
         }
         collidedWith.Clear();
        }
     internal readonly HashSet<Collider>simObjectColliders=new HashSet<Collider>();
        void OnTriggerEnter(Collider other){
         if(other.transform.root==this.transform.root){
          return;
         }
         //Log.DebugMessage("SimCollisions:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
         if(other.CompareTag("SimObjectVolume")&&!other.isTrigger){
          Log.DebugMessage("SimCollisions:OnTriggerEnter:SimObjectVolume:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
          simObjectColliders.Add(other);
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
         simObjectColliders.Remove(other);
         if(other.CompareTag("SimObjectVolume")&&!other.isTrigger){
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