using UnityEngine;
namespace AKCondinoO.SimActors{
    internal abstract class SimDescriptionElement:ScriptableObject{
     internal bool isRuntimeInstance;
     protected SimDescription simDescription;
     protected SimActor sim;
        internal virtual SimDescriptionElement InstancedFor(SimDescription simDescription){
         var instance=Instantiate(this,simDescription.sim.transform);
         instance.InitializeRuntime(simDescription);
         return instance;
        }
        internal virtual void InitializeRuntime(SimDescription simDescription){
         isRuntimeInstance=true;
         this.simDescription=simDescription;
         sim=simDescription.sim;
        }
    }
}