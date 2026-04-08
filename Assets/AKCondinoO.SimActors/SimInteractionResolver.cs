using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Bootstrap.InputInterpreter;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/AI/SimInteractionResolver")]
    internal class SimInteractionResolver:SimDescriptionElement{
     internal SimInteractionQueue interactionQueue;
        internal override void InitializeRuntime(SimDescription simDescription){
         base.InitializeRuntime(simDescription);
         interactionQueue=new(sim);
        }
     private readonly List<SimInteractionDefinition>foundInteractions=new(0);
        internal void ResolveInteractionIntent(InputIntent intent){
         if(intent.target.GetInteractable(out var interactable)){
          interactable.AvailableInteractions(sim,foundInteractions);
          Logs.Debug(()=>"foundInteractions.Count:"+foundInteractions.Count);
          foundInteractions.Clear();
          var defaultInteraction=interactable.DefaultInteraction(sim);
          if(defaultInteraction!=null){
           Logs.Debug(()=>"defaultInteraction:"+defaultInteraction);
           InteractionInstanceParameters parameters=new(){
            hitPosition=intent.mouseHit.point,
           };
           var instance=defaultInteraction.CreateInstance(sim,interactable,parameters);
           Logs.Debug(()=>"instance:"+instance);
           interactionQueue.Add(instance);
           Logs.Debug(()=>"interactionQueue.Count:"+interactionQueue.Count);
          }
         }
        }
    }
}