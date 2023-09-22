#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class AutonomyCore:MonoBehaviour,ISingletonInitialization{
     internal static AutonomyCore singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("AutonomyCore:OnDestroyingCoreEvent");
        }
        void Update(){
        }
    }
}