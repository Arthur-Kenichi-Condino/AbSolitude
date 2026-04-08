using AKCondinoO.Bootstrap;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Control/SimMovement")]
    internal class SimMovement:SimDescriptionElement{
     protected CharacterController simCharacterController;
     protected NavMeshAgent simNavMeshAgent;
        internal override SimDescriptionElement InstancedFor(SimDescription simDescription){
         var instance=base.InstancedFor(simDescription)as SimMovement;
         return instance;
        }
        internal override void InitializeRuntime(SimDescription simDescription){
         base.InitializeRuntime(simDescription);
         simCharacterController=sim.simCharacterController;
         simNavMeshAgent=sim.simNavMeshAgent;
         moveVector=Vector3.zero;
         verticalMotion=0;
         movementDelta=Vector3.zero;
        }
     private Vector3 moveVector;
     private float verticalMotion;
     internal Vector3 movementDelta;
        internal virtual void OnTick(SimActor sim){
         var controller=simCharacterController;
         if(sim.doInitialization){
          controller.transform.localPosition=Vector3.zero;
          controller.enabled=true;
          controller.Move(Vector3.zero);
         }
         float dt=Mathf.Min(Time.deltaTime,0.02f);
         //Logs.Debug(()=>"dt:"+dt);
         if(!simNavMeshAgent.enabled){
          if(controller.isGrounded){
           if(controller.velocity.y<0f){
            verticalMotion=-controller.height*0.5f;
           }
          }else{
           if(!sim.noGround){
            verticalMotion+=Physics.gravity.y*dt;
           }
          }
          moveVector=new(0f,verticalMotion,0f);
          if(!sim.noGround){
           controller.Move(moveVector*dt);
           movementDelta=controller.transform.position-sim.transform.position;
           sim.transform.position=controller.transform.position;
           controller.transform.localPosition=Vector3.zero;
          }
         }else{
          sim.transform.position=simNavMeshAgent.transform.position;
          simNavMeshAgent.transform.localPosition=Vector3.zero;
         }
        }
    }
}