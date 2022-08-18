#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class MainCamera:MonoBehaviour,ISingletonInitialization{
     internal static MainCamera singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         //Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("MainCamera:OnDestroyingCoreEvent");
        }
    }
}