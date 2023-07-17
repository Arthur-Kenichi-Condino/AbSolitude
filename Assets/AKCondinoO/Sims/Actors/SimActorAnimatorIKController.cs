#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
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
     [SerializeField]float headOnIKRotationStoppedCooldown=.1f;
      float headIKRotationStoppedTimer=0f;
     internal float  leftFootIKPositionWeight;
     internal float  leftFootIKRotationWeight;
      [SerializeField]internal FloatLerpHelper  leftFootIKPositionWeightLerp=new FloatLerpHelper();
      [SerializeField]internal FloatLerpHelper  leftFootIKRotationWeightLerp=new FloatLerpHelper();
     internal float rightFootIKPositionWeight;
     internal float rightFootIKRotationWeight;
      [SerializeField]internal FloatLerpHelper rightFootIKPositionWeightLerp=new FloatLerpHelper();
      [SerializeField]internal FloatLerpHelper rightFootIKRotationWeightLerp=new FloatLerpHelper();
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
          if(!simActorAnimatorController.actor.navMeshAgent.enabled){
           Vector3 animHeadPos=simActorAnimatorController.actor.GetHeadPosition(fromAnimator:true);
           Quaternion animBodyRot=simActorAnimatorController.animator.transform.rotation;
           Quaternion viewRot=simActorAnimatorController.actor.simActorCharacterController.viewRotation;
           float horizontalRotSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(animBodyRot,viewRot);//  horizontal rotation from body to view
           //Log.DebugMessage("horizontalRotSignedAngle:"+horizontalRotSignedAngle);
           float   verticalRotSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(animBodyRot,viewRot);//    vertical rotation from body to view
           //Log.DebugMessage("verticalRotSignedAngle:"+verticalRotSignedAngle);
           Quaternion limitedHeadRotation=
            Quaternion.AngleAxis(Mathf.Clamp(verticalRotSignedAngle,-headMaxVerticalRotationAngle,headMaxVerticalRotationAngle),animBodyRot*Vector3.left)*
            Quaternion.AngleAxis(Mathf.Clamp(horizontalRotSignedAngle,-headMaxHorizontalRotationAngle,headMaxHorizontalRotationAngle),animBodyRot*Vector3.up)*
            animBodyRot;
           Vector3 headLookAtPosition=animHeadPos+limitedHeadRotation*Vector3.forward*simActorAnimatorController.actor.simActorCharacterController.aimAtMaxDistance;
           //  TO DO: rotating? Or moving?
           bool flag;
           if((flag=(simActorAnimatorController.rotLerp.tgtRotLerpTime!=0f&&simActorAnimatorController.actor.simUMAData.transform.parent.rotation.eulerAngles!=simActorAnimatorController.rotLerp.tgtRotLerpB.eulerAngles))||headIKRotationStoppedTimer>0f){
            //Log.DebugMessage("rotating body, set target head IK to forward");
            headLookAtPosition=animHeadPos+limitedHeadRotation*Vector3.forward*simActorAnimatorController.actor.simActorCharacterController.aimAtMaxDistance;
            if(flag){
             headIKRotationStoppedTimer=headOnIKRotationStoppedCooldown;
            }else{
             headIKRotationStoppedTimer-=Time.deltaTime;
            }
           }
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
            Log.DebugMessage("angle between body and target head IK "+bodyToHeadRotYComponentSignedAngle+" is above "+(headMaxHorizontalRotationAngle+headIKLimitedHorizontalRotationAngleTolerance)+" (or, vertically, "+bodyToHeadRotXComponentSignedAngle+", "+(headMaxVerticalRotationAngle+headIKLimitedVerticalRotationAngleTolerance)+")");
            headLookAtPositionLerp.tgtPosLerpTime=0f;
            headLookAtPositionLerped=headLookAtPosition;
           }
           simActorAnimatorController.animator.SetLookAtWeight(1f);
           simActorAnimatorController.animator.SetLookAtPosition(headLookAtPositionLerped);
          }
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
          Vector3 leftToFloorRaycastOrigin=simActorAnimatorController.actor.transform.position+(simActorAnimatorController.actorLeft*(disBetweenFeet/2f));
          if(Physics.Raycast(leftToFloorRaycastOrigin,Vector3.down,out RaycastHit leftToFloorHit)){
           leftFootIKPosition.y=leftToFloorHit.point.y+footHeight;
           Vector3 slopeCorrected=-Vector3.Cross(leftToFloorHit.normal,simActorAnimatorController.animator.transform.right);
           leftFootIKRotation=Quaternion.LookRotation(slopeCorrected,leftToFloorHit.normal);
           leftFootIKRotation=RotationHelper.Clamp(leftFootIKRotation,new Vector3(-20f,-leftFoot.eulerAngles.y-20f,-20f),new Vector3(20f,-leftFoot.eulerAngles.y+20f,20f));
           //Debug.DrawRay(leftToFloorHit.point,leftToFloorHit.normal);
          }
          Vector3 rightFootIKEuler=rightFoot.eulerAngles;
          Quaternion rightFootIKRotation=rightFoot.rotation;
          Vector3 rightFootIKPosition=new Vector3(
           rightFoot.position.x,
           rightFoot.position.y,
           rightFoot.position.z
          );
          Vector3 rightToFloorRaycastOrigin=simActorAnimatorController.actor.transform.position+(simActorAnimatorController.actorRight*(disBetweenFeet/2f));
          if(Physics.Raycast(rightToFloorRaycastOrigin,Vector3.down,out RaycastHit rightToFloorHit)){
           rightFootIKPosition.y=rightToFloorHit.point.y+footHeight;
           Vector3 slopeCorrected=-Vector3.Cross(rightToFloorHit.normal,simActorAnimatorController.animator.transform.right);
           rightFootIKRotation=Quaternion.LookRotation(slopeCorrected,rightToFloorHit.normal);
           rightFootIKRotation=RotationHelper.Clamp(rightFootIKRotation,new Vector3(-20f,-rightFoot.eulerAngles.y-20f,-20f),new Vector3(20f,-rightFoot.eulerAngles.y+20f,20f));
           //Debug.DrawRay(rightToFloorHit.point,rightToFloorHit.normal);
          }
          if(simActorAnimatorController.actor is BaseAI baseAI&&
           (baseAI.motion==BaseAI.ActorMotion.MOTION_STAND||
            baseAI.motion==BaseAI.ActorMotion.MOTION_STAND_RIFLE
           )
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
          }
           leftFootIKPositionWeight= leftFootIKPositionWeightLerp.UpdateFloat( leftFootIKPositionWeight,Core.magicDeltaTimeNumber);
           leftFootIKRotationWeight= leftFootIKRotationWeightLerp.UpdateFloat( leftFootIKRotationWeight,Core.magicDeltaTimeNumber);
          rightFootIKPositionWeight=rightFootIKPositionWeightLerp.UpdateFloat(rightFootIKPositionWeight,Core.magicDeltaTimeNumber);
          rightFootIKRotationWeight=rightFootIKRotationWeightLerp.UpdateFloat(rightFootIKRotationWeight,Core.magicDeltaTimeNumber);
          float height=simActorAnimatorController.actor.height;
          Vector3 heightLimit=simActorAnimatorController.actor.transform.position+Vector3.down*(height*.5f);
          #region LeftFoot
              simActorAnimatorController.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,leftFootIKPositionWeight);
              simActorAnimatorController.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,leftFootIKRotationWeight);
              leftFootIKPosition.y=Mathf.Min(heightLimit.y,leftFootIKPosition.y);
              simActorAnimatorController.animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootIKPosition);
              simActorAnimatorController.animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootIKRotation);
          #endregion
          #region RightFoot
              simActorAnimatorController.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,rightFootIKPositionWeight);
              simActorAnimatorController.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,rightFootIKRotationWeight);
              rightFootIKPosition.y=Mathf.Min(heightLimit.y,rightFootIKPosition.y);
              simActorAnimatorController.animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootIKPosition);
              simActorAnimatorController.animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootIKRotation);
          #endregion
         }
        }
    }
}