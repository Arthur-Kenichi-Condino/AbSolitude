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
            internal override SimInteractionInstance CreateInstance(SimActor sim,IInteractable target,InteractionInstanceParameters parameters){
             if(!CanInteract(sim,target))return null;
             var instance=(GoHereInstance)InteractionDefinitions.instancing[typeof(GoHereInstance)].ObjectRent();
             instance.sim=sim;
             instance.target=target;
             instance.worldPosition=parameters.hitPosition;
             return instance;
            }
        }
        internal class GoHereInstance:SimInteractionInstance{
         internal Vector3 worldPosition;
            internal override void Reset(){
            }
            internal override void Run(){
             var simBrain=sim.simDescription.simBrain;
             simBrain.GoTo(worldPosition);
            }
        }
    }
}