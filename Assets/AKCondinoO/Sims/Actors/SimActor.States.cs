#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
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
     internal virtual float moveVelocityNormalized{
      get{
       if(isUsingAI){
        float velocityMagnitude=navMeshAgent.velocity.magnitude;
        //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
        return Mathf.Clamp01(velocityMagnitude/navMeshAgentRunSpeed);
       }
       if(simActorCharacterController!=null){
        float velocityMagnitude=Vector3.Scale(simActorCharacterController.inputMoveVelocity,new Vector3(1f,0f,1f)).magnitude;
        //Log.DebugMessage("characterController velocityMagnitude:"+velocityMagnitude);
        return Mathf.Clamp01(velocityMagnitude/(simActorCharacterController.walkSpeedAverage*2f));
       }
       return 0f;
      }
     }
     internal float turnAngle{
      get{
       if(isUsingAI){
        if(!Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
         float angle=Vector3.SignedAngle(transform.forward,navMeshAgent.velocity.normalized,transform.up);
         //Log.DebugMessage("angle:"+angle);
         return angle;
        }
       }
       return 0f;
      }
     }
    }
}