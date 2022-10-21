#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Water{
    internal class VoxelWaterChunk:MonoBehaviour{
     internal WaterSpreadingContainer waterSpreadingBG=new WaterSpreadingContainer();
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         waterSpreadingBG.IsCompleted(VoxelSystem.singleton.waterSpreadingBGThreads[0].IsRunning,-1);
        }
     bool waitingMarchingCubes;
     bool pendingMarchingCubes;
     bool waitingWaterSpread;
        internal void ManualUpdate(){
         //Log.DebugMessage("ManualUpdate");
         if(waitingWaterSpread){
             if(OnWaterSpread()){
                 waitingWaterSpread=false;
                 pendingMarchingCubes=true;
             }
         }else{
             if(CanSpreadWater()){
                 waitingWaterSpread=true;
             }
         }
        }
        bool CanSpreadWater(){
         return false;
        }
        bool OnWaterSpread(){
         return false;
        }
    }
}