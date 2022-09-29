#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI:SimActor{
     protected readonly System.Random dice=new System.Random();
        internal void OnSkillUsed(Skill skill){
         Log.DebugMessage("OnSkillUsed:"+skill);
         if(MySkill==skill){
          Log.DebugMessage("OnSkillUsed:MySkill==skill:clear used skill");
          MySkill=null;
          if(skill.revoked){
          }
          if(skill.done){
          }
         }
        }
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;
      internal ActorMotion motion{get{return MyMotion;}}
     protected State MyState=State.IDLE_ST;
      internal State state{get{return MyState;}}
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;
      internal PathfindingResult pathfinding{get{return MyPathfinding;}}
        protected override void AI(){
         base.AI();
         MyPathfinding=GetPathfindingResult();
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyState==State.FOLLOW_ST){
         }else{
          OnIDLE_ST();
         }
         if(MyPathfinding==PathfindingResult.TRAVELLING){
             MyMotion=ActorMotion.MOTION_MOVE;
         }else{
             MyMotion=ActorMotion.MOTION_STAND;
         }
        }
     protected Skill MySkill=null;
      internal Skill skillToUse{get{return MySkill;}}
      internal readonly HashSet<Skill>skillsToUse=new HashSet<Skill>();
     [SerializeField]protected bool doIdleMove=true;
     [SerializeField]protected float useRunSpeedChance=0.5f;
     [SerializeField]protected float delayToRandomMove=8.0f;
     protected float timerToRandomMove=2.0f;
        protected virtual void OnIDLE_ST(){
         if(MySkill!=null){
          if(MySkill is GenerateHomunculus generateHomunculusSkill){
           generateHomunculusSkill.DoSkill(this,generateHomunculusSkill.level);
          }
         }
         if(
          MyPathfinding==PathfindingResult.IDLE||
          MyPathfinding==PathfindingResult.REACHED
         ){
          if(timerToRandomMove>0.0f){
             timerToRandomMove-=Time.deltaTime;
          }else if(doIdleMove){
             timerToRandomMove=delayToRandomMove;
           Log.DebugMessage("can do random movement");
           if(GetRandomPosition(transform.position,8.0f,out Vector3 result)){
            //Log.DebugMessage("got random position:"+result);
            bool run=Mathf.Clamp01((float)dice.NextDouble())<useRunSpeedChance;
            if(navMeshAgentShouldUseRunSpeed||run){
             navMeshAgent.speed=navMeshAgentRunSpeed;
            }else{
             navMeshAgent.speed=navMeshAgentWalkSpeed;
            }
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