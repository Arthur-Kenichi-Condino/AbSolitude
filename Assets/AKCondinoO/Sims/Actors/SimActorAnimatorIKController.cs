#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorIKController:MonoBehaviour{
     internal SimActorAnimatorController simActorAnimatorController;
     bool initialized=false;
     internal Transform  leftFoot;
     internal Transform rightFoot;
        void OnAnimatorIK(int layerIndex){
         //Log.DebugMessage("OnAnimatorIK:layerIndex:"+layerIndex);
         if(!initialized){
           leftFoot=Util.FindChildRecursively(transform,"lFoot");
          rightFoot=Util.FindChildRecursively(transform,"rFoot");
          if(leftFoot!=null&&rightFoot!=null){
           Log.DebugMessage("OnAnimatorIK:found feet bones");
          }
          initialized=true;
         }
        }
    }
}