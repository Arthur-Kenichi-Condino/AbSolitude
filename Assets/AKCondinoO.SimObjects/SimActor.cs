using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Bootstrap.InputInterpreter;
using static AKCondinoO.SimActors.SimActor;
namespace AKCondinoO.SimActors{
    //  Com ajuda de ChatGPT
    internal class SimActor:SimObject{
     [SerializeField]internal SimDescription simDescriptionTemplate;
     [SerializeField]internal bool isPlayable=true;
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
         //Logs.Debug(()=>"simCharacterController:"+simCharacterController+";simNavMeshAgent:"+simNavMeshAgent);
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
         EvaluateNavMeshAgent();
         //Logs.Debug(()=>"simNavMeshAgentState:"+simNavMeshAgentState);
         simDescription.Tick(this);
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
             if(TryGetNavMeshPosition(transform.position,out Vector3 navPos)){
              transform.position=navPos;
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
     internal NavMeshAgentPathContext navMeshAgentPathContext;
        internal struct NavMeshAgentPathContext{
         public bool isNavigationRequested;
         public bool hasDesiredDestination;
         public Vector3 desiredDestination;
         public float pathTimeout;
         public float pathTimer;
         public float pendingTimeout;
         public float pendingTimer;
        }
     internal NavMeshAgentState simNavMeshAgentState=NavMeshAgentState.Disabled;
        internal enum NavMeshAgentState{
         Disabled,
         Idle,
         Pending,
         Travelling,
         Reached,
         Partial,
         Stuck,
         Unreachable,
        }
        void EvaluateNavMeshAgent(){
         var agent=simNavMeshAgent;
         ref var state=ref simNavMeshAgentState;
         ref var ctx=ref navMeshAgentPathContext;
         if(!agent.enabled){
          state=NavMeshAgentState.Disabled;
          return;
         }
         if(!agent.hasPath){
          if(ctx.isNavigationRequested&&
           IsAtDestination(agent)
          ){
           state=NavMeshAgentState.Reached;
           ctx.isNavigationRequested=false;
           return;
          }
          state=NavMeshAgentState.Idle;
          return;
         }
         if(agent.pathPending){
          if(ctx.pendingTimer>0f){
           ctx.pendingTimer-=Time.deltaTime;
           if(ctx.pendingTimer<=0f){
            Logs.Debug(()=>"handle timeout");
            return;
           }
          }
          state=NavMeshAgentState.Pending;
          return;
         }
         ctx.pendingTimer=ctx.pendingTimeout;
         if(agent.pathStatus==NavMeshPathStatus.PathInvalid){
          state=NavMeshAgentState.Unreachable;
          return;
         }
         if(IsDistanceInvalid(agent.remainingDistance)){
          state=NavMeshAgentState.Idle;
          return;
         }
         if(IsAtDestination(agent)){
          state=NavMeshAgentState.Reached;
          return;
         }
         if(ctx.pathTimer>0f){
          ctx.pathTimer-=Time.deltaTime;
          if(ctx.pathTimer<=0f){
           Logs.Debug(()=>"handle timeout");
           return;
          }
         }
         if(Mathf.Approximately(agent.velocity.sqrMagnitude,0f)){
          state=NavMeshAgentState.Stuck;
          return;
         }
         if(agent.pathStatus==NavMeshPathStatus.PathPartial){
          state=NavMeshAgentState.Partial;
          return;
         }
         state=NavMeshAgentState.Travelling;
        }
        static bool IsAtDestination(NavMeshAgent agent){
         return agent.remainingDistance<=agent.stoppingDistance;
        }
        static bool IsDistanceInvalid(float d){
         return float.IsNaN(d)||float.IsInfinity(d)||d<0f;
        }
        internal virtual bool IsTraversingPath(){
         return simNavMeshAgentState.IsTraversing();
        }
        internal virtual bool HasReachedDestination(){
         return simNavMeshAgentState.HasReached();
        }
        internal virtual bool HasFailedToRoute(){
         return simNavMeshAgentState.RouteFail();
        }
        internal virtual bool RouteTo(Vector3 worldPosition){
         return RouteToObjectRadius(worldPosition,null,null);
        }
        internal virtual bool RouteToObjectRadius(Vector3 worldPosition,InteractionSlot interactionSlot,SimObjectPart simObjectPart){
         if(interactionSlot!=null){
          worldPosition=interactionSlot.transform.position;
         }else if(simObjectPart!=null){
          worldPosition=simObjectPart.transform.position;
         }
         return simDescription.simMovement.GoTo(worldPosition);
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
        internal bool TryGetNavMeshPosition(Vector3 target,out Vector3 navPos){
         if(NavMesh.SamplePosition(target,out NavMeshHit hitResult,simCharacterController.height,simNavMeshQueryFilter)){
          navPos=hitResult.position+Vector3.up*simNavMeshAgent.height*0.5f;
          return true;
         }
         navPos=default;
         return false;
        }
        internal void OnReceiveInteractionIntent(InputIntent intent){
         simDescription.simInteractionResolver.ResolveInteractionIntent(intent);
        }
        internal virtual void OnDrawGizmos(){
         if(simCharacterController!=null){
          DrawGizmos.WireCapsule(transform.position,simCharacterController.height,simCharacterController.radius,Color.blue);
         }
        }
    }
    internal static class NavMeshAgentExtensions{
        internal static bool NotFollowingPath(this NavMeshAgentState s){
         return s==NavMeshAgentState.Disabled||
                s==NavMeshAgentState.Idle    ||
                s==NavMeshAgentState.Reached ||
                s==NavMeshAgentState.Unreachable;
        }
        internal static bool IsTraversing(this NavMeshAgentState s){
         return s==NavMeshAgentState.Pending   ||
                s==NavMeshAgentState.Travelling||
                s==NavMeshAgentState.Partial   ||
                s==NavMeshAgentState.Stuck;
        }
        internal static bool HasReached(this NavMeshAgentState s){
         return s==NavMeshAgentState.Reached;
        }
        internal static bool RouteFail(this NavMeshAgentState s){
         return s==NavMeshAgentState.Unreachable;
        }
        internal static bool Halted(this NavMeshAgentState s){
         return s==NavMeshAgentState.Disabled;
        }
    }
}