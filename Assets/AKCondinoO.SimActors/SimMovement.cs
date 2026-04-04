using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Control/SimMovement")]
    internal class SimMovement:ScriptableObject{
     private SimActor            sim;
     private SimDefinition       simDefinition;
     private CharacterController simCharacterController;
     private NavMeshAgent        simNavMeshAgent;
     private Vector3 moveVector;
     private float verticalVelocity;
        internal virtual void OnTick(SimActor sim){
         if((object)this.sim==null||(object)this.sim!=sim){
          this.sim=sim;
          this.simDefinition=sim.simDefinition;
          this.simCharacterController=sim.simCharacterController;
          this.simNavMeshAgent=sim.simNavMeshAgent;
         }
         float dt=Mathf.Min(Time.deltaTime,0.02f);
         var controller=simCharacterController;
         if(!simNavMeshAgent.enabled){
          if(controller.isGrounded){
           if(verticalVelocity<0f){
            verticalVelocity=-2f;
           }
          }else{
           verticalVelocity=Physics.gravity.y;
          }
          controller.transform.localPosition=Vector3.zero;
          moveVector=new(0f,verticalVelocity,0f);
          controller.Move(moveVector*Time.deltaTime);
          simDefinition.delta=controller.transform.localPosition;
         }
        }
    }
}