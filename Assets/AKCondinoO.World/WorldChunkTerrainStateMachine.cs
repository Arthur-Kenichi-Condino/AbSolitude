using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class WorldChunkTerrainStateMachine:StateMachine{
        internal WorldChunkTerrainStateMachine(IInteractable interactable,StateDefinition[]stateDefinitions):base(interactable,stateDefinitions){
        }
    }
}