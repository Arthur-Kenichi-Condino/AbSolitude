#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI:SimActor{
     internal static System.Random seedGenerator;
     internal System.Random math_random;
        protected override void Awake(){
         math_random=new System.Random(seedGenerator.Next());
         base.Awake();
        }
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;
      internal ActorMotion motion{get{return MyMotion;}}
     protected State MyState=State.IDLE_ST;
      internal State state{get{return MyState;}}
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;
      internal PathfindingResult pathfinding{get{return MyPathfinding;}}
     protected WeaponTypes MyWeaponType=WeaponTypes.None;
      internal WeaponTypes weaponType{get{return MyWeaponType;}}
        protected override void AI(){
         base.AI();
         MyPathfinding=GetPathfindingResult();
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyState==State.FOLLOW_ST){
         }else{
          OnIDLE_ST();
         }
         UpdateMotion(true);
        }
        protected override void OnCharacterControllerUpdated(){
         base.OnCharacterControllerUpdated();
         UpdateMotion(false);
        }
     [SerializeField]protected bool doIdleMove=true;
     [SerializeField]protected float useRunSpeedChance=0.5f;
     [SerializeField]protected float delayToRandomMove=8.0f;
     protected float timerToRandomMove=2.0f;
        protected virtual void OnIDLE_ST(){
         if(MySkill!=null){
          DoSkill();
         }
         if(
          MyPathfinding==PathfindingResult.IDLE||
          MyPathfinding==PathfindingResult.REACHED
         ){
          if(timerToRandomMove>0.0f){
             timerToRandomMove-=Core.magicDeltaTimeNumber;
          }else if(doIdleMove){
             timerToRandomMove=delayToRandomMove;
           Log.DebugMessage("can do random movement");
           if(GetRandomPosition(transform.position,8.0f,out Vector3 result)){
            //Log.DebugMessage("got random position:"+result);
            bool run=Mathf.Clamp01((float)math_random.NextDouble())<useRunSpeedChance;
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