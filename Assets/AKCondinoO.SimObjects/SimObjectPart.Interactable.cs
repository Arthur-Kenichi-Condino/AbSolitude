using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
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
             InteractionSlot interactionSlot=null;
             if(target is SimObjectPart part){
              var holder=part.holder;
              for(int i=0;i<holder.simObjectSlots.Count;i++){
               var slot=holder.simObjectSlots[i];
               if(slot.purpose==InteractionSlot.SlotPurpose.InteractFront){
                interactionSlot=slot;
               }
              }
             }
             Logs.Debug(()=>"interactionSlot:"+interactionSlot);
             if(interactionSlot!=null){
              instance.worldPosition=interactionSlot.transform.position;
             }else{
              instance.worldPosition=parameters.hitPosition;
             }
             return instance;
            }
        }
        internal class ToggleInstance:SimInteractionInstance{
         internal Vector3 worldPosition;
         private bool destSet;
            internal override void OnReturnToPoolRecycle(){
             destSet=false;
            }
            internal override bool Running(){
             if(sim.simNavMeshAgent==null){
              return false;
             }
             if(!sim.simNavMeshAgent.enabled){
              return true;
             }
             if(!destSet){
              if(sim.simNavMeshAgent.destination!=worldPosition){
               Logs.Debug(()=>"sim.simNavMeshAgent.destination:"+sim.simNavMeshAgent.destination+" is != from worldPosition:"+worldPosition);
               var simBrain=sim.simDescription.simBrain;
               destSet=simBrain.GoTo(worldPosition);
              }
             }
             if(destSet){
              if(!sim.simNavMeshAgent.pathPending){
               if(sim.simNavMeshAgent.remainingDistance<=sim.simNavMeshAgent.stoppingDistance){
                Logs.Debug(()=>"sim.simNavMeshAgent.remainingDistance:"+sim.simNavMeshAgent.remainingDistance+" is <= to sim.simNavMeshAgent.stoppingDistance:"+sim.simNavMeshAgent.stoppingDistance);
                return false;
               }
              }
             }
             return true;
            }
        }
    }
}