#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        internal virtual void UpdateGetters(){
         float velocityFlattened=0f;
         if(isUsingAI){
          float velocityMagnitude=moveVelocity.magnitude;
          //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
          velocityFlattened=velocityMagnitude/navMeshAgentRunSpeed;
         }else if(simActorCharacterController!=null){
          velocityFlattened=moveVelocity.z;
          //Log.DebugMessage("characterController velocityFlattened:"+velocityFlattened);
         }
         moveVelocityFlattenedLerp.tgtVal=Math.Clamp(velocityFlattened,-1f,1f);
         moveVelocityFlattened_value=moveVelocityFlattenedLerp.UpdateFloat(moveVelocityFlattened_value,Core.magicDeltaTimeNumber);
         float strafeVelocityFlattened=0f;
         if(isUsingAI){
         }else if(simActorCharacterController!=null){
          strafeVelocityFlattened=moveVelocity.x;
          //Log.DebugMessage("characterController strafeVelocityFlattened:"+strafeVelocityFlattened);
         }
         moveStrafeVelocityFlattenedLerp.tgtVal=Math.Clamp(strafeVelocityFlattened,-1f,1f);
         moveStrafeVelocityFlattened_value=moveStrafeVelocityFlattenedLerp.UpdateFloat(moveStrafeVelocityFlattened_value,Core.magicDeltaTimeNumber);
         float angle=0f;
         if(isUsingAI){
          if(!Mathf.Approximately(moveVelocity.magnitude,0f)){
           angle=Vector3.SignedAngle(transform.forward,moveVelocity.normalized,transform.up)/180f;
           //Log.DebugMessage("angle:"+angle);
          }
         }else if(simActorCharacterController!=null){
          angle=Vector3.SignedAngle(simActorCharacterController.lastBodyRotation*Vector3.forward,simActorCharacterController.bodyRotation*Vector3.forward,transform.up);
         }
         turnAngleLerp.tgtVal=Math.Clamp(angle,-.5f,.5f);
         turnAngle_value=turnAngleLerp.UpdateFloat(turnAngle_value,Core.magicDeltaTimeNumber);
        }
     internal virtual Vector3 moveVelocity{
      get{
       if(isUsingAI){
        return navMeshAgent.velocity;
       }else if(simActorCharacterController!=null){
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
        return velocity;
       }
       return Vector3.zero;
      }
     }
     internal virtual bool isMovingBackwards{
      get{
       return moveVelocityFlattened<0f;
      }
     }
     [SerializeField]internal FloatLerpHelper moveVelocityFlattenedLerp=new FloatLerpHelper();
     internal virtual float moveVelocityFlattened{
      get{
       return moveVelocityFlattened_value;
      }
     }
      protected float moveVelocityFlattened_value;
     [SerializeField]internal FloatLerpHelper moveStrafeVelocityFlattenedLerp=new FloatLerpHelper();
     internal virtual float moveStrafeVelocityFlattened{
      get{
       return moveStrafeVelocityFlattened_value;
      }
     }
      protected float moveStrafeVelocityFlattened_value;
     [SerializeField]internal FloatLerpHelper turnAngleLerp=new FloatLerpHelper();
     internal float turnAngle{
      get{
       return turnAngle_value;
      }
     }
      protected float turnAngle_value;
    }
}