using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.SimObjects.StateMachines;
using System;
using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal abstract class SimInteractionDefinition{
        internal abstract bool CanInteract(SimActor sim,IInteractable target);
        internal abstract SimInteractionInstance CreateInstance(SimActor sim,IInteractable target,InteractionInstanceParameters parameters);
        internal virtual InteractionSlot GetSlot(IInteractable target,InteractionSlot.SlotPurpose slotPurpose){
         InteractionSlot interactionSlot=null;
         if(target is SimObjectPart part){
          var holder=part.holder;
          for(int i=0;i<holder.simObjectSlots.Count;i++){
           var slot=holder.simObjectSlots[i];
           if(slot.purpose==slotPurpose){
            interactionSlot=slot;
           }
          }
         }
         return interactionSlot;
        }
    }
    internal struct InteractionInstanceParameters{
     internal Vector3 hitPosition;
    }
    internal abstract class SimInteractionInstance{
     internal SimActor sim{get;set;}
     internal IInteractable target{get;set;}
     internal Vector3 worldPosition;
     protected bool enRoute;
     protected bool machineRunning;
        internal virtual void OnReturnToPoolRecycle(){
         enRoute=false;
         machineRunning=false;
        }
        internal abstract bool Running();
        protected virtual bool RunStateMachine(StateMachine stateMachine,Func<StateDefinition,bool>state){
         if(!machineRunning){
          machineRunning=stateMachine.RunState(state);
          if(!machineRunning)
           return false;
          Logs.Debug(()=>"machineRunning:"+machineRunning);
         }
         if(stateMachine.IsBusy)
          return true;
         return false;
        }
    }
}