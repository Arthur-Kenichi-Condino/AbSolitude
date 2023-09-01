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
         if(onChaseGetDataCoroutine!=null){
          StopCoroutine(onChaseGetDataCoroutine);onChaseGetDataCoroutine=null;
         }
         onChaseGetDataCoroutine=StartCoroutine(OnChaseGetDataCoroutine());
        }
        internal override void OnDeactivated(){
         if(onChaseGetDataCoroutine!=null){
          StopCoroutine(onChaseGetDataCoroutine);onChaseGetDataCoroutine=null;
         }
         base.OnDeactivated();
         ReleaseEnemiesAndAllies();
        }
        protected override void OnResetMotion(){
         onHitSetMotion=false;
          onHitResetMotion=false;
         onDoAttackSetMotion=false;
         MyMotion=ActorMotion.MOTION_STAND;
         base.OnResetMotion();
        }
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;internal ActorMotion motion{get{return MyMotion;}}
     protected State MyState=State.IDLE_ST;internal State state{get{return MyState;}}
     protected Vector3 MyDest;internal Vector3 dest{get{return MyDest;}}
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
        protected virtual void OnCHASE_ST_START(){
         simActorCharacterController.characterController.transform.localRotation=Quaternion.identity;
         onChaseMyEnemyPos=onChaseMyEnemyPos_Last=MyEnemy.transform.position;
         onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
         onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
         onChaseMyEnemyMovedSoChangeDestination=true;
        }
     protected Coroutine onChaseGetDataCoroutine;
     protected WaitUntil onChaseGetDataThrottler;
      protected float onChaseGetDataThrottlerInterval=1f;
       protected float onChaseGetDataThrottlerTimer;
     protected RaycastHit[]onChaseInTheWayColliderHits=new RaycastHit[8];
      protected int onChaseInTheWayColliderHitsCount=0;
        protected virtual IEnumerator OnChaseGetDataCoroutine(){
         onChaseGetDataThrottler=new WaitUntil(
          ()=>{
           if(onChaseGetDataThrottlerTimer>0f){
            onChaseGetDataThrottlerTimer-=Time.deltaTime;
           }
           if(MyState==State.CHASE_ST){
            if(MyEnemy==null){
             return false;
            }
            if(onChaseGetDataThrottlerTimer<=0f){
             onChaseGetDataThrottlerTimer=onChaseGetDataThrottlerInterval;
             return true;
            }
           }
           return false;
          }
         );
         Loop:{
          yield return onChaseGetDataThrottler;
          //Log.DebugMessage("OnChaseGetDataCoroutine");
          if(simActorCharacterController!=null){
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(simActorCharacterController.characterController,transform.root);
           float maxDis=Vector3.Distance(MyEnemy.transform.position,transform.root.position);
           int inTheWayLength=0;
           _GetInTheWayColliderHits:{
            inTheWayLength=Physics.CapsuleCastNonAlloc(
             values.point0,
             values.point1,
             values.radius,
             (MyEnemy.transform.position-transform.root.position).normalized,
             onChaseInTheWayColliderHits,
             maxDis,
             PhysUtil.physObstaclesLayer
            );
           }
           if(inTheWayLength>0){
            if(inTheWayLength>=onChaseInTheWayColliderHits.Length){
             Array.Resize(ref onChaseInTheWayColliderHits,inTheWayLength*2);
             goto _GetInTheWayColliderHits;
            }
           }
           onChaseInTheWayColliderHitsCount=inTheWayLength;
           if(onChaseInTheWayColliderHitsCount>0){
            for(int i=onChaseInTheWayColliderHitsCount-1;i>=0;--i){
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root==this.transform.root){
              onChaseInTheWayColliderHits[i]=new RaycastHit();
              onChaseInTheWayColliderHitsCount--;
             }
            }
            Array.Sort(onChaseInTheWayColliderHits,OnChaseInTheWayColliderHitsArraySortComparer);
           }
          }
         }
         goto Loop;
        }
        private int OnChaseInTheWayColliderHitsArraySortComparer(RaycastHit a,RaycastHit b){
         if(a.collider==null&&b.collider==null){
          return 0;
         }
         if(a.collider==null&&b.collider!=null){
          return 1;
         }
         if(a.collider!=null&&b.collider==null){
          return -1;
         }
         return Vector3.Distance(transform.root.position,a.collider.transform.root.position).CompareTo(Vector3.Distance(transform.root.position,b.collider.transform.root.position));
        }
        internal enum OnChaseTimeoutReactionCodes:int{
         Random=0,
         GoLeft=22,
         GoRight=30,
         ResetCounter=38,
        }
     protected int onChaseTimeoutFailCount=0;
      protected OnChaseTimeoutReactionCodes onChaseTimeoutReactionCode;
        protected virtual void OnChaseTimeoutFail(){
         if(++onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.ResetCounter){
          onChaseTimeoutFailCount=0;
         }
         if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoLeft){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoLeft;
         }else if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoRight){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoRight;
         }else{
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.Random;
         }
        }
     protected Vector3 onChaseMyEnemyPos,onChaseMyEnemyPos_Last;
     protected float onChaseRenewDestinationTimeInterval=2f;
      protected float onChaseRenewDestinationTimer=2f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimeInterval=.2f;
      protected float onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
       protected bool onChaseMyEnemyMovedSoChangeDestination=true;
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
          if(MyPathfinding==PathfindingResult.TIMEOUT){
           OnChaseTimeoutFail();
          }
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
          MyDest=MyEnemy.transform.position;
          if(onChaseInTheWayColliderHitsCount>0){
           if(simActorCharacterController!=null){
            for(int i=0;i<onChaseInTheWayColliderHitsCount;++i){
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is SimActor actorHit&&actorHit.simActorCharacterController!=null&&(actorHit.transform.root.position-transform.root.position).sqrMagnitude<(MyEnemy.transform.root.position-transform.root.position).sqrMagnitude){
              Vector3 cross=Vector3.Cross(transform.root.position,actorHit.transform.root.position);
              //Debug.DrawLine(actorHit.transform.root.position,transform.root.position,Color.blue,1f);
              //Debug.DrawRay(actorHit.transform.root.position,cross,Color.cyan,1f);
              Vector3 right=cross;
              right.y=actorHit.transform.root.position.y;
              right.Normalize();
              //Debug.DrawRay(actorHit.transform.root.position,right,Color.cyan,1f);
              Vector3 cross2=Vector3.Cross(actorHit.transform.root.position+right,actorHit.transform.root.position+Vector3.up);
              Vector3 forward=cross2;
              forward.y=actorHit.transform.root.position.y;
              forward.Normalize();
              //Debug.DrawRay(actorHit.transform.root.position,forward,Color.cyan,1f);
              int rightSign=1;
              float rightDis=1.0f;
              if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.Random){
               rightSign=math_random.CoinFlip()?-1:1;
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoLeft){
               rightSign=-1;
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoRight){
               rightSign=1;
              }
              MyDest=actorHit.transform.root.position+((right*rightSign)*3.0f-forward*1.5f)*(actorHit.simActorCharacterController.characterController.radius+simActorCharacterController.characterController.radius)+Vector3.down*(height/2f);
              break;
             }
            }
           }
          }
          navMeshAgent.destination=MyDest;
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
           if(simUMA!=null){
            Vector3 animatorLookDir=-simUMA.transform.parent.forward;
            Vector3 animatorLookEuler=simUMA.transform.parent.eulerAngles;
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
        protected virtual void OnIDLE_ST_START(){
         simActorCharacterController.characterController.transform.localRotation=Quaternion.identity;
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