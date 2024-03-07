#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimCharacterController:MonoBehaviour{
     internal BaseAI actor;
     internal CharacterController character;
      internal Vector3 center;
     internal bool isStopped=false;
     [SerializeField]internal float headMaxVerticalRotationAngle=40f;
      [SerializeField]internal float headMaxHorizontalRotationAngle=60f;
        void Awake(){
         character=GetComponentInChildren<CharacterController>();
         center=character.center;
         headOffset=new Vector3(
          0f,
          character.height/2f-character.radius,
          0f
         );
         rotLerp.tgtRot=rotLerp.tgtRot_Last=character.transform.rotation;
         posLerp.tgtPos=posLerp.tgtPos_Last=character.transform.position;
         viewRotation=Quaternion.LookRotation(character.transform.forward,Vector3.up);
         bodyRotation.eulerAngles=lastBodyRotation.eulerAngles=new Vector3(
          0f,
          viewRotation.eulerAngles.y,
          0f
         );
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
     [SerializeField]internal QuaternionRotLerpHelper rotLerp=new QuaternionRotLerpHelper();//
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
      internal Quaternion viewRotation;
      internal Quaternion viewRotationRaw;
      internal Quaternion viewRotationForAiming;
       internal Quaternion bodyRotation,lastBodyRotation;
     [SerializeField]internal Vector3PosLerpHelper posLerp=new Vector3PosLerpHelper();
      internal Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(0.125f,0.125f,0.125f);
        [SerializeField]Vector3 moveDeceleration=new Vector3(0.25f,0.25f,0.25f);
         [SerializeField]internal Vector3 maxMoveSpeed=new Vector3(2f,2f,2f);
          [SerializeField]internal float walkSpeedAverage=2f;
           [SerializeField]internal float moveSpeedRunMultiplier=2f;
            [SerializeField]internal FloatLerpHelper isRunningMoveSpeedMultiplierLerp=new FloatLerpHelper();
             float isRunningMoveSpeedMultiplier_value=1f;
            internal float isRunningMoveSpeedMultiplier{
             get{
              return isRunningMoveSpeedMultiplier_value;
             }
            }
          internal Vector3 appliedControllerVelocity;
     [SerializeField]float jumpTimeLength=.125f;
      [SerializeField]float jumpSpeed=8.0f;
       float jumpingTimer;
        internal Vector3 beforeMovePos;
         internal Vector3 afterMovePos;
          internal Vector3 moveDelta;
     internal Vector3 headOffset;
     internal Vector3 aimingAt;
     internal Vector3 aimingAtRaw;
      [SerializeField]internal float aimAtMaxDistance=1000f;
     internal Vector3?predictCameraTarget=null;
        internal void ManualUpdate(){
         predictCameraTarget=null;
         if(Enabled.WALK.curState){
          isRunningMoveSpeedMultiplierLerp.tgtVal=1f;
         }else{
          isRunningMoveSpeedMultiplierLerp.tgtVal=moveSpeedRunMultiplier;
         }
         isRunningMoveSpeedMultiplier_value=isRunningMoveSpeedMultiplierLerp.UpdateFloat(isRunningMoveSpeedMultiplier_value,Core.magicDeltaTimeNumber);
         if(character.isGrounded){
          delayToConsiderNotOnGround=.2f;
         }else if(delayToConsiderNotOnGround>0f){
          delayToConsiderNotOnGround-=Time.deltaTime;
         }
         beforeMovePos=character.transform.position;
         if(!Enabled.RELEASE_MOUSE.curState){
          inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*viewRotationSmoothValue;
          inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*viewRotationSmoothValue;
           inputViewRotationEuler.x=inputViewRotationEuler.x%360f;
           inputViewRotationEuler.y=inputViewRotationEuler.y%360f;
         }
         if(inputViewRotationEuler!=Vector3.zero){
          rotLerp.tgtRot=Quaternion.Euler(rotLerp.tgtRot.eulerAngles+inputViewRotationEuler);
          inputViewRotationEuler=Vector3.zero;
         }
         viewRotationRaw=rotLerp.tgtRot;
         viewRotation=rotLerp.UpdateRotation(viewRotation,Core.magicDeltaTimeNumber);
         bodyRotation=lastBodyRotation=character.transform.rotation;
         if(!Enabled.RELEASE_MOUSE.curState){
          Vector3 viewEuler=viewRotation.eulerAngles;
          Vector3 bodyEuler=bodyRotation.eulerAngles;
          if(
           Enabled.FORWARD .curState||
           Enabled.BACKWARD.curState||
           Enabled.RIGHT   .curState||
           Enabled.LEFT    .curState
          ){
           RotateBodyToView();
          }
          void RotateBodyToView(){
           bodyRotation=Quaternion.Euler(bodyEuler.x,viewEuler.y,bodyEuler.z);
          }
         }
         float bodyToHeadRotationYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(bodyRotation,viewRotation);
         //Log.DebugMessage("bodyToHeadRotationYComponentSignedAngle:"+bodyToHeadRotationYComponentSignedAngle);
         float bodyToHeadRotationXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(bodyRotation,viewRotation);
         //Log.DebugMessage("bodyToHeadRotationXComponentSignedAngle:"+bodyToHeadRotationXComponentSignedAngle);
         if(Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)>=headMaxHorizontalRotationAngle){
          //Log.DebugMessage("angle between viewRotation and bodyRotation is equal to or above "+headMaxHorizontalRotationAngle);
          float angleToRotateBody=Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)-headMaxHorizontalRotationAngle;
          angleToRotateBody*=Mathf.Sign(bodyToHeadRotationYComponentSignedAngle);
          //Log.DebugMessage("rotate body in degrees:"+angleToRotateBody);
          bodyRotation*=Quaternion.AngleAxis(angleToRotateBody,bodyRotation*Vector3.up);
         }
         character.transform.rotation=bodyRotation;
         if(!Enabled.RELEASE_MOUSE.curState){
          if(Enabled.FORWARD .curState){if(inputMoveVelocity.z<0f){inputMoveVelocity.z+=moveDeceleration.z*isRunningMoveSpeedMultiplier;}else{inputMoveVelocity.z+=moveAcceleration.z*isRunningMoveSpeedMultiplier;}}
          if(Enabled.BACKWARD.curState){if(inputMoveVelocity.z>0f){inputMoveVelocity.z-=moveDeceleration.z*isRunningMoveSpeedMultiplier;}else{inputMoveVelocity.z-=moveAcceleration.z*isRunningMoveSpeedMultiplier;}}
         }
         if(Enabled.RELEASE_MOUSE.curState||(!Enabled.FORWARD .curState&&!Enabled.BACKWARD.curState)){
             if(inputMoveVelocity.z!=0f){
              if(inputMoveVelocity.z>0f){
                 inputMoveVelocity.z-=moveDeceleration.z*isRunningMoveSpeedMultiplier;
                 if(inputMoveVelocity.z<=0f){
                    inputMoveVelocity.z=0f;
                 }
              }else{
                 inputMoveVelocity.z+=moveDeceleration.z*isRunningMoveSpeedMultiplier;
                 if(inputMoveVelocity.z>=0f){
                    inputMoveVelocity.z=0f;
                 }
              }
             }
         }
         if( inputMoveVelocity.z>maxMoveSpeed.z*isRunningMoveSpeedMultiplier){inputMoveVelocity.z= maxMoveSpeed.z*isRunningMoveSpeedMultiplier;}
         if(-inputMoveVelocity.z>maxMoveSpeed.z*isRunningMoveSpeedMultiplier){inputMoveVelocity.z=-maxMoveSpeed.z*isRunningMoveSpeedMultiplier;}
         if(!Enabled.RELEASE_MOUSE.curState){
          if(Enabled.RIGHT   .curState){if(inputMoveVelocity.x<0f){inputMoveVelocity.x+=moveDeceleration.x*isRunningMoveSpeedMultiplier;}else{inputMoveVelocity.x+=moveAcceleration.x*isRunningMoveSpeedMultiplier;}}
          if(Enabled.LEFT    .curState){if(inputMoveVelocity.x>0f){inputMoveVelocity.x-=moveDeceleration.x*isRunningMoveSpeedMultiplier;}else{inputMoveVelocity.x-=moveAcceleration.x*isRunningMoveSpeedMultiplier;}}
         }
         if(Enabled.RELEASE_MOUSE.curState||(!Enabled.RIGHT   .curState&&!Enabled.LEFT    .curState)){
             if(inputMoveVelocity.x!=0f){
              if(inputMoveVelocity.x>0f){
                 inputMoveVelocity.x-=moveDeceleration.x*isRunningMoveSpeedMultiplier;
                 if(inputMoveVelocity.x<=0f){
                    inputMoveVelocity.x=0f;
                 }
              }else{
                 inputMoveVelocity.x+=moveDeceleration.x*isRunningMoveSpeedMultiplier;
                 if(inputMoveVelocity.x>=0f){
                    inputMoveVelocity.x=0f;
                 }
              }
             }
         }
         if( inputMoveVelocity.x>maxMoveSpeed.x*isRunningMoveSpeedMultiplier){inputMoveVelocity.x= maxMoveSpeed.x*isRunningMoveSpeedMultiplier;}
         if(-inputMoveVelocity.x>maxMoveSpeed.x*isRunningMoveSpeedMultiplier){inputMoveVelocity.x=-maxMoveSpeed.x*isRunningMoveSpeedMultiplier;}
         if(isStopped){
          inputMoveVelocity=Vector3.zero;
         }
         appliedControllerVelocity=new Vector3(
          inputMoveVelocity.x,
          0f,
          inputMoveVelocity.z
         );
         appliedControllerVelocity=Vector3.Scale(appliedControllerVelocity,Vector3.Scale(appliedControllerVelocity.normalized,new Vector3(Mathf.Sign(appliedControllerVelocity.x),Mathf.Sign(appliedControllerVelocity.y),Mathf.Sign(appliedControllerVelocity.z))));
         if(jumpingTimer>0f){
         }else{
          character.SimpleMove(character.transform.rotation*appliedControllerVelocity);
         }
         afterMovePos=character.transform.position;
         moveDelta=afterMovePos-beforeMovePos;
         aimingAtRaw=character.transform.position+(character.transform.rotation*headOffset)+(aimDir(out _,true))*aimAtMaxDistance;
         Quaternion?isFollowingViewRotation=null;
         if(MainCamera.singleton.isFollowing){
          MainCamera.singleton.PredictCameraPosFollowing(character.transform,viewRotationRaw,out Vector3 predictCameraPos,out Quaternion predictCameraRot);
          Vector3 cameraAimDir=(aimingAtRaw-predictCameraPos).normalized;
          Ray cameraRay=new Ray(predictCameraPos,cameraAimDir);
          if(Physics.Raycast(cameraRay,out RaycastHit cameraHitInfo,aimAtMaxDistance,PhysUtil.shootingHitsLayer,QueryTriggerInteraction.Collide)){
           predictCameraTarget=cameraHitInfo.point;
           isFollowingViewRotation=Quaternion.LookRotation((predictCameraTarget.Value-(character.transform.position+(character.transform.rotation*headOffset))).normalized,viewRotationRaw*Vector3.up);
          }
         }
         if(isFollowingViewRotation!=null){
          viewRotationForAiming=isFollowingViewRotation.Value;
          Debug.DrawLine(character.transform.position+(character.transform.rotation*headOffset),character.transform.position+(character.transform.rotation*headOffset)+viewRotationForAiming*Vector3.forward*aimAtMaxDistance);
         }else{
          viewRotationForAiming=viewRotation;
         }
         aimingAt=character.transform.position+(character.transform.rotation*headOffset)+(aimDir(out _))*aimAtMaxDistance;
         OnReloadInput();
         OnAction2();
         OnAction1();
        }
        internal void ManualUpdateUsingAI(){
         Vector3 dir=aimDir(out float dirDis,false,false,true);
         Vector3 dirRaw=aimDir(out float dirRawDis,true,false,true);
         aimingAtRaw=character.transform.position+(character.transform.rotation*headOffset)+(dirRaw)*dirRawDis;
         aimingAt=character.transform.position+(character.transform.rotation*headOffset)+(dir)*dirDis;
         viewRotation=Quaternion.LookRotation(aimDir(out _,false,true,true));
         viewRotationForAiming=viewRotation;
        }
        Vector3 aimDir(out float dis,bool raw=false,bool ignoreEnemy=false,bool forShooting=false){
         dis=aimAtMaxDistance;
         if(actor.isUsingAI){
          if(actor.enemy!=null&&!ignoreEnemy){
           //Log.DebugMessage("aimDir:actor.enemy!=null");
           Vector3 myHead=character.transform.position+(character.transform.rotation*headOffset);
           if(actor.enemy is BaseAI enemyAI){
            Vector3 enemyHead=enemyAI.GetHeadPosition(true,forShooting);
            dis=Vector3.Distance(enemyHead,myHead);
            return((enemyHead)-(myHead)).normalized;
           }else{
            dis=Vector3.Distance(actor.enemy.transform.position,myHead);
            return(actor.enemy.transform.position-(myHead)).normalized;
           }
          }else{
           //Log.DebugMessage("aimDir:actor.enemy==null");
           return actor.animatorController.transform.forward;
          }
         }else{
          if(raw){
           //Log.DebugMessage("aimDir:!actor.isUsingAI:raw==true");
           return viewRotationRaw*Vector3.forward;
          }else{
           //Log.DebugMessage("aimDir:!actor.isUsingAI");
           return viewRotationForAiming*Vector3.forward;
          }
         }
        }
    }
}