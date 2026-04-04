using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Control/SimMovement")]
    internal class SimMovement:ScriptableObject{
     private SimActor            sim;
     private CharacterController simCharacterController;
     private NavMeshAgent        simNavMeshAgent;
        internal virtual void OnTick(SimActor sim){
         if((object)this.sim==null||(object)this.sim!=sim){
          this.sim=sim;
          this.simCharacterController=sim.simCharacterController;
          this.simNavMeshAgent=sim.simNavMeshAgent;
         }
         if(!simNavMeshAgent.enabled){
         }
        }
    }
}