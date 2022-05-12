using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class GameMode:MonoBehaviour{
     internal enum GameModesEnum{
      BuildBuyEdit,
      Interact,
     }
     internal GameModesEnum current=GameModesEnum.BuildBuyEdit;
        internal void OnGameModeChangeTo(GameModesEnum newGameMode){
         var args=new OnGameModeChangeEventArgs(){
          last=current,
         };
         current=newGameMode;
         EventHandler handler=OnGameModeChangeEvent;
         handler?.Invoke(this,args);
        }
     internal event EventHandler OnGameModeChangeEvent;
        internal class OnGameModeChangeEventArgs:EventArgs{
         internal GameModesEnum last;
        }
    }
}