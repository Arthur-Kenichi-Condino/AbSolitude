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
     internal State state{get{return ai==null?State.IDLE_ST:ai.MyState;}}
        internal partial class AI{
         internal State MyState=State.IDLE_ST;
          State lastState;
         [NonSerialized]SimObject currentEnemy;
         [NonSerialized]internal BaseAI myEnemyBaseAI;
         [NonSerialized]internal Vector3 myEnemyPos,previousMyEnemyPos;
         [NonSerialized]internal bool myEnemyChangedPos;
         [NonSerialized]internal float myMoveSpeed;
         [NonSerialized]internal float myEnemyMoveSpeed;
         [NonSerialized]internal bool traveling;
         [NonSerialized]protected float travelMaxTime=2f;
          [NonSerialized]protected float travelTime;
         [NonSerialized]internal float predictMyEnemyDestDis;
         [NonSerialized]protected float renewDestinationMyEnemyMovedTimeInterval=.125f/2f;
          [NonSerialized]protected float renewDestinationMyEnemyMovedTimer;
         [NonSerialized]internal bool setDest;
         [NonSerialized]internal bool shouldPredictMyEnemyDest;
         [NonSerialized]readonly List<BaseAI>actorsHitInTheWay=new();
          [NonSerialized]internal Vector3 enemyAtPosOnPredictDest;
         [NonSerialized]internal bool shouldAttack;
          [NonSerialized]internal bool tryingAttack;
            void UpdateMyState(){
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
             }
             myMoveSpeed=Mathf.Max(
              me.moveMaxVelocity.x,
              me.moveMaxVelocity.y,
              me.moveMaxVelocity.z
             );
             myEnemyMoveSpeed=0f;
             if(MyEnemy==null){
              myEnemyChangedPos=false;
             }else{
              previousMyEnemyPos=myEnemyPos;
              myEnemyPos=MyEnemy.transform.position;
              myEnemyChangedPos=(myEnemyPos!=previousMyEnemyPos);
              if(myEnemyBaseAI!=null){
               myEnemyMoveSpeed=Mathf.Max(
                myEnemyBaseAI.moveMaxVelocity.x,
                myEnemyBaseAI.moveMaxVelocity.y,
                myEnemyBaseAI.moveMaxVelocity.z
               );
              }
             }
             float dis1=     myMoveSpeed*Time.deltaTime*10.0f;
             float dis2=myEnemyMoveSpeed*Time.deltaTime*10.0f;
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
              Vector3 chaseStDest=MyDest;
              Vector3 chaseStDestWithAvoidance=chaseStDest;
              if(setDest){
               chaseSt.GetMyDest(out chaseStDest,actorsHitInTheWay);
               if(actorsHitInTheWay.Count>0){
                chaseSt.GetMyDestWithAvoidance(actorsHitInTheWay,out chaseStDestWithAvoidance);
                Debug.DrawLine(me.transform.position,chaseStDestWithAvoidance,Color.yellow,1f);
                chaseStDest=chaseStDestWithAvoidance;
               }
              }
             //// if(MyState==State.ATTACK_ST){
             ////  if(tryingAttack){
             ////   SetMyState(State.ATTACK_ST);
             ////   goto _MyStateSet;
             ////  }
             //  //if(isInAttackRange){
             //  // SetMyState(State.ATTACK_ST);
             //  // goto _MyStateSet;
             //  //}
             //// }
              if(setDest){
               MyDest=chaseStDest;
              }
              if(isInAttackRange){
               turnToEnemy=true;
             ////  if(setDest){//  not traveling
             ////   if(!shouldPredictMyEnemyDest){
             ////    if(Vector3.Distance(me.navMeshAgent.destination,me.navMeshAgent.transform.position)<=me.navMeshAgent.stoppingDistance){
             ////     SetMyState(State.ATTACK_ST);
             ////     goto _MyStateSet;
             ////    }
             ////   }
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
              if(tryingAttack){
               turnToEnemy=true;
              }
              Vector3 attackStDest=MyDest;
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
               attackSt.GetMyDest(out attackStDest,null);
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