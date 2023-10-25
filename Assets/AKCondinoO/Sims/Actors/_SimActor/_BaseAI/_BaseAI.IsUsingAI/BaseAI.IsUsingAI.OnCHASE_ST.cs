#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Coroutine onChaseGetDataCoroutine;
     protected WaitUntil onChaseGetDataThrottler;
      protected float onChaseGetDataThrottlerInterval=.125f;
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
          if(characterController!=null){
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(characterController.character,transform.root);
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
            for(int i=onChaseInTheWayColliderHits.Length-1;i>=0;--i){
             if(i>=onChaseInTheWayColliderHitsCount){
              onChaseInTheWayColliderHits[i]=default(RaycastHit);
              continue;
             }
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root==this.transform.root){
              onChaseInTheWayColliderHits[i]=default(RaycastHit);
              onChaseInTheWayColliderHitsCount--;
              continue;
             }
            }
            Array.Sort(onChaseInTheWayColliderHits,OnChaseInTheWayColliderHitsArraySortComparer);
           }
          }
         }
         goto Loop;
        }
        //  ordena 'a' relativo a 'b', e retorna 'a' antes de 'b' se 'a' for menor que 'b'
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
         return Vector3.Distance(transform.root.position,a.point).CompareTo(Vector3.Distance(transform.root.position,b.point));
        }
        protected virtual void OnCHASE_ST_Start(){
         characterController.character.transform.localRotation=Quaternion.identity;
         onChaseMyEnemyPos=onChaseMyEnemyPos_Last=MyEnemy.transform.position;
         onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
         onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
         onChaseMyEnemyMovedSoChangeDestination=true;
         onChaseTookTooLongTimer=0f;
         onChaseTookTooLongCount=0;
         onChasePathfinderTimeoutCount=0;
         onChasePathfinderUnreachableCount=0;
         onChaseNoSpeedCount=0;
        }
     protected OnChaseTimeoutReactionCodes onChaseTimeoutReactionCode;
        internal enum OnChaseTimeoutReactionCodes:int{
         Random=8,
         GoLeft=16,
         GoRight=24,
         ResetCounter=32,
        }
     protected int onChaseTimeoutFailCount=0;
        protected virtual void OnChaseTimeoutFail(){
         if(++onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.ResetCounter){
          onChaseTimeoutFailCount=0;
         }
         if      (onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoRight){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoRight;
         }else if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoLeft){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoLeft;
         }else if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.Random){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.Random;
         }
        }
     protected Vector3 onChaseMyEnemyPos,onChaseMyEnemyPos_Last;
     protected float onChaseRenewDestinationTimeInterval=4f;
      protected float onChaseRenewDestinationTimer=4f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimeInterval=.2f;
      protected float onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
       protected bool onChaseMyEnemyMovedSoChangeDestination=true;
     protected float onChaseTookTooLongTime=7f;
      protected float onChaseTookTooLongTimer=0f;
       protected int onChaseTookTooLongCount=0;
       protected int onChaseTookTooLongCountResetAt=3;
        protected int[]onChaseTookTooLongCountToTeleport=new int[]{3,};
     protected int onChasePathfinderTimeoutCount=0;
     protected int onChasePathfinderTimeoutCountResetAt=3;
      protected int[]onChasePathfinderTimeoutCountToTeleport=new int[]{3,};
     protected int onChasePathfinderUnreachableCount=0;
     protected int onChasePathfinderUnreachableCountResetAt=3;
      protected int[]onChasePathfinderUnreachableCountToTeleport=new int[]{3,};
     protected int onChaseNoSpeedCount=0;
     protected int onChaseNoSpeedCountResetAt=3;
      protected int[]onChaseNoSpeedCountToTeleport=new int[]{3,};
        protected virtual void OnCHASE_ST(){
         stopPathfindingOnTimeout=false;//
         bool teleported=false;
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
           onChasePathfinderTimeoutCount++;
           if(Array.Exists(onChasePathfinderTimeoutCountToTeleport,element=>element==onChasePathfinderTimeoutCount)){
            Log.DebugMessage("onChasePathfinderTimeoutCountToTeleport");
            OnChaseTeleport();
           }
           if(onChasePathfinderTimeoutCount>=onChasePathfinderTimeoutCountResetAt){
            Log.DebugMessage("onChasePathfinderTimeoutCountResetAt");
            onChasePathfinderTimeoutCount=0;
           }
          }
          if(MyPathfinding==PathfindingResult.UNREACHABLE){
           onChasePathfinderUnreachableCount++;
           if(Array.Exists(onChasePathfinderUnreachableCountToTeleport,element=>element==onChasePathfinderUnreachableCount)){
            Log.DebugMessage("onChasePathfinderUnreachableCountToTeleport");
            OnChaseTeleport();
           }
           if(onChasePathfinderUnreachableCount>=onChasePathfinderUnreachableCountResetAt){
            Log.DebugMessage("onChasePathfinderUnreachableCountResetAt");
            onChasePathfinderUnreachableCount=0;
           }
          }
          moveToDestination|=true;
         }
         if(MyPathfinding==PathfindingResult.TRAVELLING_BUT_NO_SPEED){
          onChaseNoSpeedCount++;
          if(Array.Exists(onChaseNoSpeedCountToTeleport,element=>element==onChaseNoSpeedCount)){
           Log.DebugMessage("onChaseNoSpeedCountToTeleport");
           OnChaseTeleport();
          }
          if(onChaseNoSpeedCount>=onChaseNoSpeedCountResetAt){
           Log.DebugMessage("onChaseNoSpeedCountResetAt");
           onChaseNoSpeedCount=0;
          }
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
         onChaseTookTooLongTimer+=Time.deltaTime;
         if(onChaseTookTooLongTimer>=onChaseTookTooLongTime){
          onChaseTookTooLongTimer=0f;
          onChaseTookTooLongCount++;
          if(Array.Exists(onChaseTookTooLongCountToTeleport,element=>element==onChaseTookTooLongCount)){
           Log.DebugMessage("onChaseTookTooLongCountToTeleport");
           OnChaseTeleport();
          }
          if(onChaseTookTooLongCount>=onChaseTookTooLongCountResetAt){
           Log.DebugMessage("onChaseTookTooLongCountResetAt");
           onChaseTookTooLongCount=0;
          }
         }
         if(MyEnemy is BaseAI enemyAI&&enemyAI.enemy!=null&&enemyAI.enemy.id==masterId){
         }
         if(MyPathfinding==PathfindingResult.TRAVELLING_BUT_UNREACHABLE){
         }
         if(teleported){
         }
         void OnChaseTeleport(){
          if(teleported){return;}
          if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
           teleport.targetDest=MyEnemy.transform.position;
           teleport.cooldown=0f;
           teleport.useRandom=true;
           teleport.randomMaxDis=MyAttackRange.z;
           teleport.DoSkill(this,1);
           teleported=true;
          }
         }
         if(moveToDestination){
          MyDest=MyEnemy.transform.position;
          if(onChaseInTheWayColliderHitsCount>0){
           if(characterController!=null){
            for(int i=0;i<onChaseInTheWayColliderHitsCount;++i){
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit&&actorHit.characterController!=null&&(actorHit.transform.root.position-transform.root.position).sqrMagnitude<(MyEnemy.transform.root.position-transform.root.position).sqrMagnitude){
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
              float rightDis=3.0f;
              float forwardDis=1.5f;
              if      (onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.Random){
               rightSign=math_random.CoinFlip()?-1:1;
               rightDis=(float)math_random.NextDouble(2.0d,6d);
               forwardDis=(float)math_random.NextDouble(1.0d,6d);
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoLeft){
               rightSign=-1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoRight){
               rightSign=1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }
              MyDest=actorHit.transform.root.position+((right*rightSign)*rightDis-forward*forwardDis)*(actorHit.characterController.character.radius+characterController.character.radius)+Vector3.down*(height/2f);
              break;
             }
            }
           }
          }
          navMeshAgent.destination=MyDest;
         }
        }
    }
}