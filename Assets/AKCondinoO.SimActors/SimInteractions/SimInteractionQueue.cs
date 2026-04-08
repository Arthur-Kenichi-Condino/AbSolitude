using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal class SimInteractionQueue{
     internal readonly SimActor sim;
     internal readonly Queue<SimInteractionInstance>interactionQueue=new();
     internal SimInteractionInstance runningInteraction;
        internal SimInteractionQueue(SimActor sim){
         this.sim=sim;
        }
        internal void Add(SimInteractionInstance interactionInstance){
         interactionQueue.Enqueue(interactionInstance);
        }
        internal SimInteractionInstance Next(){
         bool ran=false;
         var interaction=runningInteraction;
         do{
          if(interaction!=null){
           //Logs.Debug(()=>"'run interaction':"+interaction);
           if(!interaction.Running()){
            interaction=null;
           }
           ran=true;
          }
          if(interaction==null){
           if(interactionQueue.Count>0){
            interaction=interactionQueue.Dequeue();
            ran=false;
           }
          }
         }while(!ran&&interaction!=null);
         runningInteraction=interaction;
         return interaction;
        }
     internal int Count{
      get{
       return interactionQueue.Count+(runningInteraction!=null?1:0);
      }
     }
    }
}