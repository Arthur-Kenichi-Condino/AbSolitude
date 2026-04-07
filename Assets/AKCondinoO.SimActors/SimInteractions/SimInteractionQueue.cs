using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal class SimInteractionQueue{
     internal readonly SimActor sim;
     internal readonly Queue<SimInteractionInstance>interactionQueue=new();
        internal SimInteractionQueue(SimActor sim){
         this.sim=sim;
        }
        internal void Add(SimInteractionInstance interactionInstance){
         interactionQueue.Enqueue(interactionInstance);
        }
        internal SimInteractionInstance Next(){
         if(interactionQueue.Count>0){
          var interaction=interactionQueue.Dequeue();
          return interaction;
         }
         return null;
        }
    }
}