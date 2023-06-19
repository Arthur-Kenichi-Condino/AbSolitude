#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Music;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
namespace AKCondinoO{
    internal class MainCamera:MonoBehaviour,ISingletonInitialization{
     internal static MainCamera singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         Camera.main.transparencySortMode=TransparencySortMode.Default;
         rotLerp.tgtRot=rotLerp.tgtRot_Last=transform.rotation;
         posLerp.tgtPos=posLerp.tgtPos_Last=transform.position;
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("MainCamera:OnDestroyingCoreEvent");
        }
        internal void OnStopFollowing(){
         Log.DebugMessage("MainCamera:OnStopFollowing");
         toFollowActor=null;
         isFollowing=false;
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.Interact);
        }
     Vector3 endOfFrameRotation;
     Vector3 endOfFramePosition;
      internal bool hasTransformChanges{get;private set;}
     internal SimActor toFollowActor;
      bool isFollowing;
       internal Vector3 thirdPersonOffset=new Vector3(
        .375f,//  radius+radius/2f
        .75f,//  height/2f-radius/2f
        -2.0f//  -radius*8f
       );
       internal Vector3 overTheShoulderOffset=new Vector3(
        .375f,//  radius+radius/2f
        .75f,//  height/2f-radius/2f
        -.75f//  -radius*3f
       );
        void LateUpdate(){
         isFollowing=false;
         if(GameMode.singleton.current==GameMode.GameModesEnum.ThirdPerson){
          if(toFollowActor!=null){
           isFollowing=true;
          }else{
           OnStopFollowing();
          }
         }
         if(isFollowing){
          Quaternion rot=toFollowActor.simActorCharacterController.rotLerp.tgtRot;
          rotLerp.tgtRot=rot;
          UpdateTransformRotation();
          posLerp.tgtPos=toFollowActor.transform.position+rotLerp.tgtRot*thirdPersonOffset;
          Vector3 onlyHeightOffsetTgtPos=toFollowActor.transform.position+rotLerp.tgtRot*new Vector3(0f,thirdPersonOffset.y,0f);
          //Debug.DrawLine(posLerp.tgtPos,onlyHeightOffsetTgtPos,Color.blue);
          UpdateTransformPosition();
          float disToActor=Vector3.Distance(transform.position,onlyHeightOffsetTgtPos);
          if(disToActor<=1f){
           Log.DebugMessage("camera is following too close to its target");
           transform.position=posLerp.EndPosition();
          }
          //  TO DO: stop following movement if paused
         }
         BGM.singleton.transform.position=this.transform.position;
         hasTransformChanges=false;
         if(
          endOfFrameRotation!=(endOfFrameRotation=transform.eulerAngles)||
          endOfFramePosition!=(endOfFramePosition=transform.position)
         ){
          hasTransformChanges=true;
         }
        }
     [SerializeField]bool DEBUG_STOP_FOLLOWING=false;
     [SerializeField]QuaternionRotLerpHelper rotLerp=new QuaternionRotLerpHelper();
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
     [SerializeField]Vector3PosLerpHelper posLerp=new Vector3PosLerpHelper();
      Vector3 inputMoveVelocity=Vector3.zero;
       [SerializeField]Vector3 moveAcceleration=new Vector3(.1f,.1f,.1f);
        [SerializeField]Vector3 maxMoveSpeed=new Vector3(1.0f,1.0f,1.0f);
        void Update(){
         if(DEBUG_STOP_FOLLOWING){
            DEBUG_STOP_FOLLOWING=false;
          OnStopFollowing();
         }
         if(GameMode.singleton.current==GameMode.GameModesEnum.ThirdPerson){
          if(toFollowActor==null){
           OnStopFollowing();
          }
         }
         if(!isFollowing){
          //Log.DebugMessage("MainCamera:camera is free");
             if(!Enabled.RELEASE_MOUSE.curState){
                 inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*viewRotationSmoothValue;
                 inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*viewRotationSmoothValue;
                 inputViewRotationEuler.x=inputViewRotationEuler.x%360f;
                 inputViewRotationEuler.y=inputViewRotationEuler.y%360f;
                 //Log.DebugMessage("MainCamera:inputViewRotationEuler.x:"+inputViewRotationEuler.x);
                 //Log.DebugMessage("MainCamera:inputViewRotationEuler.x:"+inputViewRotationEuler.y);
                 if(Enabled.FORWARD .curState){inputMoveVelocity.z+=moveAcceleration.z;}
                 if(Enabled.BACKWARD.curState){inputMoveVelocity.z-=moveAcceleration.z;}
                  if(!Enabled.FORWARD.curState&&!Enabled.BACKWARD.curState){
                   if(inputMoveVelocity.z!=0f){
                    if(inputMoveVelocity.z>0f){
                     inputMoveVelocity.z-=moveAcceleration.z;
                     if(inputMoveVelocity.z<=0f){//  reached 0f
                      inputMoveVelocity.z=0f;
                     }
                    }else{
                     inputMoveVelocity.z+=moveAcceleration.z;
                     if(inputMoveVelocity.z>=0f){//  reached 0f
                      inputMoveVelocity.z=0f;
                     }
                    }
                   }
                  }
                 if(Enabled.RIGHT   .curState){inputMoveVelocity.x+=moveAcceleration.x;}
                 if(Enabled.LEFT    .curState){inputMoveVelocity.x-=moveAcceleration.x;}
                  if(!Enabled.RIGHT  .curState&&!Enabled.LEFT    .curState){
                   if(inputMoveVelocity.x!=0f){
                    if(inputMoveVelocity.x>0f){
                     inputMoveVelocity.x-=moveAcceleration.x;
                     if(inputMoveVelocity.x<=0f){//  reached 0f
                      inputMoveVelocity.x=0f;
                     }
                    }else{
                     inputMoveVelocity.x+=moveAcceleration.x;
                     if(inputMoveVelocity.x>=0f){//  reached 0f
                      inputMoveVelocity.x=0f;
                     }
                    }
                   }
                  }
             }else{
                 if(inputMoveVelocity.z!=0f){
                  if(inputMoveVelocity.z>0f){
                   inputMoveVelocity.z-=moveAcceleration.z;
                   if(inputMoveVelocity.z<=0f){//  reached 0f
                    inputMoveVelocity.z=0f;
                   }
                  }else{
                   inputMoveVelocity.z+=moveAcceleration.z;
                   if(inputMoveVelocity.z>=0f){//  reached 0f
                    inputMoveVelocity.z=0f;
                   }
                  }
                 }
                 if(inputMoveVelocity.x!=0f){
                  if(inputMoveVelocity.x>0f){
                   inputMoveVelocity.x-=moveAcceleration.x;
                   if(inputMoveVelocity.x<=0f){//  reached 0f
                    inputMoveVelocity.x=0f;
                   }
                  }else{
                   inputMoveVelocity.x+=moveAcceleration.x;
                   if(inputMoveVelocity.x>=0f){//  reached 0f
                    inputMoveVelocity.x=0f;
                   }
                  }
                 }
             }
             if(inputViewRotationEuler!=Vector3.zero){
              rotLerp.tgtRot=Quaternion.Euler(rotLerp.tgtRot.eulerAngles+inputViewRotationEuler);
                inputViewRotationEuler=Vector3.zero;
             }
             UpdateTransformRotation();
             if( inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z= maxMoveSpeed.z;}
             if(-inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z=-maxMoveSpeed.z;}
             if( inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x= maxMoveSpeed.x;}
             if(-inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x=-maxMoveSpeed.x;}
             if(inputMoveVelocity!=Vector3.zero){
              posLerp.tgtPos+=transform.rotation*(Vector3.Scale(inputMoveVelocity,Vector3.Scale(inputMoveVelocity.normalized,new Vector3(Mathf.Sign(inputMoveVelocity.x),Mathf.Sign(inputMoveVelocity.y),Mathf.Sign(inputMoveVelocity.z)))));
             }
             UpdateTransformPosition();
         }
        }
        void UpdateTransformRotation(){
         transform.rotation=rotLerp.UpdateRotation(transform.rotation,Core.magicDeltaTimeNumber);
        }
        void UpdateTransformPosition(){
         transform.position=posLerp.UpdatePosition(transform.position,Core.magicDeltaTimeNumber);
        }
    }
}