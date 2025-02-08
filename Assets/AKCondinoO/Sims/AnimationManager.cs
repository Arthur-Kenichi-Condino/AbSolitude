#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class AnimationManager:MonoBehaviour,ISingletonInitialization{
     internal static AnimationManager singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         foreach(var o in Resources.LoadAll("AKCondinoO/Animations/",typeof(AnimationClip))){
          AnimationClip animationClip=(AnimationClip)o;
          if(animationClip==null)continue;
          //Log.DebugMessage("animationClip.wrapMode:"+animationClip.wrapMode);
          //Log.DebugMessage("animationClip.name:"+animationClip.name);
          //Log.DebugMessage("animationClip.isLooping:"+animationClip.isLooping);
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("AnimationManager:OnDestroyingCoreEvent");
        }
    }
}