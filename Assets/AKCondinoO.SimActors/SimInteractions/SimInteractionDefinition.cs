using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal abstract class SimInteractionDefinition{
        internal abstract bool CanInteract(SimActor sim,IInteractable target);
        internal abstract SimInteractionInstance CreateInstance();
    }
    internal abstract class SimInteractionInstance{
     internal SimActor sim{get;private set;}
     internal IInteractable target{get;private set;}
        internal abstract void Reset();
    }
}