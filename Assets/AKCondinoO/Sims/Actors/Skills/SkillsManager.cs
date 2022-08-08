#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class SkillsManager:MonoBehaviour{
     internal static SkillsManager singleton;
		      private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
									foreach(var o in Resources.LoadAll("AKCondinoO/Sims/Actors/Skills/",typeof(GameObject))){
				     }
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillsManager:OnDestroyingCoreEvent");
        }
    }
}