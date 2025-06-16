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
     internal Vector3PosLerpHelper headLookAtPositionLerp=new Vector3PosLerpHelper(38.0f,0.5f);//  [speed alterada em outro lugar:]
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
         CharacterController character=animatorController.actor.characterController.character;
         Vector3 headOffset=character.transform.rotation*animatorController.actor.characterController.headOffset;
         float aimAtMaxDistance=animatorController.actor.characterController.aimAtMaxDistance;
         Vector3 aimingAt=animatorController.actor.characterController.aimingAt;
         bool aiming=false;
         if(animatorController.actor.characterController.isAiming||head==null){
          aiming=true;
         }
         Quaternion rotToLook=Quaternion.identity;
         Vector3 whereToLook=aimingAt;
         Transform bodyRotationTransform;
         if(animatorController.actor.simUMA!=null){
          bodyRotationTransform=animatorController.actor.simUMA.transform;
         }else{
          bodyRotationTransform=character.transform;
         }
         Vector3 headPos=character.transform.position+headOffset;
         //Log.DebugMessage("headOffset:"+headOffset+";bodyRotationTransform.position:"+bodyRotationTransform.position+";headPos:"+headPos);
         Vector3 characterForward=bodyRotationTransform.forward;
         Vector3 lookForward=(whereToLook-headPos).normalized;
         Vector3 up=Vector3.Cross(
          -bodyRotationTransform.right,
          characterForward
         );
         Vector3 upToRotateAroundY=up;
         Vector3Util.GetBestUpAndProjectionsOnPlane(
          characterForward,lookForward,
           ref up,
            out Vector3 projCharacterForwardOnUp,out Vector3 projLookForwardOnUp,
             true
         );
         //Debug.DrawLine(headPos,headPos+projCharacterForwardOnUp,Color.blue);
         //Debug.DrawLine(headPos,headPos+projLookForwardOnUp,Color.green);
         //Debug.DrawLine(headPos,headPos+up,Color.yellow);
         float bodyToHeadRotationYComponentSignedAngle=Vector3.SignedAngle(projCharacterForwardOnUp,projLookForwardOnUp,up);
         if(!Mathf.Approximately(bodyToHeadRotationYComponentSignedAngle,0f)){
          //Log.DebugMessage("unclamped bodyToHeadRotationYComponentSignedAngle:"+bodyToHeadRotationYComponentSignedAngle);
          if(up!=upToRotateAroundY){
           bodyToHeadRotationYComponentSignedAngle=0f;
          }else{
           bodyToHeadRotationYComponentSignedAngle=Mathf.Clamp(bodyToHeadRotationYComponentSignedAngle,-40f,40f);
          }
         }
         up=Vector3.Cross(
          upToRotateAroundY,
          characterForward
         );
         Vector3 upToRotateAroundX=up;
         Vector3Util.GetBestUpAndProjectionsOnPlane(
          characterForward,lookForward,
           ref up,
            out projCharacterForwardOnUp,out projLookForwardOnUp,
             true
         );
         //Debug.DrawLine(headPos,headPos+projCharacterForwardOnUp,Color.blue);
         //Debug.DrawLine(headPos,headPos+projLookForwardOnUp,Color.green);
         //Debug.DrawLine(headPos,headPos+up,Color.yellow);
         float bodyToHeadRotationXComponentSignedAngle=Vector3.SignedAngle(projCharacterForwardOnUp,projLookForwardOnUp,up);
         if(!Mathf.Approximately(bodyToHeadRotationXComponentSignedAngle,0f)){
          //Log.DebugMessage("unclamped bodyToHeadRotationXComponentSignedAngle:"+bodyToHeadRotationXComponentSignedAngle);
          if(up!=upToRotateAroundX){
           bodyToHeadRotationXComponentSignedAngle=0f;
          }else{
           bodyToHeadRotationXComponentSignedAngle=Mathf.Clamp(bodyToHeadRotationXComponentSignedAngle,-40f,40f);
          }
         }
         rotToLook=
          Quaternion.AngleAxis(bodyToHeadRotationXComponentSignedAngle,upToRotateAroundX)*
          Quaternion.AngleAxis(bodyToHeadRotationYComponentSignedAngle,upToRotateAroundY);
         if(aiming){
          if(animatorController.actor.isUsingAI&&animatorController.actor.enemy!=null){
          }else{
           rotToLook=Quaternion.Euler(aimAtTorsoAdjust)*rotToLook;
          }
         }
         if(rotToLook!=Quaternion.identity){
          rotToLook=rotToLook*bodyRotationTransform.rotation;
          whereToLook=headPos+(rotToLook*Vector3.forward*aimAtMaxDistance);
         }
         Debug.DrawLine(headPos,whereToLook,Color.white);
         headLookAtPositionLerp.tgtPos=whereToLook;
         headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Core.magicDeltaTimeNumber);
         //float bodyToHeadLookAtYSignedAngle=Vector3.SignedAngle(animatorController.animator.transform.forward,headLookAtPositionLerped.normalized,Vector3.up);
         //float bodyToHeadLookAtXSignedAngle=Vector3.SignedAngle(animatorController.animator.transform.forward,headLookAtPositionLerped.normalized,Vector3.right);
         //if(
         // Mathf.Abs(bodyToHeadLookAtYSignedAngle)>=headMaxHorizontalRotationAngle||
         // Mathf.Abs(bodyToHeadLookAtXSignedAngle)>=headMaxVerticalRotationAngle
         //){
         // headLookAtPositionLerped=headLookAtPositionLerp.EndPosition();
         //}
         //  [https://discussions.unity.com/t/upper-torso-ik/133564/2]:
         // m_Anim.SetLookAtWeight(m_LookWeight,m_BodyWeight,m_HeadWeight,m_EyesWeight,m_ClampWeight);
         if(aiming){
          animatorController.animator.SetLookAtWeight(1f,1f,1f,1f,0.0f);
         }else{
          animatorController.animator.SetLookAtWeight(1f,0f,1f,1f,0.0f);
         }
         animatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
         //Debug.DrawLine(headPos,headLookAtPositionLerped,Color.white);
         #region feet IK
             if(leftFoot!=null&&rightFoot!=null){
              float disBetweenFeet=(leftFoot.position-rightFoot.position).magnitude;
              //Log.DebugMessage("disBetweenFeet:"+disBetweenFeet);
              Quaternion leftFootIKRotation=bodyRotationTransform.rotation;
              Vector3    leftFootIKPosition=new Vector3(
               leftFoot.position.x,
               leftFoot.position.y,
               leftFoot.position.z
              );
              Vector3 leftToFloorRaycastOrigin=character.transform.position+(-bodyRotationTransform.right*(disBetweenFeet/2f));
              if(Physics.Raycast(leftToFloorRaycastOrigin,Vector3.down,out RaycastHit leftToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
               leftFootIKPosition.y=leftToFloorHit.point.y+footHeight;
               //  direção forward com inclinação da rampa para o rotacionar o pé:
               Vector3 slopeCorrected=Vector3.Cross(leftToFloorHit.normal,-bodyRotationTransform.right);
               //Quaternion leftFootIKRotationUnclamped=Quaternion.LookRotation(slopeCorrected,leftToFloorHit.normal);
               Vector3 rotationUp=bodyRotationTransform.up;
               Vector3 upToRotateLeftFootAroundY=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                characterForward,slopeCorrected,
                 ref rotationUp,
                  out Vector3 leftFootProjCharacterForwardOnRotationUp,out Vector3 leftFootProjSlopeCorrectedOnRotationUp,
                   true
               );
               float angleToRotateLeftFootAroundY=Vector3.SignedAngle(leftFootProjCharacterForwardOnRotationUp,leftFootProjSlopeCorrectedOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateLeftFootAroundY:"+angleToRotateLeftFootAroundY);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjCharacterForwardOnRotationUp,Color.blue);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjSlopeCorrectedOnRotationUp,Color.green);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+rotationUp,Color.yellow);
               angleToRotateLeftFootAroundY=Mathf.Clamp(angleToRotateLeftFootAroundY,-30f,30f);
               rotationUp=bodyRotationTransform.right;
               Vector3 upToRotateLeftFootAroundX=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                characterForward,slopeCorrected,
                 ref rotationUp,
                  out leftFootProjCharacterForwardOnRotationUp,out leftFootProjSlopeCorrectedOnRotationUp,
                   true
               );
               float angleToRotateLeftFootAroundX=Vector3.SignedAngle(leftFootProjCharacterForwardOnRotationUp,leftFootProjSlopeCorrectedOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateLeftFootAroundX:"+angleToRotateLeftFootAroundX);
               angleToRotateLeftFootAroundX=Mathf.Clamp(angleToRotateLeftFootAroundX,-20f,50f);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjCharacterForwardOnRotationUp,Color.blue);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjSlopeCorrectedOnRotationUp,Color.green);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+rotationUp,Color.yellow);
               rotationUp=characterForward;
               Vector3 upToRotateLeftFootAroundZ=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                bodyRotationTransform.up,leftToFloorHit.normal,
                 ref rotationUp,
                  out Vector3 leftFootProjCharacterUpOnRotationUp,out Vector3 leftFootProjHitNormalOnRotationUp,
                   true
               );
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjCharacterUpOnRotationUp,Color.blue);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjHitNormalOnRotationUp,Color.green);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+rotationUp,Color.yellow);
               float angleToRotateLeftFootAroundZ=Vector3.SignedAngle(leftFootProjCharacterUpOnRotationUp,leftFootProjHitNormalOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateLeftFootAroundZ:"+angleToRotateLeftFootAroundZ);
               angleToRotateLeftFootAroundZ=Mathf.Clamp(angleToRotateLeftFootAroundZ,-10f,30f);
               leftFootIKRotation=
                Quaternion.AngleAxis(angleToRotateLeftFootAroundZ,upToRotateLeftFootAroundZ)*//  Z
                Quaternion.AngleAxis(angleToRotateLeftFootAroundX,upToRotateLeftFootAroundX)*//  X
                Quaternion.AngleAxis(angleToRotateLeftFootAroundY,upToRotateLeftFootAroundY)*//  Y
                bodyRotationTransform.rotation;
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootIKRotation*Vector3.forward,Color.blue);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootIKRotation*Vector3.left,Color.red);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootIKRotation*Vector3.up,Color.green);
              }
              Quaternion rightFootIKRotation=bodyRotationTransform.rotation;
              Vector3 rightFootIKPosition=new Vector3(
               rightFoot.position.x,
               rightFoot.position.y,
               rightFoot.position.z
              );
              Vector3 rightToFloorRaycastOrigin=character.transform.position+(bodyRotationTransform.right*(disBetweenFeet/2f));
              if(Physics.Raycast(rightToFloorRaycastOrigin,Vector3.down,out RaycastHit rightToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
               rightFootIKPosition.y=rightToFloorHit.point.y+footHeight;
               //  direção forward com inclinação da rampa para o rotacionar o pé:
               Vector3 slopeCorrected=-Vector3.Cross(rightToFloorHit.normal,animatorController.animator.transform.right);
               //Quaternion rightFootIKRotationUnclamped=Quaternion.LookRotation(slopeCorrected,rightToFloorHit.normal);
               Vector3 rotationUp=bodyRotationTransform.up;
               Vector3 upToRotateRightFootAroundY=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                characterForward,slopeCorrected,
                 ref rotationUp,
                  out Vector3 rightFootProjCharacterForwardOnRotationUp,out Vector3 rightFootProjSlopeCorrectedOnRotationUp,
                   true
               );
               float angleToRotateRightFootAroundY=Vector3.SignedAngle(rightFootProjCharacterForwardOnRotationUp,rightFootProjSlopeCorrectedOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateRightFootAroundY:"+angleToRotateRightFootAroundY);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootProjCharacterForwardOnRotationUp,Color.blue);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootProjSlopeCorrectedOnRotationUp,Color.green);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rotationUp,Color.yellow);
               angleToRotateRightFootAroundY=Mathf.Clamp(angleToRotateRightFootAroundY,-30f,30f);
               rotationUp=bodyRotationTransform.right;
               Vector3 upToRotateRightFootAroundX=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                characterForward,slopeCorrected,
                 ref rotationUp,
                  out rightFootProjCharacterForwardOnRotationUp,out rightFootProjSlopeCorrectedOnRotationUp,
                   true
               );
               float angleToRotateRightFootAroundX=Vector3.SignedAngle(rightFootProjCharacterForwardOnRotationUp,rightFootProjSlopeCorrectedOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateRightFootAroundX:"+angleToRotateRightFootAroundX);
               angleToRotateRightFootAroundX=Mathf.Clamp(angleToRotateRightFootAroundX,-50f,20f);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootProjCharacterForwardOnRotationUp,Color.blue);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootProjSlopeCorrectedOnRotationUp,Color.green);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rotationUp,Color.yellow);
               rotationUp=characterForward;
               Vector3 upToRotateRightFootAroundZ=rotationUp;
               Vector3Util.GetBestUpAndProjectionsOnPlane(
                bodyRotationTransform.up,leftToFloorHit.normal,
                 ref rotationUp,
                  out Vector3 leftFootProjCharacterUpOnRotationUp,out Vector3 leftFootProjHitNormalOnRotationUp,
                   true
               );
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjCharacterUpOnRotationUp,Color.blue);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+leftFootProjHitNormalOnRotationUp,Color.green);
               //Debug.DrawLine(leftFootIKPosition,leftFootIKPosition+rotationUp,Color.yellow);
               float angleToRotateRightFootAroundZ=Vector3.SignedAngle(leftFootProjCharacterUpOnRotationUp,leftFootProjHitNormalOnRotationUp,rotationUp);
               Log.DebugMessage("angleToRotateRightFootAroundZ:"+angleToRotateRightFootAroundZ);
               angleToRotateRightFootAroundZ=Mathf.Clamp(angleToRotateRightFootAroundZ,-30f,10f);
               rightFootIKRotation=
                Quaternion.AngleAxis(angleToRotateRightFootAroundZ,upToRotateRightFootAroundZ)*//  Z
                Quaternion.AngleAxis(angleToRotateRightFootAroundX,upToRotateRightFootAroundX)*//  X
                Quaternion.AngleAxis(angleToRotateRightFootAroundY,upToRotateRightFootAroundY)*//  Y
                bodyRotationTransform.rotation;
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootIKRotation*Vector3.forward,Color.blue);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootIKRotation*Vector3.left,Color.red);
               //Debug.DrawLine(rightFootIKPosition,rightFootIKPosition+rightFootIKRotation*Vector3.up,Color.green);
              }
              if(animatorController.actor.motion==BaseAI.ActorMotion.MOTION_STAND||
                 animatorController.actor.motion==BaseAI.ActorMotion.MOTION_STAND_RIFLE
              ){
                leftFootIKPositionWeightLerp.tgtVal=1f;
                leftFootIKRotationWeightLerp.tgtVal=1f;
               rightFootIKPositionWeightLerp.tgtVal=1f;
               rightFootIKRotationWeightLerp.tgtVal=1f;
              }else{
                leftFootIKPositionWeightLerp.tgtVal=0f;
                leftFootIKRotationWeightLerp.tgtVal=0f;
               rightFootIKPositionWeightLerp.tgtVal=0f;
               rightFootIKRotationWeightLerp.tgtVal=0f;
               if(animatorController.actor.motion==BaseAI.ActorMotion.MOTION_HIT||
                  animatorController.actor.motion==BaseAI.ActorMotion.MOTION_HIT_RIFLE
               ){
                 leftFootIKPositionWeight= leftFootIKPositionWeightLerp.EndFloat();
                 leftFootIKRotationWeight= leftFootIKRotationWeightLerp.EndFloat();
                rightFootIKPositionWeight=rightFootIKPositionWeightLerp.EndFloat();
                rightFootIKRotationWeight=rightFootIKRotationWeightLerp.EndFloat();
               }
              }
               leftFootIKPositionWeight= leftFootIKPositionWeightLerp.UpdateFloat( leftFootIKPositionWeight,Core.magicDeltaTimeNumber);
               leftFootIKRotationWeight= leftFootIKRotationWeightLerp.UpdateFloat( leftFootIKRotationWeight,Core.magicDeltaTimeNumber);
              rightFootIKPositionWeight=rightFootIKPositionWeightLerp.UpdateFloat(rightFootIKPositionWeight,Core.magicDeltaTimeNumber);
              rightFootIKRotationWeight=rightFootIKRotationWeightLerp.UpdateFloat(rightFootIKRotationWeight,Core.magicDeltaTimeNumber);
              float height=animatorController.actor.height;
              Vector3 heightLimit=animatorController.actor.transform.position+Vector3.down*(height*.5f);
              #region LeftFoot
                  animatorController.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,leftFootIKPositionWeight);
                  animatorController.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,leftFootIKRotationWeight);
                  leftFootIKPosition.y=Mathf.Min(heightLimit.y,leftFootIKPosition.y);
                  animatorController.animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootIKPosition);
                  animatorController.animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootIKRotation);
              #endregion
              #region RightFoot
                  animatorController.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,rightFootIKPositionWeight);
                  animatorController.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,rightFootIKRotationWeight);
                  rightFootIKPosition.y=Mathf.Min(heightLimit.y,rightFootIKPosition.y);
                  animatorController.animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootIKPosition);
                  animatorController.animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootIKRotation);
              #endregion
             }
         #endregion
         wasAiming=animatorController.actor.characterController.isAiming;
        }
    }
}