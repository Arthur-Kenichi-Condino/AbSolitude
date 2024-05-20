#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal Vector3 dest{get{return ai==null?transform.position:ai.MyDest;}}
        internal partial class AI{
         internal Vector3 MyDest;
        }
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;internal PathfindingResult pathfinding{get{return MyPathfinding;}}
        internal enum PathfindingResult:int{
         IDLE                      =0,
         REACHED                   =1,
         PENDING                   =2,
         TRAVELLING                =3,
         TRAVELLING_BUT_NO_SPEED   =4,
         TRAVELLING_BUT_UNREACHABLE=5,
         TIMEOUT                   =6,
         UNREACHABLE               =7,
         PAUSED                    =8,
        }
     [SerializeField]protected float pathfindingTimeout=8f;
      protected float pathfindingTimer;
      [SerializeField]protected float pathPendingTimeout=4f;
       protected float pathPendingTimer;
      protected bool stopPathfindingOnTimeout=true;
        PathfindingResult GetPathfindingResult(){
         if(Vector3.Distance(navMeshAgent.destination,transform.position)<=navMeshAgent.stoppingDistance){
          return PathfindingResult.IDLE;
         }
         if(movePaused){
          return PathfindingResult.PAUSED;
         }
         if(pathPendingTimer>0f){
            pathPendingTimer-=Time.deltaTime;
          if(pathPendingTimer<=0f){
           if(stopPathfindingOnTimeout){
            Log.DebugMessage("pathPendingTimer<=0f:PathfindingResult.TIMEOUT");
            MoveStop();
           }
           return PathfindingResult.TIMEOUT;
          }
         }
         if(navMeshAgent.pathPending){
          pathfindingTimer=0f;
          if(pathPendingTimer<=0f){
             pathPendingTimer=pathPendingTimeout;
          }
          return PathfindingResult.PENDING;
         }
         pathPendingTimer=0f;
         if(!navMeshAgent.hasPath){
          Log.DebugMessage("!navMeshAgent.hasPath");
          pathfindingTimer=0f;
          return PathfindingResult.IDLE;
         }
         if(
          navMeshAgent.remainingDistance==Mathf.Infinity||
          navMeshAgent.remainingDistance==float.NaN     ||
          navMeshAgent.remainingDistance<0
         ){
          Log.DebugMessage("navMeshAgent.remainingDistance invalid:"+navMeshAgent.remainingDistance);
          pathfindingTimer=0f;
          return PathfindingResult.IDLE;
         }
         if(pathfindingTimer>0f){
            pathfindingTimer-=Time.deltaTime;
          if(pathfindingTimer<=0f){
           if(stopPathfindingOnTimeout){
            Log.DebugMessage("pathfindingTimer<=0f:PathfindingResult.TIMEOUT");
            MoveStop();
           }
           return PathfindingResult.TIMEOUT;
          }
         }
         if(navMeshAgent.hasPath){
          if(navMeshAgent.pathStatus==NavMeshPathStatus.PathInvalid){
           return PathfindingResult.UNREACHABLE;
          }
         }
         if(navMeshAgent.remainingDistance>navMeshAgent.stoppingDistance){
          if(pathfindingTimer<=0f){
             pathfindingTimer=pathfindingTimeout;
          }
          if(Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
           return PathfindingResult.TRAVELLING_BUT_NO_SPEED;
          }
          if(navMeshAgent.pathStatus==NavMeshPathStatus.PathPartial){
           return PathfindingResult.TRAVELLING_BUT_UNREACHABLE;
          }
          return PathfindingResult.TRAVELLING;
         }
         pathfindingTimer=0f;
         return PathfindingResult.REACHED;
        }
        internal virtual bool IsTraversingPath(){
         return!(
          MyPathfinding==PathfindingResult.IDLE||
          MyPathfinding==PathfindingResult.REACHED||
          MyPathfinding==PathfindingResult.TIMEOUT||
          MyPathfinding==PathfindingResult.UNREACHABLE
         );
        }
    }
}