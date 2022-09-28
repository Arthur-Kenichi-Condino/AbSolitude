#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Music;
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
         Camera.main.transparencySortMode=TransparencySortMode.Default;
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("MainCamera:OnDestroyingCoreEvent");
        }
        void LateUpdate(){
         bool followerCamera=false;
         if(GameMode.singleton.current==GameMode.GameModesEnum.ThirdPerson){
          //  TO DO: check if is there actually something to be followed (at Gameplayer.main)
          followerCamera=true;
         }
         if(followerCamera){
         }else{
          //  TO DO: stop if paused
         }
         BGM.singleton.transform.position=this.transform.position;
        }
        void Update(){
        }
    }
}