using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Bootstrap.GameOrchestrator;
using static AKCondinoO.Bootstrap.InputInterpreter;
namespace AKCondinoO.Bootstrap.GameModes{
    internal class RagnarokOnlineMode:IGameModeLogic{
     private readonly GameOrchestrator orchestrator;
        internal RagnarokOnlineMode(GameOrchestrator orchestrator){
         this.orchestrator=orchestrator;
        }
        public void Run(SimActor activeSim,InputIntent intent){
         switch(intent.action){
          case(InputAction.MouseX):{
           Logs.Debug(()=>"InputAction.MouseX:"+intent.enabledState.curStateFloat);
           break;
          }
          case(InputAction.MouseY):{
           Logs.Debug(()=>"InputAction.MouseY:"+intent.enabledState.curStateFloat);
           break;
          }
          case(InputAction.Action):{
           if((object)activeSim!=null){
            activeSim.OnReceiveInteractionIntent(intent);
           }
           break;
          }
         }
        }
    }
}