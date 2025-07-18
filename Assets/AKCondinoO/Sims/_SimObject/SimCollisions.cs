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
           BaseAI simActor=simObject as BaseAI;
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
             simCollisionsChild.kinematicRigidbody=simCollisionsChild.gameObject.AddComponent<Rigidbody>();
             simCollisionsChild.kinematicRigidbody.isKinematic=true;
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
             simCollisionsChild.kinematicRigidbody=simCollisionsChild.gameObject.AddComponent<Rigidbody>();
             simCollisionsChild.kinematicRigidbody.isKinematic=true;
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
           var values=GetCapsuleValuesForCollisionTesting(capsule,transform.root,new Vector3(.99f,.99f,.99f));
           trigger.isTrigger=true;
           trigger.direction=values.enumDirection;
           trigger.height=values.height;
           trigger.radius=values.radius;
           trigger.center=values.center;
           triggers.Add(trigger);
          }else if(volumeCollider is BoxCollider box){
           BoxCollider trigger=this.gameObject.AddComponent<BoxCollider>();
           var values=GetBoxValuesForCollisionTesting(box,transform.root,new Vector3(.99f,.99f,.99f));
           trigger.isTrigger=true;
           trigger.size=values.halfExtents*2f;
           trigger.center=values.center;
           triggers.Add(trigger);
          }
         }
        }
        #region Get Values For Collision Testing
            #region CharacterController
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
                )GetCapsuleValuesForCollisionTesting(CharacterController capsule,Transform transform,Vector3 applyScale=default(Vector3),Vector3 applyOffset=default(Vector3)){
                 var direction=Vector3.up;
                 //Log.DebugMessage("capsule direction:"+direction);
                 var offset=capsule.height/2f-capsule.radius;
                 if(applyScale.y>0f){
                  offset*=applyScale.y;
                 }
                 var localPoint0=capsule.center-direction*offset;
                 var localPoint1=capsule.center+direction*offset;
                 var point0=transform.TransformPoint(localPoint0);point0+=applyOffset;
                 var point1=transform.TransformPoint(localPoint1);point1+=applyOffset;
                 float radius=capsule.radius;
                 if(applyScale.x>0f||applyScale.z>0f){
                  radius*=Mathf.Max(applyScale.x,applyScale.z);
                 }
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
            #region Capsule
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
                )GetCapsuleValuesForCollisionTesting(CapsuleCollider capsule,Transform transform,Vector3 applyScale=default(Vector3),Vector3 applyOffset=default(Vector3)){
                 var direction=new Vector3{[capsule.direction]=1};
                 //Log.DebugMessage("capsule direction:"+direction);
                 var offset=capsule.height/2f-capsule.radius;
                 if(applyScale.y>0f){
                  offset*=applyScale.y;
                 }
                 var localPoint0=capsule.center-direction*offset;
                 var localPoint1=capsule.center+direction*offset;
                 var point0=transform.TransformPoint(localPoint0);point0+=applyOffset;
                 var point1=transform.TransformPoint(localPoint1);point1+=applyOffset;
                 Vector3 r=transform.TransformVector(capsule.radius,capsule.radius,capsule.radius);
                 float radius=Enumerable.Range(0,3).Select(xyz=>xyz==capsule.direction?0:r[xyz]).Select(Mathf.Abs).Max();
                 if(applyScale.x>0f||applyScale.z>0f){
                  radius*=Mathf.Max(applyScale.x,applyScale.z);
                 }
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
            #endregion
            #region Box
                internal(
                 Vector3 center,
                 Vector3 halfExtents,
                 Vector3 position,
                 Quaternion orientation
                )GetBoxValuesForCollisionTesting(BoxCollider box,Transform transform,Vector3 applyScale=default(Vector3),Vector3 applyOffset=default(Vector3)){
                 return(
                  box.center,
                  (applyScale.x<=0f||applyScale.y<=0f||applyScale.z<=0f)?box.bounds.extents:Vector3.Scale(box.bounds.extents,applyScale),
                  transform.TransformPoint(box.center)+applyOffset,
                  transform.rotation
                 );
                }
            #endregion
        #endregion
        internal void GetOverlapCollisions(Dictionary<Collider,SimObject>colliders){
         foreach(Collider overlapCollider in simObjectColliders){
          SimObject overlapSimObject=overlapCollider.GetComponentInParent<SimObject>();
          if(overlapSimObject!=null&&!overlapSimObject.IgnoreOverlaps()){
           colliders[overlapCollider]=overlapSimObject;
          }
         }
        }
     [NonSerialized]private Collider[]overlapsColliders=new Collider[8];
        internal void GetOverlapsNonAlloc(Dictionary<Collider,SimObject>colliders){
         for(int i=0;i<simObject.volumeColliders.Count;++i){
          int overlappingsLength=0;
          if(simObject.volumeColliders[i]is CapsuleCollider capsule){
           var values=GetCapsuleValuesForCollisionTesting(capsule,transform.root,new Vector3(.99f,.99f,.99f));
           _GetOverlappedColliders:{
            overlappingsLength=Physics.OverlapCapsuleNonAlloc(
             values.point0,
             values.point1,
             values.radius,
             overlapsColliders,
             -1,
             QueryTriggerInteraction.Collide
            );
           }
           if(overlappingsLength>0){
            if(overlappingsLength>=overlapsColliders.Length){
             Array.Resize(ref overlapsColliders,overlappingsLength*2);
             goto _GetOverlappedColliders;
            }
            AddOverlaps();
           }
          }else if(simObject.volumeColliders[i]is BoxCollider box){
           var values=GetBoxValuesForCollisionTesting(box,transform.root,new Vector3(.99f,.99f,.99f));
           //Log.DebugMessage("box collider:"+values.position+"=="+transform.root.position);
           Debug.DrawLine(values.position,transform.root.position,Color.red,10f);
           _GetOverlappedColliders:{
            overlappingsLength=Physics.OverlapBoxNonAlloc(
             values.position,
             values.halfExtents,
             overlapsColliders,
             values.orientation,
             -1,
             QueryTriggerInteraction.Collide
            );
           }
           if(overlappingsLength>0){
            if(overlappingsLength>=overlapsColliders.Length){
             Array.Resize(ref overlapsColliders,overlappingsLength*2);
             goto _GetOverlappedColliders;
            }
            AddOverlaps();
           }
          }
          void AddOverlaps(){
           for(int j=0;j<overlappingsLength;++j){
            Collider overlapCollider=overlapsColliders[j];
            if(overlapCollider.transform.root!=this.transform.root){//  it's not myself
             SimObject overlapSimObject=overlapCollider.GetComponentInParent<SimObject>();
             if(overlapSimObject!=null&&!overlapSimObject.IgnoreOverlaps()){
              colliders[overlapCollider]=overlapSimObject;
             }
            }
           }
          }
         }
        }
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
         if(!Core.singleton.isServer){
          return;
         }
         if(other.transform.root==this.transform.root){
          return;
         }
         //Log.DebugMessage("SimCollisions:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
         if(IsValidForCollision(other,out SimObject otherSimObject)){
          //Log.DebugMessage("SimCollisions:OnTriggerEnter:SimObjectVolume:"+this.transform.root.gameObject.name+"-> collision <-"+other.transform.root.gameObject.name);
          simObjectColliders.Add(other);
          if(!otherSimObject.simCollisions.collidedWith.ContainsKey(simObject)){
           otherSimObject.simCollisions.collidedWith.Add(simObject,0);
          }else{
           otherSimObject.simCollisions.collidedWith[simObject]++;
          }
          simObject.OnCollision(other,otherSimObject);
         }
        }
        void OnTriggerExit(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         //Log.DebugMessage("SimCollisions:OnTriggerExit:"+other.transform.root.gameObject.name);
         simObjectColliders.Remove(other);
         if(IsValidForCollision(other,out SimObject otherSimObject)){
          if(otherSimObject.simCollisions.collidedWith.ContainsKey(simObject)){
           otherSimObject.simCollisions.collidedWith[simObject]--;
           if(otherSimObject.simCollisions.collidedWith[simObject]<0){
            otherSimObject.simCollisions.collidedWith.Remove(simObject);
           }
          }
         }
        }
        internal bool IsValidForCollision(Collider other,out SimObject otherSimObject){
         if(other.CompareTag("SimObjectVolume")&&!other.isTrigger&&(otherSimObject=other.GetComponentInParent<SimObject>())!=null&&otherSimObject.simCollisions!=null){
          return true;
         }
         otherSimObject=null;
         return false;
        }
    }
}