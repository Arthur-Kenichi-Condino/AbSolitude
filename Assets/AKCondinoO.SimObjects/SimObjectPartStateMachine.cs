using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class SimObjectPartStateMachine:StateMachine{
        internal SimObjectPartStateMachine(IInteractable interactable,StateDefinition[]stateDefinitions):base(interactable,stateDefinitions){
        }
    }
}