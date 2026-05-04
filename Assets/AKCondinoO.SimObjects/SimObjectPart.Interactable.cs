using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects.StateMachines;
using UnityEngine;
using static AKCondinoO.SimActors.SimInteractions.InteractionSlot;
namespace AKCondinoO.SimObjects{
    internal partial class SimObjectPart{
        internal class ToggleDefinition:SimInteractionDefinition{
            internal override bool CanInteract(SimActor sim,IInteractable target){
             return true;
            }
            internal override SimInteractionInstance CreateInstance(SimActor sim,IInteractable target,InteractionInstanceParameters parameters){
             if(!CanInteract(sim,target))return null;
             var instance=(ToggleInstance)InteractionDefinitions.instancing[typeof(ToggleInstance)].ObjectRent();
             instance.sim=sim;
             instance.target=target;
             InteractionSlot interactionSlot=GetSlot(target,SlotPurpose.InteractFront);
             if(target is SimObjectPart part){
              instance.part=part;
             }
             Logs.Debug(()=>"interactionSlot:"+interactionSlot);
             instance.interactionSlot=interactionSlot;
             if(interactionSlot!=null){
              instance.worldPosition=interactionSlot.transform.position;
             }else{
              instance.worldPosition=parameters.hitPosition;
             }
             return instance;
            }
        }
        internal class ToggleInstance:SimInteractionInstance{
         internal SimObjectPart part;
         internal InteractionSlot interactionSlot;
            internal override void OnReturnToPoolRecycle(){
             part=null;
             interactionSlot=null;
             base.OnReturnToPoolRecycle();
            }
            internal override bool Running(){
             var stateMachine=part.stateMachine;
             enRoute=sim.RouteToObjectRadius(worldPosition,interactionSlot,part);
             //Logs.Debug(()=>"enRoute:"+enRoute);
             if(!enRoute){
              return false;
             }
             if(!sim.HasReachedDestination()){
              return true;
             }
             //Logs.Debug(()=>"'sim.HasReachedDestination()'");
             bool running=RunStateMachine(stateMachine,
              def=>{
               if(def is ToggleStateDefinition toggle){
                return toggle.A is OpenObjectStateDefinition&&
                       toggle.B is CloseObjectStateDefinition;
               }
               return false;
              }
             );
             if(running){
              return true;
             }
             Logs.Debug(()=>"'state machine finished'");
             return false;
            }
        }
    }
}