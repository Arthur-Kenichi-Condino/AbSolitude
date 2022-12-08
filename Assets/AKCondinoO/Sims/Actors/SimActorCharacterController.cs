using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorCharacterController:MonoBehaviour{
     internal SimActor actor;
     internal CharacterController characterController;
      internal Vector3 center;
        void Awake(){
         characterController=GetComponentInChildren<CharacterController>();
         center=characterController.center;
         headOffset=new Vector3(
          0f,
          characterController.height/2f-characterController.radius/2f,
          0f
         );
         tgtRot=tgtRot_Last=characterController.transform.eulerAngles;
         tgtPos=tgtPos_Last=characterController.transform.position;
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
     Vector3 tgtRot,tgtRot_Last;
      float tgtRotLerpTime;
       float tgtRotLerpVal;
        Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]float tgtRotLerpSpeed=18.75f;
          [SerializeField]float tgtRotLerpMaxTime=.025f;
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
      internal Quaternion viewRotation;
     Vector3 tgtPos,tgtPos_Last;
      Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(16f,16f,16f);
        [SerializeField]Vector3 moveDeceleration=new Vector3(32f,32f,32f);
         [SerializeField]Vector3 maxMoveSpeed=new Vector3(256f,256f,256f);
          Vector3 appliedControllerVelocity;
     [SerializeField]float jumpTimeLength=.125f;
      [SerializeField]float jumpSpeed=8.0f;
       float jumpingTimer;
        internal Vector3 beforeMovePos;
         internal Vector3 afterMovePos;
          internal Vector3 moveDelta;
        internal Vector3 headOffset;
        internal Vector3 aimingAt;
        internal void ManualUpdate(){
         if(characterController.isGrounded){
          delayToConsiderNotOnGround=.2f;
         }else if(delayToConsiderNotOnGround>0f){
          delayToConsiderNotOnGround-=Time.deltaTime;
         }
         beforeMovePos=characterController.transform.position;
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
        }
    }
}