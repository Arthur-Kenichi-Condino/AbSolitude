#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal Vector3 dest{get{return ai==null?transform.position:ai.MyDest;}}
     internal PathfindingResult pathfinding{get{return ai==null?PathfindingResult.IDLE:ai.MyPathfinding;}}
        internal partial class AI{
         internal Vector3 MyDest;
         internal PathfindingResult MyPathfinding=PathfindingResult.IDLE;
        }
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
     protected float pathfindingTimeout=16f;
      protected float pathfindingTimer;
      protected float pathPendingTimeout=8f;
       protected float pathPendingTimer;
      protected bool stopPathfindingOnTimeout=true;
        PathfindingResult GetPathfindingResult(){
         if(Vector3.Distance(navMeshAgent.destination,navMeshAgent.transform.position)<=navMeshAgent.stoppingDistance){
          if(movePaused){
           return PathfindingResult.PAUSED;
          }
          return PathfindingResult.IDLE;
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
          if(movePaused){
           return PathfindingResult.PAUSED;
          }
          pathfindingTimer=0f;
          return PathfindingResult.IDLE;
         }
         if(
          navMeshAgent.remainingDistance==Mathf.Infinity||
          navMeshAgent.remainingDistance==float.NaN     ||
          navMeshAgent.remainingDistance<0
         ){
          Log.DebugMessage("navMeshAgent.remainingDistance invalid:"+navMeshAgent.remainingDistance);
          if(movePaused){
           return PathfindingResult.PAUSED;
          }
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
          if(movePaused){
           return PathfindingResult.PAUSED;
          }
          if(Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
           return PathfindingResult.TRAVELLING_BUT_NO_SPEED;
          }
          if(navMeshAgent.pathStatus==NavMeshPathStatus.PathPartial){
           return PathfindingResult.TRAVELLING_BUT_UNREACHABLE;
          }
          return PathfindingResult.TRAVELLING;
         }
         if(movePaused){
          return PathfindingResult.PAUSED;
         }
         pathfindingTimer=0f;
         return PathfindingResult.REACHED;
        }
        internal virtual bool IsTraversingPath(){
         return ai!=null&&!(
          ai.MyPathfinding==PathfindingResult.IDLE||
          ai.MyPathfinding==PathfindingResult.REACHED||
          ai.MyPathfinding==PathfindingResult.TIMEOUT||
          ai.MyPathfinding==PathfindingResult.UNREACHABLE
         );
        }
        protected override void OnDrawGizmos(){
         base.OnDrawGizmos();
         #if UNITY_EDITOR
          if(navMeshAgent!=null&&navMeshAgent.path!=null&&navMeshAgent.path.corners!=null){
           Color gizmosColor=Gizmos.color;
           Gizmos.color=Color.white;
           //  Draw lines joining each path corner
           Vector3[]pathCorners=navMeshAgent.path.corners;
           for(int i=0;i<pathCorners.Length-1;i++){
            Gizmos.DrawLine(pathCorners[i],pathCorners[i+1]);
           }
           Gizmos.color=gizmosColor;
          }
         #endif
        }
    }
}