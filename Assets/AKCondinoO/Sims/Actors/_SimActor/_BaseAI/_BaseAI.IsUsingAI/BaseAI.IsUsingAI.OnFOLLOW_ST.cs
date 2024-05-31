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
         internal FOLLOW_ST followSt;
            internal class FOLLOW_ST:ST{
                internal FOLLOW_ST(BaseAI me,AI ai):base(me,ai){
                }
                internal void DoRoutine(){
                 me.stopPathfindingOnTimeout=false;//
                 ai.DoSkill();
                 if(
                  !me.IsTraversingPath()
                 ){
                  if(me.masterSimObject is BaseAI masterAI){
                   if(masterAI.isUsingAI){
                    if(masterAI.state==State.IDLE_ST){
                     me.MoveToMasterRandom(masterAI,4f);
                    }else{
                     me.MoveToMaster      (masterAI,0f);
                    }
                   }else{
                    if(!masterAI.IsMoving()){
                     me.MoveToMasterRandom(masterAI,4f);
                    }else{
                     me.MoveToMaster      (masterAI,0f);
                    }
                   }
                  }
                 }
                }
            }
        }
        //protected virtual void OnFOLLOW_ST_Routine(){
        // //Log.DebugMessage("OnFOLLOW_ST_Routine()");
        // stopPathfindingOnTimeout=false;//
        // if(
        //  !IsTraversingPath()
        // ){
        //  if(masterSimObject is BaseAI masterAI){
        //   if(masterAI.isUsingAI){
        //    if(masterAI.state==State.IDLE_ST){
        //     MoveToMasterRandom(masterAI,4f);
        //    }else{
        //     MoveToMaster      (masterAI,0f);
        //    }
        //   }else{
        //    if(!masterAI.IsMoving()){
        //     MoveToMasterRandom(masterAI,4f);
        //    }else{
        //     MoveToMaster      (masterAI,0f);
        //    }
        //   }
        //  }else{
        //  }
        // }
        //}
    }
}