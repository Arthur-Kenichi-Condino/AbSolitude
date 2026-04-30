using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal abstract class SimInteractionDefinition{
        internal abstract bool CanInteract(SimActor sim,IInteractable target);
        internal abstract SimInteractionInstance CreateInstance(SimActor sim,IInteractable target,InteractionInstanceParameters parameters);
    }
    internal struct InteractionInstanceParameters{
     internal Vector3 hitPosition;
    }
    internal abstract class SimInteractionInstance{
     internal SimActor sim{get;set;}
     internal IInteractable target{get;set;}
        internal abstract void OnReturnToPoolRecycle();
        internal abstract bool Running();
    }
}