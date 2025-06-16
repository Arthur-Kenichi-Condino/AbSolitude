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
        internal partial class AI{
         internal IDLE_ST idleSt;
            internal class IDLE_ST:ST{
                internal IDLE_ST(BaseAI me,AI ai):base(me,ai){
                }
                internal void Start(){
                 doingRandomMove=false;
                }
             internal float delayToDoRandomMove=8.0f;
              internal float timerToDoRandomMove=8.0f;
               internal float useRunSpeedChance=0.125f;
                internal bool doingRandomMove;
                internal bool doingRandomMoveRun;
                internal void DoRoutine(){
                 me.stopPathfindingOnTimeout=false;//
                 ai.DoSkill();
                 if(
                  !me.IsTraversingPath()
                 ){
                  if(timerToDoRandomMove>0.0f){
                     timerToDoRandomMove-=Time.deltaTime;
                  }
                  if(!doingRandomMove){
                   if(timerToDoRandomMove<=0.0f){
                    if(me.doIdleMove){
                     if(me.GetRandomPosition(me.transform.position,8.0f,out Vector3 result)){
                      doingRandomMoveRun=Mathf.Clamp01((float)me.math_random.NextDouble())<useRunSpeedChance;
                      ai.MyDest=result;
                      doingRandomMove=true;
                     }else{
                      timerToDoRandomMove=delayToDoRandomMove;
                     }
                    }
                   }
                  }
                  if(doingRandomMove){
                   if(me.Move(ai.MyDest,doingRandomMoveRun)){
                    doingRandomMove=false;
                    timerToDoRandomMove=delayToDoRandomMove;
                   }
                  }
                 }
                }
            }
        }
     //   protected virtual void OnIDLE_ST_Start(){
     //   }
     //[SerializeField]protected bool doIdleMove=true;
     //[SerializeField]protected float useRunSpeedChance=0.5f;
     //[SerializeField]protected float delayToRandomMove=8.0f;
     //protected float timerToRandomMove=2.0f;
     //   protected virtual void OnIDLE_ST_Routine(){
     //    if(
     //     !IsTraversingPath()
     //    ){
     //     if(timerToRandomMove>0.0f){
     //        timerToRandomMove-=Time.deltaTime;
     //     }else if(doIdleMove){
     //        timerToRandomMove=delayToRandomMove;
     //      //Log.DebugMessage("can do random movement");
     //      if(GetRandomPosition(transform.position,8.0f,out Vector3 result)){
     //       //Log.DebugMessage("got random position:"+result);
     //       bool run=Mathf.Clamp01((float)math_random.NextDouble())<useRunSpeedChance;
     //       if(navMeshAgentShouldUseRunSpeed||run){
     //        navMeshAgent.speed=navMeshAgentRunSpeed;
     //       }else{
     //        navMeshAgent.speed=navMeshAgentWalkSpeed;
     //       }
     //       Move(result);
     //      }
     //     }
     //    }
     //   }
    }
}