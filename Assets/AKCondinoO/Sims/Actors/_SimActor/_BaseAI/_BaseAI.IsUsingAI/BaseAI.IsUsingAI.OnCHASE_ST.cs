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
        protected void OnCHASE_ST_Reset(){
         ResetRotation(onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
        }
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
     [SerializeField]internal QuaternionRotLerpHelper onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,.5f);
     protected Vector3 onChaseMyEnemyPos,onChaseMyEnemyPos_Last;
     protected float onChaseRenewDestinationTimeInterval=4f;
     protected float onChaseRenewDestinationTimer=4f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimeInterval=.2f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
     protected bool  onChaseMyEnemyMovedSoChangeDestination=true;
     protected bool onChaseGetDestGoLeft;
     protected bool onChaseGetDestGoRight;
     protected bool onChaseGetDestGoRandom;
     protected int onChaseMaxCount=128;
     protected int onChaseCount;
     protected float onChaseTravelMaxTime=8f;
     protected float onChaseTravelTime;
     protected bool onChaseTraveledForTooLong;
     protected float onChaseTraveledForTooLongSubroutineMaxTime=8f;
     protected float onChaseTraveledForTooLongSubroutineTime;
     protected int onChaseTraveledForTooLongSubroutineDestModifiersChangeAfterMoves=2;
     protected int onChaseTraveledForTooLongSubroutineMoves;
     protected int onChase_TIMEOUT_MaxCount=2;
     protected int onChase_TIMEOUT_Count;
     protected bool onChaseTimedOut;
     protected float onChaseTimedOutSubroutineMaxTime=2f;
     protected float onChaseTimedOutSubroutineTime;
     protected int onChaseTimedOutSubroutineDestModifiersChangeAfterMoves=2;
     protected int onChaseTimedOutSubroutineMoves;
     protected int onChase_UNREACHABLE_MaxCount=2;
     protected int onChase_UNREACHABLE_Count;
     protected bool onChaseIsUnreachable;
     protected float onChaseIsUnreachableSubroutineMaxTime=2f;
     protected float onChaseIsUnreachableSubroutineTime;
     protected int onChaseIsUnreachableSubroutineDestModifiersChangeAfterMoves=2;
     protected int onChaseIsUnreachableSubroutineMoves;
     protected bool onChaseAlternateMoveAttack=false;
        protected virtual void OnCHASE_ST_Start(){
         onChaseMyEnemyPos=onChaseMyEnemyPos_Last=MyEnemy.transform.position;
         onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
         onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
         onChaseMyEnemyMovedSoChangeDestination=true;
         onChaseGetDestGoLeft=false;
         onChaseGetDestGoRight=false;
         onChaseGetDestGoRandom=false;
         onChaseTravelTime=0f;
         onChaseTraveledForTooLong=false;
         onChase_TIMEOUT_Count=0;
         onChaseTimedOut=false;
         onChase_UNREACHABLE_Count=0;
         onChaseIsUnreachable=false;
        }
        protected virtual void OnCHASE_ST_Routine(Vector3 attackDistance){
         if(MyEnemy==null){
          return;
         }
         stopPathfindingOnTimeout=false;//
         if(
          !IsTraversingPath()
         ){
          bool attack=false;
          if(onChaseAlternateMoveAttack){
           attack=true;
          }
          if(attack){
           if(IsAttacking()){
            onChaseAlternateMoveAttack=false;//  change when entered attack but left state too soon
            return;
           }else{
            OnCHASE_ST_Attack(true);
            return;
           }
          }
         }
         bool moveToDestination=false;
         if(onChaseMyEnemyMovedSoChangeDestinationTimer>0f){
          onChaseMyEnemyMovedSoChangeDestinationTimer-=Time.deltaTime;
         }
         if((onChaseMyEnemyPos_Last=onChaseMyEnemyPos)!=(onChaseMyEnemyPos=MyEnemy.transform.position)){
          onChaseMyEnemyMovedSoChangeDestination=true;
         }
         if(onChaseMyEnemyMovedSoChangeDestination){
          if(onChaseMyEnemyMovedSoChangeDestinationTimer<=0f){
           onChaseMyEnemyMovedSoChangeDestinationTimer=onChaseMyEnemyMovedSoChangeDestinationTimeInterval;
           onChaseMyEnemyMovedSoChangeDestination=false;
           moveToDestination|=true;
          }
         }
         if(onChaseRenewDestinationTimer>0f){
          onChaseRenewDestinationTimer-=Time.deltaTime;
         }
         if(onChaseRenewDestinationTimer<=0f){
          onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
          moveToDestination|=true;
         }
         _Beginning:{}
         if(!onChaseTraveledForTooLong){
          onChaseTraveledForTooLongSubroutineTime=0f;
          onChaseTraveledForTooLongSubroutineMoves=0;
         }
         if(onChaseTraveledForTooLong){
          moveToDestination|=!IsTraversingPath();
          OnCHASE_ST_SubroutineTraveledForTooLong(moveToDestination,attackDistance);
          return;
         }
         if(!onChaseTimedOut){
          onChaseTimedOutSubroutineTime=0f;
          onChaseTimedOutSubroutineMoves=0;
         }
         if(onChaseTimedOut){
          moveToDestination|=!IsTraversingPath();
          OnCHASE_ST_SubroutineTimedOut(moveToDestination,attackDistance);
          return;
         }
         if(!onChaseIsUnreachable){
          onChaseIsUnreachableSubroutineTime=0f;
          onChaseIsUnreachableSubroutineMoves=0;
         }
         if(onChaseIsUnreachable){
          moveToDestination|=!IsTraversingPath();
          OnCHASE_ST_SubroutineIsUnreachable(moveToDestination,attackDistance);
          return;
         }
         if(
          !IsTraversingPath()
         ){
          onChaseTravelTime=0f;
          moveToDestination|=true;
          if(MyPathfinding==PathfindingResult.TIMEOUT){
           onChase_TIMEOUT_Count++;
           if(onChase_TIMEOUT_Count>=onChase_TIMEOUT_MaxCount){
            onChaseTimedOut=true;
            goto _Beginning;
           }
          }
          if(MyPathfinding==PathfindingResult.UNREACHABLE){
           onChase_UNREACHABLE_Count++;
           if(onChase_UNREACHABLE_Count>=onChase_UNREACHABLE_MaxCount){
            onChaseIsUnreachable=true;
            goto _Beginning;
           }
          }
         }else{
          if(characterController!=null){
           Vector3 lookDir=MyEnemy.transform.position-transform.position;
           Vector3 planarLookDir=lookDir;
           planarLookDir.y=0f;
           onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
           characterController.character.transform.rotation=onChasePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
          }
          onChaseTravelTime+=Time.deltaTime;
          if(onChaseTravelTime>=onChaseTravelMaxTime){
           onChaseTravelTime=0f;
           onChaseTraveledForTooLong=true;
           goto _Beginning;
          }
          if(MyPathfinding==PathfindingResult.TRAVELLING_BUT_NO_SPEED||
             MyPathfinding==PathfindingResult.TRAVELLING_BUT_UNREACHABLE
          ){
          }
         }
         if(moveToDestination){
          onChaseCount++;
          if(onChaseCount>=onChaseMaxCount){
           onChaseCount=0;
           //if(MyEnemy is BaseAI enemyAI&&enemyAI.enemy!=null&&enemyAI.enemy.id==masterId){
           //}//  ...and/or took damage and delay
           OnCHASE_ST_Teleport(attackDistance);
           return;
          }
          OnCHASE_ST_Move(false);
          return;
         }
        }
        protected virtual void OnCHASE_ST_SubroutineTraveledForTooLong(bool moveToDestination,Vector3 attackDistance){
         onChaseTraveledForTooLongSubroutineTime+=Time.deltaTime;
         if(onChaseTraveledForTooLongSubroutineTime>=onChaseTraveledForTooLongSubroutineMaxTime){
          OnCHASE_ST_Teleport(attackDistance);
          onChaseTraveledForTooLong=false;
          return;
         }
         if(moveToDestination){
          onChaseTraveledForTooLongSubroutineMoves++;
          if(onChaseTraveledForTooLongSubroutineMoves%onChaseTraveledForTooLongSubroutineDestModifiersChangeAfterMoves==0){
           OnCHASE_ST_DestModifiersNext();
          }
          OnCHASE_ST_Move();
          return;
         }
        }
        protected virtual void OnCHASE_ST_SubroutineTimedOut(bool moveToDestination,Vector3 attackDistance){
         onChaseTimedOutSubroutineTime+=Time.deltaTime;
         if(onChaseTimedOutSubroutineTime>=onChaseTimedOutSubroutineMaxTime){
          OnCHASE_ST_Teleport(attackDistance);
          onChaseTimedOut=false;
          return;
         }
         if(moveToDestination){
          onChaseTimedOutSubroutineMoves++;
          if(onChaseTimedOutSubroutineMoves%onChaseTimedOutSubroutineDestModifiersChangeAfterMoves==0){
           OnCHASE_ST_DestModifiersNext();
          }
          OnCHASE_ST_Move();
          return;
         }
        }
        protected virtual void OnCHASE_ST_SubroutineIsUnreachable(bool moveToDestination,Vector3 attackDistance){
         onChaseIsUnreachableSubroutineTime+=Time.deltaTime;
         if(onChaseIsUnreachableSubroutineTime>=onChaseIsUnreachableSubroutineMaxTime){
          OnCHASE_ST_Teleport(attackDistance);
          onChaseIsUnreachable=false;
          return;
         }
         if(moveToDestination){
          onChaseIsUnreachableSubroutineMoves++;
          if(onChaseIsUnreachableSubroutineMoves%onChaseIsUnreachableSubroutineDestModifiersChangeAfterMoves==0){
           OnCHASE_ST_DestModifiersNext();
          }
          OnCHASE_ST_Move();
          return;
         }
        }
        protected virtual void OnCHASE_ST_Teleport(Vector3 attackDistance){
         if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
          teleport.targetDest=MyEnemy.transform.position;
          teleport.cooldown=0f;
          teleport.useRandom=true;
          teleport.randomMaxDis=attackDistance.z*1.1f;
          teleport.DoSkill(this,1);
         }
        }
        protected virtual void OnCHASE_ST_DestModifiersNext(){
         if(onChaseGetDestGoLeft){
          onChaseGetDestGoLeft=false;
          onChaseGetDestGoRight=true;
         }else if(onChaseGetDestGoRight){
          onChaseGetDestGoRight=false;
          onChaseGetDestGoRandom=true;
         }else if(onChaseGetDestGoRandom){
          onChaseGetDestGoRandom=false;
         }else{
          onChaseGetDestGoLeft=true;
         }
        }
        protected virtual void OnCHASE_ST_GetDest(bool useModifiers=true){
         MyDest=MyEnemy.transform.position;
         if(onChaseInTheWayColliderHitsCount>0){
          if(characterController!=null){
           for(int i=0;i<onChaseInTheWayColliderHitsCount;++i){
            RaycastHit hit=onChaseInTheWayColliderHits[i];
            if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit&&actorHit.characterController!=null&&(actorHit.transform.root.position-transform.root.position).sqrMagnitude<(MyEnemy.transform.root.position-transform.root.position).sqrMagnitude){
             Vector3 cross=Vector3.Cross(transform.root.position,actorHit.transform.root.position);
             //Debug.DrawLine(actorHit.transform.root.position,transform.root.position,Color.cyan,1f);
             //Debug.DrawRay(actorHit.transform.root.position,cross,Color.cyan,1f);
             Vector3 right=cross;
             right.y=0f;
             right.Normalize();
             //Debug.DrawRay(actorHit.transform.root.position,right,Color.cyan,1f);
             Vector3 cross2=Vector3.Cross(actorHit.transform.root.position+right,actorHit.transform.root.position+Vector3.up);
             Vector3 forward=cross2;
             forward.y=0f;
             forward.Normalize();
             //Debug.DrawRay(actorHit.transform.root.position,forward,Color.cyan,1f);
             int rightSign=1;
             float rightDis=3.0f;
             float forwardDis=1.5f;
             if(useModifiers){
              if(onChaseGetDestGoRandom){
               rightSign=math_random.CoinFlip()?-1:1;
               rightDis=(float)math_random.NextDouble(2.0d,6d);
               forwardDis=(float)math_random.NextDouble(1.0d,6d);
              }else if(onChaseGetDestGoLeft){
               rightSign=-1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }else if(onChaseGetDestGoRight){
               rightSign=1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }
             }
             MyDest=actorHit.transform.root.position+((right*rightSign)*rightDis-forward*forwardDis)*(actorHit.characterController.character.radius+characterController.character.radius)+Vector3.down*(height/2f);
             break;
            }
           }
          }
         }
        }
        protected virtual void OnCHASE_ST_Move(bool useModifiers=true){
         OnCHASE_ST_GetDest(useModifiers);
         Move(MyDest);
        }
        protected virtual void OnCHASE_ST_Attack(bool canAttack){
         if(LookToMyEnemy()){
          if(canAttack){
           DoAttackOnAnimationEvent();
          }
         }
        }
    }
}