using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class StateMachine{
     protected readonly IInteractable interactable;
     protected readonly StateDefinition[]stateDefinitions;
        internal StateMachine(IInteractable interactable,StateDefinition[]stateDefinitions){
         this.interactable=interactable;
         this.stateDefinitions=stateDefinitions;
        }
        internal bool RunState(Type stateDefinition){
         for(int i=0;i<stateDefinitions.Length;i++){
          var def=stateDefinitions[i];
          if(stateDefinition==def.GetType()){
           var state=def.SetupState(interactable);
           SetState(state);
           SimObjectManager.singleton.OnStateMachinesRunning(this);
           return true;
          }
         }
         return false;
        }
     IState current;
        protected void SetState(IState next){
         current?.Exit();
         current=next;
         current?.Enter();
        }
        internal void Tick(float dt){
         current?.Tick(dt);
         if(current!=null&&current.IsComplete){
          current.Exit();
          current=null;
         }
        }
     internal bool IsBusy=>current!=null;
    }
}