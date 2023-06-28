#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
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
         InitEnemiesAndAllies();
         base.Awake();
        }
        internal override bool IsMonster(){
         return MyAggressionMode==AggressionMode.AggressiveToAll;
        }
        internal override void OnActivated(){
         base.OnActivated();
        }
        internal override void OnDeactivated(){
         base.OnDeactivated();
         ReleaseEnemiesAndAllies();
        }
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;internal ActorMotion motion{get{return MyMotion;}}
     protected State MyState=State.IDLE_ST;internal State state{get{return MyState;}}
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;internal PathfindingResult pathfinding{get{return MyPathfinding;}}
     protected WeaponTypes MyWeaponType=WeaponTypes.None;internal WeaponTypes weaponType{get{return MyWeaponType;}}
        protected override void AI(){
         base.AI();
         RenewEnemiesAndAllies();
         MyPathfinding=GetPathfindingResult();
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyEnemy!=null){
          if(IsInAttackRange(MyEnemy)){
           MyState=State.ATTACK_ST;
          }else{
           if(MyState!=State.CHASE_ST){
            OnCHASE_ST_START();
           }
           MyState=State.CHASE_ST;
          }
         }else{
          if(MyState!=State.IDLE_ST){
           OnIDLE_ST_START();
          }
          MyState=State.IDLE_ST;
         }
         if      (MyState==State.FOLLOW_ST){
         }else if(MyState==State. CHASE_ST){
           OnCHASE_ST();
         }else if(MyState==State.ATTACK_ST){
          OnATTACK_ST();
         }else{
            OnIDLE_ST();
         }
         UpdateMotion(true);
        }
        protected override void OnCharacterControllerUpdated(){
         base.OnCharacterControllerUpdated();
         UpdateMotion(false);
        }
     protected Vector3 onChaseMyEnemyPos,onChaseMyEnemyPos_Last;
     protected float onChaseRenewDestinationTimeInterval=2f;
      protected float onChaseRenewDestinationTimer=2f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimeInterval=.2f;
      protected float onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
       protected bool onChaseMyEnemyMovedSoChangeDestination=true;
        protected virtual void OnCHASE_ST_START(){
         simActorCharacterController.characterController.transform.localRotation=Quaternion.identity;
         onChaseMyEnemyPos=onChaseMyEnemyPos_Last=MyEnemy.transform.position;
         onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
         onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
          onChaseMyEnemyMovedSoChangeDestination=true;
        }
        protected virtual void OnCHASE_ST(){
         if((onChaseMyEnemyPos_Last=onChaseMyEnemyPos)!=(onChaseMyEnemyPos=MyEnemy.transform.position)){
          onChaseMyEnemyMovedSoChangeDestination=true;
         }
         bool moveToDestination=false;
         if(onChaseRenewDestinationTimer>0f){
          onChaseRenewDestinationTimer-=Time.deltaTime;
         }
         if(onChaseRenewDestinationTimer<=0f){
          onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
          moveToDestination|=true;
         }
         if(
          !IsTraversingPath()
         ){
          moveToDestination|=true;
         }
         if(onChaseMyEnemyMovedSoChangeDestinationTimer>0f){
          onChaseMyEnemyMovedSoChangeDestinationTimer-=Time.deltaTime;
         }
         if(onChaseMyEnemyMovedSoChangeDestination){
          if(onChaseMyEnemyMovedSoChangeDestinationTimer<=0f){
           onChaseMyEnemyMovedSoChangeDestinationTimer=onChaseMyEnemyMovedSoChangeDestinationTimeInterval;
           onChaseMyEnemyMovedSoChangeDestination=false;
           moveToDestination|=true;
          }
         }
         if(moveToDestination){
          navMeshAgent.destination=MyEnemy.transform.position;
         }
        }
     internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(38,.0005f);
        protected virtual void OnATTACK_ST(){
         if(
          IsTraversingPath()
         ){
          navMeshAgent.destination=navMeshAgent.transform.position;
         }else{
          if(simActorCharacterController!=null){
           Vector3 lookDir=MyEnemy.transform.position-transform.position;
           Vector3 planarLookDir=lookDir;
           planarLookDir.y=0f;
           onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
           simActorCharacterController.characterController.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(simActorCharacterController.characterController.transform.rotation,Core.magicDeltaTimeNumber);
           Debug.DrawRay(simActorCharacterController.characterController.transform.position,simActorCharacterController.characterController.transform.forward,Color.gray);
           if(simUMAData!=null){
            Vector3 animatorLookDir=-simUMAData.transform.parent.forward;
            Vector3 animatorLookEuler=simUMAData.transform.parent.eulerAngles;
            animatorLookEuler.y+=180f;
            Vector3 animatorPlanarLookEuler=animatorLookEuler;
            animatorPlanarLookEuler.x=0f;
            animatorPlanarLookEuler.z=0f;
            Vector3 animatorPlanarLookDir=Quaternion.Euler(animatorPlanarLookEuler)*Vector3.forward;
            Debug.DrawRay(simActorCharacterController.characterController.transform.position,animatorPlanarLookDir,Color.white);
            if(Vector3.Angle(simActorCharacterController.characterController.transform.forward,animatorPlanarLookDir)<=5f){
             DoAttack();
            }
           }
          }
         }
        }
     [SerializeField]protected bool doIdleMove=true;
     [SerializeField]protected float useRunSpeedChance=0.5f;
     [SerializeField]protected float delayToRandomMove=8.0f;
     protected float timerToRandomMove=2.0f;
        protected virtual void OnIDLE_ST_START(){
         simActorCharacterController.characterController.transform.localRotation=Quaternion.identity;
        }
        protected virtual void OnIDLE_ST(){
         if(MySkill!=null){
          DoSkill();
         }
         if(
          !IsTraversingPath()
         ){
          if(timerToRandomMove>0.0f){
             timerToRandomMove-=Time.deltaTime;
          }else if(doIdleMove){
             timerToRandomMove=delayToRandomMove;
           //Log.DebugMessage("can do random movement");
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
          Vector3 randomPoint=Util.GetRandomPosition(center,maxDis);
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