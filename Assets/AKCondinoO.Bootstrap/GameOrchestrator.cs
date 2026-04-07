using AKCondinoO.Bootstrap.GameModes;
using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects;
using AKCondinoO.World;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Bootstrap.InputHandler;
using static AKCondinoO.Bootstrap.InputInterpreter;
namespace AKCondinoO.Bootstrap{
    internal enum GameMode{
     RagnarokOnline=0,
     TheSims=1,
     TombRaider=2,
    }
    internal class GameOrchestrator:MonoSingleton<GameOrchestrator>{
     internal GameMode gameMode=GameMode.RagnarokOnline;
     internal readonly Dictionary<GameMode,IGameModeLogic>gameLogic=new();
        public override void Initialize(){
         base.Initialize();
         gameLogic.Add(GameMode.RagnarokOnline,new RagnarokOnlineMode(this));
        }
     private SimActor activeSimSelected;
        internal void OnInputReceived(InputIntent intent){
         Logs.Debug(()=>"intent.action:"+intent.action);
         EnsureActiveSimSelected();
         gameLogic[gameMode].Run(activeSimSelected,intent);
        }
        private void EnsureActiveSimSelected(){
         if(activeSimSelected==null||activeSimSelected.id==0){
          activeSimSelected=null;
          Logs.Debug(()=>"'find sim to be the activeSimSelected...'");
          SimDirector.singleton.GetSelectableSim(out activeSimSelected);
          if(activeSimSelected!=null){
           Logs.Debug(()=>"'activeSimSelected found!'");
          }
         }
        }
        internal interface IGameModeLogic{
            void Run(SimActor activeSim,InputIntent intent);
        }
    }
}