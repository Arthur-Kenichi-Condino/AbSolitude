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
        internal void ProcessAction2(){
         if(actor is BaseAI baseAI){
          if(Enabled.ACTION_2.curState){
          }
         }
        }
    }
}