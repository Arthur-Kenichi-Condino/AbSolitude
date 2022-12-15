#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    [Serializable]internal class QuaternionRotLerpHelper{
     internal Vector3 tgtRot,tgtRot_Last;
      internal float tgtRotLerpTime;
       internal float tgtRotLerpVal;
        internal Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]internal float tgtRotLerpSpeed=18.75f;
          [SerializeField]internal float tgtRotLerpMaxTime=.025f;
        internal Quaternion UpdateRotation(Quaternion rotation,float deltaTime){
         Quaternion result=rotation;
             if(tgtRotLerpTime==0f){
              if(tgtRot!=tgtRot_Last){
               //Log.DebugMessage("input rotation detected:start rotating to tgtRot:"+tgtRot);
               tgtRotLerpVal=0f;
               tgtRotLerpA=rotation;
               tgtRotLerpB=Quaternion.Euler(tgtRot);
               tgtRotLerpTime+=deltaTime;
               tgtRot_Last=tgtRot;
              }
             }else{
              tgtRotLerpTime+=deltaTime;
             }
             if(tgtRotLerpTime!=0f){
              tgtRotLerpVal+=tgtRotLerpSpeed*deltaTime;
              if(tgtRotLerpVal>=1f){
               tgtRotLerpVal=1f;
               tgtRotLerpTime=0f;
              }
              result=Quaternion.Lerp(tgtRotLerpA,tgtRotLerpB,tgtRotLerpVal);
              if(tgtRotLerpTime>=tgtRotLerpMaxTime){
               if(tgtRot!=tgtRot_Last){
                tgtRotLerpTime=0;
               }
              }
             }
         return result;
        }
    }
}