#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimAnimatorIKController:MonoBehaviour{
     internal SimAnimatorController animatorController;
     internal Vector3 aimAtTorsoAdjust=new Vector3(18.315f,7.08f,0f);
     internal float headMaxVerticalRotationAngle{get{return animatorController.actor.characterController.headMaxVerticalRotationAngle;}}
      internal float headMaxHorizontalRotationAngle{get{return animatorController.actor.characterController.headMaxHorizontalRotationAngle;}}
     bool initialized=false;
     internal Transform      head{get{return animatorController.actor.head;}}
     internal Vector3PosLerpHelper headLookAtPositionLerp=new Vector3PosLerpHelper(76.0f*20f,0.0005f/20f);//  [speed alterada em outro lugar:]
      internal Vector3 headLookAtPositionLerped;
     internal void ResetHeadLookAtPositionLerpSpeed(){
     }
     internal Transform  leftFoot{get{return animatorController.actor. leftFoot;}}
     internal Transform rightFoot{get{return animatorController.actor.rightFoot;}}
      internal float footHeight=.075f;
     float headOnIKRotationStoppedCooldown=.1f;
      float headIKRotationStoppedTimer=0f;
     internal float  leftFootIKPositionWeight;
     internal float  leftFootIKRotationWeight;
      internal FloatLerpHelper  leftFootIKPositionWeightLerp=new FloatLerpHelper();
      internal FloatLerpHelper  leftFootIKRotationWeightLerp=new FloatLerpHelper();
     internal float rightFootIKPositionWeight;
     internal float rightFootIKRotationWeight;
      internal FloatLerpHelper rightFootIKPositionWeightLerp=new FloatLerpHelper();
      internal FloatLerpHelper rightFootIKRotationWeightLerp=new FloatLerpHelper();
     bool wasAiming;
        //  [https://forum.unity.com/threads/setikrotation-for-feet-on-slope.510931/]
        void OnAnimatorIK(int layerIndex){
         if(!initialized){
          if(head!=null){
           //Log.DebugMessage("OnAnimatorIK:found head Bone");
          }
          if(leftFoot!=null&&rightFoot!=null){
           //Log.DebugMessage("OnAnimatorIK:found feet Bones");
          }
          headLookAtPositionLerped=headLookAtPositionLerp.tgtPos=headLookAtPositionLerp.tgtPos_Last=animatorController.actor.characterController.character.transform.position+(animatorController.actor.characterController.character.transform.rotation*animatorController.actor.characterController.headOffset)+animatorController.actor.characterController.character.transform.forward;
          initialized=true;
         }
         void OnStartedAiming(){
          ResetHeadLookAtPositionLerpSpeed();
         }
         void OnStoppedAiming(){
          ResetHeadLookAtPositionLerpSpeed();
         }
         if(animatorController.actor.characterController.isAiming){
          if(!wasAiming){
           OnStartedAiming();
          }
         }else{
          if(wasAiming){
           OnStoppedAiming();
          }
         }
         var characterController=animatorController.actor.characterController.character;
         var headOffset=characterController.transform.rotation*animatorController.actor.characterController.headOffset;
         var aimAtMaxDistance=animatorController.actor.characterController.aimAtMaxDistance;
         var rotForAim=animatorController.actor.characterController.viewRotationForAiming;
         Vector3 aimAt=animatorController.actor.characterController.aimingAt;
         bool aiming=false;
         if(animatorController.actor.characterController.isAiming||head==null){
          aiming=true;
         }
         //float bodyToHeadRotationXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(characterController.transform.rotation,rotForAim);
         //if(Mathf.Abs(bodyToHeadRotationXComponentSignedAngle)>=10f){
         // float angleToRotate=Mathf.Abs(bodyToHeadRotationXComponentSignedAngle)-10f;
         // angleToRotate*=Mathf.Sign(bodyToHeadRotationXComponentSignedAngle);
         // rotForAim=Quaternion.AngleAxis(angleToRotate,characterController.transform.rotation*Vector3.right)*rotForAim;
         //}
         //float bodyToHeadRotationYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(characterController.transform.rotation,rotForAim);
         //if(Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)>=20f){
         // float angleToRotate=Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)-20f;
         // angleToRotate*=Mathf.Sign(bodyToHeadRotationYComponentSignedAngle);
         // rotForAim=Quaternion.AngleAxis(-angleToRotate,characterController.transform.rotation*Vector3.up)*rotForAim;
         //}
         //if(aiming){
         // if(animatorController.actor.isUsingAI&&animatorController.actor.enemy!=null){
         // }else{
         //  var r=rotForAim*Quaternion.Euler(aimAtTorsoAdjust);
         //  rotForAim=r;
         // }
         //}
         //if(rotForAim!=animatorController.actor.characterController.viewRotationForAiming){
         // aimAt=characterController.transform.position+headOffset+rotForAim*Vector3.forward*aimAtMaxDistance;
         //}
         ////  [https://discussions.unity.com/t/upper-torso-ik/133564/2]:m_Anim.SetLookAtWeight(m_LookWeight,m_BodyWeight,m_HeadWeight,m_EyesWeight,m_ClampWeight);
         //if(aiming){
         // animatorController.animator.SetLookAtWeight(1f,1f,1f,1f,0.0f);
         //}else{
         // animatorController.animator.SetLookAtWeight(1f,0f,1f,1f,0.0f);
         //}
         //headLookAtPositionLerp.tgtPos=aimAt;
         //headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Core.magicDeltaTimeNumber);
         //float bodyToHeadLookAtYSignedAngle=Vector3.SignedAngle(animatorController.animator.transform.forward,headLookAtPositionLerped.normalized,Vector3.up);
         //float bodyToHeadLookAtXSignedAngle=Vector3.SignedAngle(animatorController.animator.transform.forward,headLookAtPositionLerped.normalized,Vector3.right);
         //if(
         // Mathf.Abs(bodyToHeadLookAtYSignedAngle)>=headMaxHorizontalRotationAngle||
         // Mathf.Abs(bodyToHeadLookAtXSignedAngle)>=headMaxVerticalRotationAngle
         //){
         // headLookAtPositionLerped=headLookAtPositionLerp.EndPosition();
         //}
         //animatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
         #region feet IK
             if(leftFoot!=null&&rightFoot!=null){
              //float disBetweenFeet=(leftFoot.position-rightFoot.position).magnitude;
              ////Log.DebugMessage("disBetweenFeet:"+disBetweenFeet);
              //Vector3 leftFootIKEuler=leftFoot.eulerAngles;
              //Quaternion leftFootIKRotation=leftFoot.rotation;
              //Vector3 leftFootIKPosition=new Vector3(
              // leftFoot.position.x,
              // leftFoot.position.y,
              // leftFoot.position.z
              //);
              //Vector3 leftToFloorRaycastOrigin=animatorController.actor.transform.position+(animatorController.actorLeft*(disBetweenFeet/2f));
              //if(Physics.Raycast(leftToFloorRaycastOrigin,Vector3.down,out RaycastHit leftToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
              // leftFootIKPosition.y=leftToFloorHit.point.y+footHeight;
              // Vector3 slopeCorrected=-Vector3.Cross(leftToFloorHit.normal,animatorController.animator.transform.right);
              // leftFootIKRotation=Quaternion.LookRotation(slopeCorrected,leftToFloorHit.normal);
              // Debug.DrawRay(leftFootIKPosition,-animatorController.animator.transform.right,Color.blue);
              // Quaternion leftFootIKRotationClamped;
              // leftFootIKRotationClamped=RotationHelper.Clamp(
              //  leftFootIKRotation,
              //  animatorController.actor.characterController.character.transform.rotation,
              //  new Vector3(5f,15f,5f),
              //  new Vector3(5f,15f,5f)
              // );
              // leftFootIKRotation=leftFootIKRotationClamped;
              // Debug.DrawRay(leftFootIKPosition,leftFootIKRotation*-Vector3.right,Color.green);
              // //Debug.DrawRay(leftToFloorHit.point,leftToFloorHit.normal);
              //}
              //Vector3 rightFootIKEuler=rightFoot.eulerAngles;
              //Quaternion rightFootIKRotation=rightFoot.rotation;
              //Vector3 rightFootIKPosition=new Vector3(
              // rightFoot.position.x,
              // rightFoot.position.y,
              // rightFoot.position.z
              //);
              //Vector3 rightToFloorRaycastOrigin=animatorController.actor.transform.position+(animatorController.actorRight*(disBetweenFeet/2f));
              //if(Physics.Raycast(rightToFloorRaycastOrigin,Vector3.down,out RaycastHit rightToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
              // rightFootIKPosition.y=rightToFloorHit.point.y+footHeight;
              // Vector3 slopeCorrected=-Vector3.Cross(rightToFloorHit.normal,animatorController.animator.transform.right);
              // rightFootIKRotation=Quaternion.LookRotation(slopeCorrected,rightToFloorHit.normal);
              // Debug.DrawRay(rightFootIKPosition,animatorController.animator.transform.right,Color.blue);
              // Quaternion rightFootIKRotationClamped;
              // rightFootIKRotationClamped=RotationHelper.Clamp(
              //  rightFootIKRotation,
              //  animatorController.actor.characterController.character.transform.rotation,
              //  new Vector3(5f,15f,5f),
              //  new Vector3(5f,15f,5f)
              // );
              // rightFootIKRotation=rightFootIKRotationClamped;
              // Debug.DrawRay(rightFootIKPosition,rightFootIKRotation*Vector3.right,Color.green);
              // //Debug.DrawRay(rightToFloorHit.point,rightToFloorHit.normal);
              //}
              //if(animatorController.actor.motion==BaseAI.ActorMotion.MOTION_STAND||
              //   animatorController.actor.motion==BaseAI.ActorMotion.MOTION_STAND_RIFLE
              //){
              //  leftFootIKPositionWeightLerp.tgtVal=1f;
              //  leftFootIKRotationWeightLerp.tgtVal=1f;
              // rightFootIKPositionWeightLerp.tgtVal=1f;
              // rightFootIKRotationWeightLerp.tgtVal=1f;
              //}else{
              //  leftFootIKPositionWeightLerp.tgtVal=0f;
              //  leftFootIKRotationWeightLerp.tgtVal=0f;
              // rightFootIKPositionWeightLerp.tgtVal=0f;
              // rightFootIKRotationWeightLerp.tgtVal=0f;
              // if(animatorController.actor.motion==BaseAI.ActorMotion.MOTION_HIT||
              //    animatorController.actor.motion==BaseAI.ActorMotion.MOTION_HIT_RIFLE
              // ){
              //   leftFootIKPositionWeight= leftFootIKPositionWeightLerp.EndFloat();
              //   leftFootIKRotationWeight= leftFootIKRotationWeightLerp.EndFloat();
              //  rightFootIKPositionWeight=rightFootIKPositionWeightLerp.EndFloat();
              //  rightFootIKRotationWeight=rightFootIKRotationWeightLerp.EndFloat();
              // }
              //}
              // leftFootIKPositionWeight= leftFootIKPositionWeightLerp.UpdateFloat( leftFootIKPositionWeight,Core.magicDeltaTimeNumber);
              // leftFootIKRotationWeight= leftFootIKRotationWeightLerp.UpdateFloat( leftFootIKRotationWeight,Core.magicDeltaTimeNumber);
              //rightFootIKPositionWeight=rightFootIKPositionWeightLerp.UpdateFloat(rightFootIKPositionWeight,Core.magicDeltaTimeNumber);
              //rightFootIKRotationWeight=rightFootIKRotationWeightLerp.UpdateFloat(rightFootIKRotationWeight,Core.magicDeltaTimeNumber);
              //float height=animatorController.actor.height;
              //Vector3 heightLimit=animatorController.actor.transform.position+Vector3.down*(height*.5f);
              //#region LeftFoot
              //    animatorController.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,leftFootIKPositionWeight);
              //    animatorController.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,leftFootIKRotationWeight);
              //    leftFootIKPosition.y=Mathf.Min(heightLimit.y,leftFootIKPosition.y);
              //    animatorController.animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootIKPosition);
              //    animatorController.animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootIKRotation);
              //#endregion
              //#region RightFoot
              //    animatorController.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,rightFootIKPositionWeight);
              //    animatorController.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,rightFootIKRotationWeight);
              //    rightFootIKPosition.y=Mathf.Min(heightLimit.y,rightFootIKPosition.y);
              //    animatorController.animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootIKPosition);
              //    animatorController.animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootIKRotation);
              //#endregion
             }
         #endregion
         wasAiming=animatorController.actor.characterController.isAiming;
        }
    }
}