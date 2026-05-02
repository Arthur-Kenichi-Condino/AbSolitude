using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
using static AKCondinoO.SimObjects.InspectablePart;
using static AKCondinoO.SimObjects.SimObjectPart;
namespace AKCondinoO.SimObjects{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Interactions/Definitions/InspectablePart")]
    internal class InspectablePartInteractions:InteractableInteractions{
        internal override void Register(){
         InteractionDefinitions.RegisterInteraction<InspectablePart,InspectInstance>(()=>new InspectDefinition(),true);
        }
    }
}