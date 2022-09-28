#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
namespace AKCondinoO.Sims.Actors {
    internal partial class BaseAI{
        internal enum PathfindingResult:int{
         IDLE                   =0,
         REACHED                =1,
         PENDING                =2,
         TRAVELLING             =3,
         TRAVELLING_BUT_NO_SPEED=4,
        }
        PathfindingResult GetPathfindingResult(){
         if(navMeshAgent.pathPending){
          return PathfindingResult.PENDING;
         }
         if(!navMeshAgent.hasPath){
          return PathfindingResult.IDLE;
         }
         if(
          navMeshAgent.remainingDistance==Mathf.Infinity||
          navMeshAgent.remainingDistance==float.NaN     ||
          navMeshAgent.remainingDistance<0
         ){
          return PathfindingResult.IDLE;
         }
         if(navMeshAgent.remainingDistance>navMeshAgent.stoppingDistance){
          if(Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
           return PathfindingResult.TRAVELLING_BUT_NO_SPEED;
          }
          return PathfindingResult.TRAVELLING;
         }
         return PathfindingResult.REACHED;
        }
    }
}