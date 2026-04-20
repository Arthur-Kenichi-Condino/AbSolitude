using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.SimObjects.StateMachines;
using AKCondinoO.World;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal interface IInteractable{
        StateMachine stateMachine{get;}
        public virtual void AvailableInteractions(SimActor sim,List<SimInteractionDefinition>interactionDefinitions){
         InteractionDefinitions.GetFor(this,interactionDefinitions);
        }
        public virtual SimInteractionDefinition DefaultInteraction(SimActor sim){
         var defaultInteraction=InteractionDefinitions.GetDefaultFor(this);
         return defaultInteraction;
        }
        internal static bool GetInteractable(GameObject target,out IInteractable interactable){
         interactable=null;
         if(target==null){
          return false;
         }
         Logs.Debug(()=>"target.name:"+target.name);
         SimObject simObject=target.transform.parent.GetComponent<SimObject>();
         if(simObject!=null&&simObject is IInteractable simObjectInteractable){
          interactable=simObjectInteractable;
          return true;
         }
         SimObjectPart simObjectPart=target.transform.parent.GetComponent<SimObjectPart>();
         if(simObjectPart!=null&&simObjectPart is IInteractable simObjectPartInteractable){
          interactable=simObjectPartInteractable;
          return true;
         }
         WorldChunkTerrain terrain=target.GetComponent<WorldChunkTerrain>();;
         if(terrain!=null&&terrain is IInteractable terrainInteractable){
          interactable=terrainInteractable;
          return true;
         }
         Logs.Debug(()=>"no interactable found!");
         return false;
        }
    }
}