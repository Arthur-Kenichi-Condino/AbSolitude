using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
namespace AKCondinoO.SimObjects.StateMachines{
    internal class ToggleStateDefinition:StateDefinition{
     [SerializeField]protected StateDefinition stateA;
     [SerializeField]protected StateDefinition stateB;
     public StateDefinition A=>stateA;
     public StateDefinition B=>stateB;
        internal override State SetupState(IInteractable interactable){
         if(!IsInTargetState(stateA,interactable))
          return stateA.SetupState(interactable);
         return stateB.SetupState(interactable);
        }
        private bool IsInTargetState(StateDefinition def,IInteractable interactable){
         if(def is IStateEvaluator state)
          return state.IsTheStateOf(interactable);
         return false;
        }
    }
}