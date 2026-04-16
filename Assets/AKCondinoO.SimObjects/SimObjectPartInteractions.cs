using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
using static AKCondinoO.SimObjects.SimObjectPart;
namespace AKCondinoO.SimObjects{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Interactions/Definitions/SimObjectPart")]
    internal class SimObjectPartInteractions:InteractableInteractions{
        internal override void Register(){
         InteractionDefinitions.RegisterInteraction<SimObjectPart,ToggleInstance>(()=>new ToggleDefinition(),true);
        }
    }
}