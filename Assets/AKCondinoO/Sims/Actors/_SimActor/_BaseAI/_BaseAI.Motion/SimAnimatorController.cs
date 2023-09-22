#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid;
using AKCondinoO.Sims.Actors.Humanoid.Human;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
using static AKCondinoO.Sims.Actors.SimActor;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimAnimatorController:MonoBehaviour{
     internal BaseAI actor;
      internal Vector3 actorLeft;
      internal Vector3 actorRight;
     internal Animator animator;
     internal SimAnimatorIKController animatorIKController;
     [SerializeField]TransformAdjustment[]transformAdjustments;
      internal readonly Dictionary<ActorMotion,TransformAdjustment>motionTransformAdjustment=new Dictionary<ActorMotion,TransformAdjustment>();
        [Serializable]internal class TransformAdjustment{
         [SerializeField]internal ActorMotion motion;
         [SerializeField]internal Transform adjustment;
        }
     [SerializeField]internal           QuaternionRotLerpHelper rotLerp=new           QuaternionRotLerpHelper();
     [SerializeField]internal Vector3PosComponentwiseLerpHelper posLerp=new Vector3PosComponentwiseLerpHelper();
        void Awake(){
         foreach(TransformAdjustment transformAdjustment in transformAdjustments){
          if(transformAdjustment.adjustment!=null){
           motionTransformAdjustment.Add(transformAdjustment.motion,transformAdjustment);
          }
         }
        }
        internal virtual void ManualUpdate(){
         GetAnimator();
         if(animator!=null){
          GetTransformTgtValuesFromCharacterController();
          SetTransformTgtValuesUsingActorAndPhysicData();
          if(actor.simUMA!=null){
           SetSimUMADataTransform();
          }
          GetAnimatorStateInfo();
          UpdateMotion(actor);
         }
        }
     internal int layerCount{get;private set;}
     internal readonly Dictionary<int,string>layerIndexToName=new Dictionary<int,string>();
     internal Dictionary<WeaponTypes,int>weaponLayer{get;private set;}
      internal Dictionary<WeaponTypes,int>weaponAimLayer{get;private set;}
        protected virtual void GetAnimator(){
         if(animator==null){
          animator=GetComponentInChildren<Animator>();
          if(animator!=null){
           Log.DebugMessage("add SimActorAnimatorIKController");
           animatorIKController=animator.gameObject.AddComponent<SimAnimatorIKController>();
           animatorIKController.animatorController=this;
           animatorIKController.headLookAtPositionLerp.tgtPosLerpSpeed=Mathf.Max(
            rotLerp.tgtRotLerpSpeed+1f,
            posLerp.xLerp.tgtValLerpSpeed+1f,
            posLerp.yLerp.tgtValLerpSpeed+1f,
            posLerp.zLerp.tgtValLerpSpeed+1f,
            46.875f
           );
           layerCount=animator.layerCount;
           weaponLayer=new Dictionary<WeaponTypes,int>(layerCount);
            weaponAimLayer=new Dictionary<WeaponTypes,int>(layerCount);
           animationTime=new Dictionary<int,float>(layerCount);
            animationTimeInCurrentLoop=new Dictionary<int,float>(layerCount);
           normalizedTime=new Dictionary<int,float>(layerCount);
            normalizedTimeInCurrentLoop=new Dictionary<int,float>(layerCount);
           loopCount=new Dictionary<int,int>(layerCount);
            lastLoopCount=new Dictionary<int,int>(layerCount);
             looped=new Dictionary<int,bool>(layerCount);
           animatorClip=new Dictionary<int,List<AnimatorClipInfo>>(layerCount);
           currentClipInstanceID=new Dictionary<int,int>(layerCount);
            currentClipName=new Dictionary<int,string>(layerCount);
           for(int i=0;i<layerCount;++i){
            animationTime[i]=0f;
             animationTimeInCurrentLoop[i]=0f;
            normalizedTime[i]=0f;
             normalizedTimeInCurrentLoop[i]=0f;
            loopCount[i]=0;
             lastLoopCount[i]=0;
              looped[i]=false;
            animatorClip[i]=new List<AnimatorClipInfo>();
            currentClipInstanceID[i]=0;
             currentClipName[i]="";
           }
           int GetLayer(string layerName){
            int layerIndex=animator.GetLayerIndex(layerName);
            layerIndexToName[layerIndex]=layerName;
            return layerIndex;
           }
           foreach(WeaponLayer weaponLayerName in weaponLayerNames){
               weaponLayer[   weaponLayerName.weaponType]=GetLayer(   weaponLayerName.layerName);
            Log.DebugMessage(   "weaponLayer["+   weaponLayerName.weaponType+"]="+   weaponLayerName.layerName);
           }
           foreach(WeaponAimLayer weaponAimLayerName in weaponAimLayerNames){
            weaponAimLayer[weaponAimLayerName.weaponType]=GetLayer(weaponAimLayerName.layerName);
            Log.DebugMessage("weaponAimLayer["+weaponAimLayerName.weaponType+"]="+weaponAimLayerName.layerName);
           }
           layerTransitionCoroutine=StartCoroutine(LayerTransition());
           AddAnimationEventsHandler();
           if(actor.simUMA!=null){
            actor.simUMA.transform.parent.SetParent(null);
            GetTransformTgtValuesFromCharacterController();
            rotLerp.tgtRot_Last=rotLerp.tgtRot;
            posLerp.tgtPos_Last=posLerp.tgtPos;
            actor.simUMA.transform.parent.rotation=rotLerp.tgtRot;
            actor.simUMA.transform.parent.position=posLerp.tgtPos;
           }
          }
         }
        }
     [SerializeField]float layerTransitionSpeed=64.0f;
     Coroutine layerTransitionCoroutine;
      internal readonly Dictionary<int,float>layerTargetWeight=new Dictionary<int,float>();
       internal readonly Dictionary<int,float>layerWeight=new Dictionary<int,float>();
        IEnumerator LayerTransition(){
            Loop:{
             foreach(var layer in layerTargetWeight){
              int layerIndex=layer.Key;
              float targetWeight=layer.Value;
              if(!layerWeight.TryGetValue(layerIndex,out float weight)){
               weight=layerWeight[layerIndex]=animator.GetLayerWeight(layerIndex);
              }
              if(weight!=targetWeight){
               if(weight>targetWeight){
                weight-=layerTransitionSpeed*Core.magicDeltaTimeNumber;
                if(weight<=targetWeight){
                 weight=targetWeight;
                }
               }else if(weight<targetWeight){
                weight+=layerTransitionSpeed*Core.magicDeltaTimeNumber;
                if(weight>=targetWeight){
                 weight=targetWeight;
                }
               }
               animator.SetLayerWeight(layerIndex,layerWeight[layerIndex]=weight);
              }
             }
             yield return null;
            }
            goto Loop;
        }
        protected virtual void GetTransformTgtValuesFromCharacterController(){
         if(actor. leftEye!=null){
          Debug.DrawLine(actor. leftEye.transform.position,actor.characterController.aimingAt,Color.red);
         }
         if(actor.rightEye!=null){
          Debug.DrawLine(actor.rightEye.transform.position,actor.characterController.aimingAt,Color.red);
         }
         Quaternion rotAdjustment=Quaternion.identity;
         if(motionTransformAdjustment.TryGetValue(actor.motion,out TransformAdjustment adjustment)){
          rotAdjustment=adjustment.adjustment.localRotation;
         }
         rotLerp.tgtRot=Quaternion.Euler(actor.characterController.character.transform.eulerAngles+new Vector3(0f,180f,0f))*rotAdjustment;
         posLerp.tgtPos=actor.characterController.character.transform.position+actor.simUMAPosOffset;
        }
        protected virtual void SetTransformTgtValuesUsingActorAndPhysicData(){
         actorLeft=-actor.transform.right;
         actorRight=actor.transform.right;
         Vector3 boundsMaxRight=actor.characterController.character.bounds.max;
                 boundsMaxRight.y=actor.characterController.character.transform.position.y;
                 boundsMaxRight.z=actor.characterController.character.transform.position.z;
         float maxRightDis=Vector3.Distance(actor.characterController.character.transform.position,boundsMaxRight);
         Vector3 maxRight=actor.characterController.character.transform.position+actor.characterController.character.transform.rotation*(Vector3.right*maxRightDis);
         Vector3 boundsMinLeft=actor.characterController.character.bounds.min;
                 boundsMinLeft.y=actor.characterController.character.transform.position.y;
                 boundsMinLeft.z=actor.characterController.character.transform.position.z;
         float minLeftDis=Vector3.Distance(actor.characterController.character.transform.position,boundsMinLeft);
         Vector3 minLeft=actor.characterController.character.transform.position+actor.characterController.character.transform.rotation*(Vector3.left*minLeftDis);
         if(actor.navMeshAgent.enabled||actor.characterController.isGrounded){
          Debug.DrawRay(maxRight,Vector3.down,Color.blue);
          if(Physics.Raycast(maxRight,Vector3.down,out RaycastHit rightFloorHit,2f,PhysUtil.considerGroundLayer)){
           Debug.DrawRay(rightFloorHit.point,rightFloorHit.normal);
           Vector3 bottom=actor.characterController.character.bounds.center;
                   bottom.y=actor.characterController.character.bounds.min.y;
           Plane floorPlane=new Plane(rightFloorHit.normal,bottom);
           Ray leftRay=new Ray(minLeft,Vector3.down);
           Debug.DrawRay(leftRay.origin,leftRay.direction,Color.blue);
           if(floorPlane.Raycast(leftRay,out float enter)){
            Vector3 leftFloorHitPoint=leftRay.GetPoint(enter);
            float minY=Mathf.Min(bottom.y,leftFloorHitPoint.y,rightFloorHit.point.y);
            posLerp.tgtPos.y+=minY-bottom.y;
            Debug.DrawLine(bottom,posLerp.tgtPos,Color.yellow);
           }
          }
         }
        }
        protected virtual void SetSimUMADataTransform(){
         actor.simUMA.transform.parent.rotation=rotLerp.UpdateRotation(actor.simUMA.transform.parent.rotation,Core.magicDeltaTimeNumber);
         actor.simUMA.transform.parent.position=posLerp.UpdatePosition(actor.simUMA.transform.parent.position,Core.magicDeltaTimeNumber);
        }
        void LateUpdate(){
        }
    }
}