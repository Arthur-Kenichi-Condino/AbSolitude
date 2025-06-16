#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    [Serializable]internal class Vector3PosLerpHelper{
        internal Vector3PosLerpHelper(){
        }
        internal Vector3PosLerpHelper(float tgtRotLerpSpeed,float tgtRotLerpMaxTime):this(){
         this.tgtPosLerpSpeed  =tgtRotLerpSpeed  ;
         this.tgtPosLerpMaxTime=tgtRotLerpMaxTime;
        }
     internal Vector3 tgtPos,tgtPos_Last;
      internal float tgtPosLerpTime;
       internal float tgtPosLerpVal;
        internal Vector3 tgtPosLerpA,tgtPosLerpB;
         [SerializeField]internal float tgtPosLerpSpeed=76.0f;
          [SerializeField]internal float tgtPosLerpMaxTime=0.0005f;
        internal Vector3 UpdatePosition(Vector3 position,float deltaTime){
         Vector3 result=position;
             if(tgtPosLerpTime==0){
              if(tgtPos!=tgtPos_Last||position!=tgtPos){
               tgtPosLerpVal=0;
               tgtPosLerpA=position;
               tgtPosLerpB=tgtPos;
               tgtPosLerpTime+=deltaTime;
               tgtPos_Last=tgtPos;
              }
             }else{
              tgtPosLerpTime+=deltaTime;
             }
             if(tgtPosLerpTime!=0){
              tgtPosLerpVal+=tgtPosLerpSpeed*deltaTime;
              if(tgtPosLerpVal>=1){
               tgtPosLerpVal=1;
               tgtPosLerpTime=0;
              }
              result=Vector3.Lerp(tgtPosLerpA,tgtPosLerpB,tgtPosLerpVal);
              if(tgtPosLerpTime>tgtPosLerpMaxTime){
               if(tgtPos!=tgtPos_Last){
                tgtPosLerpTime=0;
               }
              }
             }
         return result;
        }
        internal Vector3 EndPosition(){
         tgtPosLerpVal=1;
         tgtPosLerpTime=0;
         return tgtPos_Last=tgtPos;
        }
    }
}