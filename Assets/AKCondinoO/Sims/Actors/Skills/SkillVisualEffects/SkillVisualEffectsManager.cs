#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillVisualEffects{
    internal class SkillVisualEffectsManager:MonoBehaviour,ISingletonInitialization{
     internal static SkillVisualEffectsManager singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillVisualEffectsManager:OnDestroyingCoreEvent");
        }
    }
}