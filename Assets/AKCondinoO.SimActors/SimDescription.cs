using UnityEngine;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/SimDescription")]
    internal class SimDescription:ScriptableObject{
     [SerializeField]internal SimBrain simBrainTemplate;
     [SerializeField]internal SimInteractionResolver simInteractionResolverTemplate;
     [SerializeField]internal SimMovement simMovementTemplate;
     internal bool isRuntimeInstance;
     internal SimActor sim;
     internal SimBrain simBrain;
     internal SimInteractionResolver simInteractionResolver;
     internal SimMovement simMovement;
        internal virtual SimDescription InstancedFor(SimActor sim){
         var instance=Instantiate(this,sim.transform);
         instance.InitializeRuntime(sim);
         return instance;
        }
        internal virtual void InitializeRuntime(SimActor sim){
         isRuntimeInstance=true;
         this.sim=sim;
         simBrain=simBrainTemplate.InstancedFor(this)as SimBrain;
         simInteractionResolver=simInteractionResolverTemplate.InstancedFor(this)as SimInteractionResolver;
         simMovement=simMovementTemplate.InstancedFor(this)as SimMovement;
         movementDelta=Vector3.zero;
        }
     internal Vector3 movementDelta;
        internal virtual void Tick(SimActor sim){
         simBrain.AI(sim);
         simMovement.OnTick(sim);
        }
    }
}