using AKCondinoO.SimActors.SimInteractions;
using UnityEngine;
using static AKCondinoO.World.WorldChunkTerrain;
namespace AKCondinoO.World{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Interactions/Definitions/WorldChunkTerrain")]
    internal class WorldChunkTerrainInteractions:InteractableInteractions{
        internal override void Register(){
         InteractionDefinitions.RegisterInteraction<WorldChunkTerrain,GoHereInstance>(()=>new GoHereDefinition(),true);
        }
    }
}