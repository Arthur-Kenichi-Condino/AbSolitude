#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.UI.Fixed;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Context{
    internal class ContextMenuUI:MonoBehaviour,ISingletonInitialization{
     internal static ContextMenuUI singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("ContextMenu:OnDestroyingCoreEvent");
        }
    }
}