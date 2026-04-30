using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal static class InteractableHelper{
        internal static bool GetInteractable(this GameObject target,out IInteractable interactable){
         return IInteractable.GetInteractable(target,out interactable);
        }
    }
}