#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    [Serializable]internal class QuaternionRotLerpHelper{
     [SerializeField]internal bool dealWithGimbalLock=true;
     internal Quaternion tgtRot{
      get{
       return tgtRot_value;
      }
      set{
       if(dealWithGimbalLock){
         float horizontalTgtRotationSignedAngle=RotationHelper.SignedAngleFromRotationYComponentFromAToB(value,Quaternion.LookRotation(Vector3.forward));
         float   verticalTgtRotationSignedAngle=RotationHelper.SignedAngleFromRotationXComponentFromAToB(value,Quaternion.LookRotation(Vector3.forward));
         //Log.DebugMessage("horizontalTgtRotationSignedAngle:"+horizontalTgtRotationSignedAngle+";verticalTgtRotationSignedAngle:"+verticalTgtRotationSignedAngle);
         value=Quaternion.Euler(
          Mathf.Clamp(   verticalTgtRotationSignedAngle, -90f, 90f),
          Mathf.Clamp(-horizontalTgtRotationSignedAngle,-180f,180f),
          0f
         );
       }
       tgtRot_value=value;
      }
     }
      Quaternion tgtRot_value;
     internal Quaternion tgtRot_Last;
      internal float tgtRotLerpTime;
       internal float tgtRotLerpVal;
        internal Quaternion tgtRotLerpA,tgtRotLerpB;
         [SerializeField]internal float tgtRotLerpSpeed=19f;
          [SerializeField]internal float tgtRotLerpMaxTime=.0005f;
        internal Quaternion UpdateRotation(Quaternion rotation,float deltaTime){
         Quaternion result=rotation;
             if(tgtRotLerpTime==0f){
              if(tgtRot!=tgtRot_Last||rotation!=tgtRot){
               //Log.DebugMessage("input rotation detected:start rotating to tgtRot:"+tgtRot);
               tgtRotLerpVal=0f;
               tgtRotLerpA=rotation;
               tgtRotLerpB=tgtRot;
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