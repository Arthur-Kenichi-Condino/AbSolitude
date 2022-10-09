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
     internal Transform  leftFoot;
     internal Transform rightFoot;
      internal float footHeight=.075f;
        //  [https://forum.unity.com/threads/setikrotation-for-feet-on-slope.510931/]
        void OnAnimatorIK(int layerIndex){
         //Log.DebugMessage("OnAnimatorIK:layerIndex:"+layerIndex);
         if(!initialized){
           leftFoot=Util.FindChildRecursively(transform,"lFoot");
          rightFoot=Util.FindChildRecursively(transform,"rFoot");
          if(leftFoot!=null&&rightFoot!=null){
           Log.DebugMessage("OnAnimatorIK:found feet bones");
          }
          initialized=true;
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
           Debug.DrawRay(leftToFloorHit.point,leftToFloorHit.normal);
          }
          simActorAnimatorController.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f);
          simActorAnimatorController.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1f);
          simActorAnimatorController.animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootIKPosition);
         }
        }
    }
}