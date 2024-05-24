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
     [SerializeField]internal BaseAI actor;
     [SerializeField]internal CharacterController character;
      internal Vector3 center;
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
         viewRotation=viewRotationRaw=Quaternion.LookRotation(character.transform.forward,Vector3.up);
         bodyRotation.eulerAngles=lastBodyRotation.eulerAngles=new Vector3(
          0f,
          viewRotation.eulerAngles.y,
          0f
         );
         bodyRotLerp.tgtRot=bodyRotLerp.tgtRot_Last=bodyRotation;
         viewRotationForAiming=viewRotation;
         aimingRotLerp.tgtRot=aimingRotLerp.tgtRot_Last=viewRotationForAiming;
         aimingAt=aimingAtRaw=character.transform.position+(character.transform.rotation*headOffset)+(viewRotationForAiming*Vector3.forward)*1000f;
        }
     internal bool isStopped=false;
     float delayToConsiderNotOnGround=.2f;
     internal bool isGrounded{
      get{
       if(delayToConsiderNotOnGround<=0f){
        return false;
       }
       return true;
      }
     }
      Vector3 inputViewRotationEuler;
       float viewRotationSmoothValue=.025f;
      internal Quaternion viewRotation;
       internal Quaternion viewRotationRaw;
       internal QuaternionRotLerpHelper rotLerp=new QuaternionRotLerpHelper();//
       internal Quaternion bodyRotation,lastBodyRotation;
        internal QuaternionRotLerpHelper bodyRotLerp=new QuaternionRotLerpHelper();//
        internal Quaternion viewRotationForAiming;
         internal QuaternionRotLerpHelper aimingRotLerp=new QuaternionRotLerpHelper();//
     internal Vector3PosLerpHelper posLerp=new Vector3PosLerpHelper();
      internal Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(0.125f,0.125f,0.125f);
        [SerializeField]Vector3 moveDeceleration=new Vector3(0.25f,0.25f,0.25f);
         [SerializeField]internal Vector3 maxMoveSpeed=new Vector3(2f,2f,2f);
          [SerializeField]internal float walkSpeedAverage=2f;
           [SerializeField]internal float moveSpeedRunMultiplier=2f;
            internal FloatLerpHelper isRunningMoveSpeedMultiplierLerp=new FloatLerpHelper();
             float isRunningMoveSpeedMultiplier_value=1f;
            internal float isRunningMoveSpeedMultiplier{
             get{
              return isRunningMoveSpeedMultiplier_value;
             }
            }
          internal Vector3 appliedControllerVelocity;
     float jumpTimeLength=.125f;
      float jumpSpeed=8.0f;
       float jumpingTimer;
        internal Vector3 beforeMovePos;
         internal Vector3 afterMovePos;
          internal Vector3 moveDelta;
     internal Vector3 headOffset;
     internal float headMaxVerticalRotationAngle=30f;
      internal float headMaxHorizontalRotationAngle=30f;
     internal Vector3 aimingAt;
     internal Vector3 aimingAtRaw;
      internal float aimAtMaxDistance=1000f;
     internal Vector3?predictCameraTarget=null;
        internal void ManualUpdate(){
         predictCameraTarget=null;//  for aiming
         #region set walk or run move speed multiplier
             if(Enabled.WALK.curState){
              isRunningMoveSpeedMultiplierLerp.tgtVal=1f;
             }else{
              isRunningMoveSpeedMultiplierLerp.tgtVal=moveSpeedRunMultiplier;
             }
             isRunningMoveSpeedMultiplier_value=isRunningMoveSpeedMultiplierLerp.UpdateFloat(isRunningMoveSpeedMultiplier_value,Core.magicDeltaTimeNumber);
         #endregion
         #region update isGrounded
             if(character.isGrounded){
              delayToConsiderNotOnGround=.2f;
             }else if(delayToConsiderNotOnGround>0f){
              delayToConsiderNotOnGround-=Time.deltaTime;
             }
         #endregion
         beforeMovePos=character.transform.position;//  for moveDelta
         #region viewRotation
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
             #region bodyRotation
                 lastBodyRotation=bodyRotation;
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
                   bodyRotLerp.tgtRot=Quaternion.Euler(0f,viewEuler.y,0f);
                  }
                 }
                 float bodyToHeadRotationYComponentSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(bodyRotation,viewRotation);
                 float bodyToHeadRotationXComponentSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(bodyRotation,viewRotation);
                 if(Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)>=headMaxHorizontalRotationAngle){
                  //Log.DebugMessage("angle between viewRotation and bodyRotation is equal to or above "+headMaxHorizontalRotationAngle);
                  float angleToRotateBody=Mathf.Abs(bodyToHeadRotationYComponentSignedAngle)-headMaxHorizontalRotationAngle;
                  angleToRotateBody*=Mathf.Sign(bodyToHeadRotationYComponentSignedAngle);
                  //Log.DebugMessage("rotate body in degrees:"+angleToRotateBody);
                  bodyRotLerp.tgtRot*=Quaternion.AngleAxis(angleToRotateBody,bodyRotLerp.tgtRot*Vector3.up);
                 }
                 bodyRotation=bodyRotLerp.UpdateRotation(bodyRotation,Core.magicDeltaTimeNumber);
                 bodyRotation.eulerAngles=new Vector3(
                  0f,
                  bodyRotation.eulerAngles.y,
                  0f
                 );
                 character.transform.rotation=bodyRotation;
             #endregion
         #endregion
         #region Movement (moveDelta to apply to parent transform)
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
         #endregion
         #region viewRotationForAiming and aimingAt
             Vector3 toAim=character.transform.position+(character.transform.rotation*headOffset)+(aimDir(out _,true))*aimAtMaxDistance;
             aimingAtRaw=toAim;
             Quaternion?isFollowingViewRotation=null;
             if(MainCamera.singleton.isFollowing){
              MainCamera.singleton.PredictCameraPosRawFollowing(character.transform,viewRotationRaw,out Vector3 predictCameraPos,out Quaternion predictCameraRot);
              Vector3 cameraAimDir=(toAim-predictCameraPos).normalized;
              Ray cameraRay=new Ray(predictCameraPos,cameraAimDir);
              if(Physics.Raycast(cameraRay,out RaycastHit cameraHitInfo,aimAtMaxDistance,PhysUtil.shootingHitsLayer,QueryTriggerInteraction.Collide)){
               predictCameraTarget=cameraHitInfo.point;
               isFollowingViewRotation=Quaternion.LookRotation((predictCameraTarget.Value-(character.transform.position+(character.transform.rotation*headOffset))).normalized,viewRotationRaw*Vector3.up);
              }
             }
             if(isFollowingViewRotation!=null){
              aimingRotLerp.tgtRot=isFollowingViewRotation.Value;
             }else{
              aimingRotLerp.tgtRot=viewRotation;
             }
             viewRotationForAiming=aimingRotLerp.UpdateRotation(viewRotationForAiming,Core.magicDeltaTimeNumber);
             Debug.DrawLine(character.transform.position+(character.transform.rotation*headOffset),character.transform.position+(character.transform.rotation*headOffset)+viewRotationForAiming*Vector3.forward*aimAtMaxDistance);
             aimingAt=character.transform.position+(character.transform.rotation*headOffset)+(aimDir(out _))*aimAtMaxDistance;
         #endregion
         OnReloadInput();
         OnAction2();
         OnAction1();
        }
        internal void ManualUpdateUsingAI(){
         Vector3 dirRaw=aimDir(out float dirRawDis,true,false,true);
         rotLerp.tgtRot=Quaternion.LookRotation(dirRaw);
         viewRotationRaw=rotLerp.tgtRot;
         viewRotation=rotLerp.UpdateRotation(viewRotation,Core.magicDeltaTimeNumber);
         bodyRotLerp.tgtRot=Quaternion.Euler(0f,viewRotation.eulerAngles.y,0f);
         bodyRotation=bodyRotLerp.UpdateRotation(bodyRotation,Core.magicDeltaTimeNumber);
         bodyRotation.eulerAngles=new Vector3(
          0f,
          bodyRotation.eulerAngles.y,
          0f
         );
         Vector3 toAim=character.transform.position+(character.transform.rotation*headOffset)+(dirRaw)*dirRawDis;
         aimingAtRaw=toAim;
         viewRotationForAiming=viewRotation;
         Vector3 dir=aimDir(out float dirDis,false,false,true);
         aimingAt=character.transform.position+(character.transform.rotation*headOffset)+(dir)*dirDis;
        }
        Vector3 aimDir(out float dis,bool raw=false,bool ignoreEnemy=false,bool forShooting=false){
         dis=aimAtMaxDistance;
         if(actor.isUsingAI){
          if(raw){
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
            return character.transform.forward;
           }
          }else{
           return viewRotationForAiming*Vector3.forward;
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