#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    [Serializable]internal class Vector3PosComponentwiseLerpHelper{
     internal Vector3 tgtPos,tgtPos_Last,pos;
      bool init=false;
      [SerializeField]internal FloatLerpHelper xLerp=new FloatLerpHelper();
      [SerializeField]internal FloatLerpHelper yLerp=new FloatLerpHelper();
      [SerializeField]internal FloatLerpHelper zLerp=new FloatLerpHelper();
        internal Vector3 UpdatePosition(Vector3 position,float deltaTime){
             if(!init){
              pos=position;
              init=true;
             }
             if(tgtPos!=tgtPos_Last||position!=tgtPos){
              xLerp.tgtVal=tgtPos.x;
              yLerp.tgtVal=tgtPos.y;
              zLerp.tgtVal=tgtPos.z;
              tgtPos_Last=tgtPos;
             }
             pos.x=xLerp.UpdateFloat(pos.x,deltaTime);
             pos.y=yLerp.UpdateFloat(pos.y,deltaTime);
             pos.z=zLerp.UpdateFloat(pos.z,deltaTime);
         return pos;
        }
        internal Vector3 EndPosition(){
         pos.x=xLerp.EndFloat();
         pos.y=yLerp.EndFloat();
         pos.z=zLerp.EndFloat();
         return tgtPos_Last=tgtPos;
        }
    }
}