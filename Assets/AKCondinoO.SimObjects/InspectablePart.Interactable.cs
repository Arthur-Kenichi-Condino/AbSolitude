using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal partial class InspectablePart:SimObjectPart{
        internal class InspectDefinition:SimInteractionDefinition{
            internal override bool CanInteract(SimActor sim,IInteractable target){
             return true;
            }
            internal override SimInteractionInstance CreateInstance(SimActor sim,IInteractable target,InteractionInstanceParameters parameters){
             if(!CanInteract(sim,target))return null;
             var instance=(InspectInstance)InteractionDefinitions.instancing[typeof(InspectInstance)].ObjectRent();
             return instance;
            }
        }
        internal class InspectInstance:SimInteractionInstance{
            internal override void OnReturnToPoolRecycle(){
            }
            internal override bool Running(){
             return false;
            }
        }
    }
}