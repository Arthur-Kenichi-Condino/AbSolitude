using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal partial class WorldChunkTerrain{
        internal class GoHereDefinition:SimInteractionDefinition{
            internal override bool CanInteract(SimActor sim,IInteractable target){
             return true;
            }
            internal override SimInteractionInstance CreateInstance(){
             var instance=(GoHereInstance)InteractionDefinitions.instancing[typeof(GoHereInstance)].ObjectRent();
             return instance;
            }
        }
        internal class GoHereInstance:SimInteractionInstance{
            internal override void Reset(){
            }
        }
    }
}