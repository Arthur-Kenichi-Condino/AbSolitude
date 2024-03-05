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
      internal float headIKLimitedVerticalRotationAngleTolerance=7f;
      internal float headMaxHorizontalRotationAngle{get{return animatorController.actor.characterController.headMaxHorizontalRotationAngle;}}
       internal float headIKLimitedHorizontalRotationAngleTolerance=7f;
     bool initialized=false;
     internal Transform      head{get{return animatorController.actor.head;}}
     internal Vector3PosLerpHelper headLookAtPositionLerp=new Vector3PosLerpHelper();//  [speed alterada em outro lugar:]
      internal float headLookAtPositionLerpSpeedOnAI=1f;
       internal float headLookAtPositionLerpSpeed{
        get{
         if(animatorController.actor.isUsingAI){
          return 10.0f;
         }
         return Mathf.Max(
          animatorController.rotLerp.tgtRotLerpSpeed+1f,
          animatorController.posLerp.xLerp.tgtValLerpSpeed+1f,
          animatorController.posLerp.yLerp.tgtValLerpSpeed+1f,
          animatorController.posLerp.zLerp.tgtValLerpSpeed+1f,
          46.875f
         );
        }
       }
      internal float headLookAtPositionLerpMaxTimeOnAI=.5f;
       internal float headLookAtPositionLerpMaxTime{
        get{
         if(animatorController.actor.isUsingAI){
          return .125f;
         }
         return 0.0005f;
        }
       }
       internal Vector3 headLookAtPositionLerped;
     internal void ResetHeadLookAtPositionLerpSpeed(){
      headLookAtPositionLerp.tgtPosLerpSpeed=headLookAtPositionLerpSpeed;
      headLookAtPositionLerp.tgtPosLerpMaxTime=headLookAtPositionLerpMaxTime;
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
         //Log.DebugMessage("OnAnimatorIK:layerIndex:"+layerIndex);
         if(!initialized){
               //head=Util.FindChildRecursively(transform, "head");
          if(head!=null){
           Log.DebugMessage("OnAnimatorIK:found head Bone");
          }
          // leftFoot=Util.FindChildRecursively(transform,"lFoot");
          //rightFoot=Util.FindChildRecursively(transform,"rFoot");
          if(leftFoot!=null&&rightFoot!=null){
           Log.DebugMessage("OnAnimatorIK:found feet Bones");
          }
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
         if(animatorController.actor.characterController.isAiming||head==null){
          var characterController=animatorController.actor.characterController.character;
          var headOffset=animatorController.actor.characterController.headOffset;
          var viewRotation=animatorController.actor.characterController.viewRotationForAiming;
          var aimAtMaxDistance=animatorController.actor.characterController.aimAtMaxDistance;
          Vector3 aimAt;
          Quaternion viewRotationClamped;
          if(animatorController.actor.isUsingAI&&animatorController.actor.enemy!=null){
           Vector3 dir=animatorController.actor.enemy.transform.position-(characterController.transform.position+(characterController.transform.rotation*headOffset));
           dir.Normalize();
           viewRotationClamped=RotationHelper.Clamp(
            Quaternion.LookRotation(dir),
            animatorController.actor.characterController.viewRotation,
            new Vector3(0f,45f,360f),
            new Vector3(0f,45f,360f)
           );
          }else{
           viewRotationClamped=RotationHelper.Clamp(
            animatorController.actor.characterController.viewRotationForAiming,
            animatorController.actor.characterController.viewRotation,
            new Vector3(0f,45f,360f),
            new Vector3(0f,45f,360f)
           );
          }
          aimAt=characterController.transform.position+(characterController.transform.rotation*headOffset)+(viewRotationClamped*Quaternion.Euler(aimAtTorsoAdjust)*Vector3.forward)*aimAtMaxDistance;
          float disToTarget=aimAtMaxDistance;
          if(animatorController.actor.characterController.predictCameraTarget!=null){
           disToTarget=Vector3.Distance(characterController.transform.position+(characterController.transform.rotation*headOffset),animatorController.actor.characterController.predictCameraTarget.Value);
          }
          headLookAtPositionLerp.tgtPos=aimAt;
          if(!wasAiming&&!animatorController.actor.isUsingAI){
           headLookAtPositionLerped=headLookAtPositionLerp.EndPosition();
          }else{
           headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Core.magicDeltaTimeNumber);
          }
          //  [https://discussions.unity.com/t/upper-torso-ik/133564/2]:m_Anim.SetLookAtWeight(m_LookWeight,m_BodyWeight,m_HeadWeight,m_EyesWeight,m_ClampWeight);
          animatorController.animator.SetLookAtWeight(1f,1f,1f,1f,0.5f);
          animatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
         }else{
          Vector3 animHeadPos=animatorController.actor.GetHeadPosition(fromAnimator:true);
          Quaternion animBodyRot=animatorController.animator.transform.rotation;
          Quaternion viewRot;
          if(!animatorController.actor.navMeshAgent.enabled){
           viewRot=animatorController.actor.characterController.viewRotationForAiming;
           animatorController.animator.SetLookAtWeight(1f);
          }else{
           viewRot=animatorController.animator.transform.rotation;
           animatorController.animator.SetLookAtWeight(0f);
          }
          float horizontalRotSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(animBodyRot,viewRot);//  horizontal rotation from body to view
          //Log.DebugMessage("horizontalRotSignedAngle:"+horizontalRotSignedAngle);
          float   verticalRotSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(animBodyRot,viewRot);//    vertical rotation from body to view
          //Log.DebugMessage("verticalRotSignedAngle:"+verticalRotSignedAngle);
          Quaternion limitedHeadRotation=
           Quaternion.AngleAxis(Math.Clamp(verticalRotSignedAngle,-headMaxVerticalRotationAngle,headMaxVerticalRotationAngle),animBodyRot*Vector3.left)*
           Quaternion.AngleAxis(Math.Clamp(horizontalRotSignedAngle,-headMaxHorizontalRotationAngle,headMaxHorizontalRotationAngle),animBodyRot*Vector3.up)*
           animBodyRot;
          Vector3 headLookAtPosition=animHeadPos+limitedHeadRotation*Vector3.forward*animatorController.actor.characterController.aimAtMaxDistance;
          //  TO DO: rotating? Or moving? handle special conditions and rotations
          bool flag;
          if((flag=(animatorController.rotLerp.tgtRotLerpTime!=0f&&animatorController.actor.simUMA.transform.parent.rotation.eulerAngles!=animatorController.rotLerp.tgtRotLerpB.eulerAngles))||headIKRotationStoppedTimer>0f){
           //Log.DebugMessage("rotating body, set target head IK to forward");
           headLookAtPosition=animHeadPos+limitedHeadRotation*Vector3.forward*animatorController.actor.characterController.aimAtMaxDistance;
           if(flag){
            headIKRotationStoppedTimer=headOnIKRotationStoppedCooldown;
           }else{
            headIKRotationStoppedTimer-=Time.deltaTime;
           }
          }
          //  TO DO: olhar pra frente se está usando AI
          headLookAtPositionLerp.tgtPos=headLookAtPosition;
          headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Core.magicDeltaTimeNumber);
          Quaternion headRot=Quaternion.LookRotation((headLookAtPositionLerped-animHeadPos).normalized,animBodyRot*Vector3.up);
          float bodyToHeadRotYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(animBodyRot,headRot);
          //Log.DebugMessage("bodyToHeadRotYComponentSignedAngle:"+bodyToHeadRotYComponentSignedAngle);
          float bodyToHeadRotXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(animBodyRot,headRot);
          //Log.DebugMessage("bodyToHeadRotXComponentSignedAngle:"+bodyToHeadRotXComponentSignedAngle);
          if(Mathf.Abs(bodyToHeadRotYComponentSignedAngle)>headMaxHorizontalRotationAngle+headIKLimitedHorizontalRotationAngleTolerance||
             Mathf.Abs(bodyToHeadRotXComponentSignedAngle)>headMaxVerticalRotationAngle+headIKLimitedVerticalRotationAngleTolerance//  tolerance
          ){
           //Log.DebugMessage("angle between body and target head IK "+bodyToHeadRotYComponentSignedAngle+" is above "+(headMaxHorizontalRotationAngle+headIKLimitedHorizontalRotationAngleTolerance)+" (or, vertically, "+bodyToHeadRotXComponentSignedAngle+", "+(headMaxVerticalRotationAngle+headIKLimitedVerticalRotationAngleTolerance)+")");
           headLookAtPositionLerp.tgtPosLerpTime=0f;
           headLookAtPositionLerped=headLookAtPosition;
          }
          animatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
         }
         if(leftFoot!=null&&rightFoot!=null){
          float disBetweenFeet=(leftFoot.position-rightFoot.position).magnitude;
          //Log.DebugMessage("disBetweenFeet:"+disBetweenFeet);
          Vector3 leftFootIKEuler=leftFoot.eulerAngles;
          Quaternion leftFootIKRotation=leftFoot.rotation;
          Vector3 leftFootIKPosition=new Vector3(
           leftFoot.position.x,
           leftFoot.position.y,
           leftFoot.position.z
          );
          Vector3 leftToFloorRaycastOrigin=animatorController.actor.transform.position+(animatorController.actorLeft*(disBetweenFeet/2f));
          if(Physics.Raycast(leftToFloorRaycastOrigin,Vector3.down,out RaycastHit leftToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
           leftFootIKPosition.y=leftToFloorHit.point.y+footHeight;
           Vector3 slopeCorrected=-Vector3.Cross(leftToFloorHit.normal,animatorController.animator.transform.right);
           leftFootIKRotation=Quaternion.LookRotation(slopeCorrected,leftToFloorHit.normal);
           Debug.DrawRay(leftFootIKPosition,-animatorController.animator.transform.right,Color.blue);
           Quaternion leftFootIKRotationClamped;
           leftFootIKRotationClamped=RotationHelper.Clamp(
            leftFootIKRotation,
            animatorController.actor.characterController.character.transform.rotation,
            new Vector3(5f,15f,5f),
            new Vector3(5f,15f,5f)
           );
           leftFootIKRotation=leftFootIKRotationClamped;
           Debug.DrawRay(leftFootIKPosition,leftFootIKRotation*-Vector3.right,Color.green);
           //Debug.DrawRay(leftToFloorHit.point,leftToFloorHit.normal);
          }
          Vector3 rightFootIKEuler=rightFoot.eulerAngles;
          Quaternion rightFootIKRotation=rightFoot.rotation;
          Vector3 rightFootIKPosition=new Vector3(
           rightFoot.position.x,
           rightFoot.position.y,
           rightFoot.position.z
          );
          Vector3 rightToFloorRaycastOrigin=animatorController.actor.transform.position+(animatorController.actorRight*(disBetweenFeet/2f));
          if(Physics.Raycast(rightToFloorRaycastOrigin,Vector3.down,out RaycastHit rightToFloorHit,animatorController.actor.height,PhysUtil.considerGroundLayer)){
           rightFootIKPosition.y=rightToFloorHit.point.y+footHeight;
           Vector3 slopeCorrected=-Vector3.Cross(rightToFloorHit.normal,animatorController.animator.transform.right);
           rightFootIKRotation=Quaternion.LookRotation(slopeCorrected,rightToFloorHit.normal);
           Debug.DrawRay(rightFootIKPosition,animatorController.animator.transform.right,Color.blue);
           Quaternion rightFootIKRotationClamped;
           rightFootIKRotationClamped=RotationHelper.Clamp(
            rightFootIKRotation,
            animatorController.actor.characterController.character.transform.rotation,
            new Vector3(5f,15f,5f),
            new Vector3(5f,15f,5f)
           );
           rightFootIKRotation=rightFootIKRotationClamped;
           Debug.DrawRay(rightFootIKPosition,rightFootIKRotation*Vector3.right,Color.green);
           //Debug.DrawRay(rightToFloorHit.point,rightToFloorHit.normal);
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
         wasAiming=animatorController.actor.characterController.isAiming;
        }
    }
}