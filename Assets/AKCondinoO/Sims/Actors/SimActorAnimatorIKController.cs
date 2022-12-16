#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorIKController:MonoBehaviour{
     internal SimActorAnimatorController simActorAnimatorController;
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
          Vector3 headLookAtPosition=simActorAnimatorController.actor.simActorCharacterController.aimingAt;
          Quaternion bodyRot=simActorAnimatorController.animator.transform.rotation;
          Quaternion rot=simActorAnimatorController.actor.simActorCharacterController.viewRotation;
          Quaternion horizontalRot=RotationHelper.IsolateRotationYComponent(rot);
          //Log.DebugMessage("horizontalRot angle:"+Quaternion.Angle(horizontalRot,Quaternion.identity));
          //  rotation from simActorCharacterController to horizontalRot [https://forum.unity.com/threads/quaternion-how-to-compute-delta-angle-for-each-axis.242208/]
          Quaternion horizontalRotDiff=horizontalRot*Quaternion.Inverse(RotationHelper.IsolateRotationYComponent(simActorAnimatorController.actor.simActorCharacterController.transform.rotation));
          float horizontalRotDiffAngle=Quaternion.Angle(horizontalRotDiff,Quaternion.identity);
          //Log.DebugMessage("horizontalRotDiff angle:"+horizontalRotDiffAngle);
          Quaternion verticalRot=RotationHelper.IsolateRotationXComponent(rot);
          //Log.DebugMessage("verticalRot angle:"+Quaternion.Angle(verticalRot,Quaternion.identity));
          //  rotation from simActorCharacterController to verticalRot 
          Quaternion verticalRotDiff=verticalRot*Quaternion.Inverse(RotationHelper.IsolateRotationXComponent(simActorAnimatorController.actor.simActorCharacterController.transform.rotation));
          float verticalRotDiffAngle=Quaternion.Angle(verticalRotDiff,Quaternion.identity);
          //Log.DebugMessage("verticalRotDiff angle:"+verticalRotDiffAngle);
          bool flag=false;
          if(verticalRotDiffAngle>90f){
           //Log.DebugMessage("vertical angle to set to head IK is above 90f");
           if(!flag){
            headLookAtPosition=simActorAnimatorController.actor.GetHeadPosition()+bodyRot*Vector3.forward*1000f;
            flag=true;
           }
          }
          if(horizontalRotDiffAngle>90f){
           //Log.DebugMessage("horizontal angle to set to head IK is above 90f");
           if(!flag){
            headLookAtPosition=simActorAnimatorController.actor.GetHeadPosition()+bodyRot*Vector3.forward*1000f;
            flag=true;
           }
          }
          bool flag2=false;
          if(simActorAnimatorController.rotLerp.tgtRotLerpTime!=0f){
           Log.DebugMessage("rotating body, set target head IK to forward");
           if(!flag){
            headLookAtPosition=simActorAnimatorController.actor.GetHeadPosition()+bodyRot*Vector3.forward*1000f;
            flag=true;
           }
           if(!flag2){
            headLookAtPositionLerp.tgtPosLerpTime=0f;
            headLookAtPositionLerped=headLookAtPosition;
            flag2=true;
           }
          }
          headLookAtPositionLerp.tgtPos=headLookAtPosition;
          headLookAtPositionLerped=headLookAtPositionLerp.UpdatePosition(headLookAtPositionLerped,Time.deltaTime);
          Quaternion headRot=Quaternion.LookRotation((headLookAtPositionLerped-simActorAnimatorController.animator.transform.position).normalized);
          if(Quaternion.Angle(RotationHelper.IsolateRotationYComponent(headRot),RotationHelper.IsolateRotationYComponent(bodyRot))>90f||
             Quaternion.Angle(RotationHelper.IsolateRotationXComponent(headRot),RotationHelper.IsolateRotationXComponent(bodyRot))>90f
          ){
           Log.DebugMessage("angle between body and target head IK is above 90f (or, vertically, 90f)");
           if(!flag2){
            headLookAtPositionLerp.tgtPosLerpTime=0f;
            headLookAtPositionLerped=headLookAtPosition;
            flag2=true;
           }
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