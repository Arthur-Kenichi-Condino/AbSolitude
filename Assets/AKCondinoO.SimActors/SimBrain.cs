using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/AI/SimBrain")]
    internal partial class SimBrain:ScriptableObject{
     private SimActor      sim;
     private SimDefinition simDefinition;
     private NavMeshAgent  simNavMeshAgent;
        internal virtual void AI(SimActor sim){
         if((object)this.sim==null||(object)this.sim!=sim){
          this.sim=sim;
          this.simDefinition=sim.simDefinition;
          this.simNavMeshAgent=sim.simNavMeshAgent;
         }
        }
    }
}