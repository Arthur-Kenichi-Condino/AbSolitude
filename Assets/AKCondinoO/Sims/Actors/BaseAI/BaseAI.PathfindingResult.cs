#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
namespace AKCondinoO.Sims.Actors {
    internal partial class BaseAI{
     [SerializeField]protected float pathfindingTimeout=12f;
      protected float pathfindingTimer;
      [SerializeField]protected float pathPendingTimeout=12f;
       protected float pathPendingTimer;
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
           navMeshAgent.destination=navMeshAgent.transform.position;
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
           navMeshAgent.destination=navMeshAgent.transform.position;
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
    }
}