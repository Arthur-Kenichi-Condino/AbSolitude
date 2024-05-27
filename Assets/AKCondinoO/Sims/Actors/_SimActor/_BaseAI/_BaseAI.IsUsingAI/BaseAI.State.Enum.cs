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
            void UpdateMyState(){
             lastState=MyState;
             if(me.IsDead()){
              SetMyState(State.DEAD_ST);
              goto _MyStateSet;
             }else{
              if      (MyState==State. SNIPE_ST){
               //  so the animation is completed
               if(me.IsShooting()||me.IsReloading()){
                SetMyState(State. SNIPE_ST);
                goto _MyStateSet;
               }
              }else if(MyState==State.ATTACK_ST){
               //  so the animation is completed
               if(me.IsAttacking()||attackSt.firstAttack){
                SetMyState(State.ATTACK_ST);
                goto _MyStateSet;
               }
              }
              if(MyEnemy!=null){
               if(me.sniper){
                if(isInAttackRangeWithWeapon){
                 if(!isInAttackRange||(
                   me.IsFasterThan(MyEnemy)&&(
                   attackDistance.z<attackDistanceWithWeapon.z||
                   attackDistance.x<attackDistanceWithWeapon.x||
                   attackDistance.y<attackDistanceWithWeapon.y)
                  )
                 ){
                  SetMyState(State.SNIPE_ST);
                  goto _MyStateSet;
                 }
                }
               }
               if(isInAttackRange){
                SetMyState(State.ATTACK_ST);
                goto _MyStateSet;
               }
               if(MyState==State.SNIPE_ST){
                if(snipeSt.timer<snipeSt.minTimeBeforeCanChase){
                 SetMyState(State.SNIPE_ST);
                 goto _MyStateSet;
                }
               }
               SetMyState(State.CHASE_ST);
               goto _MyStateSet;
              }else{
               if(me.masterId!=null){
                float disToMaster=me.GetDistance(me,me.masterSimObject);
                if(disToMaster>=0f){
                 if(disToMaster>8f){
                  Log.DebugMessage("I should follow my master:"+me.masterSimObject+";me:"+me);
                  SetMyState(State.FOLLOW_ST);
                  goto _MyStateSet;
                 }
                }
               }
               SetMyState(State.IDLE_ST);
               goto _MyStateSet;
              }
             }
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
              }else if(state==State.  IDLE_ST){
                 //idleSt.Start();
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
                //idleSt.DoRoutine();
             }
            }
        }
        internal enum State:int{
           IDLE_ST= 0,
         FOLLOW_ST= 1,
          CHASE_ST= 2,
         ATTACK_ST= 3,
          SNIPE_ST=14,
           DEAD_ST=16,
        }
    }
}