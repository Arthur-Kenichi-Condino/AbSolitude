#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    [Serializable]internal class FloatLerpHelper{
     internal float tgtVal,tgtVal_Last;
      internal float tgtValLerpTime;
       internal float tgtValLerpVal;
        internal float tgtValLerpA,tgtValLerpB;
         [SerializeField]internal float tgtValLerpSpeed=4.75f;
          [SerializeField]internal float tgtValLerpMaxTime=1.0f;
        internal float UpdateFloat(float value,float deltaTime){
         float result=value;
             if(tgtValLerpTime==0){
              if(tgtVal!=tgtVal_Last||value!=tgtVal){
               tgtValLerpVal=0;
               tgtValLerpA=value;
               tgtValLerpB=tgtVal;
               tgtValLerpTime+=deltaTime;
               tgtVal_Last=tgtVal;
              }
             }else{
              tgtValLerpTime+=deltaTime;
             }
             if(tgtValLerpTime!=0){
              tgtValLerpVal+=tgtValLerpSpeed*deltaTime;
              if(tgtValLerpVal>=1){
               tgtValLerpVal=1;
               tgtValLerpTime=0;
              }
              result=Mathf.Lerp(tgtValLerpA,tgtValLerpB,tgtValLerpVal);
              if(tgtValLerpTime>tgtValLerpMaxTime){
               if(tgtVal!=tgtVal_Last){
                tgtValLerpTime=0;
               }
              }
             }
         return result;
        }
    }
}