using UnityEngine;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/SimDefinition")]
    internal class SimDefinition:ScriptableObject{
     [SerializeField]internal SimBrain simBrain;
     [SerializeField]internal SimMovement simMovement;
        internal virtual void OnAwake(SimActor sim){
         simBrain   =Instantiate(simBrain   ,sim.transform);
         simMovement=Instantiate(simMovement,sim.transform);
         delta=Vector3.zero;
        }
     internal Vector3 delta;
        internal virtual void Tick(SimActor sim){
         simBrain   .AI    (sim);
         simMovement.OnTick(sim);
        }
    }
}