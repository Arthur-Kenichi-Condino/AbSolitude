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
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal State state{get{return(!isUsingAI||ai==null)?State.PLAYER_CONTROLLED_ST:ai.MyState;}}
        internal partial class AI{
         internal State MyState{
          get{return(!me.isUsingAI)?State.PLAYER_CONTROLLED_ST:aiState;}
          set{aiState=value;}
         }protected State aiState=State.IDLE_ST;
          State lastState;
         [NonSerialized]SimObject currentEnemy;
         [NonSerialized]internal BaseAI myEnemyBaseAI;
         [NonSerialized]internal Vector3 myEnemyPos,previousMyEnemyPos;
         [NonSerialized]internal bool myEnemyChangedPos;
         [NonSerialized]internal float myMoveSpeed;
         [NonSerialized]internal float myEnemyMoveSpeed;
          [NonSerialized]internal Vector3 myEnemyMoveDir;
         [NonSerialized]internal Vector3 dirFromMyEnemyToMe;
         [NonSerialized]internal bool traveling;
         [NonSerialized]protected float travelMaxTime=2f;
          [NonSerialized]protected float travelTime;
         [NonSerialized]internal float predictMyEnemyDestDis;
         [NonSerialized]protected float renewDestinationMyEnemyMovedTimeInterval=.125f/2f;
          [NonSerialized]protected float renewDestinationMyEnemyMovedTimer;
         [NonSerialized]internal bool setDest;
         [NonSerialized]internal bool shouldPredictMyEnemyDest;
          [NonSerialized]internal float shouldPredictMyEnemyDestPredictionTimeSpan=10f;
           [NonSerialized]internal float shouldPredictMyEnemyDestConsiderInFrontAngle=45.0f;
         [NonSerialized]readonly List<BaseAI>actorsHitInTheWay=new();
          [NonSerialized]internal bool shouldAvoidActorWhileChasing;
           [NonSerialized]internal float actorAvoidanceWhileChasingTime=3.5f;
            [NonSerialized]internal float actorAvoidanceWhileChasingTimer;
             [NonSerialized]internal float actorAvoidanceWhileChasingCooldown=3.5f;
              [NonSerialized]internal float actorAvoidanceWhileChasingOnCooldownTimer;
          [NonSerialized]internal Vector3 enemyAtPosOnPredictDest;
         [NonSerialized]internal bool shouldAttack;
          [NonSerialized]internal bool priorityAttack;
           [NonSerialized]internal float forcedAttackOnAvoidanceCooldown=2.5f;
            [NonSerialized]internal float forcedAttackOnAvoidanceCooldownTimer;
         [NonSerialized]readonly List<BaseAI>actorsHitInTheWayOnAttack=new();
          [NonSerialized]internal bool shouldAvoidActorBeforeAttack;
           [NonSerialized]internal float actorAvoidanceBeforeAttackTime=2.5f;
            [NonSerialized]internal float actorAvoidanceBeforeAttackTimer;
             [NonSerialized]internal float actorAvoidanceBeforeAttackCooldown=2.5f;
              [NonSerialized]internal float actorAvoidanceBeforeAttackOnCooldownTimer;
            void UpdateMyState(){
             if(me.IsDead()){
              SetMyState(State.DEAD_ST);
              goto _MyStateSet;
             }
             bool turnToDest=false;
             bool turnToEnemy=false;
             lastState=MyState;
             if(MyEnemy!=currentEnemy){
              //Log.DebugMessage("new MyEnemy:"+MyEnemy);
              myEnemyBaseAI=MyEnemy as BaseAI;
              if(MyEnemy==null){
               myEnemyPos=previousMyEnemyPos=me.transform.position;
              }else{
               myEnemyPos=previousMyEnemyPos=MyEnemy.transform.position;
              }
              enemyAtPosOnPredictDest=myEnemyPos;
              currentEnemy=MyEnemy;
             }
             traveling=me.IsTraversingPath();
             if(traveling){
              travelTime+=Time.deltaTime;
             }else{
              travelTime=0f;
             }
             if(traveling){
              turnToDest=true;
              if(
               MyPathfinding==PathfindingResult.TRAVELLING_BUT_NO_SPEED||
               MyPathfinding==PathfindingResult.TRAVELLING_BUT_UNREACHABLE
              ){
               //  traveling:no movement detected
               Log.DebugMessage("traveling:no movement detected:"+MyPathfinding,this.me);
              }
             }
             if(
              !traveling||
              Vector3.Distance(
               me.navMeshAgent.destination,me.navMeshAgent.transform.position
              )<=me.navMeshAgent.stoppingDistance
             ){
              //  standing still:no movement detected
              Log.DebugMessage("standing still:no movement detected");
             }
             myMoveSpeed=Mathf.Max(
              me.moveMaxVelocity.x,
              me.moveMaxVelocity.y,
              me.moveMaxVelocity.z
             );
             dirFromMyEnemyToMe=Vector3.zero;
             myEnemyMoveSpeed=0f;
             myEnemyMoveDir=Vector3.zero;
             if(MyEnemy==null){
              myEnemyChangedPos=false;
             }else{
              dirFromMyEnemyToMe=(me.transform.position-MyEnemy.transform.position).normalized;
              previousMyEnemyPos=myEnemyPos;
              myEnemyPos=MyEnemy.transform.position;
              myEnemyChangedPos=(myEnemyPos!=previousMyEnemyPos);
              if(myEnemyChangedPos){
               myEnemyMoveDir=(myEnemyPos-previousMyEnemyPos).normalized;
              }else{
               myEnemyMoveDir=myEnemyBaseAI.characterController.transform.forward;
              }
              if(myEnemyBaseAI!=null){
               myEnemyMoveSpeed=Mathf.Max(
                myEnemyBaseAI.moveMaxVelocity.x,
                myEnemyBaseAI.moveMaxVelocity.y,
                myEnemyBaseAI.moveMaxVelocity.z
               );
              }
             }
             float dis1=     myMoveSpeed*Time.deltaTime*shouldPredictMyEnemyDestPredictionTimeSpan;
             float dis2=myEnemyMoveSpeed*Time.deltaTime*shouldPredictMyEnemyDestPredictionTimeSpan;
             float ratio;
             if(dis2<=0f){
              ratio=1f;
             }else{
              ratio=dis1/dis2;
             }
             //Log.DebugMessage("dis1:"+dis1+";dis2:"+dis2+";ratio:"+ratio);
             predictMyEnemyDestDis=ratio*dis1;
             setDest=false;
             shouldAttack=false;
             if(!traveling){
              setDest=true;
             }
             bool predict=false;
             if(renewDestinationMyEnemyMovedTimer>0f){
              renewDestinationMyEnemyMovedTimer-=Time.deltaTime;
              if(renewDestinationMyEnemyMovedTimer<=0f){
               setDest=true;
               predict=true;
              }
             }
             if(myEnemyChangedPos){
              //Log.DebugMessage("myEnemyChangedPos");
              if(renewDestinationMyEnemyMovedTimer<=0f){
               renewDestinationMyEnemyMovedTimer=renewDestinationMyEnemyMovedTimeInterval;
              }
             }
             if(shouldAvoidActorWhileChasing){
              if(actorAvoidanceWhileChasingTimer<=0f){
               actorAvoidanceWhileChasingTimer=actorAvoidanceWhileChasingTime;
              }
              if(actorAvoidanceWhileChasingTimer>0f){
               actorAvoidanceWhileChasingTimer-=Time.deltaTime;
               if(actorAvoidanceWhileChasingTimer<=0f){
                shouldAvoidActorWhileChasing=false;
                actorAvoidanceWhileChasingOnCooldownTimer=actorAvoidanceWhileChasingCooldown;
               }
              }
             }
             if(shouldAvoidActorBeforeAttack){
              if(actorAvoidanceBeforeAttackTimer<=0f){
               actorAvoidanceBeforeAttackTimer=actorAvoidanceBeforeAttackTime;
              }
              if(actorAvoidanceBeforeAttackTimer>0f){
               actorAvoidanceBeforeAttackTimer-=Time.deltaTime;
               if(actorAvoidanceBeforeAttackTimer<=0f){
                shouldAvoidActorBeforeAttack=false;
                actorAvoidanceBeforeAttackOnCooldownTimer=actorAvoidanceBeforeAttackCooldown;
               }
              }
             }
             if(forcedAttackOnAvoidanceCooldownTimer>0f){
              forcedAttackOnAvoidanceCooldownTimer-=Time.deltaTime;
             }
             if(MyEnemy==null){
             }else{
              if(!setDest){
               if(!shouldPredictMyEnemyDest){
                if(!IsInRange(
                  MyDest,
                  MyEnemy.transform.position,
                  attackDistance,
                  me.GetRadius(),
                  MyEnemy.GetRadius()
                 )
                ){
                 setDest=true;
                }
               }else{
                if(!IsInRange(
                  enemyAtPosOnPredictDest,
                  MyEnemy.transform.position,
                  attackDistance,
                  me.GetRadius(),
                  MyEnemy.GetRadius()
                 )
                ){
                 setDest=true;
                }
               }
              }
             }
             if(setDest){
              turnToDest=true;
              shouldPredictMyEnemyDest=predict;
              if(shouldPredictMyEnemyDest){
               //Log.DebugMessage("shouldPredictMyEnemyDest");
               enemyAtPosOnPredictDest=myEnemyPos;
              }
             }
             if(MyEnemy==null){
             }else{
              if(shouldPredictMyEnemyDest){
               if(Vector3.Angle(myEnemyMoveDir,dirFromMyEnemyToMe)<=shouldPredictMyEnemyDestConsiderInFrontAngle){
                //Log.DebugMessage("estou prevendo posição do inimigo mas já estou na frente");
                shouldPredictMyEnemyDest=false;
               }
              }
              Vector3 chaseStDest=MyDest;
              Vector3 chaseStDestWithAvoidance=chaseStDest;
              if(setDest){
               chaseSt.GetMyDest(out chaseStDest,actorsHitInTheWay);
               if(actorAvoidanceWhileChasingOnCooldownTimer<=0f){
                if(actorsHitInTheWay.Count>0){
                 chaseSt.GetMyDestWithAvoidance(actorsHitInTheWay,out chaseStDestWithAvoidance);
                 Debug.DrawLine(me.transform.position,chaseStDestWithAvoidance,Color.yellow,1f);
                 chaseStDest=chaseStDestWithAvoidance;
                 //Log.DebugMessage("avoiding actorsHitInTheWay on chaseSt"+me.name);
                 shouldAvoidActorWhileChasing=true;
                }
               }
               if(actorAvoidanceWhileChasingOnCooldownTimer>0f){
                actorAvoidanceWhileChasingOnCooldownTimer-=Time.deltaTime;
               }
              }
              if(setDest){
               MyDest=chaseStDest;
              }
              if(priorityAttack){
               turnToEnemy|=true;
              }
              if(isInAttackRange){
               turnToEnemy|=true;
             ////  if(setDest){//  not traveling
             ////   if(IsInRange(chaseStDest,me.navMeshAgent.transform.position,attackDistance,me.GetRadius(),MyEnemy.GetRadius())){
             ////    SetMyState(State.ATTACK_ST);
             ////    goto _MyStateSet;
             ////   }
             ////  }else{//  is traveling
             ////   if(IsInRange(MyDest     ,me.navMeshAgent.transform.position,attackDistance,me.GetRadius(),MyEnemy.GetRadius())){
             ////    SetMyState(State.ATTACK_ST);
             ////    goto _MyStateSet;
             ////   }
             ////  }
             //// }else{
              }
              Vector3 attackStDest=MyDest;
              Vector3 attackStDestWithAvoidance=attackStDest;
              if(turnToEnemy){
               if(me.TurnToMyEnemy()){
                //Log.DebugMessage("can attack");
                shouldAttack=true;
               }
              }else if(turnToDest){
               if(me.TurnToMoveDest()){
                //Log.DebugMessage("turned to destination");
               }
              }
              if(shouldAttack){
               attackSt.GetMyDest(out attackStDest,actorsHitInTheWayOnAttack);
               if(actorAvoidanceBeforeAttackOnCooldownTimer<=0f){
                if(actorsHitInTheWayOnAttack.Count>0){
                 //Log.DebugMessage("actorsHitInTheWayOnAttack.Count>0");
                 attackSt.GetMyDestWithAvoidance(actorsHitInTheWayOnAttack,out attackStDestWithAvoidance);
                 Debug.DrawLine(me.transform.position,attackStDestWithAvoidance,Color.yellow,1f);
                 attackStDest=attackStDestWithAvoidance;
                 shouldAvoidActorBeforeAttack=true;
                }
               }
               if(actorAvoidanceBeforeAttackOnCooldownTimer>0f){
                actorAvoidanceBeforeAttackOnCooldownTimer-=Time.deltaTime;
               }
               MyDest=attackStDest;
               SetMyState(State.ATTACK_ST);
               goto _MyStateSet;
              }
              //MyDest=MyEnemy.transform.position;
             //if(
             // IsTraversingPath()
             //){
             // if(
             //  IsAttacking()
             // ){
             //  MoveStop();
             //  if(ai.MyEnemy!=null){
             //   TurnToMyEnemy();
             //  }
             // }else{
             //  if(!TurnToMoveDest()){
             //   if(movePauseDelay<=0f){
             //    MovePause();
             //   }
             //  }else{
             //   MoveResume();
             //  }
             // }
             //}
              SetMyState(State.CHASE_ST);
              goto _MyStateSet;
             }
             SetMyState(State.IDLE_ST);
             goto _MyStateSet;
             //if(me.IsDead()){
             // SetMyState(State.DEAD_ST);
             // goto _MyStateSet;
             //}else{
             // if      (MyState==State. SNIPE_ST&&isInAttackRangeWithWeapon){
             //  //  so the animation is completed
             //  if(me.IsShooting()||me.IsReloading()){
             //   SetMyState(State. SNIPE_ST);
             //   goto _MyStateSet;
             //  }
             // }else if(MyState==State.ATTACK_ST&&isInAttackRange          ){
             //  //  so the animation is completed
             //  if(me.IsAttacking()||attackSt.firstAttack){
             //   SetMyState(State.ATTACK_ST);
             //   goto _MyStateSet;
             //  }
             // }
             // if(MyEnemy!=null){
             //  bool canSnipe=snipeSt.tryCount<snipeSt.maxTryCount;
             //  if(snipeSt.onStateCooldown>0f){
             //   snipeSt.onStateCooldown-=Time.deltaTime;
             //  }
             //  canSnipe&=snipeSt.onStateCooldown<=0f;
             //  if(canSnipe){
             //   if(me.sniper){
             //    if(isInAttackRangeWithWeapon&&hasWeapon){
             //     if(!isInAttackRange||(
             //       me.IsFasterThan(MyEnemy)&&(
             //       attackDistance.z<attackDistanceWithWeapon.z||
             //       attackDistance.x<attackDistanceWithWeapon.x||
             //       attackDistance.y<attackDistanceWithWeapon.y)
             //      )
             //     ){
             //      SetMyState(State.SNIPE_ST);
             //      goto _MyStateSet;
             //     }
             //    }
             //   }
             //  }
             //  if(isInAttackRange){
             //   SetMyState(State.ATTACK_ST);
             //   goto _MyStateSet;
             //  }
             //  if(canSnipe){
             //   if(MyState==State.SNIPE_ST){
             //    if(snipeSt.timer<snipeSt.minTimeBeforeCanChase){
             //     SetMyState(State.SNIPE_ST);
             //     goto _MyStateSet;
             //    }
             //   }
             //  }
             //  SetMyState(State.CHASE_ST);
             //  goto _MyStateSet;
             // }else{
             //  if(me.masterId!=null){
             //   float disToMaster=me.GetDistance(me,me.masterSimObject);
             //   if(disToMaster>=0f){
             //    if(disToMaster>8f){
             //     //Log.DebugMessage("I should follow my master:"+me.masterSimObject+";me:"+me);
             //     SetMyState(State.FOLLOW_ST);
             //     goto _MyStateSet;
             //    }
             //   }
             //  }
             //  SetMyState(State.IDLE_ST);
             //  goto _MyStateSet;
             // }
             //}
             _MyStateSet:{}
            }
            void SetMyState(State state){
             if(state!=lastState){
              if      (lastState==State. SNIPE_ST){
                snipeSt.Finish();
              }else if(lastState==State.ATTACK_ST){
               attackSt.Finish();
              }else if(lastState==State. CHASE_ST){
                chaseSt.Finish();
              }else if(lastState==State.  IDLE_ST){
                 //idleSt.Finish();
              }
              if      (state==State. SNIPE_ST){
                snipeSt.Start();
              }else if(state==State.ATTACK_ST){
               attackSt.Start();
              }else if(state==State. CHASE_ST){
                chaseSt.Start();
              }else if(state==State.FOLLOW_ST){
               //
              }else{
                 idleSt.Start();
              }
             }
             MyState=state;
            }
            void ProcessStateRoutine(){
             if      (MyState==State.  DEAD_ST){
              //
             }else if(MyState==State. SNIPE_ST){
               snipeSt.DoRoutine();
             }else if(MyState==State.ATTACK_ST){
              attackSt.DoRoutine();
             }else if(MyState==State. CHASE_ST){
               chaseSt.DoRoutine();
             }else if(MyState==State.FOLLOW_ST){
              followSt.DoRoutine();
             }else{
                idleSt.DoRoutine();
             }
            }
        }
        internal enum State:int{
         PLAYER_CONTROLLED_ST=13,
           IDLE_ST= 0,
         FOLLOW_ST= 1,
          CHASE_ST= 2,
         ATTACK_ST= 3,
          SNIPE_ST=14,
          EVADE_ST=15,
           DEAD_ST=16,
        }
    }
}