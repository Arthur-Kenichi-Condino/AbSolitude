using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    //  Com ajuda de ChatGPT
    internal class SimActor:SimObject{
     [SerializeField]internal SimDefinition simDefinition;
     internal CharacterController simCharacterController;
     internal NavMeshAgent        simNavMeshAgent;
        internal override void Awake(){
         base.Awake();
         simDefinition=Instantiate(simDefinition,transform);
         simDefinition.OnAwake(this);
         simCharacterController=GetComponentInChildren<CharacterController>();
         simNavMeshAgent       =GetComponentInChildren<NavMeshAgent       >();
         Logs.Debug(()=>"simCharacterController:"+simCharacterController+";simNavMeshAgent:"+simNavMeshAgent);
        }
     internal bool isPlayerControlled;
     internal bool isAutonomous=true;
        internal override void ManualUpdate(){
         base.ManualUpdate();
        }
        internal override void OnUpdate(){
         base.OnUpdate();
         //Logs.Debug(()=>"'updating actor of id':"+id);
         EnsureNavMeshAgentSafeState();
         simDefinition.Tick(this);
         if(!outOfBounds){
          transform.position+=simDefinition.delta;
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
         Vector3 center=controller.transform.position+controller.center;
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
         Logs.Debug(()=>"isGrounded:"+isGrounded);
        }
        internal virtual void OnDrawGizmos(){
         if(simCharacterController!=null){
          DrawGizmos.DrawWireCapsule(transform.position,simCharacterController.height,simCharacterController.radius,Color.blue);
         }
        }
    }
}