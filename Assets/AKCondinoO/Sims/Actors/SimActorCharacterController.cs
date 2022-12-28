#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorCharacterController:MonoBehaviour{
     internal SimActor actor;
     internal CharacterController characterController;
      internal Vector3 center;
     [SerializeField]internal float headMaxVerticalRotationAngle=40f;
      [SerializeField]internal float headMaxHorizontalRotationAngle=60f;
        void Awake(){
         characterController=GetComponentInChildren<CharacterController>();
         center=characterController.center;
         headOffset=new Vector3(
          0f,
          characterController.height/2f-characterController.radius,
          0f
         );
         rotLerp.tgtRot=rotLerp.tgtRot_Last=characterController.transform.eulerAngles;
         posLerp.tgtPos=posLerp.tgtPos_Last=characterController.transform.position;
         viewRotation=Quaternion.LookRotation(characterController.transform.forward,Vector3.up);
        }
     float delayToConsiderNotOnGround=.2f;
     internal bool isGrounded{
      get{
       if(delayToConsiderNotOnGround<=0f){
        return false;
       }
       return true;
      }
     }
     [SerializeField]internal QuaternionRotLerpHelper rotLerp=new QuaternionRotLerpHelper();//  tgtRotLerpSpeed:17.8125, tgtRotLerpMaxTime:.025
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
      internal Quaternion viewRotation;
       internal Quaternion bodyRotation;
     [SerializeField]internal Vector3PosLerpHelper posLerp=new Vector3PosLerpHelper();
      internal Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(16f,16f,16f);
        [SerializeField]Vector3 moveDeceleration=new Vector3(32f,32f,32f);
         [SerializeField]Vector3 maxMoveSpeed=new Vector3(256f,256f,256f);
          [SerializeField]internal float walkSpeedAverage=256f;
          Vector3 appliedControllerVelocity;
     [SerializeField]float jumpTimeLength=.125f;
      [SerializeField]float jumpSpeed=8.0f;
       float jumpingTimer;
        internal Vector3 beforeMovePos;
         internal Vector3 afterMovePos;
          internal Vector3 moveDelta;
     internal Vector3 headOffset;
     internal Vector3 aimingAt;
      [SerializeField]internal float aimAtMaxDistance=1000f;
        internal void ManualUpdate(){
         if(characterController.isGrounded){
          delayToConsiderNotOnGround=.2f;
         }else if(delayToConsiderNotOnGround>0f){
          delayToConsiderNotOnGround-=Time.deltaTime;
         }
         beforeMovePos=characterController.transform.position;
         if(!Enabled.RELEASE_MOUSE.curState){
          inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*viewRotationSmoothValue;
          inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*viewRotationSmoothValue;
           inputViewRotationEuler.x=inputViewRotationEuler.x%360f;
           inputViewRotationEuler.y=inputViewRotationEuler.y%360f;
         }
         if(inputViewRotationEuler!=Vector3.zero){
          rotLerp.tgtRot+=inputViewRotationEuler;
          inputViewRotationEuler=Vector3.zero;
         }
         viewRotation=rotLerp.UpdateRotation(viewRotation,Time.deltaTime);
         bodyRotation=characterController.transform.rotation;
         float bodyToHeadRotationYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(bodyRotation,viewRotation);
         //Log.DebugMessage("bodyToHeadRotationYComponentSignedAngle:"+bodyToHeadRotationYComponentSignedAngle);
         float bodyToHeadRotationXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(bodyRotation,viewRotation);
         //Log.DebugMessage("bodyToHeadRotationXComponentSignedAngle:"+bodyToHeadRotationXComponentSignedAngle);
         if(Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)>=headMaxHorizontalRotationAngle){
          Log.DebugMessage("angle between viewRotation and bodyRotation is equal to or above "+headMaxHorizontalRotationAngle);
          float angleToRotateBody=Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)-headMaxHorizontalRotationAngle;
          angleToRotateBody*=Mathf.Sign(bodyToHeadRotationYComponentSignedAngle);
          Log.DebugMessage("rotate body in degrees:"+angleToRotateBody);
          bodyRotation*=Quaternion.AngleAxis(angleToRotateBody,bodyRotation*Vector3.up);
         }
         if(!Enabled.RELEASE_MOUSE.curState){
          if(Enabled.FORWARD .curState){if(inputMoveVelocity.z<0f){inputMoveVelocity.z+=moveDeceleration.z;}else{inputMoveVelocity.z+=moveAcceleration.z;}}
          if(Enabled.BACKWARD.curState){if(inputMoveVelocity.z>0f){inputMoveVelocity.z-=moveDeceleration.z;}else{inputMoveVelocity.z-=moveAcceleration.z;}}
         }
         if(Enabled.RELEASE_MOUSE.curState||(!Enabled.FORWARD .curState&&!Enabled.BACKWARD.curState)){
             if(inputMoveVelocity.z!=0f){
              if(inputMoveVelocity.z>0f){
                 inputMoveVelocity.z-=moveDeceleration.z;
                 if(inputMoveVelocity.z<=0f){
                    inputMoveVelocity.z=0f;
                 }
              }else{
                 inputMoveVelocity.z+=moveDeceleration.z;
                 if(inputMoveVelocity.z>=0f){
                    inputMoveVelocity.z=0f;
                 }
              }
             }
         }
         if( inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z= maxMoveSpeed.z;}
         if(-inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z=-maxMoveSpeed.z;}
         if(!Enabled.RELEASE_MOUSE.curState){
          if(Enabled.RIGHT   .curState){if(inputMoveVelocity.x<0f){inputMoveVelocity.x+=moveDeceleration.x;}else{inputMoveVelocity.x+=moveAcceleration.x;}}
          if(Enabled.LEFT    .curState){if(inputMoveVelocity.x>0f){inputMoveVelocity.x-=moveDeceleration.x;}else{inputMoveVelocity.x-=moveAcceleration.x;}}
         }
         if(Enabled.RELEASE_MOUSE.curState||(!Enabled.RIGHT   .curState&&!Enabled.LEFT    .curState)){
             if(inputMoveVelocity.x!=0f){
              if(inputMoveVelocity.x>0f){
                 inputMoveVelocity.x-=moveDeceleration.x;
                 if(inputMoveVelocity.x<=0f){
                    inputMoveVelocity.x=0f;
                 }
              }else{
                 inputMoveVelocity.x+=moveDeceleration.x;
                 if(inputMoveVelocity.x>=0f){
                    inputMoveVelocity.x=0f;
                 }
              }
             }
         }
         if( inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x= maxMoveSpeed.x;}
         if(-inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x=-maxMoveSpeed.x;}
         float divideBy=(inputMoveVelocity.z!=0f?1f:0f)+(inputMoveVelocity.x!=0f?1f:0f)+1f;
         appliedControllerVelocity=new Vector3(
          inputMoveVelocity.x*Mathf.Min(1f,Time.deltaTime),
          0f,
          inputMoveVelocity.z*Mathf.Min(1f,Time.deltaTime)
         )/divideBy;
         if(jumpingTimer>0f){
         }else{
          characterController.SimpleMove(characterController.transform.rotation*appliedControllerVelocity);
         }
         afterMovePos=characterController.transform.position;
         moveDelta=afterMovePos-beforeMovePos;
         aimingAt=characterController.transform.position+(characterController.transform.rotation*headOffset)+(viewRotation*Vector3.forward)*aimAtMaxDistance;
        }
    }
}