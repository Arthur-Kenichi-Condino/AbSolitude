#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class GameMode:MonoBehaviour,ISingletonInitialization{
     internal enum GameModesEnum{
      BuildBuyEdit,
      Interact,
      ThirdPerson,
     }
     internal static GameMode singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("GameMode:OnDestroyingCoreEvent");
        }
     internal GameModesEnum current=GameModesEnum.BuildBuyEdit;
        internal void OnGameModeChangeTo(GameModesEnum newGameMode){
         var args=new OnGameModeChangeEventArgs(){
          last=current,
         };
         current=newGameMode;
         Log.DebugMessage("GameModesEnum current:"+current);
         EventHandler handler=OnGameModeChangeEvent;
         handler?.Invoke(this,args);
        }
     internal event EventHandler OnGameModeChangeEvent;
        internal class OnGameModeChangeEventArgs:EventArgs{
         internal GameModesEnum last;
        }
    }
}