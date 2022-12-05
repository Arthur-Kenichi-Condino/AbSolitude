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
         tgtRot=tgtRot_Last=transform.eulerAngles;
         tgtPos=tgtPos_Last=transform.position;
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
       internal Vector3 thirdPersonOffset=new Vector3(1.0f,.4f,-2.0f);
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
          tgtPos=toFollowActor.transform.position+toFollowActor.simActorCharacterController.viewRotation*thirdPersonOffset;
          UpdateTransformPosition();
          UpdateTransformRotation();
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
     Vector3 tgtRot,tgtRot_Last;
      float tgtRotLerpTime;
       float tgtRotLerpVal;
        Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]float tgtRotLerpSpeed=18.75f;
          [SerializeField]float tgtRotLerpMaxTime=.025f;
      Vector3 inputViewRotationEuler;
       [SerializeField]float viewRotationSmoothValue=.025f;
     Vector3 tgtPos,tgtPos_Last;
      float tgtPosLerpTime;
       float tgtPosLerpVal;
        Vector3 tgtPosLerpA,tgtPosLerpB;
         [SerializeField]float tgtPosLerpSpeed=18.75f;
          [SerializeField]float tgtPosLerpMaxTime=.025f;
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
              tgtRot+=inputViewRotationEuler;
                inputViewRotationEuler=Vector3.zero;
             }
             UpdateTransformRotation();
             if( inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z= maxMoveSpeed.z;}
             if(-inputMoveVelocity.z>maxMoveSpeed.z){inputMoveVelocity.z=-maxMoveSpeed.z;}
             if( inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x= maxMoveSpeed.x;}
             if(-inputMoveVelocity.x>maxMoveSpeed.x){inputMoveVelocity.x=-maxMoveSpeed.x;}
             if(inputMoveVelocity!=Vector3.zero){
              float divideBy=Mathf.Max(
               1f,
               (inputMoveVelocity.z!=0f?1f:0f)+
               (inputMoveVelocity.x!=0f?1f:0f)+
               (inputMoveVelocity.y!=0f?1f:0f)
              );
              tgtPos+=transform.rotation*(inputMoveVelocity/divideBy);
             }
             UpdateTransformPosition();
         }
        }
        void UpdateTransformRotation(){
             if(tgtRotLerpTime==0f){
              if(tgtRot!=tgtRot_Last){
               //Log.DebugMessage("input rotation detected:start rotating to tgtRot:"+tgtRot);
               tgtRotLerpVal=0f;
               tgtRotLerpA=transform.rotation;
               tgtRotLerpB=Quaternion.Euler(tgtRot);
               tgtRotLerpTime+=Time.deltaTime;
               tgtRot_Last=tgtRot;
              }
             }else{
              tgtRotLerpTime+=Time.deltaTime;
             }
             if(tgtRotLerpTime!=0f){
              tgtRotLerpVal+=tgtRotLerpSpeed*Time.deltaTime;
              if(tgtRotLerpVal>=1f){
               tgtRotLerpVal=1f;
               tgtRotLerpTime=0f;
              }
              transform.rotation=Quaternion.Lerp(tgtRotLerpA,tgtRotLerpB,tgtRotLerpVal);
              if(tgtRotLerpTime>=tgtRotLerpMaxTime){
               if(tgtRot!=tgtRot_Last){
                tgtRotLerpTime=0;
               }
              }
             }
        }
        void UpdateTransformPosition(){
             if(tgtPosLerpTime==0){
              if(tgtPos!=tgtPos_Last){
               tgtPosLerpVal=0;
               tgtPosLerpA=transform.position;
               tgtPosLerpB=tgtPos;
               tgtPosLerpTime+=Time.deltaTime;
               tgtPos_Last=tgtPos;
              }
             }else{
              tgtPosLerpTime+=Time.deltaTime;
             }
             if(tgtPosLerpTime!=0){
              tgtPosLerpVal+=tgtPosLerpSpeed*Time.deltaTime;
              if(tgtPosLerpVal>=1){
               tgtPosLerpVal=1;
               tgtPosLerpTime=0;
              }
              transform.position=Vector3.Lerp(tgtPosLerpA,tgtPosLerpB,tgtPosLerpVal);
              if(tgtPosLerpTime>tgtPosLerpMaxTime){
               if(tgtPos!=tgtPos_Last){
                tgtPosLerpTime=0;
               }
              }
             }
        }
    }
}