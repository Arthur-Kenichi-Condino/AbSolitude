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
         [NonSerialized]SimObject currentMyEnemy;
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
         [NonSerialized]internal bool changeDest;
         [NonSerialized]internal bool shouldPredictMyEnemyDest;
            void UpdateMyState(){
             lastState=MyState;
             if(MyEnemy!=currentMyEnemy){
              myEnemyBaseAI=MyEnemy as BaseAI;
              if(MyEnemy==null){
               myEnemyPos=previousMyEnemyPos=me.transform.position;
              }else{
               myEnemyPos=previousMyEnemyPos=MyEnemy.transform.position;
              }
              travelTime=0f;
              renewDestinationMyEnemyMovedTimer=0f;
              currentMyEnemy=MyEnemy;
             }
             traveling=me.IsTraversingPath();
             changeDest=false;
             shouldPredictMyEnemyDest=false;
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
             float dis1=     myMoveSpeed*Time.deltaTime*1.5f;
             float dis2=myEnemyMoveSpeed*Time.deltaTime*1.5f;
             float ratio;
             if(dis2<=0f){
              ratio=1f;
             }else{
              ratio=dis1/dis2;
             }
             predictMyEnemyDestDis=ratio*dis1;
             if(!traveling){
              changeDest=true;
             }else{
              changeDest=false;
             }
             if(MyEnemy==null){
             }else{
              if(renewDestinationMyEnemyMovedTimer>0f){
               renewDestinationMyEnemyMovedTimer-=Time.deltaTime;
               if(renewDestinationMyEnemyMovedTimer<=0f){
                changeDest=true;
                shouldPredictMyEnemyDest=true;
               }
              }
              if(myEnemyChangedPos){
               //Log.DebugMessage("myEnemyChangedPos");
               if(renewDestinationMyEnemyMovedTimer<=0f){
                renewDestinationMyEnemyMovedTimer=renewDestinationMyEnemyMovedTimeInterval;
               }
              }
              if(shouldPredictMyEnemyDest){
               Log.DebugMessage("shouldPredictMyEnemyDest");
              }
              if(changeDest){
               traveling=false;
              }
              if(traveling){
               travelTime+=Time.deltaTime;
              }else{
               travelTime=0f;
              }
              Vector3 chaseStDest=MyDest;
              if(changeDest){
               chaseSt.GetMyDest(out chaseStDest);
              }
              if(MyState==State.ATTACK_ST){
               if(isInAttackRange){
                SetMyState(State.ATTACK_ST);
                goto _MyStateSet;
               }
              }
              if(isInAttackRange){
               if(changeDest){
                if(Vector3.Distance(me.navMeshAgent.destination,me.navMeshAgent.transform.position)<=me.navMeshAgent.stoppingDistance){
                 SetMyState(State.ATTACK_ST);
                 goto _MyStateSet;
                //}else if(){
                }
               }else{
                SetMyState(State.ATTACK_ST);
                goto _MyStateSet;
               }
              }else{
              }
              if(changeDest){
               MyDest=chaseStDest;
              }
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