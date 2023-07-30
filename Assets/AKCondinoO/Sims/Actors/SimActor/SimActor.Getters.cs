#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
     internal virtual bool isMovingBackwards{
      get{
       return false;
      }
     }
     [SerializeField]internal FloatLerpHelper moveVelocityFlattenedLerp=new FloatLerpHelper();
     internal virtual float moveVelocityFlattened{
      get{
       float velocityFlattened=0f;
       if(isUsingAI){
        float velocityMagnitude=navMeshAgent.velocity.magnitude;
        //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
        moveVelocityFlattenedLerp.tgtVal=Mathf.Clamp01(velocityMagnitude/navMeshAgentRunSpeed);
        return moveVelocityFlattened_value=moveVelocityFlattenedLerp.UpdateFloat(moveVelocityFlattened_value,Core.magicDeltaTimeNumber);
       }
       if(simActorCharacterController!=null){
        float divideBy=
         (simActorCharacterController.inputMoveVelocity.z!=0f?(Mathf.Abs(simActorCharacterController.inputMoveVelocity.z)/(simActorCharacterController.maxMoveSpeed.z)):0f)+
         (simActorCharacterController.inputMoveVelocity.x!=0f?(Mathf.Abs(simActorCharacterController.inputMoveVelocity.x)/(simActorCharacterController.maxMoveSpeed.x)):0f);
        Vector3 velocity=
         Vector3.Scale(
          simActorCharacterController.inputMoveVelocity,
          new Vector3(
           1f/((divideBy==0f?1f:divideBy)*simActorCharacterController.walkSpeedAverage*2f),
           0f,
           1f/((divideBy==0f?1f:divideBy)*simActorCharacterController.walkSpeedAverage*2f)
          )
         );
        //Log.DebugMessage("characterController velocity:"+velocity);
        velocityFlattened=Mathf.Abs(velocity.x)+Mathf.Abs(velocity.z);
        //Log.DebugMessage("characterController velocityFlattened:"+velocityFlattened);
        moveVelocityFlattenedLerp.tgtVal=Mathf.Clamp01(velocityFlattened);
        return moveVelocityFlattened_value=moveVelocityFlattenedLerp.UpdateFloat(moveVelocityFlattened_value,Core.magicDeltaTimeNumber);
       }
       moveVelocityFlattenedLerp.tgtVal=Mathf.Clamp01(velocityFlattened);
       return velocityFlattened;
      }
     }
     internal virtual float moveStrafeVelocityFlattened{
      get{
       return 0f;
      }
     }
      protected float moveVelocityFlattened_value;
     [SerializeField]internal FloatLerpHelper turnAngleLerp=new FloatLerpHelper();
     internal float turnAngle{
      get{
       float angle=0f;
       if(isUsingAI){
        if(!Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
         angle=Vector3.SignedAngle(transform.forward,navMeshAgent.velocity.normalized,transform.up)/180f;
         //Log.DebugMessage("angle:"+angle);
        }
       }else if(simActorCharacterController!=null){
        angle=Vector3.SignedAngle(simActorCharacterController.lastBodyRotation*Vector3.forward,simActorCharacterController.bodyRotation*Vector3.forward,transform.up);
       }
       turnAngleLerp.tgtVal=Math.Clamp(angle,-.5f,.5f);
       return turnAngle_value=turnAngleLerp.UpdateFloat(turnAngle_value,Core.magicDeltaTimeNumber);
      }
     }
      protected float turnAngle_value;
    }
}