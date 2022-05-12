#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed{
    internal class FixedUI:MonoBehaviour{
     internal static FixedUI singleton;
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
         //  Change UI for game mode:
         GameMode.singleton.OnGameModeChangeEvent+=OnGameModeChangeEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("FixedUI:OnDestroyingCoreEvent");
        }
     [SerializeField]GameObject buildBuyEditModeUI;
        public void OnBuildBuyEditModeButtonPress(){
         Log.DebugMessage("FixedUI:OnBuildBuyEditModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.BuildBuyEdit);
        }
     [SerializeField]GameObject interactModeUI;
        public void OnInteractModeButtonPress(){
         Log.DebugMessage("FixedUI:OnInteractModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.Interact);
        }
        void OnGameModeChangeEvent(object sender,EventArgs ev){
         OnGameModeChangeEventArgs args=(OnGameModeChangeEventArgs)ev;
         buildBuyEditModeUI.SetActive(false);
             interactModeUI.SetActive(false);
         switch(GameMode.singleton.current){
          case GameModesEnum.BuildBuyEdit:{
           buildBuyEditModeUI.SetActive(true);
           break;
          }
          case GameModesEnum.Interact:{
               interactModeUI.SetActive(true);
           break;
          }
         }
        }
     [SerializeField]SimObject DEBUG_SET_PLACEHOLDER=null;
        void Update(){
         if(DEBUG_SET_PLACEHOLDER!=null){
          Log.DebugMessage("DEBUG_SET_PLACEHOLDER:"+DEBUG_SET_PLACEHOLDER);
          Placeholder.singleton.GetPlaceholderFor(DEBUG_SET_PLACEHOLDER.GetType());
            DEBUG_SET_PLACEHOLDER=null;
         }
        }
    }
}