#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
namespace AKCondinoO.Sims.Actors {
    internal partial class BaseAI{
     [SerializeField]protected float pathfindingTimeout=3f;
      protected float pathfindingTimer;
      [SerializeField]protected float pathPendingTimeout=6f;
       protected float pathPendingTimer;
      protected bool stopPathfindingOnTimeout=true;
        internal enum PathfindingResult:int{
         IDLE                   =0,
         REACHED                =1,
         PENDING                =2,
         TRAVELLING             =3,
         TRAVELLING_BUT_NO_SPEED=4,
         TIMEOUT                =5,
        }
        PathfindingResult GetPathfindingResult(){
         if(pathPendingTimer>0f){
            pathPendingTimer-=Time.deltaTime;
          if(pathPendingTimer<=0f){
           if(stopPathfindingOnTimeout){
            Log.DebugMessage("pathPendingTimer<=0f:PathfindingResult.TIMEOUT");
            navMeshAgent.destination=navMeshAgent.transform.position;
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
          pathfindingTimer=0f;
          return PathfindingResult.IDLE;
         }
         if(
          navMeshAgent.remainingDistance==Mathf.Infinity||
          navMeshAgent.remainingDistance==float.NaN     ||
          navMeshAgent.remainingDistance<0
         ){
          pathfindingTimer=0f;
          return PathfindingResult.IDLE;
         }
         if(pathfindingTimer>0f){
            pathfindingTimer-=Time.deltaTime;
          if(pathfindingTimer<=0f){
           if(stopPathfindingOnTimeout){
            Log.DebugMessage("pathfindingTimer<=0f:PathfindingResult.TIMEOUT");
            navMeshAgent.destination=navMeshAgent.transform.position;
           }
           return PathfindingResult.TIMEOUT;
          }
         }
         if(navMeshAgent.remainingDistance>navMeshAgent.stoppingDistance){
          if(pathfindingTimer<=0f){
             pathfindingTimer=pathfindingTimeout;
          }
          if(Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
           return PathfindingResult.TRAVELLING_BUT_NO_SPEED;
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
          MyPathfinding==PathfindingResult.TIMEOUT
         );
        }
    }
}