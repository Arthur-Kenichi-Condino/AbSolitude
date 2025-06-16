#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using UnityEngine;
namespace AKCondinoO.UI.WorldSpace{
    internal partial class WorldSpaceUI:MonoBehaviour,ISingletonInitialization{
     internal static WorldSpaceUI singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
        }
    }
}