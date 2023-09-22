#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal virtual void UpdateGetters(){
         float velocityFlattened=0f;
         if(isUsingAI){
          float velocityMagnitude=moveVelocity.magnitude;
          //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
          velocityFlattened=velocityMagnitude/navMeshAgentRunSpeed;
         }else if(characterController!=null){
          velocityFlattened=moveVelocity.z;
          //Log.DebugMessage("characterController velocityFlattened:"+velocityFlattened);
         }
         moveVelocityFlattenedLerp.tgtVal=Math.Clamp(velocityFlattened,-1f,1f);
         moveVelocityFlattened_value=moveVelocityFlattenedLerp.UpdateFloat(moveVelocityFlattened_value,Core.magicDeltaTimeNumber);
         float strafeVelocityFlattened=0f;
         if(isUsingAI){
         }else if(characterController!=null){
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
         }else if(characterController!=null){
          angle=Vector3.SignedAngle(characterController.lastBodyRotation*Vector3.forward,characterController.bodyRotation*Vector3.forward,transform.up);
         }
         turnAngleLerp.tgtVal=Math.Clamp(angle,-.5f,.5f);
         turnAngle_value=turnAngleLerp.UpdateFloat(turnAngle_value,Core.magicDeltaTimeNumber);
        }
     internal virtual Vector3 moveVelocity{
      get{
       if(isUsingAI){
        return navMeshAgent.velocity;
       }else if(characterController!=null){
        float divideBy=
         (characterController.inputMoveVelocity.z!=0f?(Mathf.Abs(characterController.inputMoveVelocity.z)/(characterController.maxMoveSpeed.z*characterController.isRunningMoveSpeedMultiplier)):0f)+
         (characterController.inputMoveVelocity.x!=0f?(Mathf.Abs(characterController.inputMoveVelocity.x)/(characterController.maxMoveSpeed.x*characterController.isRunningMoveSpeedMultiplier)):0f);
        Vector3 velocity=
         Vector3.Scale(
          characterController.inputMoveVelocity,
          new Vector3(
           1f/((divideBy==0f?1f:divideBy)*characterController.walkSpeedAverage*2f),
           0f,
           1f/((divideBy==0f?1f:divideBy)*characterController.walkSpeedAverage*2f)
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
      protected float moveVelocityFlattened_value;
     internal virtual float moveVelocityFlattened{
      get{
       return moveVelocityFlattened_value;
      }
     }
     [SerializeField]internal FloatLerpHelper moveStrafeVelocityFlattenedLerp=new FloatLerpHelper();
      protected float moveStrafeVelocityFlattened_value;
     internal virtual float moveStrafeVelocityFlattened{
      get{
       return moveStrafeVelocityFlattened_value;
      }
     }
     [SerializeField]internal FloatLerpHelper turnAngleLerp=new FloatLerpHelper();
      protected float turnAngle_value;
     internal float turnAngle{
      get{
       return turnAngle_value;
      }
     }
     internal bool isAiming{
      get{
       if(characterController!=null){
        return characterController.isAiming;
       }
       return false;
      }
     }
     internal bool isShooting{
      get{
       if(characterController!=null){
        return onDoShootingSetMotion;
       }
       return false;
      }
     }
    }
}