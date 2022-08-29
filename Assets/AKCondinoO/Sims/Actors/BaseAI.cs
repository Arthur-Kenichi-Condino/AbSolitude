#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal class BaseAI:SimActor{
        internal enum ActorMotion:int{
         MOTION_STAND=0,
         MOTION_MOVE =1,
        }
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;
      internal ActorMotion motion{get{return MyMotion;}}
        internal enum State:int{
         IDLE_ST  =0,
         FOLLOW_ST=1,
        }
     protected State MyState=State.IDLE_ST;
      internal State state{get{return MyState;}}
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
          if(Mathf.Approximately(navMeshAgent.velocity.sqrMagnitude,0f)){
           return PathfindingResult.TRAVELLING_BUT_NO_SPEED;
          }
          return PathfindingResult.TRAVELLING;
         }
         return PathfindingResult.REACHED;
        }
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;
        protected override void AI(){
         base.AI();
         MyPathfinding=GetPathfindingResult();
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyState==State.FOLLOW_ST){
         }else{
          OnIDLE_ST();
         }
        }
     [SerializeField]protected float delayToRandomMove=8.0f;
     protected float timerToRandomMove=2.0f;
        protected virtual void OnIDLE_ST(){
         if(
          MyPathfinding==PathfindingResult.IDLE||
          MyPathfinding==PathfindingResult.REACHED
         ){
          if(timerToRandomMove>0.0f){
             timerToRandomMove-=Time.deltaTime;
          }else{
             timerToRandomMove=delayToRandomMove;
           Log.DebugMessage("can do random movement");
           if(GetRandomPosition(transform.position,8.0f,out Vector3 result)){
            Log.DebugMessage("got random position:"+result);
            navMeshAgent.destination=result;
           }
          }
         }
        }
        //  [https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html]
        protected bool GetRandomPosition(Vector3 center,float maxDis,out Vector3 result){
         for(int i=0;i<3;++i){
          Vector3 randomPoint=center+UnityEngine.Random.insideUnitSphere*maxDis;
          if(NavMesh.SamplePosition(randomPoint,out NavMeshHit hit,Height,navMeshQueryFilter)){
           result=hit.position;
           return true;
          }
         }
         result=center;
         return false;
        }
    }
}