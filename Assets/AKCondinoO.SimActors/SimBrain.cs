using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/AI/SimBrain")]
    internal partial class SimBrain:SimDescriptionElement{
     protected NavMeshAgent simNavMeshAgent;
        internal override SimDescriptionElement InstancedFor(SimDescription simDescription){
         var instance=base.InstancedFor(simDescription)as SimBrain;
         return instance;
        }
        internal override void InitializeRuntime(SimDescription simDescription){
         base.InitializeRuntime(simDescription);
         simNavMeshAgent=sim.simNavMeshAgent;
        }
        internal virtual void AI(SimActor sim){
         var interactionQueue=sim.simDescription.simInteractionResolver.interactionQueue;
         var interaction=interactionQueue.Next();
        }
    }
}