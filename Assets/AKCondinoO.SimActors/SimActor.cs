using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Bootstrap.InputInterpreter;
namespace AKCondinoO.SimActors{
    //  Com ajuda de ChatGPT
    internal class SimActor:SimObject{
     [SerializeField]internal SimDescription simDescriptionTemplate;
     internal SimDescription simDescription;
     internal CharacterController simCharacterController;
     internal NavMeshAgent simNavMeshAgent;
     internal NavMeshQueryFilter simNavMeshQueryFilter;
        internal override void Awake(){
         base.Awake();
         simCharacterController=GetComponentInChildren<CharacterController>();
         simNavMeshAgent=GetComponentInChildren<NavMeshAgent>();
         simNavMeshQueryFilter=new(){
          agentTypeID=simNavMeshAgent.agentTypeID,
             areaMask=simNavMeshAgent.areaMask,
         };
         Logs.Debug(()=>"simCharacterController:"+simCharacterController+";simNavMeshAgent:"+simNavMeshAgent);
         simDescription=simDescriptionTemplate.InstancedFor(this);
        }
     internal bool isPlayerControlled;
     internal bool isAutonomous=true;
        internal override void DynamicUpdate(){
         base.DynamicUpdate();
        }
        internal override void OnUpdate(){
         base.OnUpdate();
         //Logs.Debug(()=>"'updating actor of id':"+id);
         TryWakeUp();
         EnsureNavMeshAgentSafeState();
         simDescription.Tick(this);
         if(!noGround){
          transform.position+=simDescription.movementDelta;
         }
        }
        void EnsureNavMeshAgentSafeState(){
         if(!isAutonomous){
          DisableNavMeshAgent();
          return;
         }
         if(!simNavMeshAgent.isOnNavMesh){
          if(!TryPlaceNavMeshAgentOnMesh()){
           DisableNavMeshAgent();
          }
         }
            bool TryPlaceNavMeshAgentOnMesh(){
             if(noGround){
              return false;
             }
             if(NavMesh.SamplePosition(transform.position,out NavMeshHit hitResult,simCharacterController.height,simNavMeshQueryFilter)){
              transform.position=hitResult.position+Vector3.up*simNavMeshAgent.height*0.5f;
              simNavMeshAgent.enabled=true;
              Logs.Debug(()=>"'simNavMeshAgent is placed'");
              return true;
             }
             return false;
            }
        }
        void DisableNavMeshAgent(){
         if(simNavMeshAgent.enabled){
          simNavMeshAgent.enabled=false;
         }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool HasGroundBelow(Vector3 origin,float maxDistance){
         var controller=simCharacterController;
         Vector3 center=transform.position+controller.center;
         float topOffset=controller.height*0.5f;
         origin=center+Vector3.up*topOffset;
         return base.HasGroundBelow(origin,maxDistance);
        }
        internal override void UpdateGroundedState(){
         base.UpdateGroundedState();
         if(simCharacterController==null){
          isGrounded=true;
         }else{
          isGrounded=simCharacterController.isGrounded;
         }
         //Logs.Debug(()=>"isGrounded:"+isGrounded);
        }
     //private readonly List<SimInteractionDefinition>foundInteractions=new(0);
        internal void OnReceiveInteractionIntent(InputIntent intent){
         //if(IInteractable.GetInteractable(intent.target,out var interactable)){
         // interactable.AvailableInteractions(activeSim,interactionDefinitions);
         // Logs.Debug(()=>"interactionDefinitions.Count:"+interactionDefinitions.Count);
         //}
        }
        internal virtual void OnDrawGizmos(){
         if(simCharacterController!=null){
          DrawGizmos.DrawWireCapsule(transform.position,simCharacterController.height,simCharacterController.radius,Color.blue);
         }
        }
    }
}