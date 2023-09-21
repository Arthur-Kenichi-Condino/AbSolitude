#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorCharacterController{
     internal bool isAiming;
        internal void OnAction2(){
         isAiming=false;
         if(Enabled.ACTION_2.curState){
          if(actor is BaseAI baseAI){
           if(MainCamera.singleton.toFollowActor==actor){
            //Log.DebugMessage("Enabled.ACTION_2.curState:mouse aim");
            isAiming=true;
           }
          }
         }
        }
    }
}