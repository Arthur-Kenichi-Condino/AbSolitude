#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorIKController:MonoBehaviour{
     internal SimActorAnimatorController simActorAnimatorController;
     internal float headMaxVerticalRotationAngle{get{return simActorAnimatorController.actor.simActorCharacterController.headMaxVerticalRotationAngle;}}
      internal float headIKLimitedVerticalRotationAngleTolerance=7f;
      internal float headMaxHorizontalRotationAngle{get{return simActorAnimatorController.actor.simActorCharacterController.headMaxHorizontalRotationAngle;}}
       internal float headIKLimitedHorizontalRotationAngleTolerance=7f;
     bool initialized=false;
     internal Transform      head;
     [SerializeField]internal Vector3PosLerpHelper headLookAtPositionLerp=new Vector3PosLerpHelper();
      Vector3 headLookAtPositionLerped;
     internal Transform  leftFoot;
     internal Transform rightFoot;
      internal float footHeight=.075f;
        //  [https://forum.unity.com/threads/setikrotation-for-feet-on-slope.510931/]
        void OnAnimatorIK(int layerIndex){
         //Log.DebugMessage("OnAnimatorIK:layerIndex:"+layerIndex);
         if(!initialized){
               head=Util.FindChildRecursively(transform, "head");
          if(head!=null){
           Log.DebugMessage("OnAnimatorIK:found head Bone");
          }
           leftFoot=Util.FindChildRecursively(transform,"lFoot");
          rightFoot=Util.FindChildRecursively(transform,"rFoot");
          if(leftFoot!=null&&rightFoot!=null){
           Log.DebugMessage("OnAnimatorIK:found feet Bones");
          }
          initialized=true;
         }
         if(head!=null){
          Vector3 animHeadPos=simActorAnimatorController.actor.GetHeadPosition(fromAnimator:true);
          Quaternion animBodyRot=simActorAnimatorController.animator.transform.rotation;
          Quaternion viewRot=simActorAnimatorController.actor.simActorCharacterController.viewRotation;
          float horizontalRotSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(animBodyRot,viewRot);//  horizontal rotation from body to view
          //Log.DebugMessage("horizontalRotSignedAngle:"+horizontalRotSignedAngle);
          float verticalRotSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(animBodyRot,viewRot);//  vertical rotation from body to view
          //Log.DebugMessage("verticalRotSignedAngle:"+verticalRotSignedAngle);
          Quaternion limitedHeadRotation=
           Quaternion.AngleAxis(Mathf.Clamp(verticalRotSignedAngle,-headMaxVerticalRotationAngle,headMaxVerticalRotationAngle),animBodyRot*Vector3.left)*
           Quaternion.AngleAxis(Mathf.Clamp(horizontalRotSignedAngle,-headMaxHorizontalRotationAngle,headMaxHorizontalRotationAngle),animBodyRot*Vector3.up)*
           animBodyRot;
          Vector3 headLookAtPosition=animHeadPos+limitedHeadRotation*Vector3.forward*simActorAnimatorController.actor.simActorCharacterController.aimAtMaxDistance;
          //  TO DO: rotating? Or moving?
          if(simActorAnimatorController.rotLerp.tgtRotLerpTime!=0f){
           Log.DebugMessage("rotating body, set target head IK to forward");
           headLookAtPosition=animHeadPos+animBodyRot*Vector3.forward*simActorAnimatorController.actor.simActorCharacterController.aimAtMaxDistance;
          }
          headLookAtPositionLerp.tgtPos=headLookAtPosition;
          headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Time.deltaTime);
          Quaternion headRot=Quaternion.LookRotation((headLookAtPositionLerped-animHeadPos).normalized,animBodyRot*Vector3.up);
          float bodyToHeadRotYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(animBodyRot,headRot);
          //Log.DebugMessage("bodyToHeadRotYComponentSignedAngle:"+bodyToHeadRotYComponentSignedAngle);
          float bodyToHeadRotXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(animBodyRot,headRot);
          //Log.DebugMessage("bodyToHeadRotXComponentSignedAngle:"+bodyToHeadRotXComponentSignedAngle);
          if(Mathf.Abs(bodyToHeadRotYComponentSignedAngle)>headMaxHorizontalRotationAngle+headIKLimitedHorizontalRotationAngleTolerance||
             Mathf.Abs(bodyToHeadRotXComponentSignedAngle)>headMaxVerticalRotationAngle+headIKLimitedVerticalRotationAngleTolerance//  tolerance
          ){
           Log.DebugMessage("angle between body and target head IK "+bodyToHeadRotYComponentSignedAngle+" is above "+(headMaxHorizontalRotationAngle+headIKLimitedHorizontalRotationAngleTolerance)+" (or, vertically, "+bodyToHeadRotXComponentSignedAngle+", "+(headMaxVerticalRotationAngle+headIKLimitedVerticalRotationAngleTolerance)+")");
           headLookAtPositionLerp.tgtPosLerpTime=0f;
           headLookAtPositionLerped=headLookAtPosition;
          }
          simActorAnimatorController.animator.SetLookAtWeight(1f);
          simActorAnimatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
         }
         if(leftFoot!=null&&rightFoot!=null){
          float disBetweenFeet=(leftFoot.position-rightFoot.position).magnitude;
          //Log.DebugMessage("disBetweenFeet:"+disBetweenFeet);
          Vector3 leftFootIKPosition=new Vector3(
           leftFoot.position.x,
           leftFoot.position.y,
           leftFoot.position.z
          );
          Vector3 leftToFloorRaycastOrigin=simActorAnimatorController.actor.transform.position+(simActorAnimatorController.actorLeft*(disBetweenFeet/2f));
          if(Physics.Raycast(leftToFloorRaycastOrigin,Vector3.down,out RaycastHit leftToFloorHit)){
           leftFootIKPosition.y=leftToFloorHit.point.y+footHeight;
           //Debug.DrawRay(leftToFloorHit.point,leftToFloorHit.normal);
          }
          Vector3 rightFootIKPosition=new Vector3(
           rightFoot.position.x,
           rightFoot.position.y,
           rightFoot.position.z
          );
          Vector3 rightToFloorRaycastOrigin=simActorAnimatorController.actor.transform.position+(simActorAnimatorController.actorRight*(disBetweenFeet/2f));
          if(Physics.Raycast(rightToFloorRaycastOrigin,Vector3.down,out RaycastHit rightToFloorHit)){
           rightFootIKPosition.y=rightToFloorHit.point.y+footHeight;
           //Debug.DrawRay(rightToFloorHit.point,rightToFloorHit.normal);
          }
          if(simActorAnimatorController.actor is BaseAI baseAI&&
           (baseAI.motion==BaseAI.ActorMotion.MOTION_STAND||
            baseAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_STAND
           )
          ){
           simActorAnimatorController.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f);
           simActorAnimatorController.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1f);
           simActorAnimatorController.animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootIKPosition);
           simActorAnimatorController.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1f);
           simActorAnimatorController.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,1f);
           simActorAnimatorController.animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootIKPosition);
          }
         }
        }
    }
}