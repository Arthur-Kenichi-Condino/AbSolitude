using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.SimActors{
    internal class SimActor:SimObject{
     [SerializeField]internal SimDefinition simDefinition;
     internal CharacterController simCharacterController;
     internal NavMeshAgent        simNavMeshAgent;
        internal override void Awake(){
         base.Awake();
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
         if((object)simDefinition!=null){
          simDefinition.Tick(this);
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
        internal override void UpdateGroundedState(){
         if(simCharacterController==null){
          isGrounded=true;
         }else{
          isGrounded=simCharacterController.isGrounded;
         }
         Logs.Debug(()=>"isGrounded:"+isGrounded);
        }
    }
}